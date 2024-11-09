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
        Validate();
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
    
    private AspectRatioFitter aspectRatioFitter => _syncAspectRatioFitter ? GetComponent<AspectRatioFitter>() : null;
    
    private static Action<RoundedImageWithCornerSync> _Validate;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        Validate();
        _Validate += SyncedValidate;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        Validate();
        _Validate -= SyncedValidate;
    }
    
    private void SyncedValidate(RoundedImageWithCornerSync other)
    {
        if (notSyncing && other != _otherRoundedImage) return;
        Validate();
    }

    protected void Validate()
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
                
                if (aspectRatioFitter != null && _otherRoundedImage.GetComponent<AspectRatioFitter>() != null) 
                {
                    aspectRatioFitter.aspectMode = _otherRoundedImage.GetComponent<AspectRatioFitter>().aspectMode;
                    aspectRatioFitter.aspectRatio = _otherRoundedImage.GetComponent<AspectRatioFitter>().aspectRatio;
                }
            }
            
            _bottomLeft = _topLeft = _topRight = _bottomRight = new Gilzoide.RoundedCorners.RoundedCorner { Radius = radius, TriangleCount = _bottomLeft.TriangleCount };
        }

        //base.OnValidate();
        
        if (!syncWithOtherRoundedImage) _Validate?.Invoke(this);
        
        // AssetDatabase.SaveAssets();
    }
    
    #if UNITY_EDITOR

    protected override void OnValidate()
    {
        Validate();
    }


    protected override void OnRectTransformDimensionsChange() {
        base.OnRectTransformDimensionsChange();
       // Validate();
    }
    
    #endif

    // Update is called once per frame
    void Update()
    {
        Validate();
    }
}
