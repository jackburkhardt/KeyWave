using System;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Unity.Mathematics;
using UnityEngine;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.Manager
{
    public class GameState
    {
        public enum Type
        {
            Normal,
            EndOfDay
        }



        public string current_scene = "Hotel";
        public int day = 1;
        //default values

        public Type type = Type.Normal;


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
        
        public int EngagementScore 
        {
            get => DialogueLua.GetVariable("points.engagement").asInt;
            set => DialogueLua.SetVariable("points.engagement", Math.Min(value, MaxEngagementScore));
        }
        
        public int CredibilityScore 
        {
            get => DialogueLua.GetVariable("points.credibility").asInt;
            set => DialogueLua.SetVariable("points.credibility", Math.Min(value, MaxCredibilityScore));
        }
        
        public int WellnessScore 
        {
            get => DialogueLua.GetVariable("points.wellness").asInt;
            set => DialogueLua.SetVariable("points.wellness", Math.Min(value, MaxWellnessScore));
        }
        
        public int CommitmentScore 
        {
            get => DialogueLua.GetVariable("points.commitment").asInt;
            set => DialogueLua.SetVariable("points.commitment", Math.Min(value, CommitmentScore));
        }
        
        
        public int MaxEngagementScore 
        {
            get => DialogueLua.GetVariable("points.engagement.max").asInt;
            set => DialogueLua.SetVariable("points.engagement.max", value);
        }
        
        public int MaxCredibilityScore 
        {
            get => DialogueLua.GetVariable("points.credibility.max").asInt;
            set => DialogueLua.SetVariable("points.credibility.max", value);
        }
        
        public int MaxWellnessScore 
        {
            get => DialogueLua.GetVariable("points.wellness.max").asInt;
            set => DialogueLua.SetVariable("points.wellness.max", value);
        }
        
        public int MaxCommitmentScore 
        {
            get => DialogueLua.GetVariable("points.commitment.max").asInt;
            set => DialogueLua.SetVariable("points.commitment.max", value);
        }
        
        
    }

    public class GameStateManager : PlayerEventHandler
    {
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
                    //gameState.current_conversation_title = (string)playerEvent.Data;
                    break;
                case "conversation_end":
                    //gameState.current_conversation_title = string.Empty;
                    break;
                case "conversation_line":
                    // note: removed, this should not trigger
                    // gameState.current_conversation_actor = playerEvent.Source;
                    // gameState.current_conversation_conversant = playerEvent.Target;
                    //  gameState.current_conversation_line = (int)playerEvent.Data;
                    break;
                case "awaiting_response":
                    // gameState.most_recent_response_node = (string)playerEvent.Data;
                    break;
                case "end_day":
                    gameState.type = GameState.Type.EndOfDay;
                    break;
                case "conversation_decision":
                    //   gameState.most_recent_response_node = string.Empty;
                    break;
                case "points":
                    var pointsField = Points.PointsField.FromJObject(playerEvent.Data);
                    switch (pointsField.Type)
                    {
                        case Points.Type.Wellness:
                            gameState.WellnessScore += pointsField.Points;
                            break;
                        case Points.Type.Engagement:
                            gameState.EngagementScore += pointsField.Points;
                            break;
                        case Points.Type.Commitment:
                            gameState.CommitmentScore += pointsField.Points;
                            break;
                        case Points.Type.Credibility:
                            gameState.CredibilityScore += pointsField.Points;
                            break;
                    }
                    
                    Points.OnPointsChange?.Invoke(pointsField.Type);
                    Debug.Log("invoking points");
                    break;
            }

            switch (gameState.type)
            {
                case GameState.Type.Normal:
                gameState.Clock += playerEvent.Duration;
                    break;
                case GameState.Type.EndOfDay:
                    gameState.Clock = Clock.DailyLimit;
                    break;
            }
        
            OnGameStateChanged?.Invoke(gameState);
        }

        public void StartNextDay()
        {
            gameState.day += 1;
            gameState.Clock = 21600;
            gameState.PlayerLocation = "Hotel";
            // gameState.current_conversation_title = string.Empty;
            gameState.type = GameState.Type.Normal;
        }
    }
}