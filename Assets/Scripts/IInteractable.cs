using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IInteractable : IPointerEnterHandler, IPointerExitHandler
{
    GameObject gameObject { get; }
    Transform transform { get; }
    string name { get; set; }
    void Interact();
    void EndInteraction();

}
