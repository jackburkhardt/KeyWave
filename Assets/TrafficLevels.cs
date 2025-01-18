using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class TrafficLevels : MonoBehaviour
{
    [Tooltip("The content holder that contains the individual traffic level elements.")]
    public RectTransform content;


    public float maxValue = 500f;

    public enum Action
    {
        AdjustHeight,
        AdjustWidth
        
    }
    
    public Action action;
    
    public float currentTrafficLevelAlpha;
    public float defaultTrafficLevelAlpha = 0.5f;

    private void OnValidate()
    {
        SetTrafficLevels();
    }

    private void OnEnable()
    {
        SetTrafficLevels();
    }
    
    private RectTransform GetElementFromPercentage(float percentage)
    {
        var elementCount = content.childCount;
        var index = Mathf.FloorToInt(percentage * elementCount);
        return content.GetChild(index).GetComponent<RectTransform>();
    }
    
    private void SetTrafficElementAlpha(RectTransform element)
    {
        if (Application.isEditor && !Application.isPlaying)
        {
          
            element.GetComponent<CanvasGroup>().alpha =  element.GetSiblingIndex() == 0 ? currentTrafficLevelAlpha : defaultTrafficLevelAlpha;
            return;
        }
        
        
        
        if (GetElementFromPercentage(Clock.DayProgress) == element)
        {
            element.GetComponent<CanvasGroup>().alpha = currentTrafficLevelAlpha;
        }
        else
        {
            element.GetComponent<CanvasGroup>().alpha = defaultTrafficLevelAlpha;
        }
    }
    
    private void DoActionOnElement(RectTransform element)
    {
        switch (action)
        {
            case Action.AdjustHeight:
                element.sizeDelta = new Vector2(element.rect.width,
                    Traffic.GetTrafficMultiplier(element.GetSiblingIndex() / (float)content.childCount, false) * maxValue);
                break;
            case Action.AdjustWidth:
                element.sizeDelta = new Vector2(
                    Traffic.GetTrafficMultiplier(element.GetSiblingIndex() / (float)content.childCount, false) * maxValue,
                    element.rect.height);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Button]
    private void SetTrafficLevels()
    {
        if (content == null) return;
        
        var elementCount = content.childCount;

        foreach (var t in content.GetComponentsInChildren<RectTransform>().ToList().Where(t =>
                 {
                     Transform c;
                     return t.transform != (c = content.transform) && t.parent == c;
                 }))
        {
            
            SetTrafficElementAlpha(t);
            
            DoActionOnElement(t);
            
        }
    }
}
