using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MindmapCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
        [SerializeField]
        private Color _leftOutlineColor;
        [SerializeField]
        private Color _leftBasecolor;

        [SerializeField]
        private Color _rightOutlineColor;
        [SerializeField]
        private Color _rightBaseColor;

        private Color _defaultBaseColor;
        private Color _defaultOutlineColor;

        private RectTransform _rectTransform;
    private RectTransform _childRectTransform;
        private Image _image;
        private TMP_Text _text;
        private Outline _outline;


    float threshhold = 400;
        
    
    

    // Start is called before the first frame update
    void Start()
    {
            _rectTransform = GetComponent<RectTransform>();
            _childRectTransform = GetComponentInChildren<RectTransform>();
            _text = GetComponentInChildren<TMP_Text>();
            _outline = GetComponent<Outline>();
            _image = GetComponent<Image>();

            _defaultBaseColor = _image.color;
            _defaultOutlineColor = _text.color;

        SetupColors();

    }

    public void SetupColors()
    {
        if (_rectTransform.anchoredPosition.x < -threshhold) SetupColorsInternal(_leftBasecolor, _leftOutlineColor, SideOfScreen.Left);
        else if (_rectTransform.anchoredPosition.x > threshhold) SetupColorsInternal(_rightBaseColor, _rightOutlineColor, SideOfScreen.Right);
        else SetupColorsInternal(_defaultBaseColor, _defaultOutlineColor, SideOfScreen.Default);
        

        void SetupColorsInternal(Color baseColor, Color outlineColor, SideOfScreen currentScreenSide)
        {
            _image.color = baseColor;
            _text.color = outlineColor;
            _outline.effectColor = outlineColor;
            sideOfScreen = currentScreenSide;
        }
    }


    public void OnPointerDown(PointerEventData pointerEventData)
    {
      
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
      //  GameEvent.OnReleaseMindMapCard();
    }



    enum SideOfScreen
    {
        Left,
        Right,
        Default
    }

    SideOfScreen sideOfScreen;

    
    void ChangeColorOnScreenSide(Color baseColor, Color outlineColor, SideOfScreen currentScreenSide)
    {
        if (sideOfScreen == currentScreenSide) return;

        LeanTween.color(_rectTransform, baseColor, 0.25f);
        //  LeanTween.textColor(_childRectTransform, _leftOutlineColor, 0.5f);
        _text.color = outlineColor;
        _outline.effectColor = outlineColor;

        sideOfScreen = currentScreenSide;

      //  if (sideOfScreen == SideOfScreen.Left) GameEvent.OnCardTurnsBlue();
     //   else if (sideOfScreen == SideOfScreen.Right) GameEvent.OnCardTurnsRed();
    }


    // Update is called once per frame
    void Update()
    {
        if (_rectTransform == null) return;
        if (_rectTransform.GetSiblingIndex() != _rectTransform.parent.childCount - 1) return;

        if (_rectTransform.anchoredPosition.x < -threshhold) ChangeColorOnScreenSide(_leftBasecolor, _leftOutlineColor, SideOfScreen.Left);
        else if (_rectTransform.anchoredPosition.x > threshhold) ChangeColorOnScreenSide(_rightBaseColor, _rightOutlineColor, SideOfScreen.Right);
        else ChangeColorOnScreenSide(_defaultBaseColor, _defaultOutlineColor, SideOfScreen.Default);
    }
}


