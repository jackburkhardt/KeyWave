using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwapOnSmartWatchAppOpen : MonoBehaviour, IHighContrastHandler
{

    private ColorSwapData _currentColorData = null;
    
    private void OnEnable() 
    {
        SmartWatchPanel.onAppOpen += SetColor;
    }
    
    private void OnDisable() 
    {
        SmartWatchPanel.onAppOpen -= SetColor;
    }
    
    public void OnGameSceneEnd() 
    {
        SetColor(_defaultColor);
     
    }

    public List<ColorSwapData> colors;
    private ColorSwapData _defaultColor => colors[0];
    
    [Range(0, 1)]
    public float transitionAmount = 0.5f;
    
    public float transitionDuration = 0.15f;
    
    [Serializable]
    public class ColorSwapData
    {
        [SmartWatchAppPopup]
        public string name;
        public Color color;
        public Color highContrastModeColor;
    }
    
    public void SetColor(SmartWatchAppPanel appPanel)
    {
        if (appPanel.Name == "Home")
        {
            SetColor(_defaultColor);
        }
        else
        {
            foreach (var color in colors)
            {
                if (color.name == appPanel.Name)
                {
                    SetColor(color);
                    return;
                }
            }
        }
    }
    
    public void SetColor(string name)
    {
        foreach (var color in colors)
        {
            if (color.name == name)
            {
                SetColor(color);
                return;
            }
        }
    }
    
    public void SetColor(ColorSwapData colorData)
    {

        colorData ??= _defaultColor;
        
        var graphic = GetComponent<Graphic>();
        if (graphic == null) return;

        var newColor = GameManager.settings.HighContrastMode ? colorData.highContrastModeColor : colorData.color;
        if (newColor == graphic.color) return;
        
        if (!Application.isPlaying)
        {
            graphic.color = Color.Lerp(_defaultColor.color, newColor, transitionAmount);
        }
        else
        {
            newColor = Color.Lerp(_defaultColor.color, newColor, transitionAmount);
            DOTween.To(() => graphic.color, x => graphic.color = x, newColor, transitionDuration);
        }

        _currentColorData = colorData;
    }
    
    [Button("Revert to Default")]
    public void ResetColor()
    {
        SetColor(_defaultColor);
    }

    private void OnValidate()
    {
        if (GetComponent<Graphic>() == null) return;
        if (colors.Count == 0)
        {
            colors.Add(new ColorSwapData
            {
                name = "Default",
                color = GetComponent<Graphic>().color
            });
        }
    }
    
    [Button]
    private void SetDefaultColor()
    {
        if (colors.Find(p => p.name == "PreviousDefault") != null)
        {
            colors.Remove(colors.Find(p => p.name == "PreviousDefault"));
        }
        colors.Add(new ColorSwapData
        {
            name = "PreviousDefault",
            color = colors[0].color
        });
        colors[0].color = GetComponent<Graphic>().color;
        
        
    }

    public void OnHighContrastModeEnter()
    {
       SetColor(_currentColorData);
    }

    public void OnHighContrastModeExit()
    {
        SetColor(_currentColorData);
    }
}
