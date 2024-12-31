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
   
    
    public string credibilityAnimationTrigger = "Credibility";
    public string engagementAnimationTrigger = "Engagement";
    public string commitmentAnimationTrigger = "Commitment";
    public string wellnessAnimationTrigger = "Wellness";
    
    [SerializeField] private float dialogueSystemPauseDuration = 2f;
    
    [SerializeField] private Animator animator;


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
        Debug.Log("Visualizing points");
        DialogueManager.instance.Pause();
        
        switch (type)
        {
            case Points.Type.Credibility:
                animator.SetTrigger(credibilityAnimationTrigger);
                break;
            case Points.Type.Engagement:
                animator.SetTrigger(engagementAnimationTrigger);
                break;
            case Points.Type.Commitment:
                animator.SetTrigger(commitmentAnimationTrigger);
                break;
            case Points.Type.Wellness:
                animator.SetTrigger(wellnessAnimationTrigger);
                 break;
        }
        
        StartCoroutine(ResumeDialogue());
        
        IEnumerator ResumeDialogue()
        {
            yield return new WaitForSeconds(dialogueSystemPauseDuration);
            DialogueManager.instance.Unpause();
        }
    }
    
    
}
