using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PauseEvents : MonoBehaviour
{
    bool isPaused = false;
    public UnityEvent onPause;
    public UnityEvent onUnpause;
    
    public bool broadcastEvents = false;
    private void Update()
    {
        if (Time.timeScale == 0 && !isPaused)
        {
            isPaused = true;
            onPause.Invoke();
            if (broadcastEvents)
            {
                BroadcastMessage("OnPause", SendMessageOptions.DontRequireReceiver);
            }
        }
        else if (Time.timeScale != 0 && isPaused)
        {
            isPaused = false;
            onUnpause.Invoke();
            if (broadcastEvents)
            {
                BroadcastMessage("OnUnpause", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
