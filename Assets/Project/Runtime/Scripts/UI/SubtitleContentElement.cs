using System;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem.Wrappers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleContentElement : MonoBehaviour
{
    public Image portrait;
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProTypewriterEffect typewriterEffect;
    [SerializeField] private UITextField timeText;
    [SerializeField] private UITextField portraitText;
    [SerializeField] private TMP_Text mainClock;

    
    
    private struct TextElement
    {
        public TextElement(UITextField text)
        {
            this._text = text;
            this._defaultColor = text.color;
        }
        
        private UITextField _text;
        private Color _defaultColor;

        public void Clear()
        {
            this._text.color = Color.clear;
        }
        
        public void Show()
        {
            this._text.color = this._defaultColor;
        }
        
    }
    
    private struct RectElement
    {
        public RectElement(RectTransform rectTransform)
        {
            this._rectTransform = rectTransform;
            this._defaultWidth = rectTransform.sizeDelta.x;
            this._defaultHeight = rectTransform.sizeDelta.y;
        }

        private RectTransform _rectTransform;
        private float _defaultWidth;
        private float _defaultHeight;

        public void Clear()
        {
            _rectTransform.sizeDelta = new Vector2(0, 0);
        }
        
        public void Show()
        {
            _rectTransform.sizeDelta = new Vector2(_defaultWidth, _defaultHeight);
        }
        
    }

    private TextElement _timeTextElement;
    private TextElement _portraitTextElement;
    private RectElement _containerRect;
    
    private bool _isAnimationPlaying = false;

    private void Awake()
    {
        _timeTextElement = new TextElement(timeText);
        _portraitTextElement = new TextElement(portraitText);
        _containerRect = new RectElement(portrait.transform.parent.GetComponent<RectTransform>());
    }

    public void Clear()
    {
        typewriterEffect.GetComponent<TMP_Text>().text = string.Empty;
    }

    public void UpdateTime()
    {
       if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) timeText.text = mainClock.text.Replace(" ", ":");
       canvasGroup.alpha = 1;

    }

    
    public void HidePortraitHelper()
    {
        _portraitTextElement.Clear();
        _timeTextElement.Clear();
        _containerRect.Clear();
    }

    
    public void ShowPortraitHelper()
    {
        _portraitTextElement.Show();
        _timeTextElement.Show();
        _containerRect.Show();
    }

    public void Update()
    {if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent)
        timeText.text = Clock.CurrentTime;
    }


    private void OnEnable()
    {
        RefreshLayoutGroups.Refresh(gameObject);
        if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
        Destroy(typewriterEffect);
        canvasGroup.alpha = 0.25f;
        
    }
}
