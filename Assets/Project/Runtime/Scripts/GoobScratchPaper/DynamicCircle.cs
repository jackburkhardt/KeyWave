using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(AspectRatioFitter))]
[RequireComponent(typeof(RoundedImageWithCornerSync))]
public class DynamicCircle : MonoBehaviour
{
    private static Action<DynamicCircle> _onValidate;
    //this script uses a collection of scripts from the project to dynamically alter UI elements

    private RectTransform _rectTransform;
    private AspectRatioFitter _aspectRatioFitter;
    private RoundedImageWithCornerSync _roundedImageWithCornerSync;
    

    [SerializeField] private float _size;
    [SerializeField] private Vector2 _rectPosition;
    
    
    

    private float Scale
    {
        set
        {
            //_rectTransform.rect.Set( _rectTransform.rect.x,  _rectTransform.rect.y,  _rectTransform.rect.width, value * _size);
            _rectTransform.sizeDelta = new Vector2( _rectTransform.sizeDelta.x, value * _size);
        }
    }

    private float AspectRatio {
        set => _aspectRatioFitter.aspectRatio = value;
    }
    
    private float CornerRadius
    {
        set => _roundedImageWithCornerSync.SetProportionalRadius(value);
    }
    
    private Vector2 RectOffset
    {
        set
        {
            //_rectTransform.anchoredPosition = _rectPosition + value;
           // _rectTransform.rect.Set(_rectPosition.x + value.x, _rectPosition.y + value.y, _rectTransform.rect.width, _rectTransform.rect.height);
            transform.localPosition = _rectPosition + value;
            
        }
    }
    
    
    [Serializable]
    public class CustomShape
    {
        public string name;
        [Range (0, 2)]
        public float scale;
        [Range (0, 2)]
        public float aspectRatio;
        [Range (0, 0.5f)]
        public float cornerRadius;
        public Vector2 rectOffset;
        [ReadOnly] public string animatorTrigger;
        public UnityEvent<float> onShapeChangeLerp;
        //public UnityEvent<float> onShapeRevertLerp;
    }


    [Dropdown("shapePresetDropdown")]
    public CustomShape currentShape;
    private CustomShape _previousCustomShape;
    
    [HideInInspector] public int shapePresetIndex = -1;
    
    
    
    public bool lerp = false;

    [ShowIf("lerp")]
    [Dropdown("shapePresetDropdown")]
    public CustomShape nextShape;
    private CustomShape _previousNextShape;
    
    [ShowIf("lerp")]
    [Range (0, 1)]
    public float lerpAmount = 0;
    
    [HideInInspector] public int nextShapePresetIndex = -1;
    
    private DropdownList<CustomShape> shapePresetDropdown
    {
        get
        {
            var dropdown = new DropdownList<CustomShape>();
        
            foreach (var shape in shapePresets)
            {
                dropdown.Add(shape.name, shape);
            }
            
            OnValidate();

            return dropdown;
        }
    }
    

    public List<CustomShape> shapePresets = new List<CustomShape>();

    private int _shapePresetCount = 0;


    public void Update()
    {
        OnValidate();
    }

    public void OnValidate()
    {
        if (!this.enabled) return;
        
        _rectTransform ??= GetComponent<RectTransform>();
        _aspectRatioFitter ??= GetComponent<AspectRatioFitter>();
        _roundedImageWithCornerSync ??= GetComponent<RoundedImageWithCornerSync>();
        
        if (shapePresets.Count == 0)
        {
            if (_size == 0 &&  _rectTransform.sizeDelta.y != 0)
            {
                _size =  _rectTransform.sizeDelta.y;
            }
        
            if (_rectPosition == Vector2.zero &&  _rectTransform.anchoredPosition != Vector2.zero)
            {
                _rectPosition =  _rectTransform.anchoredPosition;
            }
            
            shapePresets.Add(new CustomShape());
            shapePresets[0].name = "New Shape";
            shapePresets[0].scale = 1;
            shapePresets[0].aspectRatio = 1;
            shapePresets[0].cornerRadius = 0.5f;
            currentShape = shapePresets[0];
        }
        
        if (_previousCustomShape != currentShape)
        {
            _previousCustomShape = currentShape;
            shapePresetIndex = shapePresets.IndexOf(currentShape);
        }
        
        if (shapePresetIndex >= 0) currentShape = shapePresets[shapePresetIndex % shapePresets.Count];
        
        if (_previousNextShape != nextShape)
        {
            _previousNextShape = nextShape;
            nextShapePresetIndex = shapePresets.IndexOf(nextShape);
        }
        
        if (nextShapePresetIndex >= 0) nextShape = shapePresets[nextShapePresetIndex % shapePresets.Count];
        
        
        
        if (lerp)
        {
            Scale = Mathf.Lerp(currentShape.scale, nextShape.scale, lerpAmount);
            AspectRatio = Mathf.Lerp(currentShape.aspectRatio, nextShape.aspectRatio, lerpAmount);
            CornerRadius = Mathf.Lerp(currentShape.cornerRadius, nextShape.cornerRadius, lerpAmount);
            RectOffset = Vector2.Lerp(currentShape.rectOffset, nextShape.rectOffset, lerpAmount);
            
        }

        else
        {
            Scale = currentShape.scale;
            AspectRatio = currentShape.aspectRatio;
            CornerRadius = currentShape.cornerRadius;
            RectOffset = currentShape.rectOffset;
            
        }
        
        _aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;

       
    }

    public void SetCurrentShapeToNext()
    {
        
        currentShape = nextShape;
        shapePresetIndex = nextShapePresetIndex;
        lerpAmount = 0;
    }
    
}

