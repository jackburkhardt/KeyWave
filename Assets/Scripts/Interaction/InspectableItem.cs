using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Interaction
{
    /// <summary>
    /// An item in the game space which can be clicked on for internal dialogue.
    /// </summary>
    public class InspectableItem : MonoBehaviour, IInteractable
    {
        private Outline _outline;
    
        // action to be executed after the main interaction completes
        [SerializeField] private UnityEvent _postInteractAction;
        [SerializeField] [TextArea] private string _internalDialogue;

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
            StartCoroutine(Dialogue.Instance.Run(_internalDialogue, this));
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
