using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class Settings : ScriptableObject
{
    public DialogueDatabase dialogueDatabase;
    
    [Expandable]
    public SmartWatch smartWatchSettings;
    [Expandable]
    public TrafficSettings trafficSettings;
    [Expandable]
    public ClockSettings clockSettings;
    [Expandable]
    public AudioSettings audioSettings;

    [SerializeField]
    public bool autoPauseOnFocusLost = false;

    
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

