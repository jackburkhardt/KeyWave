using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.AssetLoading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StandardUIPauseButton : MonoBehaviour
{
    // Start is called before the first frame update
    public void TogglePause()
    {
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
}
