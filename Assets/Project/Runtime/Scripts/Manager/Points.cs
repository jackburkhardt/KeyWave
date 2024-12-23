using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Runtime.Scripts.Manager
{
    public static class Points
    {
        /// <summary>
        /// Type of points given to player.
        /// </summary>
    
        // If you want to add or change these for the Dialogue Manager to use, make sure to update CustomFieldType_PointsType.cs
        public enum Type
        {
            Wellness,
            Credibility,
            Engagement,
            Null,
            Commitment
        }

        public const int MaxScore = 1000;

        private static bool isAnimating;

        private static Vector2 spawnPosition;

        public static Action<Type> OnAnimationStart;

        public static Action OnAnimationComplete;
        public static int EngagementScore => GameStateManager.instance.gameState.Engagement_score;
        public static int CredibilityScore => GameStateManager.instance.gameState.credibility_score;
        public static int WellnessScore => GameStateManager.instance.gameState.wellness_score;
        public static int CommitmentScore => GameStateManager.instance.gameState.commitment_score;
        public static int TotalScore =>  EngagementScore + CredibilityScore + WellnessScore + CommitmentScore;
        public static bool IsAnimating => isAnimating;

        /// <summary>
        /// Gets the position currently used for spawning orbs in animations.
        /// </summary>
        public static Vector2 SpawnPosition => spawnPosition;

        public static string ToString(this PointsField field)
        {
            return $"{field.Type}:{field.Points}";
        }

        public static int Score(Type type)
        {
            var score = 0;
            switch (type)
            {
                case Type.Engagement:
                    score = GameStateManager.instance.gameState.Engagement_score;
                    break;
                case Type.Credibility:
                    score = GameStateManager.instance.gameState.credibility_score;
                    break;
                case Type.Wellness:
                    score = GameStateManager.instance.gameState.wellness_score;
                    break;
                case Type.Commitment:
                    score = GameStateManager.instance.gameState.commitment_score;
                    break;
            }

            return score;
        }

        public static Action OnPointsAnimEnd;
        public static Action OnPointsAnimStart;

        public static void SetSpawnPosition(Vector2 position)
        {
            spawnPosition = position;
        }

        /// <summary>
        /// Color associated with a given point type.
        /// </summary>
        /// <param name="type">Type of point.</param>
        /// <returns></returns>
        public static Color Color(Type type)
        {
            switch (type)
            {
                case Type.Wellness:
                    return UnityEngine.Color.green;
                case Type.Credibility:
                    return UnityEngine.Color.red;
                case Type.Engagement:
                    return new Color(0, 153, 255, 255);
                default:
                    return UnityEngine.Color.white;
            }
        }

        public static void AnimationStart(Type type, Vector2 position)
        {
            spawnPosition = position;
            AnimationStart(type);
        }

        public static void AnimationStart(Type type)
        {
            isAnimating = true;
            OnAnimationStart?.Invoke(type);
        }

        public static void AnimationComplete()
        {
            isAnimating = false;
            Sequencer.Message("Animated");
            OnAnimationComplete?.Invoke();
        }


        public struct PointsField
        {
            [FormerlySerializedAs("type")] 
            [JsonConverter(typeof(StringEnumConverter))]
            public Type Type;
        
            [FormerlySerializedAs("points")] 
            public int Points;
        
            public override string ToString()
            {
                return $"{Enum.GetName(typeof(Type), this.Type)}:{Points}";
            }
        
            public static PointsField FromString(string data)
            {
                if (string.IsNullOrEmpty(data)) return new PointsField {Type = Type.Null, Points = 0};
                var split = data.Split(':');
                var type = (Type) Enum.Parse(typeof(Type), split[0]);
                var points = int.Parse(split[1]);
                return new PointsField {Type = type, Points = points};
            }
            
            public static PointsField FromJObject(JObject data)
            {
                if (data["points"] == null || data["pointsType"] == null) return new PointsField {Type = Type.Null, Points = 0};
                
                var type = (Type) Enum.Parse(typeof(Type), (string) data["pointsType"]);
                var points = (int) data["points"];
                return new PointsField {Type = type, Points = points};
            }

        
        }
    }
}