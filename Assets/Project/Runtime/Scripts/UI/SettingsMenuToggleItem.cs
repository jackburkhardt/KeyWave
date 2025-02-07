using System;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI
{
    public class SettingsMenuToggleItem : MonoBehaviour, IPointerEnterHandler
    {
        public string settingName;
        public string settingDescription;
        public Toggle toggle;
        
        private void OnEnable()
        {
            toggle.isOn = GameManager.settings.autoPauseOnFocusLost;
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
    }
}