using System;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using UnityEngine;

namespace Project.Runtime.Scripts.Utility
{
    public class SmartWatchAppPanel : MonoBehaviour
    {

        public UIPanel panel;
        

        [SmartWatchAppPopup] [Label("App")]
        public string Name;

        public void OnEnable()
        {
            SmartWatchPanel.onAppOpen?.Invoke(this);

            foreach (var standardUIResponseButton in GetComponentsInChildren<StandardUIResponseButton>(true))
            {
                foreach (var subcomponent in standardUIResponseButton.GetComponentsInChildren<SmartWatchAppSubcomponent>(true))
                {
                    subcomponent.Evaluate(standardUIResponseButton);
                }
            }
        }
        
        public void OnAppOpen(SmartWatchAppPanel app)
        {
            if (app.Name != this.Name && this.gameObject.activeSelf)
            {
                panel.Close();
            }
        }
    }
}
