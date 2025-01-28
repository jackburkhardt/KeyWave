using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartApplication : MonoBehaviour
{
    // Start is called before the first frame update
    public void Restart()
    {
        Project.Runtime.Scripts.App.App.Instance.ChangeScene("StartMenu", GameManager.gameState.current_scene, LoadingScreen.LoadingScreenType.Black);
    }
}
