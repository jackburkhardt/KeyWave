using Project.Runtime.Scripts.App;
using UnityEngine;
using UnityEngine.Events;

public class AppEvents : MonoBehaviour
{
    public UnityEvent OnLoadStart;

    public UnityEvent OnLoadEnd;

    private void OnEnable()
    {
        App.OnSceneLoadStart += OnAppLoadStart;
        App.OnSceneLoadEnd += OnAppLoadEnd;
    }

    private void OnDisable()
    {
        App.OnSceneLoadStart -= OnAppLoadStart;
        App.OnSceneLoadEnd -= OnAppLoadEnd;
    }

    private void OnAppLoadStart(string scene)
    {
        OnLoadStart?.Invoke();
        
    }
    
    private void OnAppLoadEnd(string scene)
    {
        OnLoadEnd?.Invoke();
    }
}
