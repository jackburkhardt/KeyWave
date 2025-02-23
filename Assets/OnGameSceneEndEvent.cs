using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnGameSceneEndEvent : MonoBehaviour
{
    public UnityEvent onGameSceneEnd;
    public void OnGameSceneEnd()
    {
        onGameSceneEnd.Invoke();
    }
}
