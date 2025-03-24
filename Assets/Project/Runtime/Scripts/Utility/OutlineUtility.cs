using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Outline))]
public class OutlineUtility : MonoBehaviour
{

    public bool MatchAlphaWithCanvasGroup;


    CanvasGroup canvasGroup;
    Outline outline;
    
    
    private void Update()
    {
        
        outline ??= GetComponent<Outline>();
        
        if (!outline) return;
        
        if (MatchAlphaWithCanvasGroup)
        {
            
            canvasGroup ??= GetComponent<CanvasGroup>();
           
            if (canvasGroup)
            {
                var color = outline.effectColor;
                color.a = canvasGroup.alpha;
                outline.effectColor = color;
            }
        }
    }
}
