using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class Traffic
{
    

    public static float CurrentTrafficMultiplier => GetTrafficMultiplier(Clock.DayProgress);
    
    public static float GetRawTrafficMultiplier(float progress)
    {
        return EvaluateTrafficCurve(progress);
    }
    
    private static float GetTrafficMultiplier(float progress)
    {
        if (!Settings.Traffic  || Settings.Traffic.trafficCurve == null) return 1f;
        
        var range = Settings.Traffic.peakTrafficLevel - Settings.Traffic.baseTrafficLevel;
    
        return EvaluateTrafficCurve(progress) * range + Settings.Traffic.baseTrafficLevel;
    }
    
    public static float GetNormalizedTrafficMultiplier(float progress)
    {
        var trafficMultiplier = GetTrafficMultiplier(progress);
        return (trafficMultiplier - Settings.Traffic.baseTrafficLevel) / Settings.Traffic.peakTrafficLevel;
    }
    
    private static float EvaluateTrafficCurve(float progress)
    {
        progress = Settings.Traffic.discreteTrafficLevels ? Mathf.Round(progress * Settings.Traffic.discreteLevels + 1) / (Settings.Traffic.discreteLevels + 1) : progress;
        
        return Settings.Traffic.trafficCurve.Evaluate(progress);
    }
    
 
    
}

[CreateAssetMenu(fileName = "TrafficSettings", menuName = "TrafficSettings")]
public class TrafficSettings : ScriptableObject
{
    
    public float baseTrafficLevel = 1f;
    public float peakTrafficLevel = 1f;
    
    [Tooltip("Calculate traffic levels discretely to avoid interpolation.")]
    public bool discreteTrafficLevels;
    [ShowIf("discreteTrafficLevels")] public int discreteLevels = 12;
   
    
    [CurveRange(0, 1, 0, 1, EColor.Blue)]
    public AnimationCurve trafficCurve;
    
    public static Action OnTrafficSettingsChanged;

    private void OnValidate()
    {
        var orderedKeysByTime = trafficCurve.keys.ToList().OrderBy(p => p.time);
        var orderedKeysByValue = trafficCurve.keys.ToList().OrderBy(p => p.value);
        
        if (orderedKeysByTime.Count() == 0 || orderedKeysByValue.Count() == 0) return;
        
        var minTime = orderedKeysByTime.First().time;
        var maxTime = orderedKeysByTime.Last().time;
        
        
        var minValue = orderedKeysByValue.First().value;
        var maxValue = orderedKeysByValue.Last().value;
        
       // normalize curve
        for (var i = 0; i < trafficCurve.keys.Length; i++)
        {
            var key = trafficCurve.keys[i];
            key.time = Mathf.InverseLerp(minTime, maxTime, key.time);
            key.value = Mathf.InverseLerp(minValue, maxValue, key.value);
            trafficCurve.MoveKey(i, key);
        }
        
        OnTrafficSettingsChanged?.Invoke();
        
        
    }
}
