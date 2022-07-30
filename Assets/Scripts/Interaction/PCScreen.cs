using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Interaction
{
    /// <summary>
    /// Representation of in-scene PC screen.
    /// </summary>
    public class PCScreen : MonoBehaviour, IInteractable
    {
        private Outline _outline;
    
        // action to be executed after the main interaction completes
        [SerializeField] private UnityEvent _interactAction;
        [SerializeField] private UnityEvent _postInteractAction;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            GameEvent.OnPCClose += EndInteraction;
        }

        public void Discard()
        {
            Destroy(this.gameObject);
        }

        public void Interact()
        {
            GameEvent.InteractionStart(this);
            _interactAction?.Invoke();
        }

        public void EndInteraction()
        {
            PreviouslyInteractedWith = true;
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

        public bool PreviouslyInteractedWith { get; private set; }
    }
}
