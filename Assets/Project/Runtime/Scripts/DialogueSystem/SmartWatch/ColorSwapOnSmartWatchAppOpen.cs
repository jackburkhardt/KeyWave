using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwapOnSmartWatchAppOpen : MonoBehaviour
{
    private void OnEnable() 
    {
        SmartWatchPanel.onAppOpen += SetColor;
    }
    
    private void OnDisable() 
    {
        SmartWatchPanel.onAppOpen -= SetColor;
    }
    
    private void OnGameSceneEnd() 
    {
        SetColor(_defaultColor);
     
    }

    public List<ColorSwapData> colors;
    private Color _defaultColor => colors[0].color;
    
    [Range(0, 1)]
    public float transitionAmount = 0.5f;
    
    public float transitionDuration = 0.15f;
    
    [Serializable]
    public class ColorSwapData
    {
        [SmartWatchAppPopup]
        public string name;
        public Color color;
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
                    SetColor(color.color);
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
                SetColor(color.color);
                return;
            }
        }
    }
    
    public void SetColor(Color color)
    {
        var graphic = GetComponent<Graphic>();
        if (graphic == null) return;
        if (color == graphic.color) return;
        if (!Application.isPlaying)
        {
            graphic.color = Color.Lerp(_defaultColor, color, transitionAmount);
        }
        else
        {
            var newColor = Color.Lerp(_defaultColor, color, transitionAmount);
            DOTween.To(() => graphic.color, x => graphic.color = x, newColor, transitionDuration);
        }
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
    
}
