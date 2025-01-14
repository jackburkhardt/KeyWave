using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using NaughtyAttributes;

public class CutsceneLetterbox : MonoBehaviour, ICutsceneStartHandler, ICutsceneEndHandler
{
    public RectTransform topBar;
    public RectTransform bottomBar;
    
    public float topBarMaxHeight;
    public float bottomBarMaxHeight;
    
    public float animationDuration = 1f;

    private void Awake()
    {
        HideImmediate();
    }

    [Button]
    public void Show()
    {
        if (topBar == null || bottomBar == null) return;
        if (Application.isPlaying)
        {
            DOTween.To(() => topBar.sizeDelta, x => topBar.sizeDelta = x, new Vector2(topBar.sizeDelta.x, topBarMaxHeight), animationDuration);   
            DOTween.To(() => bottomBar.sizeDelta, x => bottomBar.sizeDelta = x, new Vector2(bottomBar.sizeDelta.x, bottomBarMaxHeight), animationDuration);
        }
        
        else if (Application.isEditor)
        {
            ShowImmediate();
        }
    }
    
    [Button]
    public void Hide()
    {
        if (topBar == null || bottomBar == null) return;

        if (Application.isPlaying)
        {
            DOTween.To(() => topBar.sizeDelta, x => topBar.sizeDelta = x, new Vector2(topBar.sizeDelta.x, 0), animationDuration);   
            DOTween.To(() => bottomBar.sizeDelta, x => bottomBar.sizeDelta = x, new Vector2(bottomBar.sizeDelta.x, 0), animationDuration);
        }
        
        else if (Application.isEditor)
        {
            HideImmediate();
        }
       
    }
    
    private void ShowImmediate()
    {
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, topBarMaxHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, bottomBarMaxHeight);
    }
    
    private void HideImmediate()
    {
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, 0);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, 0);
    }

    public void OnCutsceneStart()
    {
        Show();
    }

    public void OnCutsceneEnd()
    {
        Hide();
    }
}