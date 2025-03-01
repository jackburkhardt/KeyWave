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

        [SmartWatchAppPopup]
        public string app;
        
        

        public void OnEnable()
        {
            SmartWatch.OnAppOpen?.Invoke(SmartWatch.GetApp(app));

            foreach (var standardUIResponseButton in GetComponentsInChildren<StandardUIResponseButton>(true))
            {
                foreach (var subcomponent in standardUIResponseButton.GetComponentsInChildren<SmartWatchAppSubcomponent>(true))
                {
                    subcomponent.Evaluate(standardUIResponseButton);
                }
            }
        }
        
        public void OnAppOpen(SmartWatch.App app)
        {
            if (app.name != this.app && this.gameObject.activeSelf)
            {
                panel.Close();
            }
        }
    }
}
