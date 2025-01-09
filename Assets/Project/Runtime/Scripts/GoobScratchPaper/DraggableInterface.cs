using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[ExecuteInEditMode]
public class DraggableInterface : MonoBehaviour, IDragHandler, IScrollHandler
{
   

    [SerializeField]
    private RectTransform _rectTransform;

    [SerializeField]
    private RectTransform _container;

    [Range(scaleMin,scaleMax)]
    [SerializeField] private float _scale;
    private Vector2 _scaleAsVector;

    private const float scaleMin = 0.95f;
    private const float scaleMax = 1.6f;

    public UnityEvent onDrag;
    public UnityEvent onScroll;
    
    private float Scale
    {
        get => _scale;
        set => _scale = Mathf.Clamp(value, scaleMin, scaleMax);
    }
    
    private Vector2 NearestValidPosition
    {
        get
        {
            var pos = _rectTransform.anchoredPosition;
        
            var xDeltaLimit = (_rectTransform.rect.width * Scale - _container.rect.width)/2;
            var yDeltaLimit = (_rectTransform.rect.height * Scale - _container.rect.height)/2;
        
            pos.x = Mathf.Clamp(pos.x, -xDeltaLimit, xDeltaLimit);
            pos.y = Mathf.Clamp(pos.y, -yDeltaLimit, yDeltaLimit);
            return pos;
        }
        
    }
    
    private Vector2 _lastZoomPosition;
    private Vector2 _currentWorldPosition;


    private bool ignoreDrag = false;

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
        _rectTransform.localScale = new Vector3(Scale, Scale, 1);
        _rectTransform.anchoredPosition = NearestValidPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ignoreDrag) return;
        if (_rectTransform == null) return;
        var pos = _rectTransform.anchoredPosition;
        pos += eventData.delta;
        _rectTransform.anchoredPosition = pos;
        onDrag?.Invoke();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (ignoreDrag) return;
        Scale += eventData.scrollDelta.y * 0.03f;
        onScroll?.Invoke();
    }
    
    public void ZoomInOnPosition(Vector3 position)
    {
        float targetScale = Math.Max(Scale, 1.3f);
        var newPosition = -position * targetScale;
        
        ignoreDrag = true;
        DOTween.To(() => transform.localPosition, x => transform.localPosition = x, newPosition, 0.5f);
        DOTween.To(() => Scale, x => Scale = x, targetScale, 0.5f).onComplete += () => ignoreDrag = false;;
    }
    
    public void ZoomInOnPosition(Transform t)
    {
        ZoomInOnPosition(t.localPosition);
    }
}
