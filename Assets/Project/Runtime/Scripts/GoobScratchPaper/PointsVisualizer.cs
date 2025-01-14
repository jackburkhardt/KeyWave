using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Michsky.MUIP;
using Project.Runtime.Scripts.Events;
using UnityEngine;

using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;

public class PointsVisualizer : MonoBehaviour
{
   
    [SerializeField] private float dialogueSystemPauseDuration = 2f;
    
    [SerializeField] private Animator animator;
    
    public string animationTrigger = "Points";
    
    private bool _isAnimating;


    private void OnEnable()
    {
        Points.OnPointsChange += VisualizePointType;
    }

    private void OnDisable()
    {
        Points.OnPointsChange -= VisualizePointType;
    }


    private void VisualizePointType(Points.Type type)
    {
        DialogueManager.instance.Pause();
        
        _isAnimating = true;
        
        StartCoroutine(Animate(type));
        
        IEnumerator Animate(Points.Type type)
        {
            yield return new WaitForEndOfFrame();
        
            animator.SetTrigger(animationTrigger);
        
            yield return new WaitForSeconds(dialogueSystemPauseDuration);
            DialogueManager.instance.Unpause();
        }
        
        
    }
    
    
}
