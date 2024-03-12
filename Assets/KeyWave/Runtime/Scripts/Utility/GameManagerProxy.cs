using UnityEngine;

public class GameManagerProxy : MonoBehaviour
{
    //this class should be used in scenes where GameManager is not present, but requires access to GameManager
    //typically this will be used in Buttons, UI, etc

    private GameManager gameManager;
    
    // Start is called before the first frame update
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene");
        }
    }

    // Update is called once per frame
    public void LoadScene(string sceneName)
    {
        gameManager.LoadScene(sceneName);
    }

    public void StartNextDay()
    {
        gameManager.StartNewDay();
    }
}
