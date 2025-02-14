﻿using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Utility;
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
        public Dictionary<string, AudioSource> activeAudio = new();
        
        public UnityEvent onActiveAudioChange;
        
        public AudioMixer UserAudioMixer => _userAudioMixer;
        
        public AudioClipDatabase ClipDatabase => _clipDatabase;
        

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        #region Clip Management

        private void LoadClipAndPlay(string clipAddress, AudioSource source, Action followup = null,
            bool isVariant = false)
        {
            var clipData = _clipDatabase.audioData.Find(data => data.clipAddress == clipAddress);
            
            AddressableLoader.RequestLoad<AudioClip>(clipAddress, clip =>
            {
                if (ClipAlreadyPlaying(clipAddress)) return;
                
                try
                {
                    activeAudio.Add(clipAddress, source);
                }
                catch (ArgumentException e)
                {
                    Debug.LogError($"AudioEngine: Attempting to play the same clip \"{clipAddress}\" at the same time. This is not supported.");
                    return;
                }
                
                source.clip = clip;
                
                if (clipData.volume != 0) // no clip settings found
                {
                    source.volume = clipData.volume;
                }
                source.outputAudioMixerGroup = clipData.channel;
                source.Play();

                followup?.Invoke();

                onActiveAudioChange.Invoke();
                
                
                IEnumerator SendSequencerMessage(string message)
                {
                    while (!source.isPlaying) yield return null;
                    Sequencer.Message(message);
                }

                StartCoroutine(SendSequencerMessage("PlayClip"));

            });
            
            
            if (!isVariant && clipData.includeVariants)
            {
                foreach (var variant in clipData.Variants)
                {
                    AudioSource newSource = gameObject.AddComponent<AudioSource>();
                    newSource.loop = source.loop;
                    LoadClipAndPlay(variant.variantAddress, newSource, () =>
                    {
                        newSource.volume = clipData.volume;
                        newSource.outputAudioMixerGroup = variant.variantChannel;
                    }, true);
                }
            }
        }
        
        private bool ClipAlreadyPlaying(string clipAddress)
        {
            var clipIsPlaying = activeAudio.ContainsKey(clipAddress);
            if (clipIsPlaying)
            {
                StartCoroutine(SendSequencerMessage("PlayClip"));
            }
            
            IEnumerator SendSequencerMessage(string message)
            {
                yield return null;
                Sequencer.Message(message);
            }

            return clipIsPlaying;
        }
        
        public void SetClipVolume(string clipAddress, float volume)
        {
            if (!ClipAlreadyPlaying(clipAddress)) return;
            
            var defaultVolume = _clipDatabase.audioData.Find(data => data.clipAddress == clipAddress).volume;
            activeAudio[clipAddress].volume = volume * defaultVolume;
        }
        
        public float GetClipVolume(string clipAddress)
        {
            if (!ClipAlreadyPlaying(clipAddress)) return 0;
            
            return activeAudio[clipAddress].volume;
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
            
            activeAudio[clipAddress].Stop();
            AddressableLoader.Release(clipAddress);
            Destroy(activeAudio[clipAddress]);
            
            activeAudio.Remove(clipAddress);
            onActiveAudioChange.Invoke();
        }
        
        public void StopAllAudio()
        {
            foreach (var audio in activeAudio)
            {
                audio.Value.Stop();
                AddressableLoader.Release(audio.Key);
                Destroy(audio.Value);
            }
            
            activeAudio.Clear();
            onActiveAudioChange.Invoke();
        }
        
        public void PauseAllAudio()
        {
            foreach (var audio in activeAudio)
            {
                audio.Value.Pause();
            }
        }
        
        public void ResumeAllAudio()
        {
            foreach (var audio in activeAudio)
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
            _userAudioMixer.SetFloat(channelName + "/Master", mappedVolume);
        }

        public float GetChannelVolume(string channelName)
        {
            
            _userAudioMixer.GetFloat(channelName + "/Master", out float volume);
            return volume.Map(minVolume, maxVolume, 0, 1);
        }
        
        public void MuteChannel(string channelName)
        {
            _userAudioMixer.SetFloat(channelName + "/Master", -80);
        }
        
        public void UnmuteChannel(string channelName)
        {
            _userAudioMixer.SetFloat(channelName + "/Master", 0);
        }
        
        public void StopAllAudioOnChannel(string channelName)
        {
            List<string> keysToRemove = new();
            foreach (var entry in activeAudio)
            {
                if (entry.Value.outputAudioMixerGroup.name.StartsWith(channelName))
                {
                    keysToRemove.Add(entry.Key);
                }
            }
            keysToRemove.ForEach(StopClip);
        }
        
        public void PauseAllAudioOnChannel(string channelName)
        {
            foreach (var entry in activeAudio)
            {
                if (entry.Value.outputAudioMixerGroup.name.StartsWith(channelName))
                {
                    entry.Value.Pause();
                }
            }
        }
        
        public void ResumeAllAudioOnChannel(string channelName)
        {
            foreach (var entry in activeAudio)
            {
                if (entry.Value.outputAudioMixerGroup.name.StartsWith(channelName))
                {
                    entry.Value.UnPause();
                }
            }
        }
        
        #endregion

        
        
    }
}