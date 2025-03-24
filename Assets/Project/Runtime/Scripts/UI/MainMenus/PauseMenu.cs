using System;
using System.Collections;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    public Animator animator;
    public Button quitGameButton;
    public static PauseMenu instance;
    public static bool active = false;
    public static Action OnPause;
    public static Action OnPaused;
    public static Action OnUnpause;
    public static Action OnUnpaused;

    public UITextField settingsTitle;
    public UITextField settingsDescription;
    
    private void Awake()
    {
        settingsTitle.text = "";
        settingsDescription.text = "";
        
        instance = this;
        PauseGame();
       
        if ( SceneManager.GetSceneByName("StartMenu").isLoaded)
        {
            quitGameButton.gameObject.SetActive(false);
        }
    }
    
    private void PauseGame()
    {
     
        OnPause?.Invoke();
        
        StartCoroutine(Pause());
        
        IEnumerator Pause()
        {
            animator.SetTrigger("Show");
            Time.timeScale = 0;
            DialogueManager.Pause();
            yield return new WaitForSecondsRealtime(0.5f);
            active = true;
            OnPaused?.Invoke();
        }
        
    }
    
    
    public void UnpauseGame()
    {
        OnUnpause?.Invoke();
       
        StartCoroutine(Unpause());
        
        IEnumerator Unpause()
        {
            active = false;
           
            animator.SetTrigger("Hide");
            yield return new WaitForSecondsRealtime(0.5f);
            Time.timeScale = 1;
            UserSettingsSaver.SaveSettings();
            DialogueManager.Unpause();
            yield return new WaitForEndOfFrame();
            App.Instance.UnloadScene("PauseMenu");
            OnUnpaused?.Invoke();
        }
    }
    
    public void QuitGame()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("location.reload()");
#endif
    }
}
