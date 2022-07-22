using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Interaction
{
    /// <summary>
    /// An item in the game space which will create popups when clicked on.
    /// </summary>
    public class PopupItem : MonoBehaviour, IInteractable
    {
        private Outline _outline;
        [SerializeField] [TextArea] private string _internalDialogue;
        // action to be executed after the main interaction completes
        [SerializeField] private UnityEvent _postInteractAction;
        [SerializeField] private List<Object> _containedPopupWindows;
        private List<GameObject> _generatedPopups = new List<GameObject>();

        private void Awake()
        {
            GameEvent.OnPopupClose += EndInteraction;
            _outline = GetComponent<Outline>();
        }

        public void Interact()
        {
            GameEvent.InteractionStart(this);
            if (_internalDialogue != "" && !PreviouslyInteractedWith) StartCoroutine(Dialogue.Instance.Run(_internalDialogue, this));
            GameEvent.PopupCreated();
            foreach (var window in _containedPopupWindows)
            {
                GameObject p = Instantiate(window, Camera.main.transform.position, Quaternion.identity, Camera.main.transform) as GameObject;
                _generatedPopups.Add(p);
            }
            PreviouslyInteractedWith = true;
        }

        public void EndInteraction()
        {
            foreach (var popup in _generatedPopups)
            {
                Destroy(popup);
            }
            PreviouslyInteractedWith = false;
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

        public bool PreviouslyInteractedWith { get; private set;  }

        private void OnDestroy()
        {
            GameEvent.OnPopupClose -= EndInteraction;
        }
    }
}