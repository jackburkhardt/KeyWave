using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class PointsPanel : MonoBehaviour
{

    public UITextField label;
    public UITextField description;
    public Outline outline;
    public Color baseColor;
    public PointsFishBowl fishBowl;
    public Graphic backgroundColor;
    public UITextField prefix;
    public CanvasGroup canvasGroup;
    
    public void SetPanel(Item item)
    {
        label.text = item.Name;
        description.text = item.Description;
        outline.effectColor = item.LookupColor("Inverse Color");
        prefix.color = outline.effectColor;
        backgroundColor.color = Color.Lerp(outline.effectColor, baseColor, 0.95f);
        fishBowl.SetPointType( item);
    }

    private void OnEnable()
    {
        if (canvasGroup) canvasGroup.alpha = 0;
    }
    
    public void Show()
    {
       // DOTween.Clear();
        if (canvasGroup)
        {
            DOTween.To( () => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, 0.1f);
        }
    }
    
    public void Hide()
    {
      //  DOTween.Clear();
        if (canvasGroup)
        {
            DOTween.To( () => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, 0.1f);
        }
    }
}
