using System;
using System.Collections;
using System.Collections.Generic;
using Apps.PC;
using Assignments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

    public static IEnumerator FadeIn(Renderer renderer, float speed = 0.5f)
    {
        var color = renderer.material.color;

        while (renderer.material.color.a < 1)
        {
            float fadeAmount = color.a + (speed * Time.deltaTime);
            color = new Color(color.r, color.g, color.b, fadeAmount);
            renderer.material.color = color;

            yield return null;
        }
    }
    
    public static IEnumerator FadeOut(Renderer renderer, float speed = 0.5f, bool destroy = false)
    {
        var color = renderer.material.color;
        
        while (renderer.material.color.a > 0)
        {
            float fadeAmount = color.a - (speed * Time.deltaTime);
            color = new Color(color.r, color.g, color.b, fadeAmount);
            renderer.material.color = color;

            yield return null;
        }
        
        if (destroy) Destroy(renderer.gameObject);
    }
}