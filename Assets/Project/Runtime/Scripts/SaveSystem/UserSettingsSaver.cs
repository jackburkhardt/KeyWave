using Project.Runtime.Scripts.Manager;
using UnityEngine;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class UserSettingsSaver
    {
        public static void SaveSettings()
        {
            PlayerPrefs.SetFloat("sfxVol", GameManager.settings.audioSettings.sfxVolume);
            PlayerPrefs.SetFloat("musicVol", GameManager.settings.audioSettings.musicVolume);
            PlayerPrefs.SetString("autoPause", GameManager.settings.autoPauseOnFocusLost ? "1" : "0");
            PlayerPrefs.Save();
        }
        
        public static void ApplySettings()
        {
            if (!PlayerPrefs.HasKey("sfxVol")) // we have no settings to apply
            {
                return;
            }
            
            if (GameManager.TryGetSettings( out var settings))
            {
                settings.audioSettings.sfxVolume = PlayerPrefs.GetFloat("sfxVol");
                settings.audioSettings.musicVolume = PlayerPrefs.GetFloat("musicVol");
                settings.autoPauseOnFocusLost = PlayerPrefs.GetString("autoPause") == "1";
            }
        }
    }
}