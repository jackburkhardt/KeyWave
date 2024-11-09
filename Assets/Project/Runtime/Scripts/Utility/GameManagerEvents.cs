using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.Events;

public class GameManagerEvents : MonoBehaviour
{
    public UnityEvent OnGameManagerAwake;
    // Start is called before the first frame update
    private void OnEnable()
    {
        GameManager.OnGameManagerAwake += OnGameManagerAwakeEvent;
    }

    private void OnDisable()
    {
        GameManager.OnGameManagerAwake -= OnGameManagerAwakeEvent;
    }
    
    private void OnGameManagerAwakeEvent()
    {
        OnGameManagerAwake?.Invoke();
    }
}
