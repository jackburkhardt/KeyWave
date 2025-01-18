using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio Settings", menuName = "Settings/Audio Settings")]
public class AudioSettings : ScriptableObject
{
   [Range(0, 1)]
   public float masterVolume = 1f;
   [Range(0, 1)]
   public float musicVolume = 1f;
   [Range(0, 1)]
   public float environmentVolume = 1f;
   [Range(0, 1)]
   public float sfxVolume = 1f;
   
}
