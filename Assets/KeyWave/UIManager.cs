using System.Collections;
using Apps.PC;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the general game UI.
/// </summary>
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

    /// <summary>
    /// Fades renderers for an object to a given alpha value. Caller must provide renderers.
    /// Note that this differs from CameraFade in that it does not fade the whole screen.
    /// </summary>
    public static IEnumerator FadeRenderers(Renderer[] renderers, float startAlpha, float endAlpha, float duration = 0.5f)
    {
        float t = 0;
        while (t < duration)
        {
            foreach (var renderer in renderers)
            {
                var color = renderer.material.color;
                color.a = Mathf.Lerp(startAlpha, endAlpha, t / duration);
                renderer.material.color = color;
            }
            t += Time.deltaTime;
            yield return null;
        }
    }


}