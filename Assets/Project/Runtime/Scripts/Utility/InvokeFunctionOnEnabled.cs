using UnityEngine;
using UnityEngine.Events;

public class InvokeFunctionOnEnabled : MonoBehaviour
{
    public UnityEvent onEnabled;

    private void Awake()
    {
        enabled = false;
    }

    public void OnEnable()
    {
        onEnabled.Invoke();
        enabled = false;
    }
    
    
}
