using System;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Events;
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

        public int Engagement_score = 0;
        public int commitment_score = 0;


        public string current_scene = "Hotel";
        public int day = 1;
        public int credibility_score = 0;

        //default values

        public Type type = Type.Normal;
        public int wellness_score = 0;


        public int Clock
        {
            get => DialogueLua.GetVariable("clock").asInt;
            set => DialogueLua.SetVariable("clock", value);
        }

        public string LastNonCaféLocation
        {
            get => DialogueLua.GetVariable("last_non_café_location").asString;
            set => DialogueLua.SetVariable("last_non_café_location", value);
        }

        public string PlayerLocation
        {
            get => DialogueLua.GetVariable("player_location").asString;
            set => DialogueLua.SetVariable("player_location", value);
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
                            gameState.wellness_score += pointsField.Points;
                            break;
                        case Points.Type.Engagement:
                            gameState.Engagement_score += pointsField.Points;
                            break;
                        case Points.Type.Commitment:
                            gameState.commitment_score += pointsField.Points;
                            break;
                        case Points.Type.Credibility:
                            gameState.credibility_score += pointsField.Points;
                            break;
                    }
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