using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.Manager
{
    [Serializable]


    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public static GameStateManager gameStateManager;
        public static PlayerEventStack playerEventStack;
        public List<Location> locations;
        public bool capFramerate = false;
        public Canvas mainCanvas;
        
        public SmartWatch smartWatchAsset;

        [ShowIf("capFramerate")] public int framerateLimit;

        private CustomLuaFunctions _customLuaFunctions;
        public DailyReport dailyReport;

        private string last_convo = string.Empty;

        public static GameState gameState => GameStateManager.instance.gameState;

        public static Action OnGameManagerAwake;
        public static Action OnMapOpen;

        public UnityEvent OnGameSceneStart;
        public UnityEvent OnGameSceneEnd;
        
        private static string _mostRecentApp = "SmartWatch/Home";

        public static string MostRecentApp
        {
            get
            {
                var convo = _mostRecentApp;
                _mostRecentApp = "SmartWatch/Home";
                return convo;
            }
            
            set => _mostRecentApp = $"SmartWatch/{value}";
        }


        private void Awake()
        {
            OnGameManagerAwake?.Invoke();

            //if instance of GameManager already exists, destroy it

            if (instance == null)
            {
                instance = this;
            }

            else if (instance != this)
            {
                Destroy(this);
            }


            if (gameStateManager == null)
            {
                gameStateManager = this.AddComponent<GameStateManager>();
            }

            if (playerEventStack == null)
            {
                playerEventStack = ScriptableObject.CreateInstance<PlayerEventStack>();
            }

            _customLuaFunctions = GetComponent<CustomLuaFunctions>() ?? gameObject.AddComponent<CustomLuaFunctions>();

        }

        private void Start()
        {
            GameEvent.OnPlayerEvent += OnPlayerEvent;
            //OnSaveDataApplied();
        }

        private void Update()
        {
            if (capFramerate) Application.targetFrameRate = framerateLimit;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                DialogueManager.instance.PlaySequence("Continue()");
            }
        }

        private void OnDestroy()
        {
            Destroy(playerEventStack);
            Destroy(gameStateManager);
        }

        public void PauseDialogueSystem()
        {
            DialogueManager.instance.Pause();
        }

        public void UnpauseDialogueSystem()
        {
            DialogueManager.instance.Unpause();
        }

        public void OnSaveDataApplied()
        {
            dailyReport ??= new DailyReport(gameState.day);
        }

        private void OnPlayerEvent(PlayerEvent e)
        {
            if (gameState.Clock > Clock.DailyLimit)
            {
                GameEvent.OnDayEnd();
                DialogueManager.StopAllConversations();
                EndOfDay();
            }
        }

        public void StartNewDay()
        {
            GameStateManager.instance.StartNextDay();
            dailyReport = new DailyReport(gameState.day);
            App.App.Instance.ChangeScene("Hotel", "EndOfDay");
        }

        public IEnumerator StartNewSave()
        {
            dailyReport = new DailyReport(gameState.day);
            
            while (App.App.isLoading)
            {
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(0.25f);
            while (!DialogueManager.hasInstance)
            {
                yield return null;
            }

            if (DialogueLua.GetVariable("skip_content").asBool)
                TravelTo(Location.PlayerLocation, LoadingScreen.LoadingScreenType.Black);

            else DialogueManager.instance.StartConversation("Intro");
        }

        public static void OnConversationStart()
        {
            var activeConversation = DialogueManager.instance.activeConversation;
            var conversation = DialogueManager.masterDatabase.GetConversation(activeConversation.conversationTitle);
            if (Field.FieldExists(conversation.fields, "Base") &&
                !DialogueLua.GetConversationField(conversation.id, "Base").asBool) return;
            instance.mainCanvas.BroadcastMessage("SetTrigger", "Show", SendMessageOptions.DontRequireReceiver);
        }
        
        
        public static void DoLocalSave()
        {
            PixelCrushers.SaveSystem.SaveToSlot(1);
        }

        public static void OnQuestStateChange(string questName)
        {
          SaveDataStorer.WebStoreGameData(PixelCrushers.SaveSystem.RecordSavedGameData());

            var quest = DialogueManager.masterDatabase.GetQuest(questName);
            var state = QuestLog.GetQuestState(questName);
            var points = DialogueUtility.GetPointsFromField(quest!.fields);
            
            if (quest.Group == "Main Task")
            {
           
                if (state == QuestState.Active)
                {
                    Debug.Log("Setting time start");
                    DialogueLua.SetQuestField(questName, "Time Start", Clock.CurrentTime);
                }
                
                else if (state == QuestState.Success)
                {
                    DialogueLua.SetQuestField(questName, "Time Complete", Clock.CurrentTime);
                }
            }

            if (state == QuestState.Success && points.Length > 0)
            {
                foreach (var pointField in points)
                {
                    GameEvent.OnPointsIncrease(pointField, questName);
                    quest.fields.RemoveAll(f => f.title == "Points");
                    
                }
            }
        
            var duration = state == QuestState.Success ? DialogueUtility.GetQuestDuration(quest) : 0;
            
            Debug.Log($"Quest {questName} state changed to {state}");
        
            GameEvent.OnQuestStateChange(questName, state, duration);
        }

        public void OpenMap()
        {
            //last_convo = gameState.current_conversation_title;
            DialogueManager.StopConversation();
            
            App.App.Instance.LoadScene("Map");
            
            OnMapOpen?.Invoke();
        }

        public void CloseMap(bool returnToConvo)
        {
            App.App.Instance.UnloadScene("Map");
            if (returnToConvo)
            {
                DialogueManager.instance.PlaySequence("SetDialoguePanel(true)");
                DialogueManager.StartConversation(last_convo);
            }
            
            DialogueManager.instance.PlaySequence(
                "ChannelFade(Music, out, 1);"
            );
            
        }


        public void EndOfDay() => App.App.Instance.ChangeScene("EndOfDay", gameStateManager.gameState.current_scene);

        public void StartOfDay() => App.App.Instance.ChangeScene("StartOfDay", gameStateManager.gameState.current_scene);

        public void TravelTo(Location location, LoadingScreen.LoadingScreenType? type =  LoadingScreen.LoadingScreenType.Default)
        {
            TravelTo(location.Name, loadingScreenType: type);
        }

        public void TravelTo(string newLocation, string currentScene = "", LoadingScreen.LoadingScreenType? loadingScreenType =  LoadingScreen.LoadingScreenType.Default)
        {

            DialogueManager.StopConversation();
            
            OnGameSceneEnd?.Invoke();
            
            if (currentScene == "")
            {
                if (SceneManager.GetSceneByName("StartMenu").isLoaded) currentScene = "StartMenu";
                else currentScene = gameState.current_scene;
            }


            SmartWatch.ResetCurrentApp();
            
            
            StartCoroutine(TravelToHandler());

            DialogueManager.PlaySequence("ChannelFade(Music, out, 1);");
            DialogueManager.PlaySequence("ChannelFade(Environment, out, 1);");

            IEnumerator TravelToHandler()
            {
                
                yield return App.App.Instance.ChangeScene(newLocation, currentScene, loadingScreenType);
            
                while (App.App.isLoading)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1f);

                DialogueManager.StartConversation($"{newLocation}/Base");
                
                OnGameSceneStart?.Invoke();

                if (newLocation != "StartMenu") Log("Arrived at " + newLocation, LogType.Travel);
                gameState.current_scene = newLocation;
            }
        }

        public void Wait(int duration)
        {
            GameEvent.OnWait(duration);
        }
        
        public enum LogType
        {
            Default,
            Travel,
        }
        
        public static void Log(string message, LogType type = LogType.Default)
        {
            var log = DialogueLua.GetVariable("game.log").asString;
            var color = type switch
            {
                LogType.Default => string.Empty,
                LogType.Travel => "<color=#98fff3>",
                _ => string.Empty
            };
            
            
            log += $"[br({color}{Clock.CurrentTime})] {color}{message}";
            DialogueLua.SetVariable("game.log", log);
        }

       

        
    }
}