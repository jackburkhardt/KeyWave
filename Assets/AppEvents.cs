using System;
using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.App;
using UnityEngine;
using UnityEngine.Events;

public class AppEvents : MonoBehaviour
{
    public UnityEvent OnLoadStart;

    public UnityEvent OnLoadEnd;

    private void OnEnable()
    {
        App.OnLoadStart += OnAppLoadStart;
        App.OnLoadEnd += OnAppLoadEnd;
    }

    private void OnDisable()
    {
        App.OnLoadStart -= OnAppLoadStart;
        App.OnLoadEnd -= OnAppLoadEnd;
    }

    private void OnAppLoadStart()
    {
        OnLoadStart?.Invoke();
        
    }
    
    private void OnAppLoadEnd()
    {
        OnLoadEnd?.Invoke();
    }
}
