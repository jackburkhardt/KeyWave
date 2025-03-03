using System;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SmartWatchAnimatorTriggers : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public enum TriggerType
    {
        Default = 1 << 0,
        Custom = 1 << 1
    }
    
    [EnumFlags] public TriggerType triggerType = TriggerType.Default;
    
    
    private bool ShowDefaultTriggers => triggerType.HasFlag(TriggerType.Default);
    private bool ShowCustomTriggers => triggerType.HasFlag(TriggerType.Custom);
    
    [ShowIf("ShowDefaultTriggers")]
    [ReadOnly] [SerializeReference] private List<string> animatorTriggers;

    [Serializable]
    public class CustomTrigger
    {
        [ReadOnly] public string name;
        public string trigger;
        
        public CustomTrigger(string name)
        {
            this.name = name;
        }
    }
    
    [ShowIf("ShowCustomTriggers")]
    [SerializeReference] private List<CustomTrigger> customTriggers;

    private void OnEnable()
    {
        SmartWatchPanel.onAppOpen += OnAppOpen;
    }
    
    private void OnDisable()
    {
        SmartWatchPanel.onAppOpen -= OnAppOpen;
    }

    private void OnValidate()
    {
        animator ??= GetComponent<Animator>();
        
        if (animatorTriggers == null) animatorTriggers = new List<string>();
        var apps = SmartWatchPanel.GetAllApps();
        if (animatorTriggers != null && animatorTriggers.Count != apps.Count && apps.Count > 0)
        {
            animatorTriggers.Clear();
            foreach (var app in apps)
            {
                animatorTriggers.Add(app.Name);
            }
        }
        
        if (customTriggers == null) customTriggers = new List<CustomTrigger>();
       
        if (customTriggers.Count < animatorTriggers.Count && animatorTriggers.Count > 0)
        {
            for (var i = customTriggers.Count; i < animatorTriggers.Count; i++)
            {
                customTriggers.Add(new CustomTrigger(animatorTriggers[i]));
            }
        }
        
        if (customTriggers.Count > animatorTriggers.Count)
        {
            customTriggers.RemoveRange(animatorTriggers.Count, customTriggers.Count - animatorTriggers.Count);
        }
        

    }
    
    private void OnAppOpen(SmartWatchAppPanel app)
    {
        if (app == null) return;
        if (animatorTriggers == null) return;
        if (animatorTriggers.Count == 0) return;
        if (animator == null) return;
        if (animatorTriggers.Contains(app.Name))
        {
            if (triggerType.HasFlag(TriggerType.Default)) animator.SetTrigger(app.Name);
            if (triggerType.HasFlag(TriggerType.Custom))
            {
                var customTrigger = customTriggers[animatorTriggers.IndexOf(app.Name)].trigger;
                if (!string.IsNullOrEmpty(customTrigger)) animator.SetTrigger(customTrigger);
            }
        }
    }

   
    
}
