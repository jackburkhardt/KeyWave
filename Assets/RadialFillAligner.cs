using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class RadialFillAligner : MonoBehaviour
{
    
    Image image;
    public bool rotateChildren;

    public void OnValidate()
    {
        image ??= GetComponent<Image>();
    }
    
    private void Align()
    {
        var fillAngle = image.fillAmount * 360;
        var angle = fillAngle / 2;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        if (rotateChildren)
        {
            foreach (Transform child in transform)
            {
                child.localRotation = Quaternion.Euler(0, 0, -angle);
            }
        }
    }

    public void Update()
    {
        Align();
    }
}
