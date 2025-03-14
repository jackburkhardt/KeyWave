using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.AssetLoading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StandardUIPauseButton : MonoBehaviour, ISelectHandler
{
    // Start is called before the first frame update
    public void TogglePause()
    {
        if (SceneManager.GetSceneByName("LoadingScreen").isLoaded || SceneManager.GetSceneByName("EndOfDay").isLoaded) return;
        if (SceneManager.GetSceneByName("PauseMenu").isLoaded != PauseMenu.active) return;
            
        if (PauseMenu.active)
        {
            PauseMenu.instance.UnpauseGame();
        }
        else
        {
            Project.Runtime.Scripts.App.App.Instance.LoadScene("PauseMenu", transition: LoadingScreen.Transition.None);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject( null);
    }
}
