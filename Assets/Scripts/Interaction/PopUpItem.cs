using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class PopUpItem : MonoBehaviour, IInteractable
{
    private Outline _outline;
    private bool _previouslyInteracted;
    
    // action to be executed after the main interaction completes
    [SerializeField] private UnityEvent _postInteractAction;
    [SerializeField] [TextArea] private string _internalDialogue;
    [SerializeField] private GameObject _imageView;
    [SerializeField] private GameObject _textView;
    public bool TextViewActive;

    private void Awake()
    {
    }

    public void Discard()
    {
        Destroy(this.gameObject);
    }

    public void ToggleTextView()
    {
        // there are two popups active, but only one is shown. this switches between which is visible
        if (!TextViewActive)
        {
            _textView.transform.position = _imageView.transform.position;
            _imageView.SetActive(false);
            _textView.SetActive(true);
            TextViewActive = true;
        }
        else
        {
            TextViewActive = false;
            _imageView.transform.position = _textView.transform.position;
            _textView.SetActive(false);
            _imageView.SetActive(true);
        }
    }

    public void Interact()
    {
        GameEvent.InteractionStart(this);
        if (_internalDialogue != "" && !PreviouslyInteractedWith) StartCoroutine(Dialogue.Instance.Run(_internalDialogue, this));
        
    }

    public void EndInteraction()
    {
        PreviouslyInteractedWith = true;
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
}
