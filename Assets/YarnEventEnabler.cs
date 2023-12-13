using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YarnEventEnabler : MonoBehaviour

{
    YarnEvent yarnEvent;
    private void Awake()
    {
        yarnEvent = GetComponent<YarnEvent>();
    }

    public Transform OnPlayerExit;




    private void OnEnable()
    {
        GameEvent.OnActorExitRoom += EnableYarnOnPlayerExit;
    }

    private void OnDisable()
    {
        GameEvent.OnActorExitRoom -= EnableYarnOnPlayerExit;
    }

    void EnableYarnOnPlayerExit(Actor actor, Transform room)
    {
        if (room == OnPlayerExit) yarnEvent.enabled = true;
    }

    

    


}
