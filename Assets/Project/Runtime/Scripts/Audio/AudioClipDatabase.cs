using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
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
        public bool includeVariants;

        [Serializable]
        public struct AudioDataVariant
        {
            public VariantType type;
            public string variantAddress;
            public AudioMixerGroup variantChannel;
        }

        public AudioDataVariant[] Variants;
        
        public enum VariantType
        {
            None,
            Pause
        }
        
    }
    
    public List<AudioData> audioData;
}
