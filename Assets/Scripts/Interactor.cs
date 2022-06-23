using System;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    public static Interactor Instance;
    private bool _isInteracting;
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Interact(InteractableObject interacObj)
    {
        /*switch (interacObj.)
        {
            case InteractionType.Travel:
        }*/
    }

    public bool IsInteracting => _isInteracting;
}

public enum InteractionType
{
    Inspect,
    Travel,
    Converse
}
