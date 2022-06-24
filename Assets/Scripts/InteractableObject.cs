using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// An object in the world space that the player can click on to interact with.
/// </summary>
[RequireComponent(typeof(Outline))]
public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnityEvent _postInteractAction;
    [SerializeField] private InteractionType _interactionType;
    [SerializeField] private Outline _outline;

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
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
        gameObject.layer = 3;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _outline.enabled = false;
    }
} 