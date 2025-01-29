using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class Settings : ScriptableObject
{
    private const string Path = "Settings.asset";

    public const string FIELD_INT_ACTION_REPEAT_HISTORY_COUNT = "Repeat Count";
    public const string FIELD_FLOAT_POINTS_MULTIPLIER_ON_ACTION_REPEAT = "Points Repeat";
    public const string FIELD_BOOL_ACTION_IS_REPEATABLE = "Repeatable";
    
    
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
            if (clockSettings != null) return clockSettings;
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
            if (trafficSettings != null) return trafficSettings;
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
            if (smartWatchSettings != null) return smartWatchSettings;
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
            if (audioSettings != null) return audioSettings;
            else
            {
                var _audioSettings = CreateInstance<AudioSettings>();
                return _audioSettings;
            }
        }
    }
    
    
}

