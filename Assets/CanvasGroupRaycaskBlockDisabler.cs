using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupRaycaskBlockDisabler : MonoBehaviour
{
    
   private CanvasGroup _canvasGroup;
   
   private bool _blockRaycasts;
   
   internal float alpha = -1;
   
   
   // Update is called once per frame
    void Update()
    {
        _canvasGroup ??= GetComponent<CanvasGroup>();
        
        if (_canvasGroup == null) return;

        if (Math.Abs(alpha - _canvasGroup.alpha) > 0.001f)
        {
            alpha = _canvasGroup.alpha;
            
            if (_canvasGroup.alpha == 0)
            {
                _blockRaycasts = _canvasGroup.blocksRaycasts;
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                _canvasGroup.blocksRaycasts = _blockRaycasts;
            }
        }
        
        
    }
}
