using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.Utility;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Project.Runtime.Scripts.Manager
{
    public class GameState
    {
        public string current_scene = "Hotel";
        public int day = 1;

        public string LastNonCaféLocation
        {
            get
            {
                var lastLocation = DialogueLua.GetVariable("game.player.lastNonCaféLocation").asString;
                return lastLocation == string.Empty || lastLocation == "nil" ? GetPlayerLocation().Name : lastLocation;
            }
            set => DialogueLua.SetVariable("game.player.lastNonCaféLocation", value);
        }
        
        private Actor PlayerActor =>  DialogueManager.masterDatabase.actors.First(p => p.IsPlayer && p.IsFieldAssigned("Location"));

        /// <summary>
        /// Sets the player's location or sublocation to the specified location.
        /// </summary>
        /// <returns>This will only return the player's root location, not the sublocation.</returns>
        public void SetPlayerLocation(Location value)
        {
            DialogueLua.SetActorField( PlayerActor.Name, "Location", value.id);
                
            var location = DialogueManager.masterDatabase.GetLocation(DialogueLua.GetActorField(PlayerActor.Name, "Location").asInt);
            var rootLocation = DialogueManager.masterDatabase.GetLocation(location.RootID);

            if (rootLocation.Name != "Café")
                LastNonCaféLocation = DialogueManager.masterDatabase.GetLocation(value.RootID).Name;
        }
        
        public Location GetPlayerLocation(bool specifySublocation = false)
        {
            var location = DialogueManager.masterDatabase.GetLocation(DialogueLua.GetActorField(PlayerActor.Name, "Location").asInt);
            var rootLocation = DialogueManager.masterDatabase.GetLocation(location.RootID);
            
            if (specifySublocation) return location;
            return rootLocation;
        }
        
        
        public int MostRecentSublocation
        {
            get => (DialogueLua.GetVariable("game.player.mostRecentSublocation").asInt);
            set => DialogueLua.SetVariable("game.player.mostRecentSublocation", value);
        }
        
        
        
    }

    public class GameStateManager : PlayerEventHandler
    {
        public enum State
        {
            Travel,
            PreBase,
            Base,
            Action,
            StorySequence
        }
        
        public static State state = State.Base;
        
        public static GameStateManager instance;
        public static Action<GameState> OnGameStateChanged;

        public GameState gameState = new GameState();

        protected void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            else if (instance != this)
            {
                Destroy(this);
            }

            state = State.PreBase;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Clock.onTimeChange += OnTimeChange;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            Clock.onTimeChange -= OnTimeChange;
        }

        protected override void OnPlayerEvent(PlayerEvent playerEvent)
        {
            switch (playerEvent.EventType)
            {
                case "move":
                    StopAllCoroutines(); 
                    var locationName = playerEvent.Data["newLocation"]?.ToString();
                    
                    Debug.Log("Moving to: " + locationName);

                    var location = DialogueManager.masterDatabase.GetLocation(locationName);
                    
                    if (location.FieldExists("Spawn Point"))
                    {
                        var spawnPoint = DialogueLua.GetLocationField(location.Name, "Spawn Point").asInt;
                        if (spawnPoint < 0) spawnPoint = location.id;
                        location = DialogueManager.masterDatabase.GetLocation(spawnPoint);
                        Debug.Log("Spawn point: " + location.Name);
                    }
                    gameState.SetPlayerLocation(location);
                    break;
                case "conversation_start":
                    break;
                case "conversation_end":
                    break;
                case "conversation_line":
                    // note: removed, this should not trigger
                    break;
                case "awaiting_response":
                    break;
                case "end_day":
                   // gameState.Clock = Clock.DayEndTime;
                    break;
                case "conversation_decision":
                    break;
            }

            Clock.AddSeconds(playerEvent.Duration);
        
            OnGameStateChanged?.Invoke(gameState);
        }
        
        public void OnTimeChange(Clock.TimeChangeData timeChangeData)
        {
            for (int i = timeChangeData.previousTime; i < timeChangeData.newTime; i++)
            {
                if (i % 600 == 0)
                {
                    Points.AddPoints( "Wellness", -1, false);
                }
            }
        }
        
        public void OnConversationStart()
        {
            var conversation = DialogueManager.masterDatabase.GetConversation(DialogueManager.currentConversationState
                .subtitle.dialogueEntry.conversationID);
            
            var actions = conversation.fields.Where(p => p.title == "Action").ToList();
            

            foreach (var action in actions)
            {
                var item = DialogueManager.masterDatabase.GetItem(int.Parse(action.value));
                if (item.FieldExists("New Sublocation"))
                {
                    var sublocationSwitcherMethod = item.AssignedField("Sublocation Switcher Method");
                    if (sublocationSwitcherMethod != null && sublocationSwitcherMethod.value.StartsWith("MoveBeforeConversation"))
                    {
                        var currentSublocation = gameState.GetPlayerLocation(true);
                        var sublocation = DialogueManager.masterDatabase.GetLocation(item.LookupInt("New Sublocation"));
                        GameManager.instance.SetSublocation(sublocation);
                        
                        if (sublocationSwitcherMethod.value.Contains( "ReturnWhenDone"))
                        {
                           if (gameState.MostRecentSublocation == -1) gameState.MostRecentSublocation = currentSublocation.id;
                        }
                    }
                }
            }
            
            if (conversation.Title == "Base")
            {
                GameManager.DoLocalSave();
                state = State.Base;
                var playerLocation = gameState.GetPlayerLocation(true);

                if (playerLocation.IsFieldAssigned("Music"))
                {
                    var music = playerLocation.LookupValue("Music");
                    AudioEngine.Instance.PlayClipLooped(music);
                }
            }
            
            else if (conversation.Title == "Intro" || conversation.Title == "EndOfDay")
            {
                state = State.StorySequence;
            }
            
            else if (state != State.PreBase) state = State.Action;
            
            if (state is State.Base or State.PreBase)
            {
                var playerLocation = gameState.GetPlayerLocation(true);
                if (playerLocation.IsFieldAssigned("Environment"))
                {
                    var environment = playerLocation.LookupValue("Environment");
                    AudioEngine.Instance.PlayClipLooped(environment);
                }
            }
        }

        public void OnLinkedConversationStart()
        {
            OnConversationStart();
        }

        public void OnGameSceneStart()
        {
            StartCoroutine(QueueConversationEndEvent(() => { 
                GameManager.instance.StartBaseOrPreBaseConversation();
                state = State.PreBase;
            }));
        }
        
        public void OnConversationEnd()
        {
            
            var conversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
            var conversation = DialogueManager.masterDatabase.GetConversation(conversationID);
            
            var actions = conversation.fields.Where(p => p.title == "Action").ToList();

            if (actions.Count > 0) state = State.Action;
            
            foreach (var action in  actions)
            {
                var item = DialogueManager.masterDatabase.GetItem(int.Parse(action.value));
                
                if (item.IsFieldAssigned("Script"))
                {
                    var script = item.LookupValue("Script");
                    Lua.Run(script);
                }
                
                if (item.IsFieldAssigned("New Sublocation"))
                {
                    if (!item.FieldExists("Conversation") || item.AssignedField("Sublocation Switcher Method").value == "MoveAfterConversation")
                    {
                        var location = DialogueManager.masterDatabase.GetLocation(item.LookupInt("New Sublocation"));
                        GameManager.instance.SetSublocation(location);
                    }
                        
                    else if (item.AssignedField("Sublocation Switcher Method").value.Contains("ReturnWhenDone"))
                    {
                        GameManager.instance.SetSublocation(DialogueManager.masterDatabase.GetLocation(gameState.MostRecentSublocation));
                        gameState.MostRecentSublocation = -1;
                    }
                }
            }

            conversation.fields.RemoveAll(p =>  p.title == "Action");
        
            if (conversation.Title.Contains("/GENERATED/"))
            {
                DialogueManager.masterDatabase.conversations.Remove(conversation);
            }

            if (conversation.IsFieldAssigned("Location"))
            {
                var location = DialogueManager.masterDatabase.GetLocation(conversation.LookupInt("Location"));
                if (location.IsFieldAssigned("Music"))
                {
                    var music = location.LookupValue("Music");
                    AudioEngine.Instance.PlayClipLooped(music);
                }
            }


            switch (state)
            {
                case State.PreBase:
                    StartCoroutine(QueueConversationEndEvent(() =>
                    {
                        DialogueManager.StartConversation("Base"); 
                   
                    }));
                    break;
                case State.Base:
                    break;
                case State.Travel: // do nothing
                    break;
                case State.StorySequence:  // do nothing
                    break;
                default:
                    StartCoroutine(QueueConversationEndEvent(() =>
                    {
                        state = State.PreBase;
                        GameManager.instance.StartBaseOrPreBaseConversation();
                    }));
                    break;
            }
        }
        
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
            }
            
            subtitle.dialogueEntry.fields.RemoveAll(p => p.title == "Action");


            if (subtitle.dialogueEntry.outgoingLinks.Count == 1 && subtitle.dialogueEntry.outgoingLinks[0].destinationConversationID != subtitle.dialogueEntry.conversationID)
            {
                var newConversation = DialogueManager.masterDatabase.GetConversation(subtitle.dialogueEntry.outgoingLinks[0].destinationConversationID);
                foreach (var action in conversation.fields.Where(p => p.title == "Action"))
                {
                    newConversation.fields.Add(action);
                }
                
                conversation.fields.RemoveAll(p => p.title == "Action");
            }


            
            var subtitleActor = DialogueManager.masterDatabase.GetActor(subtitle.dialogueEntry.ActorID);
            if (subtitleActor != null && subtitleActor.Name.Split(" ").Any( p => subtitle.formattedText.text.Split(" ").Contains(p)))
            {
                DialogueLua.SetActorField(subtitleActor.Name, "Introduced", true);
            }

            var conversationActor = DialogueManager.masterDatabase.GetActor(conversation.ActorID);
            
            if (conversationActor != null && conversationActor.Name.Split(" ").Any( p => subtitle.formattedText.text.Split(" ").Contains(p)))
            {
                DialogueLua.SetActorField(conversationActor.Name, "Introduced", true);
            }
            
            
            if (subtitle.dialogueEntry.fields.Any(p => p.title == "Duration"))
            {
                var duration = int.Parse(subtitle.dialogueEntry.fields.First(p => p.title == "Duration").value);
                GameEvent.OnWait( duration);
            }

        }
        
        IEnumerator QueueConversationEndEvent(Action callback)
        {
            Debug.Log( "queing event from state: " + state);
            yield return new WaitForEndOfFrame();
            while (DialogueManager.instance.isConversationActive || DialogueTime.isPaused) yield return new WaitForSecondsRealtime(0.25f);
            callback?.Invoke();
        }
        
       
        public void OnTravel()
        {
            state = State.Travel;
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
                points = DialogueUtility.GetPointsFromField(quest!.fields);
                var repeatCount =  DialogueLua.GetQuestField(questName, "Repeat Count").asInt;
                
                foreach (var pointField in points)
                {
                    var multiplier = 1 - quest.LookupFloat("Repeat Points Reduction");
                        
                    for (int i = 0; i < repeatCount; i++)
                    {
                        if (pointField.Points > 0) pointField.Points = (int) (pointField.Points * multiplier);
                    }
                    
                    if (pointField.Points == 0) continue;
                }

                if (quest.IsAction)
                {
                    if (quest.IsStatic) QuestLog.SetQuestState(questName, QuestState.Active);
                    DialogueLua.SetQuestField(questName, "Repeat Count", repeatCount + 1);
                }
            }
        
            var duration = 0;
            
            if (quest.IsQuest) GameEvent.OnQuestStateChange(questName, state, points, duration);
            else if (quest.IsAction && ((quest.IsStatic && state == QuestState.Success) || !quest.IsStatic)) GameEvent.OnActionStateChange(questName, state, points, duration);
            
            if (state == QuestState.Success)
            {

                foreach (var pointsField in points)  Points.AddPoints(pointsField.Type, pointsField.Points);
            }
            
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
            
            Debug.Log("Quest entry state change: " + args.questName + ", " + args.entryNumber);
            
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
                    
                    var pointAsItem = Points.GetDatabaseItem( pointsField.Type);
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