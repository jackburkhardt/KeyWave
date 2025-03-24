using System;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenuSliderItem : MonoBehaviour, IPointerEnterHandler
{
    public string settingName;
    public string settingDescription;
    public UITextField valueIndicator;
    public Slider slider;
    
    
    public enum SliderSettingType
    {
        MusicVolume,
        SFXVolume
    }
    
    public SliderSettingType settingType;

    public void OnEnable()
    {
        switch (settingType)
        {
            case SliderSettingType.MusicVolume:
                slider.value = GameManager.settings.audioSettings.musicVolume;
                valueIndicator.text = ((int)(GameManager.settings.audioSettings.musicVolume * 100)).ToString();
                break;
            case SliderSettingType.SFXVolume:
                slider.value = GameManager.settings.audioSettings.sfxVolume;
                valueIndicator.text = ((int)(GameManager.settings.audioSettings.sfxVolume * 100)).ToString();
                break;
            
        }
    }

    public void SliderValueChanged(float value)
    {
        valueIndicator.text = ((int)(value * 100)).ToString();
        
        switch (settingType)
        {
            case SliderSettingType.MusicVolume:
                GameManager.settings.audioSettings.musicVolume = value;
                break;
            case SliderSettingType.SFXVolume:
                GameManager.settings.audioSettings.sfxVolume = value;
                break;
            
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PauseMenu.instance.settingsTitle.text = settingName;
        PauseMenu.instance.settingsDescription.text = settingDescription;
    }
}
