using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

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
    
    private static bool isAnimating;
    public static bool IsAnimating => isAnimating;
    
    public static Action<Type, int> OnPointsIncrease;

    private static Vector2 spawnPosition;
    
   
    /// <summary>
    /// Gets the position currently used for spawning orbs in animations.
    /// </summary>
    public static Vector2 SpawnPosition => spawnPosition;
    public static void AddPoints(Type pointsType, int pointsCount)
    {
        
        OnPointsIncrease?.Invoke(pointsType, pointsCount);
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
            case Type.LocalSavvy:
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
