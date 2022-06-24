using System;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{

    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactLayerMask;
    private bool _controlsEnabled = true;
    private GameObject lastHitGO;
    
    private void Update()
    {
        if (!_controlsEnabled) return;
        
        // casts ray from mouse position
        RaycastHit2D hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), 
            Vector2.zero, _interactLayerMask);
        
        // check if the ray hits interactable object, if so highlight
        if (hit.collider)
        {
            var hitGO = hit.collider.gameObject;
            hitGO.GetComponent<Outline>().enabled = true;
            lastHitGO = hitGO;
            
            /*// if the player also clicks, interact with this object
            if (Input.GetMouseButtonDown(1))
            {
                Interactor.Instance.Interact(hitGO.GetComponent<InteractableObject>());
            }*/
        } else if ((!hit.collider || hit.collider.gameObject != lastHitGO) && lastHitGO )
        {
            // if there's no longer a raycast hit or it hits something else, remove outline from old selection
            lastHitGO.GetComponent<Outline>().enabled = false;
            lastHitGO = null;
        }
    }
}
