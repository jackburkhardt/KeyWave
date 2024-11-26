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
    
    [ShowIf("syncWithOtherRoundedImage")]
    [SerializeField] private RoundedImageWithCornerSync _otherRoundedImage;

    [ShowIf("_syncingWithOtherRoundedImage")] [SerializeField]
    private bool _syncAspectRatioFitter;
    
    private bool _syncingWithOtherRoundedImage => syncWithOtherRoundedImage && _otherRoundedImage != null;
    
    //for the ShowIf attribute
    private bool notSyncing => !_syncingWithOtherRoundedImage;
    
    [SerializeField] private AspectRatioFitter _aspectRatioFitter;
    private AspectRatioFitter aspectRatioFitter => _aspectRatioFitter != null ? _aspectRatioFitter : GetComponent<AspectRatioFitter>();
    

    protected void Update()
    {
        if (syncCorners)
        {
            var radius = proportionalToRect
                ? proportionalRadius * GetComponent<RectTransform>().rect.width
                : _bottomLeft.Radius;
            
            
            if (_syncingWithOtherRoundedImage)
            {

                radius = _otherRoundedImage.syncCorners && _otherRoundedImage.proportionalToRect
                    ? _otherRoundedImage.proportionalRadius * GetComponent<RectTransform>().rect.width
                    : _otherRoundedImage._bottomLeft.Radius;
                
                if (aspectRatioFitter != null && _otherRoundedImage.aspectRatioFitter != null) 
                {
                    //Debug.Log("Syncing aspect ratio fitter");
                    aspectRatioFitter.aspectMode = _otherRoundedImage.aspectRatioFitter.aspectMode;
                    aspectRatioFitter.aspectRatio = _otherRoundedImage.aspectRatioFitter.aspectRatio;
                }
            }
            
            _bottomLeft = _topLeft = _topRight = _bottomRight = new Gilzoide.RoundedCorners.RoundedCorner { Radius = radius, TriangleCount = _bottomLeft.TriangleCount };
        }
    }
}
