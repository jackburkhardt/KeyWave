using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnPointerEnterEvent : MonoBehaviour, IPointerEnterHandler
{
    public UnityEvent onPointerEnter;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter.Invoke();
    }
}
