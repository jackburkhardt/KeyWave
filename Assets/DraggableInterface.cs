using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[ExecuteInEditMode]
public class DraggableInterface : MonoBehaviour, IDragHandler, IScrollHandler
{
   

    [SerializeField]
    private RectTransform _rectTransform;

    [SerializeField]
    private RectTransform _container;

    [Range(0.85f, 1.6f)]
    [SerializeField] private float _scale;

    Vector2 MinPosition
    {
        get
        {
            var containerRect = _container.rect;
            var rect = _rectTransform.rect;
            return new Vector2(rect.width - containerRect.width, rect.height - containerRect.height);
        }
    }
    
    Vector2 MaxPosition
    {
        get
        {
            var containerRect = _container.rect;
            var rect = _rectTransform.rect;
            return new Vector2(containerRect.width - rect.width, containerRect.height - rect.height);
        }
    }

    private void OnValidate()
    {
        _rectTransform ??= GetComponent<RectTransform>();
        _container ??= transform.parent.GetComponentInParent<RectTransform>();
    }

    private void Update()
    {
        if (_rectTransform == null) return;
        var pos = _rectTransform.anchoredPosition;
        _rectTransform.localScale = new Vector3(_scale, _scale, 1);
        
        var xDeltaLimit = (_rectTransform.rect.width * _scale - _container.rect.width)/2;
        var yDeltaLimit = (_rectTransform.rect.height * _scale - _container.rect.height)/2;
        
        pos.x = Mathf.Clamp(pos.x, -xDeltaLimit, xDeltaLimit);
        pos.y = Mathf.Clamp(pos.y, -yDeltaLimit, yDeltaLimit);
       
        _rectTransform.anchoredPosition = pos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_rectTransform == null) return;
        var pos = _rectTransform.anchoredPosition;
        pos += eventData.delta;
        _rectTransform.anchoredPosition = pos;
    }

    public void OnScroll(PointerEventData eventData)
    {
        _scale = Mathf.Clamp(_scale + eventData.scrollDelta.y * 0.03f, 0.85f, 1.6f);
    }
}
