using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Item : MonoBehaviour, IInteractable
{
    private Outline _outline;
    
    // action to be executed after the main interaction completes
    [SerializeField] private UnityEvent _postInteractAction;
    [SerializeField] private bool _popup;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }

    public void Interact()
    {
        GameEvent.InteractionStart(this);
    }

    public void EndInteraction()
    {
        _postInteractAction?.Invoke();
        GameEvent.InteractionEnd(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _outline.enabled = false;
    }
}
