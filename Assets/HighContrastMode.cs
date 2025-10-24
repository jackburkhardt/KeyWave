using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

public class HighContrastMode : MonoBehaviour, IHighContrastHandler
{
    [SerializeField] [ReadOnly] private Graphic _graphic;

    [Label("Default Color")] [ReadOnly] [SerializeField]
    private Color _graphicColorHighContrastModeOff;

    [Label("High Contrast Mode Color")] [SerializeField]
    private Color _graphicColorHighContrastModeOn = Color.clear;

    [SerializeField] private bool _showHighContrastColorInEditorMode;

    private Color _highContrastTempColor = Color.clear;
    private Animator _animator;


    public void OnEnable()
    {
        _graphic ??= GetComponentInChildren<Graphic>();
        _animator ??= GetComponentInChildren<Animator>();
        if (!_graphic) return;
        _showHighContrastColorInEditorMode = false;
        if (GameManager.settings == null) return;
        if (GameManager.settings.HighContrastMode) OnHighContrastModeEnter();
        else OnHighContrastModeExit();

    }

    private void OnValidate()
    {
        //editor mode stuff to test the high contrast mode color whithout overriding default values
        _graphic ??= GetComponentInChildren<Graphic>();
        if (!_graphic) return;
        if (Application.isPlaying) return;

        if (_graphic.color == _graphicColorHighContrastModeOn)
            _graphic.color = _graphicColorHighContrastModeOff;

        if (_graphicColorHighContrastModeOn == Color.clear)
        {
            _graphicColorHighContrastModeOn = Color.Lerp(_graphic.color, Color.black, 0.65f);
            _graphicColorHighContrastModeOn.a = 1;
        }

        if (_showHighContrastColorInEditorMode)
        {
            if (_highContrastTempColor == Color.clear) _highContrastTempColor = _graphic.color;
            _graphic.color = _graphicColorHighContrastModeOn;
        }

        else
        {
            if (_highContrastTempColor != Color.clear)
            {
                _graphic.color = _highContrastTempColor;
                _highContrastTempColor = Color.clear;
            }

            _graphicColorHighContrastModeOff = _graphic.color;
        }
    }

    [Button("Enter High Contrast Mode")]
    public void OnHighContrastModeEnter()
    {
        if (_animator) _animator.enabled = false;
        _graphic.color = _graphicColorHighContrastModeOn;
        if (_animator) _animator.enabled = true;
    }

    [Button("Exit High Contrast Mode")]
    public void OnHighContrastModeExit()
    {
        if (_animator) _animator.enabled = false;
        _graphic.color = _graphicColorHighContrastModeOff;
        if (_animator) _animator.enabled = true;
    }
}
