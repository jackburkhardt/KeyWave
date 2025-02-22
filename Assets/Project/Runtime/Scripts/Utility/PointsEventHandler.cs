using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.Events;

public class PointsEventHandler : MonoBehaviour
{
    public UnityEvent onPointsChange;
    public UnityEvent onPointsIncrease;
    public UnityEvent onPointsDecrease;

    public void OnEnable()
    {
        Points.OnPointsChange += OnPointsChange;
    }
    
    
    public void OnPointsChange(string pointType, int amount)
    {
        onPointsChange.Invoke();
        if (amount > 0)
        {
            onPointsIncrease.Invoke();
        }
        else
        {
            onPointsDecrease.Invoke();
        }
    }
    
    
    
}
