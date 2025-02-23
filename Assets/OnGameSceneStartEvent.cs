using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnGameSceneStartEvent : MonoBehaviour
{
    public UnityEvent onGameSceneStart;
    
    public void OnGameSceneStart()
    {
        onGameSceneStart.Invoke();
        Debug.Log("Game Scene Started");
    }
}
