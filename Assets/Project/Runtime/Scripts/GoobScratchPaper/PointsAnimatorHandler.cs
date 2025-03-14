using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class PointsAnimatorHandler : MonoBehaviour
{
   [PointsPopup]
    [HideIf("hideType")]
    public string type;

    
    
    private bool hideType => GetComponent<PointsFishBowl>() != null;
    
    public string showTrigger = "Show";
    public string hideTrigger = "Hide";

    public string onPointsIncreaseTrigger = "OnPointsIncrease";
    public string onPointsDecreaseTrigger = "OnPointsDecrease";
    
    
    
    public string onPointsAnimationStartTrigger = "OnPointsAnimStart";
    public string onPointsAnimationEndTrigger = "OnPointsAnimEnd";

    public float duration = 2f;
    
    private Animator animator;
    
    public UnityEvent OnAnimationStart;
    
    public UnityEvent OnAnimationEnd;

    private void OnValidate()
    {
        animator ??= GetComponent<Animator>();
        if (hideType) type = GetComponent<PointsFishBowl>().GetType().Name;
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
       if (!string.IsNullOrEmpty(showTrigger)) animator.SetTrigger(showTrigger);
    }
    
    private void Hide()
    {
        if (!string.IsNullOrEmpty(hideTrigger)) animator.SetTrigger(hideTrigger);
    }
    
    private void SetTriggerIfValid(string points, string trigger)
    {
        if (!string.IsNullOrEmpty(trigger) && (points == this.type || points == string.Empty)) animator.SetTrigger(trigger);
    }
    
    private void HandlePointsChange(string pointType, int amount)
    {
        
        if (amount > 0)
        {
            SetTriggerIfValid(pointType, onPointsIncreaseTrigger);
        }
        else if (amount < 0)  SetTriggerIfValid(pointType, onPointsDecreaseTrigger); 
           
        
        StartCoroutine(Animate());
        
        IEnumerator Animate()
        {
            OnAnimationStart?.Invoke();
            SetTriggerIfValid(pointType, onPointsAnimationStartTrigger);
            if (pointType == type) Show();
            yield return new WaitForSeconds(duration);
            if (pointType == type) Hide();
            OnAnimationEnd?.Invoke();
            SetTriggerIfValid(pointType, onPointsAnimationEndTrigger);
        }
    }
}
