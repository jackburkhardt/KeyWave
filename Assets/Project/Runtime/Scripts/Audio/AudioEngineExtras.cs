using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.Audio;

namespace Project.Runtime.Scripts.Audio
{
    [RequireComponent(typeof(AudioEngine))]
    public class AudioEngineExtras : MonoBehaviour
    {
        private AudioEngine _audioEngine;
        private AudioMixerSnapshot PausedSnapshot => _audioEngine.UserAudioMixer.FindSnapshot("Paused");
        private AudioMixerSnapshot DefaultSnapshot => _audioEngine.UserAudioMixer.FindSnapshot("Default");
       // private float _cutoffValue = 0;

       private static AudioEngineExtras _instance;
        
        private void Awake()
        {
            _audioEngine = GetComponent<AudioEngine>();
            
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(this);
            }
            
            var audioSettings = GameManager.settings.audioSettings;
            if (PauseMenu.active) audioSettings.SetVolume();
        }
        
        private void OnEnable()
        {
            PauseMenu.OnPause += OnPause;
            PauseMenu.OnUnpause += OnUnpause;
        }
        
        
        
        
        private void OnPause()
        {
            PausedSnapshot.TransitionTo(0.25f);
        }
        

        private void OnUnpause()
        {
            DefaultSnapshot.TransitionTo(0.25f);
        }
        
        
        
        public static void SetParameter(string parameter, float value)
        {
            if (_instance == null) return;
            _instance._audioEngine.UserAudioMixer.SetFloat(parameter, value);
        }
        

        private void Update()
        {
            var audioSettings = GameManager.settings.audioSettings;
            if (PauseMenu.active) audioSettings.SetVolume();
        }
    }
}
