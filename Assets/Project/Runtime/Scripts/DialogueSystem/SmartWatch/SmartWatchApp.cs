using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class SmartWatchApp : MonoBehaviour
{
    [Dropdown("apps")]
    public string app;

    private List<string> apps => SmartWatch.instance.appNames;

    public void OnEnable()
    {
        SmartWatch.OnAppOpen?.Invoke(SmartWatch.GetApp(app));
    }
}
