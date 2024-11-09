using System;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using UnityEngine;
using UnityEngine.Events;

public class ButtonTextSwitcher : MonoBehaviour
{
    private int _currentIndex;
    public string defaultText;
    
    public UITextField textField;
    // Start is called before the first frame update
    [Serializable]
    public class ButtenTextSwitcherEntry
    {
        public string text;
        public UnityEvent onClick;
    }
    
    public List<ButtenTextSwitcherEntry> entries;

    public void OnValidate()
    {
        textField.text = defaultText.Contains("{0}") ? defaultText.Replace("{0}", entries[_currentIndex].text) : entries[_currentIndex].text;
    }
    
    [Button]
    public void Next()
    {
        
        _currentIndex++;
        if (_currentIndex >= entries.Count)
        {
            _currentIndex = 0;
        }
        textField.text = defaultText.Contains("{0}") ? defaultText.Replace("{0}", entries[_currentIndex].text) : entries[_currentIndex].text;
        
        if (Application.isPlaying)
        {
            entries[_currentIndex].onClick.Invoke();
        }
    }
    
    private void Awake()
    {
        _currentIndex = 0;
        textField.text = defaultText.Contains("{0}") ? defaultText.Replace("{0}", entries[_currentIndex].text) : entries[_currentIndex].text;
    }
    
}
