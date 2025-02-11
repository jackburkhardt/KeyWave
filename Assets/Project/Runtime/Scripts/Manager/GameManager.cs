using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;
using Transition = Project.Runtime.Scripts.AssetLoading.LoadingScreen.Transition;

namespace Project.Runtime.Scripts.Manager
{
    [Serializable]


    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public static GameStateManager gameStateManager;
        public static PlayerEventStack playerEventStack;

        public static Settings settings
        {
            get
            {
                #if UNITY_EDITOR
                if (instance == null) return AssetDatabase.LoadAssetAtPath<Settings>("Assets/Game Settings.asset");
                #else
                if (instance == null)
                {
                    // force instantiation of GM instance since there are dependants in Awake, OnEnable of other scripts
                    instance = FindAnyObjectByType<GameManager>();
                }
                #endif
                return instance.currentSettings;
            }
            set => instance.currentSettings = value;
        }
        
        public List<Location> locations;
        public bool capFramerate = false;
        public Canvas mainCanvas;
        [SerializeField] private Settings currentSettings;
        
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
        
        private float autoPauseCooldown = 0.5f;

        private void Update()
        {
            
            if (autoPauseCooldown > 0)
            {
                autoPauseCooldown -= Time.deltaTime;
            }

            if (!Application.isFocused && !SceneManager.GetSceneByName("PauseMenu").isLoaded && autoPauseCooldown <= 0)
            {
                TogglePause();
                autoPauseCooldown = 0.5f;
            }
            
            if (capFramerate) Application.targetFrameRate = framerateLimit;
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
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
            if (gameState.Clock > Clock.DayEndTime)
            {
                GameEvent.OnDayEnd();
                DialogueManager.StopAllConversations();
                DialogueManager.StartConversation("EndOfDay");
            }
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
                TravelTo(Location.PlayerLocation, Transition.Black);

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

        public void OnQuestStateChange(string questName)
        {
            var quest = DialogueManager.masterDatabase.GetQuest(questName);
            var state = QuestLog.GetQuestState(questName);
            var points = DialogueUtility.GetPointsFromField(quest!.fields);
      
            // if this quest already succeeded, we don't want to retrigger events
            if (dailyReport.CompletedTasks.Contains(questName) && !quest.IsRepeatable) return;

            if (state == QuestState.Success)
            {                
                foreach (var pointField in points)
                {
                    if (pointField.Points == 0) continue;

                    if (quest.IsRepeatable)
                    {
                        var repeatCount = DialogueLua.GetQuestField(questName, "Repeat Count").asInt;
                        var multiplier = 1 - quest.LookupFloat("Repeat Points Reduction");
                        
                        for (int i = 0; i < repeatCount; i++)
                        {
                            pointField.Points = (int) (pointField.Points * multiplier);
                        }
                        
                    }
                    
                    GameEvent.OnPointsIncrease(pointField, questName);
                    
                }
                    
                if (quest.IsRepeatable)
                {
                    QuestLog.SetQuestState(questName, QuestState.Active);
                    var completionCount = quest.LookupInt("Repeat Count");
                    quest.AssignedField("Repeat Count").value = (completionCount + 1).ToString();
                    Debug.Log( $"Quest {questName} has been repeated {completionCount + 1} times.");
                }
                
            }
        
            var duration = state == QuestState.Success ? DialogueUtility.GetQuestDuration(quest) : 0;
            
            GameEvent.OnQuestStateChange(questName, state, duration);
            SaveDataStorer.WebStoreGameData(PixelCrushers.SaveSystem.RecordSavedGameData());
        }

        public void EndOfDay() => App.App.Instance.ChangeScene("EndOfDay", gameStateManager.gameState.current_scene, Transition.Black);

        public void StartOfDay() => App.App.Instance.ChangeScene("StartOfDay", gameStateManager.gameState.current_scene);

        public void TravelTo(Location location, Transition? type =  Transition.Default)
        {
            TravelTo(location.Name, loadingScreenType: type);
        }

        public void TravelTo(string newLocation, string currentScene = "", Transition? loadingScreenType =  Transition.Default, Action onStart = null, Action onComplete = null)
        {
            BroadcastMessage( "OnTravel");

            DialogueManager.StopConversation();
            
            OnGameSceneEnd?.Invoke();
            
            onStart?.Invoke();
            
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
                
                AudioEngine.Instance.StopAllAudioOnChannel("Music");
                while (App.App.isLoading)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1f);

                DialogueManager.StartConversation($"{newLocation}/Base");
                
                OnGameSceneStart?.Invoke();
                
                
                gameState.current_scene = newLocation;
                
                onComplete?.Invoke();
            }
        }

        public void Wait(int duration)
        {
            GameEvent.OnWait(duration);
        }
        
        
        public void TogglePause()
        {
            if (SceneManager.GetSceneByName("PauseMenu").isLoaded != PauseMenu.active) return;
            
            if (PauseMenu.active)
            {
                PauseMenu.instance.UnpauseGame();
            }
            else
            {
                App.App.Instance.LoadScene("PauseMenu", transition: Transition.None);
            }
        }

        public UnityEvent OnGameClose;
        public void CloseGame()
        {
            AudioEngine.Instance.StopAllAudio();
            
            OnGameClose?.Invoke();
        }

        public void OnConversationLineEnd()
        {
            
        }
        
        
    }
}