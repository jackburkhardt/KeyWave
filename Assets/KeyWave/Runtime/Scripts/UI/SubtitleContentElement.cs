using System.Collections.Generic;
using PixelCrushers.DialogueSystem.Wrappers;
using TMPro;
using UnityEngine;

public class SubtitleContentElement : MonoBehaviour
{
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProTypewriterEffect typewriterEffect;
    [SerializeField] private List<Animator> _animatorsToWaitFor;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text mainClock;

    private bool _isAnimationPlaying = false;

    private void Awake()
    {
       // timeText.text = GameManager.instance.HoursMinutes(GameManager.instance.gameStateManager.gameState.clock);
        
    }

    public void UpdateTime()
    {
       if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) timeText.text = mainClock.text.Replace(" ", ":");
        
    }

    public void Update()
    {if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent)
        timeText.text = Clock.CurrentTime;
    }


    private void OnEnable()
    {
        if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
        Destroy(typewriterEffect);
        canvasGroup.alpha = 0.25f;
    }
}
