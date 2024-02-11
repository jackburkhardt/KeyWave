using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEvent = PlayerEvents.PlayerEvent;

public abstract class PlayerEventHandler : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        GameEvent.OnPlayerEvent += OnPlayerEvent;
    }

    protected virtual void OnDisable()
    {
        GameEvent.OnPlayerEvent -= OnPlayerEvent;
    }

    protected abstract void OnPlayerEvent(PlayerEvent playerEvent);
}
