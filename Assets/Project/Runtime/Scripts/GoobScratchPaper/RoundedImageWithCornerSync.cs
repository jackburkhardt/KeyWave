using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class RoundedImageWithCornerSync : Gilzoide.RoundedCorners.RoundedImage
{
    [HideIf("_syncingWithOtherRoundedImage")]
    [SerializeField] protected bool syncCorners = true;
    [HideIf("_syncingWithOtherRoundedImage")]
    [SerializeField] protected bool proportionalToRect = true;
    
    [ShowIf(EConditionOperator.And, "proportionalToRect", "syncCorners", "notSyncing")]
    [Range (0, 0.5f)]
    public float proportionalRadius = 0.5f;

    public void SetProportionalRadius(float value)
    {
        proportionalRadius = value;
    }
   
    [ShowIf("syncCorners")]
    [SerializeField] protected bool syncWithOtherRoundedImage = false;
    

    
    
   
    
    [SerializeField] private AspectRatioFitter _aspectRatioFitter;
    private AspectRatioFitter aspectRatioFitter => _aspectRatioFitter != null ? _aspectRatioFitter : GetComponent<AspectRatioFitter>();
    
    protected override void Awake()
    {
       base.Awake();
       SetVerticesDirty();
       StartCoroutine(DelayedReset());
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        SetVerticesDirty();
        StartCoroutine(DelayedReset());
    }
    IEnumerator DelayedReset()
    {
        yield return new WaitForEndOfFrame();
        SetVerticesDirty();
    }
  

    protected void Update()
    {
        if (syncCorners)
        {
            var radius = proportionalToRect
                ? proportionalRadius * GetComponent<RectTransform>().rect.width
                : _bottomLeft.Radius;
            
            _bottomLeft = _topLeft = _topRight = _bottomRight = new Gilzoide.RoundedCorners.RoundedCorner { Radius = radius, TriangleCount = _bottomLeft.TriangleCount };
        }
        
    }

    protected override void OnRectTransformDimensionsChange()
    {
        SetVerticesDirty();
        base.OnRectTransformDimensionsChange();
    }
    
    [Button]
    public void RecalculateCorners()
    {
        
        
    }
}
