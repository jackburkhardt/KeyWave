using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
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

        [ShowIf("capFramerate")]
        public int framerateLimit;

        private CustomLuaFunctions _customLuaFunctions;
        public DailyReport dailyReport;

        private string last_convo = string.Empty;

        public static GameState gameState => GameStateManager.instance.gameState;


        private void Awake()
        {
       
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
            WatchHandCursor.GlobalFreeze();
        }

        public void UnpauseDialogueSystem()
        {
            DialogueManager.instance.Unpause();
            WatchHandCursor.GlobalUnfreeze();
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
            yield return App.App.Instance.LoadScene("Hotel");
            dailyReport = new DailyReport(gameState.day);
        
            while (App.App.isLoading)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.25f);
            DialogueManager.StartConversation("Intro");
        }


        public static void OnQuestStateChange(string questName)
        {
            Debug.Log("quest state change");
            SaveDataStorer.WebStoreGameData(PixelCrushers.SaveSystem.RecordSavedGameData());

            var quest = DialogueManager.masterDatabase.GetQuest(questName);
            var state = QuestLog.GetQuestState(questName);
            var points = DialogueUtility.GetPointsFromField(quest!.fields);
            
            Debug.Log("Quest State Change: " + questName + " " + state);

            if (quest.Group == "Main Task")
            {
                Debug.Log("Main Task");
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

            if (state == QuestState.Success && points.Points > 0)
            {
                GameEvent.OnPointsIncrease(points, questName);
                points.Points = 0;
                DialogueLua.SetItemField(questName, "Points", points.ToString());
            }
        
            var duration = state == QuestState.Success ? DialogueUtility.GetQuestDuration(quest) : 0;
        
            GameEvent.OnQuestStateChange(questName, state, duration);
        }

        public void OpenMap()
        {
            //last_convo = gameState.current_conversation_title;
            DialogueManager.StopConversation();
            DialogueManager.instance.PlaySequence("SetDialoguePanel(false)");
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
        }


        public void EndOfDay() => App.App.Instance.ChangeScene("EndOfDay", gameStateManager.gameState.current_scene);

        public void StartOfDay() => App.App.Instance.ChangeScene("StartOfDay", gameStateManager.gameState.current_scene);

        public void TravelTo(Location location)
        {
            TravelTo(location.Name);
        }

        public void TravelTo(string newLocation, string currentScene = "")
        {

            DialogueManager.StopConversation();
            if (currentScene == "") currentScene = gameState.current_scene;
            StartCoroutine(TravelToHandler());

            IEnumerator TravelToHandler()
            {
                yield return App.App.Instance.ChangeScene(newLocation, currentScene);
            
                while (App.App.isLoading)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1f);

                DialogueManager.StartConversation($"{newLocation}/Base");

                gameState.current_scene = newLocation;
            
            
            }
        }

        public void Wait(int duration)
        {
            GameEvent.OnWait(duration);
        }
    }
}