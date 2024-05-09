using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class CircularButtonTextPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private RectTransform watchFace;
    [SerializeField] private RectTransform watchContainer;
    
    private RectTransform _rectTransform;
    private VerticalLayoutGroup _verticalLayoutGroup;
    private string previousText = string.Empty;
    [SerializeField] private bool widthLimitReached;
    private bool adjustedWidthOnLimit;
    [SerializeField] private Image parentImage;
    [SerializeField] private float shrinkRate = 50f;
    [SerializeField] private bool flipText = false;
    [SerializeReference] private Expansion expansion;

    public float padding;

    private enum Expansion
    {
        Horizontal,
        Vertical
    }
    
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _tmpText = GetComponentInChildren<TMP_Text>();
        _verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        _verticalLayoutGroup.childControlWidth = expansion == Expansion.Horizontal;
        _verticalLayoutGroup.childControlHeight = expansion == Expansion.Vertical;
        _verticalLayoutGroup.childAlignment = expansion == Expansion.Horizontal ? TextAnchor.LowerCenter : TextAnchor.MiddleLeft;

        parentImage.fillAmount = expansion == Expansion.Horizontal ? _rectTransform.rect.width / watchContainer.rect.width / Mathf.PI : _rectTransform.rect.height / watchContainer.rect.height / Mathf.PI;
        _rectTransform.localEulerAngles = expansion == Expansion.Horizontal ? new Vector3(0, 0, 90 + parentImage.fillAmount * 360 / 2f) :  new Vector3(0, 0, 180 + parentImage.fillAmount * 360 / 2f);

        if (expansion == Expansion.Horizontal)
            _tmpText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(watchContainer.rect.width - watchFace.rect.width) / 2f);

        var textAngle = 0;
        textAngle += flipText ? 180 : 0;
        textAngle += expansion == Expansion.Horizontal ? 0 : 180;
        _tmpText.rectTransform.localEulerAngles = new Vector3(0, 0, textAngle);
        
        if (_tmpText.text.Length == 0) return;

        if (previousText != _tmpText.text)
        {
            _tmpText.textInfo.ClearLineInfo();
            widthLimitReached = false;
            previousText = _tmpText.text;
            _rectTransform.offsetMin = new Vector2(0, 0);
            _rectTransform.offsetMax = -_rectTransform.offsetMin;
            shrinkRate = 100f;
            adjustedWidthOnLimit = false;
            return;
        }
        
        

        if (widthLimitReached)
        {
            if (adjustedWidthOnLimit) return;
            _rectTransform.offsetMin = expansion == Expansion.Horizontal ? new Vector2(_rectTransform.offsetMin.x - shrinkRate * 2, 0) : new Vector2(0, _rectTransform.offsetMin.y - shrinkRate * 2);
            _rectTransform.offsetMax = -_rectTransform.offsetMin;
            shrinkRate /= 2;
            if (shrinkRate <= 5)
            {
                _rectTransform.offsetMin = expansion == Expansion.Horizontal ? new Vector2(_rectTransform.offsetMin.x - shrinkRate * 2, 0) : new Vector2(0, _rectTransform.offsetMin.y - shrinkRate * 2);
                _rectTransform.offsetMax = -_rectTransform.offsetMin;
                adjustedWidthOnLimit = true;
                return;
            }
            widthLimitReached = false;
            return;
        }
        
        
        
        //shorten width slightly

        var offsetMin = _rectTransform.offsetMin;
        offsetMin =  expansion == Expansion.Horizontal ? new Vector2(offsetMin.x + shrinkRate, 0) : new Vector2(0, offsetMin.y + shrinkRate);
        _rectTransform.offsetMin = offsetMin;
        _rectTransform.offsetMax = -offsetMin;
        
        // get last non-whitespace character 
        
        char lastCharacter = _tmpText.text[^1];
        var characterCount = _tmpText.text.Length;
        
        while (char.IsWhiteSpace(lastCharacter))
        {
            lastCharacter = _tmpText.text[characterCount - 1];
            characterCount--;

            if (characterCount == 0) break;
        }

        char? lastVisibleCharacterInCurrentLine = null;
        
        //check if line-breaking is occurring
        int indexOfLastVisibleCharInCurrentLine = 0;
        
       
        for (int i = 0; i < _tmpText.textInfo.lineInfo.Length; i++)
        {
          _tmpText.textInfo.ClearLineInfo();
            if (previousText != _tmpText.text) return;
            var lineInfo = _tmpText.textInfo.lineInfo[i]; 
            if (lineInfo.lastVisibleCharacterIndex < indexOfLastVisibleCharInCurrentLine || widthLimitReached) continue;
            
            //Debug.Log("last visible character in current line: index: " + lineInfo.lastVisibleCharacterIndex + ", character: " + _tmpText.text[lineInfo.lastVisibleCharacterIndex]);
            
            
            
            if (lineInfo.lastVisibleCharacterIndex + 1 == characterCount )
            {
                widthLimitReached = lineInfo.lastVisibleCharacterIndex != characterCount - 1;
                if (widthLimitReached)
                {
                    continue;
                }
            } 
            if (lineInfo.lastVisibleCharacterIndex + 1 < characterCount) widthLimitReached = !char.IsWhiteSpace(_tmpText.text[lineInfo.lastVisibleCharacterIndex + 1]) && lineInfo.lastVisibleCharacterIndex + 1 < characterCount;
          //  if (widthLimitReached) Debug.Log("widthLimitReached = true at line 96: " + _tmpText.text[lineInfo.lastVisibleCharacterIndex + 1] + " != whitespace");
            indexOfLastVisibleCharInCurrentLine = lineInfo.lastVisibleCharacterIndex >= _tmpText.text.Length ? indexOfLastVisibleCharInCurrentLine : lineInfo.lastVisibleCharacterIndex;
        }
        
        

        widthLimitReached = widthLimitReached || _tmpText.text[indexOfLastVisibleCharInCurrentLine] != lastCharacter ||
                            _tmpText.rectTransform.rect.position.x == 0;

    }
}
