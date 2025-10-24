using System.Linq;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class Settings : ScriptableObject
{
    public DialogueDatabase dialogueDatabase;
    [Expandable]
    public TrafficSettings trafficSettings;
    [Expandable]
    public ClockSettings clockSettings;
    [Expandable]
    public AudioSettings audioSettings;
    
    
    public bool skipIntro;

    [SerializeField]
    public bool autoPauseOnFocusLost = false;

    [SerializeField]
    private bool _highContrastMode;

   

    public bool HighContrastMode
    {
        get => _highContrastMode;
        set
        {
            _highContrastMode = value;
            IHighContrastHandler[] handlers = GameManager.FindObjectsOfType<MonoBehaviour>(true).OfType<IHighContrastHandler>().ToArray();
            Debug.Log(handlers.Length);
            foreach (var handler in handlers)
            {
                if (value) handler.OnHighContrastModeEnter();
                else handler.OnHighContrastModeExit();
            }
        }
    }
    


    
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
    
    private bool _newHighContrastMode;

    private void OnValidate()
    {
        if (_newHighContrastMode != _highContrastMode)
        {
            HighContrastMode = _highContrastMode;
            _newHighContrastMode = _highContrastMode;
        }
    }
}

