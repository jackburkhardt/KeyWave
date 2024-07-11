using System;
using System.Collections;
using System.Collections.Generic;
using Project.Runtime.Scripts.App;
using UnityEngine;

public class BroadcastOnLoad : MonoBehaviour
{
    private void OnEnable()
    {
        App.OnLoadEnd += BroadcastOnLoadEnd;
    }

    private void OnDisable()
    {
        App.OnLoadEnd -= BroadcastOnLoadEnd;
    }
    
    private void BroadcastOnLoadEnd()
    {
        BroadcastMessage("OnLoad");
    }
}
