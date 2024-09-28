using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.Audio
{
    public class AudioEngine : MonoBehaviour
    {
        public static AudioEngine Instance;
        
        [SerializeField] private AudioClipDatabase _clipDatabase;
        [SerializeField] private AudioMixer _userAudioMixer;
        public Dictionary<string, AudioSource> _activeAudio = new();
        
        public UnityEvent onActiveAudioChange;
        
        public AudioMixer UserAudioMixer => _userAudioMixer;
        
        public AudioClipDatabase ClipDatabase => _clipDatabase;
        

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        #region Clip Management
        
        private void LoadClipAndPlay(string clipAddress, AudioSource source, Action followup = null)
        {
            AddressableLoader.RequestLoad<AudioClip>(clipAddress, clip =>
            {
                source.clip = clip;
                var clipData = _clipDatabase.audioData.Find(data => data.clipAddress == clipAddress);
                if (clipData.volume != 0) // no clip settings found
                {
                    source.volume = clipData.volume;
                }
                source.outputAudioMixerGroup = clipData.channel;
                source.Play();
                
                _activeAudio.Add(clipAddress, source);
                
                followup?.Invoke();

                onActiveAudioChange.Invoke();

                
                if (clipData.includeVariants)
                {
                    foreach (var variant in clipData.Variants)
                    {
                        AudioSource newSource = gameObject.AddComponent<AudioSource>();
                        newSource.loop = source.loop;
                        LoadClipAndPlay(variant.variantAddress, newSource, () =>
                        {
                            newSource.volume = 0;
                            newSource.outputAudioMixerGroup = variant.variantChannel;
                        });
                    }
                }
                
                IEnumerator SendSequencerMessage(string message)
                {
                    while (!source.isPlaying) yield return null;
                    yield return new WaitForSeconds(0.2f);
                    Sequencer.Message(message);
                }

                StartCoroutine(SendSequencerMessage("PlayClip"));

            });
        }
        
        private bool ClipAlreadyPlaying(string clipAddress)
        {
            return _activeAudio.ContainsKey(clipAddress);
        }
        
        public void SetClipVolume(string clipAddress, float volume)
        {
            if (!ClipAlreadyPlaying(clipAddress)) return;
            
            var defaultVolume = _clipDatabase.audioData.Find(data => data.clipAddress == clipAddress).volume;
            _activeAudio[clipAddress].volume = volume * defaultVolume;
        }
        
        public float GetClipVolume(string clipAddress)
        {
            if (!ClipAlreadyPlaying(clipAddress)) return 0;
            
            return _activeAudio[clipAddress].volume;
        }

        public void PlayClip(string clipAddress)
        {
            if (ClipAlreadyPlaying(clipAddress)) return;
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            LoadClipAndPlay(clipAddress, newSource, () => 
                StartCoroutine(WaitForClipEnd(clipAddress, newSource)));
        }

        public void PlayClip(string clipAddress, int repeats)
        {
            if (ClipAlreadyPlaying(clipAddress)) return;
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            LoadClipAndPlay(clipAddress, newSource, () => 
                StartCoroutine(PlayClipRepeat(clipAddress, newSource, repeats)));  // LoadClipAndPlay already plays once
        }
        
        public void PlayClipLooped(string clipAddress)
        {
            if (ClipAlreadyPlaying(clipAddress)) return;
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.loop = true;
            LoadClipAndPlay(clipAddress, newSource);
        }

        public void StopClip(string clipAddress)
        {
            if (!ClipAlreadyPlaying(clipAddress)) return;
            
            _activeAudio[clipAddress].Stop();
            AddressableLoader.Release(clipAddress);
            Destroy(_activeAudio[clipAddress]);
            
            _activeAudio.Remove(clipAddress);
            onActiveAudioChange.Invoke();
        }
        
        public void StopAllAudio()
        {
            foreach (var audio in _activeAudio)
            {
                audio.Value.Stop();
                AddressableLoader.Release(audio.Key);
                Destroy(audio.Value);
            }
            
            _activeAudio.Clear();
            onActiveAudioChange.Invoke();
        }
        
        public void PauseAllAudio()
        {
            foreach (var audio in _activeAudio)
            {
                audio.Value.Pause();
            }
        }
        
        public void ResumeAllAudio()
        {
            foreach (var audio in _activeAudio)
            {
                audio.Value.UnPause();
            }
        }

        private IEnumerator WaitForClipEnd(string clipAddress, AudioSource source)
        {
            while (source && !Mathf.Approximately(source.time, source.clip.length))
            {
                yield return null;
            }
            if (source) StopClip(clipAddress);
        }
        
        private IEnumerator PlayClipRepeat(string clipAddress, AudioSource source, int repeats)
        {
            for (int i = 0; i < repeats; i++)
            {
                while (!Mathf.Approximately(source.time, source.clip.length))
                {
                    yield return null;
                }
                source.Play();
            }
            StopClip(clipAddress);
        }
        
        #endregion
        
        #region Channel Management
        
        private const float maxVolume = 0;
        private const float minVolume = -20;
        
        
        public void SetChannelVolume(string channelName, float volume)
        {
            var mappedVolume = volume.Map(0, 1, minVolume, maxVolume);
            _userAudioMixer.SetFloat(channelName + "/Volume", mappedVolume);
        }

        public float GetChannelVolume(string channelName)
        {
            
            _userAudioMixer.GetFloat(channelName + "/Volume", out float volume);
            return volume.Map(minVolume, maxVolume, 0, 1);
        }
        
        public void MuteChannel(string channelName)
        {
            _userAudioMixer.SetFloat(channelName + "/Volume", -80);
        }
        
        public void UnmuteChannel(string channelName)
        {
            _userAudioMixer.SetFloat(channelName + "/Volume", 0);
        }
        
        public void StopAllAudioOnChannel(string channelName)
        {
            List<string> keysToRemove = new();
            foreach (var entry in _activeAudio)
            {
                if (entry.Value.outputAudioMixerGroup.name == channelName)
                {
                    keysToRemove.Add(entry.Key);
                }
            }
            keysToRemove.ForEach(StopClip);
        }
        
        public void PauseAllAudioOnChannel(string channelName)
        {
            foreach (var entry in _activeAudio)
            {
                if (entry.Value.outputAudioMixerGroup.name == channelName)
                {
                    entry.Value.Pause();
                }
            }
        }
        
        public void ResumeAllAudioOnChannel(string channelName)
        {
            foreach (var entry in _activeAudio)
            {
                if (entry.Value.outputAudioMixerGroup.name == channelName)
                {
                    entry.Value.UnPause();
                }
            }
        }
        
        #endregion

        
        
    }
}