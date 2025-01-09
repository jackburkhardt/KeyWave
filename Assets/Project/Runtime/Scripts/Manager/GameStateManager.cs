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
                    break;
                case "conversation_end":
                    break;
                case "conversation_line":
                    // note: removed, this should not trigger
                    break;
                case "awaiting_response":
                    break;
                case "end_day":
                    gameState.Clock = Clock.DailyLimit;
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
                    break;
            }

            gameState.Clock += playerEvent.Duration;
        
            OnGameStateChanged?.Invoke(gameState);
        }

        public void StartNextDay()
        {
            gameState.day += 1;
            gameState.Clock = 21600;
            gameState.PlayerLocation = "Hotel";

        }
    }
}