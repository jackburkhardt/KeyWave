using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;

public static class Points
{
    public const int MaxScore = 1000;
    public static int BusinessScore => GameStateManager.instance.gameState.business_score;
    public static int SavvyScore => GameStateManager.instance.gameState.local_savvy_score;
    public static int WellnessScore => GameStateManager.instance.gameState.wellness_score;
    public static int TotalScore =>  BusinessScore + SavvyScore + WellnessScore;
    
    
    public struct PointsField
    {
        [FormerlySerializedAs("type")] 
        [JsonConverter(typeof(StringEnumConverter))]
        public Points.Type Type;
        
        [FormerlySerializedAs("points")] 
        public int Points;
        
        public override string ToString()
        {
            return $"{Enum.GetName(typeof(Points.Type), this)}:{Points}";
        }
        
        public static PointsField FromString(string data)
        {
            if (string.IsNullOrEmpty(data)) return new PointsField {Type = Type.Null, Points = 0};
            var split = data.Split(':');
            var type = (Points.Type) Enum.Parse(typeof(Points.Type), split[0]);
            var points = int.Parse(split[1]);
            return new PointsField {Type = type, Points = points};
        }
    }
    
    /// <summary>
    /// Type of points given to player.
    /// </summary>
    
    // If you want to add or change these for the Dialogue Manager to use, make sure to update CustomFieldType_PointsType.cs
    public enum Type
    {
        Wellness,
        Savvy,
        Business,
        Null
    }
    public static int Score(Type type)
    {
        var score = 0;
        switch (type)
        {
            case Type.Business:
                score = GameStateManager.instance.gameState.business_score;
                break;
            case Type.Savvy:
                score = GameStateManager.instance.gameState.local_savvy_score;
                break;
            case Type.Wellness:
                score = GameStateManager.instance.gameState.wellness_score;
                break;
        }

        return score;
    }
    
    private static bool isAnimating;
    public static bool IsAnimating => isAnimating;
    
    private static Vector2 spawnPosition;
    
    public static void SetSpawnPosition(Vector2 position)
    {
        spawnPosition = position;
    }
   
    /// <summary>
    /// Gets the position currently used for spawning orbs in animations.
    /// </summary>
    public static Vector2 SpawnPosition => spawnPosition;

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
            case Type.Savvy:
                return UnityEngine.Color.red;
            case Type.Business:
                return new Color(0, 153, 255, 255);
            default:
                return UnityEngine.Color.white;
        }
    }

    public static Action<Type> OnAnimationStart;

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

    public static Action OnAnimationComplete;
    
    public static void AnimationComplete()
    {
        isAnimating = false;
        Sequencer.Message("Animated");
        OnAnimationComplete?.Invoke();
    }
}
