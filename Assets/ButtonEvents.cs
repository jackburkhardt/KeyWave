using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]

public class ButtonEvents : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent OnClick, OnClicked, OnHover, OnHoverEnd;
    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnClicked.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hover end");
        OnHoverEnd.Invoke();
    }
}
