using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Project.Runtime.Scripts.AssetLoading;
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
        SmartWatch.OnAppOpen += OnAppOpen;
    }
    
    private void OnDisable()
    {
        SmartWatch.OnAppOpen -= OnAppOpen;
    }

    private void OnValidate()
    {
        animator ??= GetComponent<Animator>();
        if (SmartWatch.instance == null) return;
        
        if (animatorTriggers == null) animatorTriggers = new List<string>();
        if (animatorTriggers.Count != SmartWatch.instance.apps.Count)
        {
            animatorTriggers.Clear();
            foreach (var app in SmartWatch.instance.apps)
            {
                animatorTriggers.Add(app.animatorTrigger);
            }
        }
        
        if (customTriggers == null) customTriggers = new List<CustomTrigger>();
       
        if (customTriggers.Count < animatorTriggers.Count)
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
    
    private void OnAppOpen(SmartWatch.App app)
    {
        if (app == null) return;
        if (animatorTriggers == null) return;
        if (animatorTriggers.Count == 0) return;
        if (app.name == null) return;
        if (app.animatorTrigger == null) return;
        if (animator == null) return;
        if (animatorTriggers.Contains(app.animatorTrigger))
        {
            if (triggerType.HasFlag(TriggerType.Default)) animator.SetTrigger(app.animatorTrigger);
            if (triggerType.HasFlag(TriggerType.Custom))
            {
                var customTrigger = customTriggers[animatorTriggers.IndexOf(app.animatorTrigger)].trigger;
                if (!string.IsNullOrEmpty(customTrigger)) animator.SetTrigger(customTrigger);
            }
        }
    }
}
