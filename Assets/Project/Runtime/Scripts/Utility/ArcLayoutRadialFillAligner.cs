using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(ArcLayoutGroup))]
public class ArcLayoutRadialFillAligner : MonoBehaviour
{
    
    [SerializeField] Image image;
    [SerializeField] ArcLayoutGroup arcLayoutGroup;
    [SerializeField] RectTransform rect;
    public float angleOffset;
    public bool rotateChildren;

    public bool useMaterial;
    
    [ShowIf("useMaterial")]
    public string thicknessProperty = "_Thickness";
    
    private void Align()
    {
        var fillAngle = image.fillAmount * 360;
        var angle = fillAngle / 2;
        angle += angleOffset;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        if (rotateChildren)
        {
            foreach (Transform child in transform)
            {
                child.localRotation = Quaternion.Euler(0, 0, -angle);
            }
        }

        if (useMaterial)
        {
            var material = image.materialForRendering;
            if (material != null)
            {
                if (!material.HasFloat(thicknessProperty)) return;
                var radius = rect.rect.width / 2;
                material.SetFloat( thicknessProperty,  1 - (1 - arcLayoutGroup.Radius / radius) * 2 );
            }
            
        }
    }

    public void Update()
    {
        Align();
    }
}
