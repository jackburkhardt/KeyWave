using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioClipVolumeSettings", menuName = "Audio Clip Database")]
public class AudioClipDatabase : ScriptableObject
{
    [Serializable]
    public struct AudioData
    {
        public string clipAddress;
        [Range(0, 1)]
        public float volume;
        public AudioMixerGroup channel;
    }
    
    public List<AudioData> audioData;
}
