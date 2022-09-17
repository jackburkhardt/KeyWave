using System;
using System.Collections.Generic;
using Assignments;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Interaction
{
    /// <summary>
    /// Representation of an NPC which can be conversed with.
    /// </summary>
    public class CharacterPortrait : MonoBehaviour, IInteractable
    {
        private Outline _outline;
        private Character _character;
        
        private void Awake()
        {
            _outline = GetComponent<Outline>();
        }

        public void Interact()
        {
            // todo: should start dialogue (but where?)
            throw new NotImplementedException();
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
