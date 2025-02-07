using UnityEngine;
using UnityEngine.Events;

public class OnAwake : MonoBehaviour
{
    public UnityEvent onAwake;
    void Awake()
    {
        onAwake.Invoke();
    }
}
