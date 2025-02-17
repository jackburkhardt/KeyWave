using System;
using System.Text.RegularExpressions;
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
            Skills,
            Teamwork,
            Context,
            Null
        }

        private static bool isAnimating;

        private static Vector2 spawnPosition;
        
        public static Action<Type, int> OnPointsChange;

        public static Action<Type> OnAnimationStart;

        public static Action OnAnimationComplete;
        public static int TeamworkScore => GameStateManager.instance.gameState.TeamworkScore;
        public static int SkillsScore => GameStateManager.instance.gameState.SkillsScore;
        public static int WellnessScore => GameStateManager.instance.gameState.WellnessScore;
        public static int ContextScore => GameStateManager.instance.gameState.ContextScore;
        public static int TotalScore =>  TeamworkScore + SkillsScore + WellnessScore + ContextScore;
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
                case Type.Teamwork:
                    score = GameStateManager.instance.gameState.TeamworkScore;
                    break;
                case Type.Skills:
                    score = GameStateManager.instance.gameState.SkillsScore;
                    break;
                case Type.Wellness:
                    score = GameStateManager.instance.gameState.WellnessScore;
                    break;
                case Type.Context:
                    score = GameStateManager.instance.gameState.ContextScore;
                    break;
            }

            return score;
        }

        public static int MaxScore(Type type)
        {
            var score = 0;
            switch (type)
            {
                case Type.Teamwork:
                    score = GameStateManager.instance.gameState.MaxTeamworkScore;
                    break;
                case Type.Skills:
                    score = GameStateManager.instance.gameState.MaxSkillsScore;
                    break;
                case Type.Wellness:
                    score = GameStateManager.instance.gameState.MaxWellnessScore;
                    break;
                case Type.Context:
                    score = GameStateManager.instance.gameState.MaxContextScore;
                    break;
            }

            return score;
        }
        
        public static int TotalMaxScore
        => GameStateManager.instance.gameState.MaxTeamworkScore + GameStateManager.instance.gameState.MaxSkillsScore + GameStateManager.instance.gameState.MaxWellnessScore + GameStateManager.instance.gameState.MaxContextScore;
         

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
                case Type.Skills:
                    return UnityEngine.Color.red;
                case Type.Teamwork:
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
        
        public static bool IsPointsField(this Field field)
        {
            string pattern = @"^(.*?) Points$";
            return Regex.IsMatch(field.title, pattern);
        }


        public class PointsField
        {
            [FormerlySerializedAs("type")] 
            [JsonConverter(typeof(StringEnumConverter))]
            public Type Type;
        
            [FormerlySerializedAs("points")] 
            public int Points;
            
            public static PointsField FromJObject(JObject data)
            {
                if (data["points"] == null || data["pointsType"] == null) return new PointsField {Type = Type.Null, Points = 0};
                
                var type = (Type) Enum.Parse(typeof(Type), (string) data["pointsType"]);
                var points = (int) data["points"];
                return new PointsField {Type = type, Points = points};
            }

            public static PointsField FromLuaField(Field field)
            {
                string pattern = @"^(.*?) Points$";
                var pointType = Regex.Replace(field.title, pattern, "$1");
                
                if (pointType.Split(" ").Length > 1) pointType = pointType.Split(" ")[^1];
                var type = (Type)Enum.Parse(typeof(Type), pointType);
                var points = int.Parse(field.value);
                return new PointsField { Type = type, Points = points };

            }

        }
    }
}