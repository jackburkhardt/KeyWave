using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    
    [SerializeField] private SceneAsset scene;

    private bool isSceneLoaded = false;

    public void OnInteract()
    {
        if (!isSceneLoaded)
        {
            SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
            
        }

        else
        {
            SceneManager.UnloadSceneAsync(scene.name);
        }
        
        isSceneLoaded = !isSceneLoaded;
       
    }

}
