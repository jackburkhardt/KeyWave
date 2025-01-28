using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class Settings : ScriptableObject
{
    private const string Path = "Settings.asset";
    private static Settings _instance;


    public const string FIELD_INT_ACTION_REPEAT_HISTORY_COUNT = "Repeat Count";
    public const string FIELD_FLOAT_POINTS_MULTIPLIER_ON_ACTION_REPEAT = "Points Repeat";
    public const string FIELD_BOOL_ACTION_IS_REPEATABLE = "Repeatable";
    
    
    public static Settings Instance
    {
        get
        {
            if (_instance != null) return _instance;
              
            if (TryGetSettings(out var settings))
            {
                return settings;
            }
            else
            {
                return null;
            }
        }
    }
    
    public DialogueDatabase dialogueDatabase;
    
    [Expandable]
    public SmartWatch smartWatchSettings;
    [Expandable]
    public TrafficSettings trafficSettings;
    [Expandable]
    public ClockSettings clockSettings;
    [Expandable]
    public AudioSettings audioSettings;

    
    public ClockSettings Clock
    {
        get
        {
            if (Instance != null) return Instance.clockSettings;
            else
            {
                var _clockSettings = CreateInstance<ClockSettings>();
                return _clockSettings;
            }
        }
    }
    
    public TrafficSettings Traffic
    {
        get
        {
            if (Instance != null) return Instance.trafficSettings;
            else
            {
                var _trafficSettings = CreateInstance<TrafficSettings>();
                return _trafficSettings;
            }
        }
    }
    
    public SmartWatch SmartWatch
    {
        get
        {
            if (Instance != null) return Instance.smartWatchSettings;
            else
            {
                var _smartWatchSettings = CreateInstance<SmartWatch>();
                return _smartWatchSettings;
            }
        }
    }
    
    public AudioSettings Audio
    {
        get
        {
            if (Instance != null) return Instance.audioSettings;
            else
            {
                var _audioSettings = CreateInstance<AudioSettings>();
                return _audioSettings;
            }
        }
    }
    

    public static bool TryGetSettings(out Settings settings)
    {
        if (_instance != null)
        {
            settings = _instance;
            return true;
        }

        else
        {
    
#if UNITY_WEBGL
            
            AddressableLoader.RequestLoad<Settings>(Path, asset =>
            {
                _instance = asset;
            });
            settings = _instance;
            return false;
            
#else
            _instance = Resources.Load<Settings>("Settings");
            settings = _instance;
            return _instance != null;
#endif
        }
    }
    
    
}

