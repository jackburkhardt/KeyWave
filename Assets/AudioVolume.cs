using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioVolume", menuName = "AudioVolume")]
public class AudioVolume : ScriptableObject
{
    [Serializable]
    public struct AudioData
    {
        public AudioClip clip;
        [Range(0, 1)]
        public float volume;
    }
    public List<AudioData> audioData;
}
