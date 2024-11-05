using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(AspectRatioFitter))]
[RequireComponent(typeof(RoundedImageWithCornerSync))]
public class DynamicShape : MonoBehaviour
{
    //this script uses a collection of scripts from the project to dynamically alter UI elements
    
    protected float size = 1345;
    [Range (0, 1)]
    protected float scale = 1;
    [Range (0, 2)]
    protected float aspectRatio = 1;
    [Range (0, 0.5f)]
    protected float cornerRadius = 0.5f;
    
    [Serializable]
    public class CustomShape
    {
        public string name;
        public float size;
        [Range (0, 1)]
        public float scale;
        [Range (0, 2)]
        public float aspectRatio;
        [Range (0, 0.5f)]
        public float cornerRadius;
        private DynamicShape dynamicShape;

        public CustomShape(DynamicShape dynamicShape)
        {
            this.dynamicShape = dynamicShape;
            name = $"New Shape({dynamicShape.shapePresets.Count})";
            size = dynamicShape.size;
            scale = dynamicShape.scale;
            aspectRatio = dynamicShape.aspectRatio;
            cornerRadius = dynamicShape.cornerRadius;
        }
    }


    [Dropdown("shapePresetDropdown")]
    public CustomShape currentShape;
    private CustomShape _previousCustomShape;
    
    [HideInInspector] public int shapePresetIndex = -1;
    
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
    
    
    
    public bool lerp = false;

    [ShowIf("lerp")]
    [Dropdown("shapePresetDropdown")]
    public CustomShape nextShape;
    private CustomShape _previousNextShape;
    
    [ShowIf("lerp")]
    [Range (0, 1)]
    public float lerpAmount = 0;
    
    [HideInInspector] public int nextShapePresetIndex = -1;



    public void OnValidate()
    {
        if (_previousCustomShape != currentShape)
        {
            _previousCustomShape = currentShape;
            shapePresetIndex = shapePresets.IndexOf(currentShape);
        }
        
        if (shapePresetIndex >= 0) currentShape = shapePresets[shapePresetIndex];
        
        if (_previousNextShape != nextShape)
        {
            _previousNextShape = nextShape;
            nextShapePresetIndex = shapePresets.IndexOf(nextShape);
        }
        
        if (nextShapePresetIndex >= 0) nextShape = shapePresets[nextShapePresetIndex];
        
        
        if (shapePresets.Count == 0)
        {
            shapePresets.Add(new CustomShape(this));
        }

        if (lerp)
        {
            size = Mathf.Lerp(currentShape.size, nextShape.size, lerpAmount);
            scale = Mathf.Lerp(currentShape.scale, nextShape.scale, lerpAmount);
            aspectRatio = Mathf.Lerp(currentShape.aspectRatio, nextShape.aspectRatio, lerpAmount);
            cornerRadius = Mathf.Lerp(currentShape.cornerRadius, nextShape.cornerRadius, lerpAmount);
            
            
            
        }

        else
        {
            size = currentShape.size;
            scale = currentShape.scale;
            aspectRatio = currentShape.aspectRatio;
            cornerRadius = currentShape.cornerRadius;
            
        }
        
        
        
        RectTransform rt = GetComponent<RectTransform>();
        rt.rect.Set(rt.rect.x, rt.rect.y, rt.rect.width, scale * size);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, scale * size);
        GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
        GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        if (GetComponent<RoundedImageWithCornerSync>() != null)
            GetComponent<RoundedImageWithCornerSync>().SetProportionalRadius(cornerRadius);

        
    }

    public void SetCurrentShapeToNext()
    {
        currentShape = nextShape;
        shapePresetIndex = nextShapePresetIndex;
        lerpAmount = 0;
    }
}
