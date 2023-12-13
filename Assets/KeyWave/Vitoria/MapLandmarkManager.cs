using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapLandmarkManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        GameEvent.UIElementMouseHover(transform);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        GameEvent.UIElementMouseExit(transform);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        GameEvent.UIElementMouseClick(transform);
    }
  

}

    




