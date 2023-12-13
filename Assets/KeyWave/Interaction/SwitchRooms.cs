using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Interaction
{
    /// <summary>
    /// An object in the game space which can be clicked on to travel to other rooms.
    /// </summary>
    public class SwitchRooms : MonoBehaviour, IInteractable
    {
        private Outline _outline;
        [SerializeField] private Transform _destination;
        [SerializeField] private UnityEvent _postSwitchEvent;
        
        private void Awake()
        {
            _outline = GetComponent<Outline>();
        }

        public void Interact()
        {
            if (!GameManager.isControlEnabled) return;
            GameEvent.InteractionStart(this);
     //       GameManager.playerActor.RelocateToRoom(_destination); 
        }

        

        public void EndInteraction()
        {
            PreviouslyInteractedWith = true;
            _postSwitchEvent?.Invoke();
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
    
        public bool PreviouslyInteractedWith { get; private set;  }


        


    }
}
