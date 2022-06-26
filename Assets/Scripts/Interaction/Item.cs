using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour, IInteractable
{
    private Outline _outline;
    
    // action to be executed after the main interaction completes
    [SerializeField] private UnityEvent _postInteractAction;
    [SerializeField] private bool _popup;
    [SerializeField] [TextArea] private string _flavortext;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }

    public void Discard()
    {
        Destroy(this.gameObject);
    }

    public void Interact()
    {
        GameEvent.InteractionStart(this);
        if (!_popup)
        {
            StartCoroutine(DialogueDisplay.Instance.Run(_flavortext, this));
        }
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
