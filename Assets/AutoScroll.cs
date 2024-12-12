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

    private void OnEnable()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect); // Ensure initial layout
        ScrollToBottomImmediate(); // Scroll to the bottom on initialization
        _oldSize = contentRect.sizeDelta;
    }
    
    private void OnValidate()
    {
        scrollRect ??= GetComponent<ScrollRect>();
        contentRect ??= scrollRect != null ? scrollRect.content : contentRect;

    }

    private void Update()
    {
        if (scrollRect == null || contentRect == null) return;
        
        if (_oldSize != contentRect.sizeDelta)
        {
            Debug.Log("Content size changed, scrolling to bottom.");
            shouldScroll = true;
            _oldSize = contentRect.sizeDelta;
            Canvas.ForceUpdateCanvases();
        }
        
        scrollSpeed = Mathf.Max(scrollSpeed, 0.1f); // Ensure scroll speed is not negative
        
        if (shouldScroll)
        {
            float targetVerticalPosition = 0f; // Bottom is 0 in ScrollRect
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
            }
        }
        
    }
    
    private void ScrollToBottomImmediate()
    {
        scrollRect.verticalNormalizedPosition = 0f;
    }
}