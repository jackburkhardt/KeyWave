using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
    public interface IInteractable : IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        string name { get; set; }
        void Interact();
        void EndInteraction();
        bool PreviouslyInteractedWith { get; }

        void IPointerClickHandler.OnPointerClick(PointerEventData data)
        {
            if (!GameManager.ControlsEnabled) return;
            Interact();
        }

    }
}
