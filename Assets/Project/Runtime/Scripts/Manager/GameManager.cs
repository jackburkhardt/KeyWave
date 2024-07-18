using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
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

        [ShowIf("capFramerate")] public int framerateLimit;

        private CustomLuaFunctions _customLuaFunctions;
        public DailyReport dailyReport;

        private string last_convo = string.Empty;

        public static GameState gameState => GameStateManager.instance.gameState;

        public static Action OnGameManagerAwake;


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
        }

        // Start is called before the first frame update

        private void OnEnable()
        {

        }

        private void OnDestroy()
        {
            Destroy(playerEventStack);
            Destroy(gameStateManager);
        }

        public void PauseDialogueSystem()
        {
            DialogueManager.instance.Pause();
           // WatchHandCursor.GlobalFreeze();
        }

        public void UnpauseDialogueSystem()
        {
            DialogueManager.instance.Unpause();
            //WatchHandCursor.GlobalUnfreeze();
        }

        public void OnSaveDataApplied()
        {
            dailyReport ??= new DailyReport(gameState.day);

            FindObjectOfType<PointsBar>().ApplySaveData();
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
            // yield return App.App.Instance.LoadScene("Hotel");
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

            DialogueManager.instance.StartConversation("Intro");
        }

        public void StartMapConversation()
        {
            var database = DialogueManager.instance.masterDatabase;
            var conversationName = "Map";

            if (!DialogueManager.instance.isConversationActive)
            {
                DialogueManager.instance.StartConversation(conversationName);
                return;
            }
            
            var conversation = database.GetConversation(conversationName);
            var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
            var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
            DialogueManager.instance.BroadcastMessage("OnConversationBase", dialogueEntry);
            DialogueManager.conversationController.GotoState(state);
        }

        public void StartTalkConversation()
        {
          
            var database = DialogueManager.instance.masterDatabase;
            var conversationName = Location.PlayerLocation.name + "/Talk/Base";

            if (!DialogueManager.instance.isConversationActive)
            {
                DialogueManager.instance.StartConversation(conversationName);
                return;
            }
            
            var conversation = database.GetConversation(conversationName);
            var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
            var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
            DialogueManager.instance.BroadcastMessage("OnConversationBase", dialogueEntry);
            DialogueManager.conversationController.GotoState(state);
        }

        public void StartActionConversation()
        {
            var database = DialogueManager.instance.masterDatabase;
            var conversationName = Location.PlayerLocation.name + "/Action/Base";
            
            
            if (!DialogueManager.instance.isConversationActive)
            {
                DialogueManager.instance.StartConversation(conversationName);
                return;
            }
            
            var conversation = database.GetConversation(conversationName);
            var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
            
            DialogueManager.instance.BroadcastMessage("OnConversationBase", dialogueEntry);
            
            var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
            DialogueManager.conversationController.GotoState(state);
        }

    public static void OnConversationStart()
        {
            var activeConversation = DialogueManager.instance.activeConversation;
            var conversation = DialogueManager.masterDatabase.GetConversation(activeConversation.conversationTitle);
            if (Field.FieldExists(conversation.fields, "Base") &&
                !DialogueLua.GetConversationField(conversation.id, "Base").asBool) return;
            instance.mainCanvas.BroadcastMessage("SetTrigger", "Show");
        }
        
        public static void OnConversationEnd()
        {
            /*
            var activeConversation = DialogueManager.instance.activeConversation;
            if (activeConversation == null) return;
            var conversation = DialogueManager.masterDatabase.GetConversation(activeConversation.conversationTitle);
            if (Field.FieldExists(conversation.fields, "Base") &&
                !DialogueLua.GetConversationField(conversation.id, "Base").asBool) return;
            instance.mainCanvas.BroadcastMessage("SetTrigger", "Hide");
            */
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
        
            GameEvent.OnQuestStateChange(questName, state, duration);
        }

        public void OpenMap()
        {
            //last_convo = gameState.current_conversation_title;
            DialogueManager.StopConversation();
            DialogueManager.instance.PlaySequence(
                "SetDialoguePanel(false);" +
                "SetMenuPanelTrigger(1, false);" +
                "ChannelFade(Music, 0, 2);" +
                "StopChannel(Music)@2;" +
                "ChannelFade(Music, 1)@2;" +
                "PlayClip(music/intro)@2.5;"
                );
            App.App.Instance.LoadScene("Map");
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
                "ChannelFade(Music, 0, 0.75);" +
                "StopChannel(Music)@0.75;" +
                "ChannelFade(Music, 1)@1;"
                
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
            if (currentScene == "")
            {
                if (SceneManager.GetSceneByName("StartMenu").isLoaded) currentScene = "StartMenu";
                else currentScene = gameState.current_scene;
            }
            StartCoroutine(TravelToHandler());

            IEnumerator TravelToHandler()
            {
                yield return App.App.Instance.ChangeScene(newLocation, currentScene, loadingScreenType);
            
                while (App.App.isLoading)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1f);

                DialogueManager.StartConversation($"{newLocation}/Base");

                Debug.Log("current scene chaned to :" + newLocation);
                gameState.current_scene = newLocation;
            }
        }

        public void Wait(int duration)
        {
            GameEvent.OnWait(duration);
        }
    }
}