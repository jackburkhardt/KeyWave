using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool _isInteracting;

    private void Awake()
    {
        GameEvent.OnInteractionStart += obj => _isInteracting = true;
        GameEvent.OnInteractionEnd += obj => _isInteracting = false;
    }
}