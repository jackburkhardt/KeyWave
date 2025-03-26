using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public static class Traffic
{
    

    public static float CurrentTrafficMultiplier => GetTrafficMultiplier(Clock.DayProgress);
    
    private static float GetTrafficMultiplier(float progress)
    {
        if (!GameManager.settings.Traffic  || GameManager.settings.Traffic.trafficCurve == null) return 1f;
        
        var range = GameManager.settings.Traffic.peakTrafficLevel - GameManager.settings.Traffic.baseTrafficLevel;
    
        return EvaluateTrafficCurve(progress) * range + GameManager.settings.Traffic.baseTrafficLevel;
    }
    
    public static float EvaluateTrafficCurve(float progress)
    {
        
        if (GameManager.settings == null) return 0f;
      
        if (GameManager.settings.Traffic.useDiscreteTrafficLevels)
        {
            return GameManager.settings.Traffic.discreteLevels[ Mathf.FloorToInt(progress * GameManager.settings.Traffic.discreteLevelsCount)];
        }
        
        return GameManager.settings.Traffic.trafficCurve.Evaluate(progress);
    }

    public static int CurrentDiscreteTrafficLevel
    {
        get
        {
            if (GameManager.settings == null || GameManager.settings.Traffic == null) return 0;
            return Mathf.FloorToInt(Clock.DayProgress * GameManager.settings.Traffic.discreteLevelsCount);
        }
    }
    
    public static float BaseTrafficLevel
    {
        get
        {
            if (GameManager.settings == null || GameManager.settings.Traffic == null) return 1f;
            return GameManager.settings.Traffic.baseTrafficLevel;
        }
    }

    public static float PeakTrafficLevel
    {
        get
        {
            if (GameManager.settings == null || GameManager.settings.Traffic == null) return 1f;
            return GameManager.settings.Traffic.peakTrafficLevel;
        }
    }
    
 
    
}

[CreateAssetMenu(fileName = "TrafficSettings", menuName = "TrafficSettings")]
public class TrafficSettings : ScriptableObject
{
    
    public float baseTrafficLevel = 1f;
    public float peakTrafficLevel = 1f;
    
    [Tooltip("Calculate traffic levels discretely to avoid interpolation.")]
    public bool useDiscreteTrafficLevels;
    [ShowIf("useDiscreteTrafficLevels")] public int discreteLevelsCount = 12;
   
    
    [CurveRange(0, 1, 0, 1, EColor.Blue)]
    public AnimationCurve trafficCurve;
    
    public static Action OnTrafficSettingsChanged;
    
    
    [ShowIf("useDiscreteTrafficLevels")] [ReadOnly]
    public List<float> discreteLevels;
    

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
        
        
        if (useDiscreteTrafficLevels)
        {
            if (discreteLevels.Count != discreteLevelsCount)
            {
                discreteLevels.Clear();
                for (var i = 0; i < discreteLevelsCount; i++)
                {
                    discreteLevels.Add(i / (float)discreteLevelsCount);
                }
            }
            
            for (var i = 0; i < discreteLevels.Count; i++)
            {
                discreteLevels[i] = trafficCurve.Evaluate( (float)i / discreteLevelsCount +  0.5f / discreteLevelsCount );
            }
        }
        
    }

}
