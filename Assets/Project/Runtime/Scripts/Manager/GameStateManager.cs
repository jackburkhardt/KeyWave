using System;
using System.Collections;
using System.Linq;
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

        public int Clock
        {
            get => DialogueLua.GetVariable("clock").asInt;
            set => DialogueLua.SetVariable("clock", value);
        }

        public string LastNonCaféLocation
        {
            get
            {
                var lastLocation = DialogueLua.GetVariable("game.player.lastNonCaféLocation").asString;
                return lastLocation == string.Empty || lastLocation == "nil" ? PlayerLocation().Name : lastLocation;
            }
            set => DialogueLua.SetVariable("game.player.lastNonCaféLocation", value);
        }
        
        


        public PixelCrushers.DialogueSystem.Actor PlayerActor =>  DialogueManager.masterDatabase.actors.First(p => p.IsPlayer && p.IsFieldAssigned("Location"));


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
        
        public Location PlayerLocation(bool specifySublocation = false)
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

        public int TeamworkScore 
        {
            get => DialogueLua.GetVariable("points.Teamwork").asInt;
            set => DialogueLua.SetVariable("points.Teamwork", Math.Min(value, MaxTeamworkScore));
        }
        
        public int SkillsScore 
        {
            get => DialogueLua.GetVariable("points.Skills").asInt;
            set => DialogueLua.SetVariable("points.Skills", Math.Min(value, MaxSkillsScore));
        }
        
        public int WellnessScore 
        {
            get => DialogueLua.GetVariable("points.wellness").asInt;
            set => DialogueLua.SetVariable("points.wellness", Math.Min(value, MaxWellnessScore));
        }
        
        public int ContextScore 
        {
            get => DialogueLua.GetVariable("points.Context").asInt;
            set => DialogueLua.SetVariable("points.Context", Math.Min(value, ContextScore));
        }
        
        
        public int MaxTeamworkScore 
        {
            get => DialogueLua.GetVariable("points.Teamwork.max").asInt;
            set => DialogueLua.SetVariable("points.Teamwork.max", value);
        }
        
        public int MaxSkillsScore 
        {
            get => DialogueLua.GetVariable("points.Skills.max").asInt;
            set => DialogueLua.SetVariable("points.Skills.max", value);
        }
        
        public int MaxWellnessScore 
        {
            get => DialogueLua.GetVariable("points.wellness.max").asInt;
            set => DialogueLua.SetVariable("points.wellness.max", value);
        }
        
        public int MaxContextScore 
        {
            get => DialogueLua.GetVariable("points.Context.max").asInt;
            set => DialogueLua.SetVariable("points.Context.max", value);
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
            Talk,
            SmartWatch
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
                    gameState.Clock = Clock.DayEndTime;
                    break;
                case "conversation_decision":
                    break;
                case "points":
                    var pointsField = Points.PointsField.FromJObject(playerEvent.Data);
                    switch (pointsField.Type)
                    {
                        case Points.Type.Wellness:
                            gameState.WellnessScore += pointsField.Points;
                            break;
                        case Points.Type.Teamwork:
                            gameState.TeamworkScore += pointsField.Points;
                            break;
                        case Points.Type.Context:
                            gameState.ContextScore += pointsField.Points;
                            break;
                        case Points.Type.Skills:
                            gameState.SkillsScore += pointsField.Points;
                            break;
                    }
                    
                    Points.OnPointsChange?.Invoke(pointsField.Type, pointsField.Points);
                    break;
            }

            gameState.Clock += playerEvent.Duration;
        
            OnGameStateChanged?.Invoke(gameState);
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
                        var currentSublocation = gameState.PlayerLocation(true);
                        var sublocation = DialogueManager.masterDatabase.GetLocation(item.LookupInt("New Sublocation"));
                        GameManager.instance.SetSublocation(sublocation);
                        
                        if (sublocationSwitcherMethod.value.Contains( "ReturnWhenDone"))
                        {
                           gameState.MostRecentSublocation = currentSublocation.id;
                        }
                    }
                }
            }
            
            if (conversation.Title.StartsWith("SmartWatch")) state = State.SmartWatch;
            else if (conversation.Title == "Base")
            {
                state = State.Base;
                var playerLocation = gameState.PlayerLocation(true);

                if (playerLocation.IsFieldAssigned("Music"))
                {
                    var music = playerLocation.LookupValue("Music");
                    AudioEngine.Instance.PlayClipLooped(music);
                }
            }
            
            else if (state != State.PreBase) state = State.Action;
            
            if (state is State.Base or State.PreBase)
            {
                var playerLocation = gameState.PlayerLocation(true);
                if (playerLocation.IsFieldAssigned("Environment"))
                {
                    var environment = playerLocation.LookupValue("Environment");
                    AudioEngine.Instance.PlayClipLooped(environment);
                }
            }
            
            
            
            
            Debug.Log("State: " + state + ", conversation Title: " + conversation.Title);
        }

        public void OnLinkedConversationStart()
        {
            OnConversationStart();
        }

        public void OnGameSceneStart()
        {
            
            state = State.PreBase;
            GameManager.instance.StartBaseOrPreBaseConversation();
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

            conversation.fields.RemoveAll(p =>  p.title == "Action" && DialogueManager.masterDatabase.GetItem(int.Parse(p.value)).GetQuestState() == QuestState.Success);
        
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
                    StartCoroutine(QueueConversationEndEvent(() => DialogueManager.StartConversation("Base")));
                    break;
                case State.Base:
                    StartCoroutine(QueueConversationEndEvent(SmartWatch.GoToCurrentApp));
                    break;
                case State.SmartWatch:
                    //  StartCoroutine(QueueConversationEndEvent(SmartWatch.GoToCurrentApp));
                    break;
                case State.Travel: // do nothing
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
        
            foreach (var action in  subtitle.dialogueEntry.fields.Where(p => p.title == "Action"))
            {
                var conversation = DialogueManager.masterDatabase.GetConversation(subtitle.dialogueEntry.conversationID);
                
                conversation.fields.Add( action);
                
                Debug.Log("Added action to conversation: " + action.value);
            }
        }
        
        IEnumerator QueueConversationEndEvent(Action callback)
        {
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
            var quest = DialogueManager.masterDatabase.GetQuest(questName);
            var state = QuestLog.GetQuestState(questName);
            var points = DialogueUtility.GetPointsFromField(quest!.fields);
      
            // if this quest already succeeded, we don't want to retrigger events
            if (GameManager.instance.dailyReport.CompletedTasks.Contains(questName) && !quest.IsStatic) return;

            if (state == QuestState.Success)
            {                
                foreach (var pointField in points)
                {
                    if (pointField.Points == 0) continue;

                    var repeatCount = DialogueLua.GetQuestField(questName, "Repeat Count").asInt;
                    var multiplier = 1 - quest.LookupFloat("Repeat Points Reduction");
                        
                    for (int i = 0; i < repeatCount; i++)
                    {
                        pointField.Points = (int) (pointField.Points * multiplier);
                    }
                    GameEvent.OnPointsIncrease(pointField, questName);
                    
                }

                if (quest.IsAction)
                {
                    if (quest.IsStatic) QuestLog.SetQuestState(questName, QuestState.Active);
                    var completionCount = quest.LookupInt("Repeat Count");
                    quest.AssignedField("Repeat Count").value = (completionCount + 1).ToString();
                    Debug.Log( $"Quest {questName} has been repeated {completionCount + 1} times.");
                    
                   
                }
            }
        
            var duration = state == QuestState.Success ? DialogueUtility.GetQuestDuration(quest) : 0;
            
            GameEvent.OnQuestStateChange(questName, state, duration);
            SaveDataStorer.WebStoreGameData(PixelCrushers.SaveSystem.RecordSavedGameData());
        }


        public void OnQuestEntryStateChange(QuestEntryArgs args)
        {
            var quest = DialogueManager.masterDatabase.GetQuest(args.questName);
            var entry = args.entryNumber;
            var prefix = $"Entry {entry} ";
            
            var state = QuestLog.GetQuestEntryState( quest.Name, entry);
            
            var points = DialogueUtility.GetPointsFromField(quest!.fields, prefix);

            if (state == QuestState.Success)
            {
                foreach (var pointField in points)
                {
                    if (pointField.Points == 0) continue;

                    var repeatCount = DialogueLua.GetQuestField(quest.Name, "Repeat Count").asInt;
                    var multiplier = 1 - quest.LookupFloat("Repeat Points Reduction");
                        
                    for (int i = 0; i < repeatCount; i++)
                    {
                        pointField.Points = (int) (pointField.Points * multiplier);
                    }
                    GameEvent.OnPointsIncrease(pointField, quest.Name);
                    
                }
            }

            if (quest.LookupBool("Auto Set Success"))
            {
                
               // get all states and check if they are success
               
               var entryCount = QuestLog.GetQuestEntryCount( quest.Name);
               
               for  (int i = 1; i <= entryCount; i++)
               {
                   if (i == entry) continue;
                   var otherState = QuestLog.GetQuestEntryState( quest.Name, i);
                   if (otherState != QuestState.Success) return;
               }
               
               QuestLog.SetQuestState(quest.Name, QuestState.Success);
               
            }
        }
    }
}