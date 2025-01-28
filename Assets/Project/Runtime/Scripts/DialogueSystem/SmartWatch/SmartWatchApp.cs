using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.Utility
{
    public class SmartWatchApp : MonoBehaviour
    {
        [Dropdown("apps")] public string app;

        private List<string> apps => SmartWatch.instance.appNames;

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
    }
}
