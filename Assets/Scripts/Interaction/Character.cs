using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Representation of an NPC which can be conversed with.
/// </summary>
public class Character : MonoBehaviour, IInteractable
{
    private Outline _outline;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }

    public void Interact()
    {
        throw new System.NotImplementedException();
    }

    public void EndInteraction()
    {
        PreviouslyInteractedWith = true;
        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _outline.enabled = false;
    }
    
    public bool PreviouslyInteractedWith { get; private set;  }
}
