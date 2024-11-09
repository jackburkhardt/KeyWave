using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextMeshProTypewriterEffect = PixelCrushers.DialogueSystem.Wrappers.TextMeshProTypewriterEffect;

namespace Project.Runtime.Scripts.UI
{
    public class SubtitleContentElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image portrait;
        [SerializeField] private SubtitleManager subtitleManager;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProTypewriterEffect typewriterEffect;
        [SerializeField] private UITextField timeText;
        [SerializeField] private UITextField portraitText;
        [SerializeField] private UITextField subtitleText;

        private float _alpha;
        private RectElement? _containerRect;

        private bool _isAnimationPlaying = false;
        
        public bool PortraitActive => portrait != null && portrait.gameObject.activeSelf;
        
        private string _storedSubtitleText;
        private string _storedTimeText;
        private string _storedPortraitText;
        
        public string SubtitleText => subtitleText.text;

        public void Initialize(string subtitle, string time = null, string portraitName = null)
        {
            _storedPortraitText = portraitName;
            _storedSubtitleText = subtitle;
            _storedTimeText = time;
        }
        
        public void Initialize(SubtitleContentElement subtitleContentElement)
        {
            Initialize(subtitleContentElement.subtitleText.text, subtitleContentElement.timeText.text, subtitleContentElement.portraitText.text);
        }

        private void Awake()
        {
            if (portrait != null) _containerRect = new RectElement(portrait.transform.parent.GetComponent<RectTransform>());
            if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) subtitleText.text = string.Empty;
        }

        public void Update()
        {if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent)
            timeText.text = Clock.CurrentTime;
        }


        private void OnEnable()
        {
            RefreshLayoutGroups.Refresh(gameObject);
            if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
           
            if (!string.IsNullOrEmpty(_storedSubtitleText)) subtitleText.text = _storedSubtitleText;
            if (!string.IsNullOrEmpty(_storedTimeText)) timeText.text = _storedTimeText;
            if (!string.IsNullOrEmpty(_storedPortraitText)) portraitText.text = _storedPortraitText;
        
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
            _alpha = canvasGroup.alpha;
            LeanTween.alphaCanvas(canvasGroup, 1, 0.25f);
        
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (subtitleManager.duplicatedSubtitleContentContainer != transform.parent) return;
            LeanTween.alphaCanvas(canvasGroup, _alpha, 0.25f);

            var isPointerOnAnySubtitle = false;
        
          
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if ((typewriterEffect != null) && typewriterEffect.isPlaying)
            {
                typewriterEffect.Stop();
            }
        }
        
        

        public void Clear()
        {
            subtitleText.text = string.Empty;
        }

    

        public void HidePortraitHelper()
        {
          
            _containerRect?.Clear();
        }


        public void ShowPortraitHelper()
        {
            _containerRect?.Show();
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