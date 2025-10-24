using System;
using System.Collections;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Transition = Project.Runtime.Scripts.AssetLoading.LoadingScreen.Transition;



namespace Project.Runtime.Scripts.Manager
{
    [Serializable]
    [RequireComponent( typeof( InputManager))]
    [RequireComponent( typeof( ConversationFlowManager))]
    [RequireComponent( typeof( LocationManager))]
    [RequireComponent( typeof( PointsManager))]
    
    public class GameManager : PlayerEventHandler
    {
        public static GameManager instance;
        public static int CurrentTime => settings != null && settings.Clock != null ? settings.Clock.CurrentTime : 0;
        public static PlayerEventStack playerEventStack;
        public static DialogueDatabase dialogueDatabase => settings != null ? settings.dialogueDatabase : null;

        public Actor PlayerActor =>
            DialogueManager.masterDatabase.actors.First(p => p.IsPlayer && p.IsFieldAssigned("Location"));

        public static Settings settings
        {
            get
            {
                instance ??= FindObjectOfType<GameManager>();

                if (instance == null) return null;
                return instance.currentSettings;
            }
            set => instance.currentSettings = value;
        }
        public bool capFramerate = false;
        public Canvas mainCanvas;
        [SerializeField] private Settings currentSettings;


        [ShowIf("capFramerate")] public int framerateLimit;

        public DailyReport dailyReport;

        public static Action OnGameManagerAwake;

        public UnityEvent OnGameSceneStart;
        public UnityEvent OnGameSceneEnd;

        
        public static bool TryGetSettings( out Settings settings)
        {
            instance ??= FindObjectOfType<GameManager>();
            if (instance == null)
            {
                settings = null;
                return false;
            }
            settings = instance.currentSettings;
            if (instance.currentSettings == null)
            {
                return false;
            }
            return true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
           App.App.OnSceneLoadEnd += StartGameScene;
           App.App.OnSceneDeloadStart += EndGameScene;
         
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            App.App.OnSceneLoadEnd -= StartGameScene;
            App.App.OnSceneDeloadStart -= EndGameScene;
        }

        protected override void OnPlayerEvent(PlayerEvent playerEvent)
        {
            Clock.AddSeconds(playerEvent.Duration);
        }




        private void Awake()
        {
            OnGameManagerAwake?.Invoke();


            if (instance == null)
            {
                instance = this;
            }

            else if (instance != this)
            {
                Destroy(this);
            }


            if (playerEventStack == null)
            {
                playerEventStack = ScriptableObject.CreateInstance<PlayerEventStack>();
            }

        }

        private void Update()
        {
            

            if (capFramerate) Application.targetFrameRate = framerateLimit;
        }
        
        private void OnDestroy()
        {
            Destroy(playerEventStack);
        }

        public void OnSaveDataApplied()
        {
            dailyReport ??= new DailyReport(1);
        }

        public void DoEndOfDay()
        {
            GameEvent.OnDayEnd();
            DialogueManager.instance.gameObject.BroadcastMessageExt( "OnEndOfDay");
            App.App.Instance.ChangeScene("EndOfDay", SceneManager.GetActiveScene().name, Transition.Black);
        }

        public IEnumerator StartNewSave()
        {
            dailyReport = new DailyReport(1);
            
            while (App.App.isLoading)
            {
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(0.25f);
            while (!DialogueManager.hasInstance)
            {
                yield return null;
            }

            if (DialogueLua.GetVariable("skip_content").asBool) LocationManager.SetPlayerLocation(LocationManager.instance.PlayerLocation.GetRootLocation(), Transition.Black);
            else DialogueManager.instance.StartConversation("Intro");
        }

        public void OnConversationStart()
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

        public void Wait(int duration)
        {
            GameEvent.OnWait(duration);
        }

        public void StartGameScene(string scene)
        {
            if (DialogueManager.masterDatabase.GetLocation(scene) == null) return;
            
            StartCoroutine(OnSceneStart());
            
            IEnumerator OnSceneStart()
            {
                yield return new WaitForSeconds(1f);
                OnGameSceneStart?.Invoke();
                DialogueManager.instance.gameObject.BroadcastMessageExt( "OnGameSceneStart");
            }
         
        }

        public void EndGameScene(string scene)
        {
            if (DialogueManager.masterDatabase.GetLocation(scene) == null) return;
            
            DialogueManager.StopConversation();
            OnGameSceneEnd?.Invoke();
            DialogueManager.instance.gameObject.BroadcastMessageExt( "OnGameSceneEnd");
            FindObjectOfType<SmartWatchPanel>().ResetCurrentApp();
            DialogueManager.PlaySequence("ChannelFade(Music, out, 1);");
            DialogueManager.PlaySequence("ChannelFade(Environment, out, 1);");
          
        }


        public void OnGameActionEnd(Item item)
        {
            if (item.IsFieldAssigned("Script"))
            {
                var script = item.LookupValue("Script");
                Lua.Run(script);
            }
        }
        
     

        public UnityEvent OnGameClose;
        public void CloseGame()
        {
            AudioEngine.Instance.StopAllAudio();
            
            OnGameClose?.Invoke();
        }

        public void ForceConversation(string conversation)
        {
            var conversationState = DialogueManager.currentConversationState;

            if (conversationState == null)
            {
                DialogueManager.instance.StartConversation(conversation);
            }

            else
            {
                var dialogueEntry = DialogueManager.currentConversationState.subtitle.dialogueEntry;
                var currentConversation = DialogueManager.masterDatabase.GetConversation(dialogueEntry.conversationID);
                
                if (currentConversation.Title != conversation)
                {
                    DialogueManager.instance.StopConversation();
                    DialogueManager.instance.StartConversation(conversation);
                }
            }
        }

        /// <summary>
        ///  Checks if the current line is associated with an Action, and if so, starts the action
        /// </summary>
        public void OnConversationLine(Subtitle subtitle)
        {
            var conversation = DialogueManager.masterDatabase.GetConversation(subtitle.dialogueEntry.conversationID);

            foreach (var action in subtitle.dialogueEntry.fields.Where(p => p.title == "Action"))
            {
                if (subtitle.dialogueEntry.outgoingLinks.Count > 0 &&
                    subtitle.dialogueEntry.outgoingLinks[0].destinationConversationID !=
                    subtitle.dialogueEntry.conversationID)
                {

                    var destinationConversation =
                        DialogueManager.masterDatabase.GetConversation(subtitle.dialogueEntry.outgoingLinks[0]
                            .destinationConversationID);
                    destinationConversation.fields.Add(action);
                }
                else conversation.fields.Add(action);
                
                
                DialogueManager.instance.gameObject.BroadcastMessage( "OnGameActionStart", DialogueManager.masterDatabase.GetItem(int.Parse(action.value)), SendMessageOptions.DontRequireReceiver );
            }
            
            subtitle.dialogueEntry.fields.RemoveAll(p => p.title == "Action");

            // if the next conversation is not the same as the current conversation, move the action to the next conversation

            if (subtitle.dialogueEntry.outgoingLinks.Count == 1 && subtitle.dialogueEntry.outgoingLinks[0].destinationConversationID != subtitle.dialogueEntry.conversationID)
            {
                var newConversation = DialogueManager.masterDatabase.GetConversation(subtitle.dialogueEntry.outgoingLinks[0].destinationConversationID);
                foreach (var action in conversation.fields.Where(p => p.title == "Action"))
                {
                    newConversation.fields.Add(action);
                }
                
                conversation.fields.RemoveAll(p => p.title == "Action");
            }
        }

        /// <summary>
        /// Checks if the current conversation is associated with an Action, and if so, ends the action
        /// </summary>

        public void OnConversationEnd()
        {
            
            
            var conversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
            var conversation = DialogueManager.masterDatabase.GetConversation(conversationID);
            
            var actions = conversation.fields.Where(p => p.title == "Action").ToList();
            
            foreach (var action in  actions)
            {
                var item = DialogueManager.masterDatabase.GetItem(int.Parse(action.value));
                DialogueManager.instance.gameObject.BroadcastMessage( "OnGameActionEnd", item, SendMessageOptions.DontRequireReceiver );
            }

            conversation.fields.RemoveAll(p => p.title == "Action");

        }
        
        public void OnQuestStateChange(string questName)
        {
            var quest = DialogueManager.masterDatabase.GetItem(questName);
            if (!quest.IsAction && !quest.IsQuest) return;
            
            var state = QuestLog.GetQuestState(questName);
            
            Points.PointsField[] points =  new []{ new Points.PointsField()};
      
            // if this quest already succeeded, we don't want to retrigger events
            if (GameManager.instance.dailyReport.CompletedTasks.Contains(questName) && !quest.IsStatic) return;

            if (state == QuestState.Success)
            {                
                
                DialogueManager.instance.gameObject.BroadcastMessage("OnQuestSuccess", questName, SendMessageOptions.DontRequireReceiver);
                
                points = PointsManager.GetQuestPoints( questName, true);
                
                if (quest.IsAction)
                {
                    if (quest.IsStatic) QuestLog.SetQuestState(questName, QuestState.Active);
                }
            }
        
            var duration = 0;
            
            if (quest.IsQuest) GameEvent.OnQuestStateChange(questName, state, points, duration);
            else if (quest.IsAction && ((quest.IsStatic && state == QuestState.Success) || !quest.IsStatic)) GameEvent.OnActionStateChange(questName, state, points, duration);
            
            SaveDataStorer.WebStoreGameData(PixelCrushers.SaveSystem.RecordSavedGameData());
            
            if (QuestLog.GetQuestEntryCount( questName) > 0 && quest.LookupBool("Auto Set Success"))
            {
                for (int i = 1; i < QuestLog.GetQuestEntryCount( questName) + 1; i++)
                {
                    var entryState = QuestLog.GetQuestEntryState(questName, i);
                    if (entryState != QuestState.Success) QuestLog.SetQuestEntryState( questName, i, QuestState.Success);
                }
            }
            
        }


        public void OnQuestEntryStateChange(QuestEntryArgs args)
        {
            
            var quest = DialogueManager.masterDatabase.GetItem(args.questName);
            if (!quest.IsAction || !quest.IsQuest) return;
            
            var entry = args.entryNumber;
            var prefix = $"Entry {entry}";
            
            var state = QuestLog.GetQuestEntryState( quest.Name, entry);
            
            var points = quest.fields.Where(p => p.title.StartsWith(prefix) && p.title.EndsWith(" Points")).ToList();
            
          

            if (state == QuestState.Success)
            {
                foreach (var pointField in points)
                {
                    var value = pointField.value;
                    var pointType = pointField.title.Split(" ")[^2];
                    
                    var pointsField = new Points.PointsField
                    {
                        Points = int.Parse(value),
                        Type = pointType
                    };
                    
                    if (pointsField.Points == 0) continue;
                    Points.AddPoints(pointsField.Type, pointsField.Points);
                }
            }
            
            
            bool autoSetSuccess = quest.LookupBool("Auto Set Success");
            for (int i = 0; i < QuestLog.GetQuestEntryCount( quest.Name); i++)
            {
                autoSetSuccess = autoSetSuccess && QuestLog.GetQuestEntryState(quest.Name, i) == QuestState.Success;
            }
            
            if (autoSetSuccess)
            {
                QuestLog.SetQuestState(quest.Name, QuestState.Success);
            }
            
        }
        
    }
}
