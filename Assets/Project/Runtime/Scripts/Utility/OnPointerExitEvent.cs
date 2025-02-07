using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnPointerExitEvent : MonoBehaviour, IPointerExitHandler
{
    public UnityEvent onPointerExitEvent;
    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExitEvent.Invoke();
    }
}