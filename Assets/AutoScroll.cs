using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform contentRect;
    private bool shouldScroll;
    
    [SerializeField, Tooltip("How fast the scroll happens. Higher values are faster.")]
    private float scrollSpeed = 10f;

    private Vector2 _oldSize;
    
    private enum ScrollDirection
    {
        Top,
        Bottom
    }
    
    [SerializeField] private ScrollDirection scrollDirection = ScrollDirection.Bottom; 

    private void OnEnable()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect); // Ensure initial layout
        ScrollToEndImmediate(); // Scroll to the bottom on initialization
        _oldSize = contentRect.sizeDelta;
    }
    
    private void OnValidate()
    {
        scrollRect ??= GetComponent<ScrollRect>();
        contentRect ??= scrollRect != null ? scrollRect.content : contentRect;

    }

    private float _previousScrollSensitivity = 0;

    private void Update()
    {
        if (scrollRect == null || contentRect == null) return;
        
        if (_oldSize != contentRect.sizeDelta)
        {
            shouldScroll = true;
            _oldSize = contentRect.sizeDelta;
            Canvas.ForceUpdateCanvases();
        }
        
        scrollSpeed = Mathf.Max(scrollSpeed, 0.1f); // Ensure scroll speed is not negative
        
        if (shouldScroll)
        {
            if (_previousScrollSensitivity == 0) _previousScrollSensitivity = scrollRect.scrollSensitivity;
            scrollRect.scrollSensitivity = 0;
            
             float targetVerticalPosition = scrollDirection == ScrollDirection.Bottom ? 0f : 1f;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                scrollRect.verticalNormalizedPosition,
                targetVerticalPosition,
                Time.deltaTime * scrollSpeed
            );

            // Stop scrolling if close enough to the target position
            if (Mathf.Abs(scrollRect.verticalNormalizedPosition - targetVerticalPosition) < 0.001f)
            {
                scrollRect.verticalNormalizedPosition = targetVerticalPosition;
                shouldScroll = false;
                scrollRect.scrollSensitivity = _previousScrollSensitivity;
                _previousScrollSensitivity = 0;
            }
        }
        
    }
    
    private void ScrollToEndImmediate()
    {
        if (scrollDirection == ScrollDirection.Bottom) scrollRect.verticalNormalizedPosition = 0f;
        else scrollRect.verticalNormalizedPosition = 1f;
    }
    
    public void ScrollNow()
    {
        shouldScroll = true;
    }
}