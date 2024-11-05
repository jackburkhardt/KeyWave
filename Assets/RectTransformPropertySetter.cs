using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformPropertySetter : MonoBehaviour
{
    public void SetWidth(float width)
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.rect.Set(rt.rect.x, rt.rect.y, width, rt.rect.height);
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }
    
    public void SetHeight(float height)
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.rect.Set(rt.rect.x, rt.rect.y, rt.rect.width, height);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }
    
   
}
