using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]

public class ButtonEvents : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent OnClick, OnClicked, OnHover, OnHoverEnd, OnButtonDisabled;
    private Button _button;
    [Tooltip("If true, the button events will run even if the button itself is not interactable.")]
    public bool ignoreButtonInteractability;
    
    private bool _oldInteractable;
    private bool _currentInteractable;


    private void Update()
    {
        _currentInteractable = _button.interactable;
        if (_oldInteractable != _currentInteractable)
        {
            OnCanvasGroupChanged();
            _oldInteractable = _currentInteractable;
            if (!_currentInteractable)
            {
                OnButtonDisabled.Invoke();
            }
        }
    }
    
    #region statics + type definitions
    private static readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();
    #endregion
 
    #region properties
    public bool IsInteractable { get; private set; }
    #endregion
 
    #region Unity Messages
 
    private void OnCanvasGroupChanged()
    {
        //default to true incase no canvas group to root
        bool interactibleCheck = true;
 
        Transform cg_transform = transform;
        while(cg_transform != null)
        {
            cg_transform.GetComponents(m_CanvasGroupCache);
            bool ignoreParentGroups = false;
 
            for(int i = 0, count = m_CanvasGroupCache.Count; i < count; i++)
            {
                var canvasGroup = m_CanvasGroupCache[i];
 
                interactibleCheck &= canvasGroup.interactable;
                ignoreParentGroups |= canvasGroup.ignoreParentGroups || !canvasGroup.interactable;
            }
 
            if(ignoreParentGroups)
            {
                break;
            }
 
            cg_transform = cg_transform.parent;
        }
 
        IsInteractable = interactibleCheck;
    }
    #endregion
    
    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_button.interactable && IsInteractable || ignoreButtonInteractability)
            OnClick.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_button.interactable && IsInteractable || ignoreButtonInteractability)
            OnClicked.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {  if (_button.interactable && IsInteractable || ignoreButtonInteractability)
        OnHover.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_button.interactable && IsInteractable || ignoreButtonInteractability)
            OnHoverEnd.Invoke();
    }

    public void OnButtonDisable()
    {
        OnButtonDisabled.Invoke();
    }
}
