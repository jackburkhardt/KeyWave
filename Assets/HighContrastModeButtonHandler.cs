using System;
using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HighContrastModeButtonHandler : MonoBehaviour, IHighContrastHandler
{
    [SerializeField] [HideInInspector] private Button _button;

    [Header("Default Colors")]
    public ColorBlock defaultColorBlock;
    [Header("High Contrast Colors")]
    public ColorBlock highContrastModeColorBlock;

    public void OnValidate()
    {
        _button ??= GetComponent<Button>();
    }

    public void Start()
    {
        _button.colors = GameManager.settings.HighContrastMode ? highContrastModeColorBlock : defaultColorBlock;
    }


    public void OnHighContrastModeEnter()
    {
        _button.colors = highContrastModeColorBlock;
    }

    public void OnHighContrastModeExit()
    {
        _button.colors = defaultColorBlock;
    }
}
