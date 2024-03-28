using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    
    [SerializeField] private SceneAsset scene;

    private bool isSceneLoaded = false;

    public void OnInteract()
    {
        SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
    }
}
