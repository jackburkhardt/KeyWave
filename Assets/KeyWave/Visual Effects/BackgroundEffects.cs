
/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager of certain visual effects, such as when the player enters or leaves a room.
/// </summary>

public class BackgroundEffects : MonoBehaviour
{
    

    private void OnEnable()
    {
        GameEvent.OnActorExitRoom += ActorExitRoomEffects;
        GameEvent.OnActorEnterRoom += ActorEnterRoomEffects;
    }   

    private void OnDisable()
    {
        GameEvent.OnActorExitRoom -= ActorExitRoomEffects;
        GameEvent.OnActorEnterRoom -= ActorEnterRoomEffects;
    }



    delegate IEnumerator EnterEffect(Actor actor, Transform destination);
    delegate IEnumerator ExitEffect(Actor actor);

    void ActorEnterRoomEffects(Actor actor, Transform destination)
    {
       // EnterEffect func = null;
        
        if (actor == GameManager.playerActor)
        {
            if (destination.name == "Map") StartCoroutine(PlayerEnterMap(actor, destination));
            StartCoroutine(PlayerEnterRoom(actor, destination));
        }

       

    }

    void ActorExitRoomEffects(Actor actor, Transform destination)
    {
        ExitEffect func = null;

        if (actor == GameManager.playerActor)
        {
            if (destination.name == "Map") { func = PlayerExitMap; }
            else { func = PlayerExitRoom; }
        }

        StartCoroutine(func(actor));
    }


    IEnumerator PlayerExitRoom(Actor actor)
    {
        RemovePlayerControl.AddToDisablerQueue(this);
        yield return StartCoroutine(CameraFader.Instance.FadeToColor(Color.black, actor.travelTime * 2));

    }

    IEnumerator PlayerEnterRoom(Actor actor, Transform destination)
    {
        CameraMover.Instance.CameraTeleport(destination);
        yield return StartCoroutine(CameraFader.Instance.FadeFromColor(actor.travelTime * 2));
        RemovePlayerControl.RemoveFromDisablerQueue(this);
    }

    IEnumerator PlayerEnterMap(Actor actor, Transform destination)
    {
        CameraZoom.Instance.SetRelativeCameraSize(-1);
        while (GameManager.playerActor.location.name == "Map") {
            yield return StartCoroutine(CameraZoom.Instance.ZoomOut());
            yield return StartCoroutine(CameraZoom.Instance.ZoomIn());
        }
        //  yield return StartCoroutine(CameraZoom.Instance.ZoomIn());

    }

    IEnumerator PlayerExitMap(Actor actor)
    {
        RemovePlayerControl.AddToDisablerQueue(this);
        yield return StartCoroutine(CameraFader.Instance.FadeToColor(Color.black, actor.travelTime * 2));
    }



}

*/
