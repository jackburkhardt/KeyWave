using System;
using System.Collections.Generic;
using Assignments;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;


namespace Interaction
{
    /// <summary>
    /// Representation of an NPC which can be conversed with.
    /// </summary>
    public class CharacterPortrait : MonoBehaviour, IInteractable
    {
        private Outline _outline;
        private Character _character;
        public UnityEvent runOnInteract;
        
        private void Awake()
        {
            
        }

        private void Start()
        {
            _outline = GetComponent<Outline>();
        }

        public void Interact()
        {
            if (!GameManager.isControlEnabled) return;
            GameEvent.InteractionStart(this);
         //   GameEvent.AnyEvent();
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
}
