using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.Wrappers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleContentElement : MonoBehaviour
{
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProTypewriterEffect typewriterEffect;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text mainClock;

    private void Awake()
    {
        timeText.text = mainClock.text;
    }

    public void UpdateTime()
    {
        if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) timeText.text = mainClock.text.Replace(" ", ":");
        
    }


    private void OnEnable()
    {
        if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
        Destroy(typewriterEffect);
        canvasGroup.alpha = 0.25f;
    }
}
