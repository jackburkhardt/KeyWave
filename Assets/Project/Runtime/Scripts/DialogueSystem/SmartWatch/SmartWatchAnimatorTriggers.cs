using System;
using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.AssetLoading;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SmartWatchAnimatorTriggers : MonoBehaviour
{
    
    [SerializeField] private Animator animator;

    
    
    [ReadOnly] [SerializeReference] private List<string> animatorTriggers;

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
            animator.SetTrigger(app.animatorTrigger);
        }
    }
}
