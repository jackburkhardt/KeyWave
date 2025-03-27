using DG.Tweening;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneLetterbox : MonoBehaviour, ICutsceneStartHandler, ICutsceneEndHandler
{
    public RectTransform topBar;
    public RectTransform bottomBar;
    
    public float topBarMaxHeight;
    public float bottomBarMaxHeight;
    
    public float animationDuration = 1f;
    
    public enum State
    {
        Inactive,
        Active
    }
    
    private State state = State.Inactive;

    private void Awake()
    {
        HideImmediate();
    }

    [Button]
    public void Show()
    {
        if (state == State.Active) return;
        state = State.Active;
        
        if (topBar == null || bottomBar == null) return;
        if (Application.isPlaying)
        {
            DOTween.To(() => topBar.sizeDelta, x => topBar.sizeDelta = x, new Vector2(topBar.sizeDelta.x, topBarMaxHeight), animationDuration);   
            DOTween.To(() => bottomBar.sizeDelta, x => bottomBar.sizeDelta = x, new Vector2(bottomBar.sizeDelta.x, bottomBarMaxHeight), animationDuration);
        }
        
        else if (Application.isEditor)
        {
            ShowImmediate();
        }
        
        var continueButtons = FindObjectsByType<StandardUIContinueButtonFastForward>( FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var standardUIContinueButtonFastForward in continueButtons)
        {
            standardUIContinueButtonFastForward.enabled = false;
          if (standardUIContinueButtonFastForward.TryGetComponent<Button>( out var _button)) _button.interactable = false;
        }
    }
    
    [Button]
    public void Hide()
    {
        if (state == State.Inactive) return;
        state = State.Inactive;
        
        
        if (topBar == null || bottomBar == null) return;

        if (Application.isPlaying)
        {
            DOTween.To(() => topBar.sizeDelta, x => topBar.sizeDelta = x, new Vector2(topBar.sizeDelta.x, 0), animationDuration);
            DOTween.To(() => bottomBar.sizeDelta, x => bottomBar.sizeDelta = x, new Vector2(bottomBar.sizeDelta.x, 0),
                animationDuration).onComplete = TryStartConversation;
            
            var continueButtons = FindObjectsByType<StandardUIContinueButtonFastForward>( FindObjectsInactive.Include, FindObjectsSortMode.None);
        
            foreach (var standardUIContinueButtonFastForward in continueButtons)
            {
                standardUIContinueButtonFastForward.enabled = true;
                if (standardUIContinueButtonFastForward.TryGetComponent<Button>( out var _button)) _button.interactable = true;
            }
        }
        
        else if (Application.isEditor)
        {
            HideImmediate();
        }
    }

    /// <summary>
    /// fallback for if the letterbox closes and the conversation does not start from ConversationFlowManager for some reason
    /// </summary>
    public void TryStartConversation()
    { 
        var playerLocation = LocationManager.instance.PlayerLocation;
        if (SceneManager.GetSceneByName(playerLocation.Name).isLoaded)
        {
            if (DialogueManager.instance.isConversationActive) return;
            DialogueManager.instance.StartConversation(playerLocation.Name);
        }
    }
    
    private void ShowImmediate()
    {
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, topBarMaxHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, bottomBarMaxHeight);
    }
    
    private void HideImmediate()
    {
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, 0);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, 0);
    }

    public void OnCutsceneStart()
    {
        Show();
    }

    public void OnCutsceneEnd()
    {
        Hide();
    }
}