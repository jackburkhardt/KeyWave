using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartApplication : MonoBehaviour
{
    // Start is called before the first frame update
    public void Restart()
    {
        if (SceneManager.GetSceneByName("PauseMenu").isLoaded)
        {
            PauseMenu.instance.QuitGame();
        }
        
        else App.Instance.ChangeScene("StartMenu", SceneManager.GetActiveScene().name, LoadingScreen.Transition.Black);
    }
}
