using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CircularResponseMenuTextContainer : MonoBehaviour
{
    [SerializeField] private Text _text;
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private int lineCount;
    [SerializeField] private char lastVisibleCharacter;
    [SerializeField] private float _minWidth;
    
    
    
    // Start is called before the first frame update
    void Awake()
    {
        _minWidth = _rectTransform.offsetMin.x;
    }

    private void OnEnable()
    {
        _rectTransform.offsetMin = new Vector2(_minWidth, 0);
    }

    private void OnDisable()
    {
        _rectTransform.offsetMin = new Vector2(_minWidth, 0);
    }

    // Update is called once per frame
    void Update()
    {
       bool isOverflowing = false;

       var textInfo = _tmpText.textInfo;
       lineCount = textInfo.lineInfo.Length;
       int currentLineBreakIndex = 0;

       // get last non-whitespace character
       
       char lastCharacter = _tmpText.text[^1];
       var characterCount = textInfo.characterCount;
       while (char.IsWhiteSpace(lastCharacter))
       {
           lastCharacter = _tmpText.text[characterCount - 1];
            characterCount--;

            if (characterCount == 0) break;

       }
       
       //check if linebreaking is occuring mid-word
       
       for (int i = 0; i < lineCount; i++)
       { 
          var _lineInfo = textInfo.lineInfo[i]; 
          var lineBreakIndex = _lineInfo.lastVisibleCharacterIndex;
           
           if (lineBreakIndex <= currentLineBreakIndex || isOverflowing) continue;
           
           currentLineBreakIndex = lineBreakIndex;

           var lastCharacterInLine = textInfo.characterInfo[lineBreakIndex].character;
           lastVisibleCharacter = lastCharacterInLine;
           if (textInfo.characterInfo.Length <= lineBreakIndex + 1)
           {
               isOverflowing = lastCharacterInLine != lastCharacter;
               return;
           } 
          
           var breakingCharacter = (textInfo.characterInfo[lineBreakIndex + 1].character);
           isOverflowing = !char.IsWhiteSpace(breakingCharacter);
          
       }
       
       if (lastVisibleCharacter != lastCharacter)
       {
           isOverflowing = true;
       }

       if (isOverflowing)
       {
           var _rT_Left = _rectTransform.offsetMin.x;
           
       }

       

       /*

       if (_tmpText != null && _tmpText.IsOverflowingVertical())
       {
           var _rT_Left = _rectTransform.offsetMin.x;
           _rectTransform.offsetMin = new Vector2(_rectTransform.offsetMin.x - 5, 0);
       }
        */




    }
}
