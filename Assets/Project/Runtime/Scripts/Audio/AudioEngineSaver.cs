using System;
using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;

namespace Project.Runtime.Scripts.Audio
{
    [RequireComponent(typeof(AudioEngine))]
    public class AudioEngineSaver : Saver
    {
        [Serializable]
        public class ActiveAudioData
        {
            public string clipName;
            public bool loop;
        }
        
        private AudioEngine audioEngine => GetComponent<AudioEngine>();
        
        public override string RecordData()
        {
            if (audioEngine == null) return string.Empty;
            
            var activeAudio = new List<ActiveAudioData>();
            
            
            foreach (var audioSource in audioEngine.activeAudio)
            {
                activeAudio.Add(new ActiveAudioData
                {
                    clipName = audioSource.Key,
                    loop = audioSource.Value.loop
                });
            }
            
            return PixelCrushers.SaveSystem.Serialize(activeAudio);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s) || audioEngine == null) return;

            var loadedData = PixelCrushers.SaveSystem.Deserialize<List<ActiveAudioData>>(s);
            if (loadedData == null) return;
            
            audioEngine.StopAllAudio();
            foreach (var source in loadedData)
            {
                Debug.Log($"AudioEngineSaver: Playing {source.clipName}");
                if (source.loop)
                {
                    audioEngine.PlayClipLooped(source.clipName);
                }
                else
                {
                    audioEngine.PlayClip(source.clipName);
                }
            }
            
        }
    }
}
