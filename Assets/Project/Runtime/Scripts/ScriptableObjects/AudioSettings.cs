using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Audio Settings", menuName = "Settings/Audio Settings")]
public class AudioSettings : ScriptableObject
{
   public AudioMixer userMixerGroup;
   private bool showVolumeSettings => PauseMenu.active || !Application.isPlaying;
   [ShowIf("showVolumeSettings")]
   [Range(0, 1)]
   public float musicVolume = 1f;
   [ShowIf("showVolumeSettings")]
   [Range(0, 1)]
   public float sfxVolume = 1f;
   
   [InfoBox("These settings are only available when the game is paused.", EInfoBoxType.Error )]
   [HideIf("showVolumeSettings")]
   [Range(0, 1)]
   [SerializeField] private float _musicVolume = 1f;
   [HideIf("showVolumeSettings")]
   [Range(0, 1)]
   [SerializeField] private float _sfxVolume = 1f;
   
   public void SetVolume()
   {
      userMixerGroup.SetFloat("AudioSettingsMusicVolume", Mathf.Log10(musicVolume == 0 ? 0.00001f : musicVolume) * 20);
      userMixerGroup.SetFloat("AudioSettingsSoundEffectsVolume", Mathf.Log10(sfxVolume == 0 ? 0.00001f : sfxVolume) * 20);
   }

   public void OnValidate()
   {
      _musicVolume = musicVolume;
      _sfxVolume = sfxVolume;
   }
}
