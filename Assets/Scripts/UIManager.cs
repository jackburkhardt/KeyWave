using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _togglePopupViewMode;
    [SerializeField] private GameObject _closePopup;
    [SerializeField] private Image _uiBackground;
    
    private void Awake()
    {
        GameEvent.OnPopupCreate += () => TogglePopupIcons(true);
        GameEvent.OnPCClose += () => _uiBackground.enabled = false;
        GameEvent.OnPCOpen += () => _uiBackground.enabled = true;
    }

    // using unity events to trigger c# events...society...
    public void TogglePopupViewMode()
    {
        GameEvent.ChangePopupView();
    }

    private void TogglePopupIcons(bool visible)
    {
        _togglePopupViewMode.SetActive(visible);
        _closePopup.SetActive(visible);
        _uiBackground.enabled = visible;
    }

    public void ClosePopups()
    {
        TogglePopupIcons(false);
        GameEvent.PopupClose();
    }
}