using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class PointsAnimatorHandler : MonoBehaviour
{
    public Points.Type type;
    
    [HideIf("type", Points.Type.Null)]
    public string showTrigger = "Show";
    [HideIf("type", Points.Type.Null)]
    public string hideTrigger = "Hide";

    public float duration = 2f;
    
    private Animator animator;
    
    public UnityEvent OnAnimationStart;
    public UnityEvent OnAnimationEnd;

    private void OnValidate()
    {
        animator ??= GetComponent<Animator>();
    }

    private void OnEnable()
    {
        Points.OnPointsChange += HandlePointsChange;
        animator ??= GetComponent<Animator>();
    }
    
    private void OnDisable()
    {
        Points.OnPointsChange -= HandlePointsChange;
    }
    
    private void Show()
    {
        animator.SetTrigger(showTrigger);
    }
    
    private void Hide()
    {
        animator.SetTrigger(hideTrigger);
    }
    
    private void HandlePointsChange(Points.Type pointType)
    {
        
        
        StartCoroutine(Animate());
        
        IEnumerator Animate()
        {
            OnAnimationStart?.Invoke();
            if (pointType == type) Show();
            yield return new WaitForSeconds(duration);
            if (pointType == type) Hide();
            OnAnimationEnd?.Invoke();
        }
    }
}
