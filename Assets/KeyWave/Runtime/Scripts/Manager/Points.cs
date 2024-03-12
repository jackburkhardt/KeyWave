using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public static class Points
{
    
    /// <summary>
    /// Type of points given to player.
    /// </summary>
    
    // If you want to add or change these for the Dialogue Manager to use, make sure to update CustomFieldType_PointsType.cs
    public enum Type
    {
        Wellness,
        LocalSavvy,
        Business
    }

    public static int Score(Type type)
    {
        var score = 0;
        switch (type)
        {
            case Type.Business:
                score = GameStateManager.instance.gameState.business_score;
                break;
            case Type.LocalSavvy:
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
            case Type.LocalSavvy:
                return UnityEngine.Color.red;
            case Type.Business:
                return new Color(0, 153, 255, 255);
            default:
                return UnityEngine.Color.white;
        }
    }
    
    public static Type TypeFromString(string type)
    {
        return (Type) Enum.Parse(typeof(Type), type);
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
