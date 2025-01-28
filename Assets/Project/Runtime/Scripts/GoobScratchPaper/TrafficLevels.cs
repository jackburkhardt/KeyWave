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
    
    [ReadOnly] public float minValue = 30f;
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
        
        TrafficSettings.OnTrafficSettingsChanged += SetTrafficLevels;
    }
    
    private void OnDisable()
    {
        TrafficSettings.OnTrafficSettingsChanged -= SetTrafficLevels;
    }
    
    private RectTransform GetElementFromPercentage(float percentage)
    {
        var index = Mathf.FloorToInt(Mathf.Round(percentage * (content.childCount + 1)));
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
        var value = Traffic.GetRawTrafficMultiplier((element.GetSiblingIndex()) / ((float)content.childCount)) * (maxValue - minValue) + minValue;
        switch (action)
        {
            case Action.AdjustHeight:
                element.sizeDelta = new Vector2(element.rect.width,
                    value);
                break;
            case Action.AdjustWidth:
                element.sizeDelta = new Vector2(
                    value,
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
        
        minValue = maxValue * Traffic.BaseTrafficLevel / Traffic.PeakTrafficLevel;
        
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
