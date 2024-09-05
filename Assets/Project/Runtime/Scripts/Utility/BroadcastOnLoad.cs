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
        App.OnDeloadEnd += BroadcastOnDeloadEnd;
    }

    private void OnDisable()
    {
        App.OnLoadEnd -= BroadcastOnLoadEnd;
        App.OnDeloadEnd -= BroadcastOnDeloadEnd;
    }
    
    private void BroadcastOnLoadEnd()
    {
        BroadcastMessage("OnLoad", SendMessageOptions.DontRequireReceiver);
        Debug.Log("Broadcasting OnLoad");
    }
    
    private void BroadcastOnDeloadEnd()
    {
        BroadcastMessage("OnDeload", SendMessageOptions.DontRequireReceiver);
    }
}
