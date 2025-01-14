using System;
using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public static class Traffic
{
    
    public static float globalMultiplier = 1f;

    public static float CurrentTrafficMultiplier => GetTrafficMultiplier(Clock.DayProgress);
    
    
    public static float GetTrafficMultiplier(float progress, bool includeGlobalMultiplier = true)
    {
        var trafficLevels = new float[]
            { 1, 2.4f, 2.77f, 1.77f, 1.99f, 1f, 2.2f, 2.5f, 1.97f, 1.32f, 0.67f, 0.322f };

        var lerpPercentage = (progress * 12) % 1;

        var lowerBound = (int)Math.Floor(progress * 12);
        var upperBound = (int)Math.Ceiling(progress * 12);
        if (upperBound > 11) upperBound = 0;
            
        return Mathf.Lerp(trafficLevels[lowerBound], trafficLevels[upperBound], lerpPercentage) * (includeGlobalMultiplier ? globalMultiplier : 1f);
    }
    
}
