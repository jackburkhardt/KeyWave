using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class RoundedImageWithCornerSync : Gilzoide.RoundedCorners.RoundedImage
{
    [HideIf("_syncingWithOtherRoundedImage")]
    [SerializeField] protected bool syncCorners = true;
    [HideIf("_syncingWithOtherRoundedImage")]
    [SerializeField] protected bool proportionalToRect = true;
    
    
    
    [ShowIf(EConditionOperator.And, "proportionalToRect", "syncCorners", "notSyncing")]
    [Range (0, 0.5f)]
    [SerializeField] private float _proportionalRadius = 0.5f;
    
    [ShowIf("syncCorners")]
    [SerializeField] protected bool syncWithOtherRoundedImage = false;
    
    [ShowIf("syncWithOtherRoundedImage")]
    [SerializeField] private RoundedImageWithCornerSync _otherRoundedImage;

    [ShowIf("_syncingWithOtherRoundedImage")] [SerializeField]
    private bool _syncAspectRatioFitter;
    
    private bool _syncingWithOtherRoundedImage => syncWithOtherRoundedImage && _otherRoundedImage != null;
    
    //for the ShowIf attribute
    private bool notSyncing => !_syncingWithOtherRoundedImage;
    
    private AspectRatioFitter aspectRatioFitter => _syncAspectRatioFitter ? GetComponent<AspectRatioFitter>() : null;
    
    private static Action<RoundedImageWithCornerSync> _onValidate;
    
    private new void OnEnable()
    {
        _onValidate += SyncedValidate;
    }
    
    private new void OnDisable()
    {
        _onValidate -= SyncedValidate;
    }
    
    private void SyncedValidate(RoundedImageWithCornerSync other)
    {
        if (notSyncing && other != _otherRoundedImage) return;
        OnValidate();
    }

    protected override void OnValidate()
    {
        if (syncCorners)
        {
            var radius = proportionalToRect
                ? _proportionalRadius * GetComponent<RectTransform>().rect.width
                : _bottomLeft.Radius;
            
            
            if (_syncingWithOtherRoundedImage)
            {

                radius = _otherRoundedImage.syncCorners && _otherRoundedImage.proportionalToRect
                    ? _otherRoundedImage._proportionalRadius * GetComponent<RectTransform>().rect.width
                    : _otherRoundedImage._bottomLeft.Radius;
                
                 if (aspectRatioFitter != null && _otherRoundedImage.GetComponent<AspectRatioFitter>() != null) 
                {
                    aspectRatioFitter.aspectMode = _otherRoundedImage.GetComponent<AspectRatioFitter>().aspectMode;
                    aspectRatioFitter.aspectRatio = _otherRoundedImage.GetComponent<AspectRatioFitter>().aspectRatio;
                }
            }
            
            _bottomLeft = _topLeft = _topRight = _bottomRight = new Gilzoide.RoundedCorners.RoundedCorner { Radius = radius, TriangleCount = _bottomLeft.TriangleCount };
        }
        
        base.OnValidate();
        
        if (!syncWithOtherRoundedImage) _onValidate?.Invoke(this);
        
        
    }
    
    protected override void OnRectTransformDimensionsChange() {
        base.OnRectTransformDimensionsChange();
        OnValidate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
