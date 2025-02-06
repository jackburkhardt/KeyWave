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
    
    [ReadOnly] [SerializeField] private string imagePlaceholder = "[img($addressablePath)]";
    

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
        
        var imagePattern = @"img\(([A-Za-z0-9_\/]+)\)";
        var matches = Regex.Matches(text, imagePattern);

        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count > 0)
            {
                var imagePath = match.Groups[0].Value;
                text = text.Replace(match.Value, "");
                
                
                var imageContainer = Instantiate(_imageTemplateHolder.gameObject, transform);
                imageContainer.SetActive(true);

                var image = Instantiate(_imageTemplate.gameObject, imageContainer.transform)
                    .GetComponent<Image>();
                image.gameObject.SetActive(true);

                AddressableLoader.RequestLoad<Sprite>(imagePath, sprite => { image.sprite = sprite; });
            }
        }
        
        var textContainer = Instantiate(_textTemplateHolder.gameObject, transform);
        textContainer.SetActive(true);

        var textObject = Instantiate(_textTemplate.gameObject, textContainer.transform);
        textObject.SetActive(true);
                
        if (_textTemplate.textMeshProUGUI != null)
        {
            textObject.GetComponent<TextMeshProUGUI>().text = text;
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
