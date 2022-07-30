using System;
using Apps.PC;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _togglePopupViewMode;
    [SerializeField] private GameObject _closePopup;
    [SerializeField] private Image _uiBackground;
    
    private void Awake()
    {
        GameEvent.OnPopupCreate += () =>
        {
            TogglePopupIcons(true);
            _closePopup.GetComponent<Button>().onClick.AddListener(ClosePopups);
        };
        GameEvent.OnPCClose += () =>
        {
            _uiBackground.enabled = false;
            TogglePopupIcons(false);
        };
        GameEvent.OnPCOpen += () =>
        {
            _uiBackground.enabled = true;
            _closePopup.SetActive(true);
            _closePopup.GetComponent<Button>().onClick.AddListener(() => PC.Instance.ClosePC());
        };
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
        _closePopup.GetComponent<Button>().onClick.RemoveAllListeners();
        _uiBackground.enabled = visible;
    }

    public void ClosePopups()
    {
        TogglePopupIcons(false);
        GameEvent.PopupClose();
    }
}