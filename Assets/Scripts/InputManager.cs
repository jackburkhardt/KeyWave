using System;
using UnityEngine;
using UnityEngine.EventSystems;
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

    }
}
