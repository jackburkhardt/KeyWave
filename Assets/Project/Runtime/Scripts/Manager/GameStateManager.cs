using System;
using System.Collections;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Utility;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

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
                return lastLocation == string.Empty || lastLocation == "nil" ? PlayerLocation : lastLocation;
            }
            set => DialogueLua.SetVariable("game.player.lastNonCaféLocation", value);
        }

        public string PlayerLocation
        {
            get => DialogueLua.GetVariable("game.player.currentLocation").asString;
            set
            {
                DialogueLua.SetVariable("game.player.currentLocation", value);
                
                if (value != "Café")
                {
                    LastNonCaféLocation = value;
                }
            } 
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
                    var location = Location.FromString(playerEvent.Data["newLocation"]?.ToString());
                    gameState.PlayerLocation = location.Name; 
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
            
            var playerLocation = Location.PlayerLocationWithSublocation;
            
            if (conversation.Title.StartsWith("SmartWatch")) state = State.SmartWatch;
            else if (conversation.Title.EndsWith("/Base")) state = State.Base;
            else state = State.Action;
            
            Debug.Log("State: " + state + ", conversation Title: " + conversation);
        }

        public void OnLinkedConversationStart()
        {
            OnConversationStart();
        }
        
        public void OnConversationEnd()
        {
            
            switch (state)
            {
                case State.Base:
                    StartCoroutine(QueueConversationEndEvent(SmartWatch.GoToCurrentApp));
                    break;
                case State.SmartWatch:
                  //  StartCoroutine(QueueConversationEndEvent(SmartWatch.GoToCurrentApp));
                    break;
                case State.Travel: // do nothing
                    break;
                default:
                    StartCoroutine(QueueConversationEndEvent(() =>  DialogueManager.StartConversation( Location.PlayerLocationWithSublocation + "/Base")));
                    Debug.Log("Returning to base");
                    break;
            }

        }

       
        
        IEnumerator QueueConversationEndEvent(Action callback)
        {
            yield return new WaitForEndOfFrame();
            while (DialogueManager.instance.isConversationActive || DialogueTime.isPaused) yield return new WaitForSecondsRealtime(0.25f);
            Debug.Log("now im gonna frick");
            callback?.Invoke();
        }
        
       
        public void OnTravel()
        {
            state = State.Travel;
        }
    }
}