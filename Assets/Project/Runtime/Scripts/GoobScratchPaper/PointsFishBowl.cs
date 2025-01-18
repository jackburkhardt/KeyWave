using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PointsFishBowl : MonoBehaviour
{
    [SerializeField] private Image filledImage;

    [Range(0, 1)] [SerializeField] private float fillAmount;

    [SerializeField] private float timeToFill = 1f;
    
    public Points.Type type;

    [SerializeField] Animator animator;
    [SerializeField] private string animationTrigger = "Fill";
    
    

    private float Fill
    {
        get => fillAmount;
        set
        {
            fillAmount = value;
            filledImage.fillAmount = fillAmount;
        } 
    }

    
    private void OnValidate()
    {
        
        if (filledImage != null) filledImage.fillAmount = fillAmount;
        
     
        
    }

    private void OnEnable()
    {
        Points.OnPointsChange += SetPoints;
    }

    private void OnDisable()
    {
        Points.OnPointsChange -= SetPoints;
    }

    private void SetPoints(Points.Type pointType, int amount)
    {
        if (pointType != type)
        {
            GetComponent<Selectable>().interactable = false;
            return;
        }
      
        
        animator.SetTrigger(animationTrigger);
        if (Points.MaxScore(pointType) != 0) 
            DOTween.To(() => Fill, x => Fill = x, Points.Score(pointType) / (float) Points.MaxScore(pointType), timeToFill);
        
        
    }


}
