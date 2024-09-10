using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using Project.Runtime.Scripts.Audio;
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

 
        
        private void Awake()
        {
            _audioEngine = GetComponent<AudioEngine>();
        }
        
        
        private void OnPause()
        {
          var audioClipDatabase = _audioEngine.ClipDatabase.audioData;

          foreach (var activeAudio in _audioEngine._activeAudio)
          {
              var audioData = audioClipDatabase.Find(x => x.clipAddress == activeAudio.Key);
              if (audioData.includeVariants)
              {
                  foreach (var variant in audioData.Variants)
                  {
                      if (variant.type == AudioClipDatabase.AudioData.VariantType.Pause)
                      {
                          var originalSource = _audioEngine._activeAudio[audioData.clipAddress];
                          var variantSource = _audioEngine._activeAudio[variant.variantAddress];

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

            foreach (var activeAudio in _audioEngine._activeAudio)
            {
                var audioData = audioClipDatabase.Find(x => x.clipAddress == activeAudio.Key);
                if (audioData.includeVariants)
                {
                    foreach (var variant in audioData.Variants)
                    {
                        if (variant.type == AudioClipDatabase.AudioData.VariantType.Pause)
                        {
                            var originalSource = _audioEngine._activeAudio[audioData.clipAddress];
                            var variantSource = _audioEngine._activeAudio[variant.variantAddress];
                            StartCoroutine(CrossFadeAudioSources(originalSource, variantSource, 0.5f));
                        }
                    }
                }
            }
          
            DefaultSnapshot.TransitionTo(0.25f);
        }
    }
}
