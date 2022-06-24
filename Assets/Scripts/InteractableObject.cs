using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An object in the world space that the player can click on to interact with.
/// </summary>
[RequireComponent(typeof(Collider2D))]
//[RequireComponent(typeof(Outline))]
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private UnityEvent _postInteractAction;
    [SerializeField] private InteractionType _interactionType;

    [Header("For inspection")][SerializeField][TextArea] private string _flavorText;
    [Header("For travel")] [SerializeField] private Location _destination;
    
    public bool PreviouslyInteractedWith { get; set; }
    public InteractionType InteractionType => _interactionType;
    public UnityEvent PostInteractAction => _postInteractAction;
    public string FlavorText => _flavorText;
    public Location Destination => _destination;

    private void Awake()
    {
        // Just in case someone forgets to set these fields in the editor.
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.layer = 3;
    }
} 