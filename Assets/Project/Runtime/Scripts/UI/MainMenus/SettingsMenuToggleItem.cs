using System;
using System.Linq;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI
{
    public class SettingsMenuToggleItem : MonoBehaviour, IPointerEnterHandler
    {
        private string[] _settingNameStrings = {"Auto Pause", "High Contrast"};
        [Dropdown( "_settingNameStrings")]
        public string settingName;
        
        public string settingDescription;
        public Toggle toggle;
        
        private void OnEnable()
        {
            toggle.isOn = Array.IndexOf(_settingNameStrings, settingName) switch
            {
                0 => GameManager.settings.autoPauseOnFocusLost,
                1 => GameManager.settings.HighContrastMode,
                _ => toggle.isOn
            };
        }

        public void ToggleAutoPause()
        {
            GameManager.settings.autoPauseOnFocusLost = toggle.isOn;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            PauseMenu.instance.settingsTitle.text = settingName;
            PauseMenu.instance.settingsDescription.text = settingDescription;
        }
        
        public void ToggleHighContrast()
        {
            GameManager.settings.HighContrastMode = toggle.isOn;
        }
    }
}