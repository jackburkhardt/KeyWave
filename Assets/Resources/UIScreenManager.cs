using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIScreenManager : MonoBehaviour
{

    protected bool _isScreenOpen = false;
    public bool IsScreenOpen { get { return _isScreenOpen; } }

    [SerializeField] protected Canvas overlayCanvas;
    [SerializeField] protected Canvas backgroundCanvas;

    [SerializeField] protected GameObject _overlayElementPrefab;
    [SerializeField] protected GameObject _backgroundPrefab;

    protected Transform overlayElementsContainer;
    protected Transform backgroundContainer;

    List<UIScreenManager> _screenManagers = new List<UIScreenManager>();

    protected virtual void Awake()
    {
        foreach (var screenManager in FindObjectsOfType<UIScreenManager>())
        {
            _screenManagers.Add(screenManager);
        }
    }


    protected virtual void OnEnable()
    {
        GameEvent.onUIScreenOpen += OpenScreen;
        GameEvent.onUIScreenClose += CloseScreen;
    }

    protected virtual void OnDisable()
    {
        GameEvent.onUIScreenOpen -= OpenScreen;
        GameEvent.onUIScreenClose -= CloseScreen;
    }

    protected virtual void OpenScreen(Transform screen)
    {
        if (screen != transform) return;

        foreach (var screenManager in _screenManagers)
        {
            if (screenManager.transform != transform) screenManager.gameObject.SetActive(false);
        }

       

        _isScreenOpen = true;
        SetupBackgroundProperties();
        SetupOverlayElementProperties();

        StartCoroutine(OpenScreenInternal()); 

        IEnumerator OpenScreenInternal ()
        {
            yield return StartCoroutine(BackgroundAnimationOnOpen());
            yield return StartCoroutine(OverlayElementsAnimationOnOpen());
        }   
        
     }


    protected virtual void SetupBackgroundProperties()
    {
        if (_backgroundPrefab == null) return;

        backgroundContainer = Instantiate(_backgroundPrefab, backgroundCanvas.transform).transform;
        var canvas = backgroundContainer.GetComponent<Canvas>();
        var rectTransform = backgroundContainer.GetComponent<RectTransform>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = 1;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
    }

    protected virtual void SetupOverlayElementProperties()
    {
        if (_overlayElementPrefab == null) return;
            
        overlayElementsContainer = Instantiate(new GameObject(), overlayCanvas.transform).transform;
        

        //must fill this in
    }

    protected virtual IEnumerator BackgroundAnimationOnOpen()
    {
        yield return null;
    }

    protected virtual IEnumerator BackgroundAnimationOnClose()
    {
        yield return null;
    }

    

    protected virtual IEnumerator OverlayElementsAnimationOnOpen()
    {
        yield return null;
    }

    protected virtual IEnumerator OverlayElementsAnimationOnClose()
    {
        yield return null;
    }


    protected virtual void CloseScreen(Transform screen)
    {
        GameEvent.SaveGame();

        if (screen != transform) return;

        foreach (var screenManager in _screenManagers)
        {
            if (screenManager.transform != transform) screenManager.gameObject.SetActive(true);
        }

        _isScreenOpen = false;

        StartCoroutine(CloseScreenInternal());

        IEnumerator CloseScreenInternal()
        {
            yield return StartCoroutine(OverlayElementsAnimationOnClose());
            yield return StartCoroutine(BackgroundAnimationOnClose());
            if (backgroundContainer != null) Destroy(backgroundContainer.gameObject);
            if (overlayElementsContainer != null) Destroy(overlayElementsContainer.gameObject);
        }

    }




    public void ToggleScreen()
    {
        if (_isScreenOpen) GameEvent.CloseUIScreen(transform); else GameEvent.OpenUIScreen(transform);
    }


}
