using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        GameEvent.InteractionStart(this);
        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        StartCoroutine(CameraFader.Instance.FadeInOut(Color.black, 1));
        // TODO: tweak numbers here
        yield return new WaitForSeconds(1);
        Camera main = Camera.allCameras[0]; // bit faster than camera.main methinks
        Vector3 newPos = new Vector3(_destination.position.x, _destination.position.y, main.transform.position.z);
        main.transform.position = newPos;
        EndInteraction();
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
