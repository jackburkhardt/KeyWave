using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Representation of an NPC which can be conversed with.
/// </summary>
public class Character : MonoBehaviour, IInteractable
{
    private Outline _outline;
    private List<Assignment> _delegatedAssignments = new List<Assignment>();

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

    public bool TryRecieveAssignment(Assignment assignment)
    {
        throw new NotImplementedException();
    }
    
    public bool PreviouslyInteractedWith { get; private set;  }
}
