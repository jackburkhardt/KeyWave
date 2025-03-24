using System;
using System.Collections;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class MutuallyExclusiveAnimatorHandler : MonoBehaviour
{
    public string showTrigger;
    public string hideTrigger;

    public bool delayShow;
    
    [ShowIf("delayShow")]
    
    public float delay;
    
    private float Delay => delayShow ? delay : 0;
    
    [Serializable]
    public class AnimatorTriggers
    {
        [Tooltip("Exception is applied when Show/Hide target is this animator")]
        public enum ExceptionType
        {
            ApplyTriggersOnlyToThisAnimator,
            ApplyTriggersOnAllAnimators
        }
        
        public ExceptionType exceptionType;
        
        public Animator animator;
        public string showTrigger;
        public string hideTrigger;
    }
    
    public AnimatorTriggers[] exceptions;
    
    public void Show(Animator animator)
    {
        
        
        StopAllCoroutines();
        
        foreach (var getAnimator in GetComponentsInChildren<Animator>())
        {
            
            if (getAnimator == animator) continue;
            
            
            if (exceptions.Any(p => p.animator == animator && p.exceptionType == AnimatorTriggers.ExceptionType.ApplyTriggersOnAllAnimators))
            {
                getAnimator.SetTrigger(GetHideTrigger(animator));
            }
            
            else getAnimator.SetTrigger(GetHideTrigger(getAnimator));
        }
        
        if (Delay > 0)
        {
            StartCoroutine(Show(Delay));
        }
        else
        {
            animator.SetTrigger(GetShowTrigger(animator));
        }
        
        IEnumerator Show(float delay)
        {
            yield return new WaitForSeconds(delay);
            animator.SetTrigger(GetShowTrigger(animator));
        }
    }
    
    public void Hide(Animator animator)
    {
        animator.SetTrigger(GetHideTrigger(animator));
    }
    
    
    
    
    private string GetShowTrigger(Animator animator)
    {
        return exceptions.Any(p => p.animator == animator) ? exceptions.First(p => p.animator == animator).showTrigger : showTrigger;
    }
    
    private string GetHideTrigger(Animator animator)
    {
        return exceptions.Any(p => p.animator == animator && p.exceptionType == AnimatorTriggers.ExceptionType.ApplyTriggersOnlyToThisAnimator) ? exceptions.First(p => p.animator == animator).hideTrigger : hideTrigger;
    }

    private void OnValidate()
    {
        delay = Mathf.Max(delay, 0);
    }
}
