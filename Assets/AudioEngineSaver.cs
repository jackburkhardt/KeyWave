using UnityEngine;
using System;
using System.Collections.Generic;
using Project.Runtime.Scripts.Audio;

namespace PixelCrushers
{

    /// <summary>
    /// Saves an animator's state.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    [RequireComponent(typeof(AudioEngine))]
    public class AudioEngineSaver : Saver
    {
       
        [Serializable]
        public class Data
        {
            public List<string> audioSources = new List<string>();
            public List<bool> loopFlags = new List<bool>();
        }

        private Data m_data = new Data();
        private AudioEngine m_audioEngine;
        private AudioEngine audioEngine
        {
            get
            {
                if (m_audioEngine == null) m_audioEngine = GetComponent<AudioEngine>();
                return m_audioEngine;
            }
        }

        private void CheckAudioEngine()
        {
            if (audioEngine == null) return;
            if (m_data == null) m_data = new Data();
        }
        
        public void SaveAudioEngine()
        {
            RecordData();
        }

        public override string RecordData()
        {
            if (audioEngine == null) return string.Empty;
            CheckAudioEngine();
            
            //record active audiosource clips
            
            m_data.audioSources.Clear();
            m_data.loopFlags.Clear();
            
            foreach (var audioSource in audioEngine._activeAudio)
            {
               
                m_data.audioSources.Add(audioSource.Key);
                m_data.loopFlags.Add(audioSource.Value.loop);
            }
            
//            Debug.Log("Recording audio data");
            
            foreach (var clip in m_data.audioSources)
            {
               // Debug.Log(clip);
            }
            
            return SaveSystem.Serialize(m_data);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s) || audioEngine == null) return;
            
            m_data = SaveSystem.Deserialize<Data>(s, m_data);
            Debug.Log(m_data.ToString());
            if (m_data == null)
            {
                m_data = new Data();
            }
            else
            {
                audioEngine.StopAllAudio();
                Debug.Log("Applying audio data: " + m_data.audioSources.Count);
                for (int i = 0; i < m_data.audioSources.Count; i++)
                {
                    audioEngine.PlayClip(m_data.audioSources[i]);
                    audioEngine._activeAudio[m_data.audioSources[i]].loop = m_data.loopFlags[i];
                }
            }
        }
    }
}
