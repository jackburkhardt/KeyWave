using System.Collections;
using DG.Tweening;
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
        }
        
        
        
        
        private void OnPause()
        {
          var audioClipDatabase = _audioEngine.ClipDatabase.audioData;

          foreach (var activeAudio in _audioEngine.activeAudio)
          {
              var audioData = audioClipDatabase.Find(x => x.clipAddress == activeAudio.Key);
              if (audioData.includeVariants)
              {
                  foreach (var variant in audioData.Variants)
                  {
                      if (variant.type == AudioClipDatabase.AudioData.VariantType.Pause)
                      {
                          var originalSource = _audioEngine.activeAudio[audioData.clipAddress];
                          var variantSource = _audioEngine.activeAudio[variant.variantAddress];

                          StartCoroutine(CrossFadeAudioSources(originalSource, variantSource, 0.5f));
                         
                          
                          Debug.Log("Pausing audio");
                      }
                  }
              }
          }
          
          PausedSnapshot.TransitionTo(0.25f);
        }
        
        
        private IEnumerator CrossFadeAudioSources(AudioSource source1, AudioSource source2, float duration)
        {
            var source1Volume = source1.volume;
            var source2Volume = source2.volume;
            
            source1.DOFade(source2Volume, duration).SetUpdate(true);
            source2.DOFade(source1Volume, duration).SetUpdate(true);
            
            yield return new WaitForSeconds(duration);
        }

        private void OnUnpause()
        {
            var audioClipDatabase = _audioEngine.ClipDatabase.audioData;

            foreach (var activeAudio in _audioEngine.activeAudio)
            {
                var audioData = audioClipDatabase.Find(x => x.clipAddress == activeAudio.Key);
                if (audioData.includeVariants)
                {
                    foreach (var variant in audioData.Variants)
                    {
                        if (variant.type == AudioClipDatabase.AudioData.VariantType.Pause)
                        {
                            var originalSource = _audioEngine.activeAudio[audioData.clipAddress];
                            var variantSource = _audioEngine.activeAudio[variant.variantAddress];
                            StartCoroutine(CrossFadeAudioSources(originalSource, variantSource, 0.5f));
                        }
                    }
                }
            }
          
            DefaultSnapshot.TransitionTo(0.25f);
        }
        
        
        
        public static void SetParameter(string parameter, float value)
        {
            if (_instance == null) return;
            _instance._audioEngine.UserAudioMixer.SetFloat(parameter, value);
        }
        
        
    }
}
