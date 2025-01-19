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
    
    public static float GetTrafficMultiplier(float progress, bool includeGlobalMultiplier = true)
    {
        if (!Settings.Traffic  || Settings.Traffic.trafficCurve == null) return 1f;
        return Settings.Traffic.trafficCurve.Evaluate(progress) * (includeGlobalMultiplier ? Settings.Traffic.globalMultiplier : 1f);
    }
    
}

[CreateAssetMenu(fileName = "TrafficSettings", menuName = "TrafficSettings")]
public class TrafficSettings : ScriptableObject
{
    public float globalMultiplier = 1f;
    
    [CurveRange(0, 1, 0, 1, EColor.Blue)]
    public AnimationCurve trafficCurve;

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
        
        
    }
}
