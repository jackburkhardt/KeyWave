using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PixelCrushers;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using Project.Runtime.Scripts.AssetLoading;
using UnityEngine.AddressableAssets;


public class TextImageFormatter : MonoBehaviour
{
    

    [SerializeField] private UITextField _sourceText;
    
    [SerializeField] private UITextField _textTemplate;
    [SerializeField] private RectTransform _textTemplateHolder;
    
    [SerializeField] private Image _imageTemplate;
    [SerializeField] private RectTransform _imageTemplateHolder;
    
    [ReadOnly] private string imagePlaceholder = "[img($addressablePath)]";
    
 
    
    
    private enum TextTemplateType
    {
        Default,
        LayoutGroup
    }
    
    
    
    

    private void OnValidate()
    {


       
    }

    private void Awake()
    {
        
        _textTemplate.gameObject.SetActive(false);
        _textTemplateHolder.gameObject.SetActive(false);
        _imageTemplate.gameObject.SetActive(false);
        _imageTemplateHolder.gameObject.SetActive(false);
    }
    
    private string _previousText;

    private void Update()
    {
        if (!Application.isPlaying) return;
        
     
        
        if (_sourceText == null) return;
        
        if (_sourceText.text == _previousText) return;
        
        _previousText = _sourceText.text;
        
        SetFormattedText(_sourceText.text);
    }

    private void SetFormattedText(string text)
    {
        DeleteInstantiatedChildren();
        var pattern = @"(\[img\(.*?\)\])";
        var result = Regex.Split(text, pattern);
        
        GameObject currentImageContainer = null;
        GameObject currentTextContainer = null;
        
        foreach (var part in result)
        {
            Debug.Log(part);
            
            if (Regex.IsMatch(part, pattern))
            {
                currentTextContainer = null;
                var imagePath = part.Replace("[img(", "").Replace(")]", "");

                if (currentImageContainer == null)
                {
                    currentImageContainer = Instantiate(_imageTemplateHolder.gameObject, transform);
                    currentImageContainer.SetActive(true);
                }
                
                var image = Instantiate(_imageTemplate.gameObject, currentImageContainer.transform).GetComponent<Image>();
                image.gameObject.SetActive(true);
                
                AddressableLoader.RequestLoad<Sprite>(imagePath, sprite =>
                {
                    image.sprite = sprite;
                });
                
                
            }
            else
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                
                currentImageContainer = null;
                
                if (currentTextContainer == null)
                {
                    currentTextContainer = Instantiate(_textTemplateHolder.gameObject, transform);
                    currentTextContainer.SetActive(true);
                }
                
                var textObject = Instantiate(_textTemplate.gameObject, currentTextContainer.transform);
                textObject.SetActive(true);
                var textType = _textTemplate.textMeshProUGUI != null ? typeof(TextMeshProUGUI) : typeof(Text);
                
                if (_textTemplate.textMeshProUGUI != null)
                {
                    textObject.GetComponent<TextMeshProUGUI>().text = part;
                }
                if (_textTemplate.uiText != null)
                {
                    textObject.GetComponent<Text>().text = part;
                }
            }
        }
        
    }
    

    private void DeleteInstantiatedChildren()
    {
        foreach (var t in GetComponentsInChildren<Transform>().ToList())
        {
            if (t == transform) continue;
            if (t == _textTemplateHolder.transform) continue;
            if (t == _imageTemplateHolder) continue;
            if (t == _imageTemplate.transform) continue;
            if (t == _textTemplate.gameObject.transform) continue;
            if (t == null) continue;
            
            Destroy(t.gameObject);
        }
    }
    
}
