using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextMeshProTypewriterEffect = PixelCrushers.DialogueSystem.Wrappers.TextMeshProTypewriterEffect;

namespace Project.Runtime.Scripts.UI
{
    public class SubtitleContentElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image portrait;
        [SerializeField] private SubtitleManager subtitleManager;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProTypewriterEffect typewriterEffect;
        [SerializeField] private UITextField timeText;
        [SerializeField] private UITextField portraitText;
        [SerializeField] private TMP_Text mainClock;
        [SerializeField] private UITextField subtitleText;
        [SerializeField] public Button continueButton;

        private float _alpha;
        private RectElement? _containerRect;

        private bool _isAnimationPlaying = false;
        private TextElement? _portraitTextElement;

        private TextElement? _timeTextElement;
        public TextElement SubtitleText;

        private List<SubtitleContentElement> DuplicatedSubtitles =>
            subtitleManager.GetComponentsInChildren<SubtitleContentElement>().ToList();

        public bool PortraitActive => portrait != null && portrait.gameObject.activeSelf;

        private void Awake()
        {
            if (timeText != null) _timeTextElement = new TextElement(timeText);
            if (portraitText != null) _portraitTextElement = new TextElement(portraitText);
            if (portrait != null) _containerRect = new RectElement(portrait.transform.parent.GetComponent<RectTransform>());
            if (subtitleText != null) SubtitleText = new TextElement(subtitleText);
            // else subtitleText = GetComponentInChildren<UITextField>();
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


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
            _alpha = canvasGroup.alpha;
            canvasGroup.alpha = 1;
        
            //DialogueManager.Pause();
        
         
            // subtitleManager.templateSubtitleContentElement.GetComponentInChildren<TextMeshProTypewriterEffect>().Pause();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
            canvasGroup.alpha = _alpha;

            var isPointerOnAnySubtitle = false;
        
            //  DialogueManager.Unpause();
        
            //  subtitleManager.templateSubtitleContentElement.GetComponentInChildren<TextMeshProTypewriterEffect>().Unpause();




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
            _portraitTextElement?.Clear();
            _timeTextElement?.Clear();
            _containerRect?.Clear();
        }


        public void ShowPortraitHelper()
        {
            _portraitTextElement?.Show();
            _timeTextElement?.Show();
            _containerRect?.Show();
        }


        public struct TextElement
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

            public new string ToString()
            {
                return _text.text;
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
    }
}