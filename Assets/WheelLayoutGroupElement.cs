using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class WheelLayoutGroupElement : MonoBehaviour
{
    public bool overrideElement;
    [Range(0, 2)] [SerializeField] private float _slide;
    public float Radius => overrideElement ? _slide : 1;

    private float _currentRadius;
    
    private WheelLayoutGroup _wheelLayoutGroup;
    private void Update()
    {
        _wheelLayoutGroup ??= GetComponentInParent<WheelLayoutGroup>();
        
        if (_currentRadius != Radius)
        {
            _currentRadius = Radius;
            if (_wheelLayoutGroup != null)
            {
                _wheelLayoutGroup.ArrangeElements();
            }
        }
    }
}
