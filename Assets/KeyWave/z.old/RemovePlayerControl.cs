using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

///Contains a queue that, when populated, removes certain control from the game.
///This does not remove the player's ability to enter menus or continue dialogue. It mostly handles things like interactable objects or clicking on actors.

public class RemovePlayerControl : MonoBehaviour
{
    

    [SerializeField]
    public static List<Object> controlDisablerQueue = new List<Object>();
    public List<string> controlDisablerQueueClone;

    public static void AddToDisablerQueue(Object obj)
    {
        if (controlDisablerQueue.Contains(obj)) return;
        controlDisablerQueue.Add(obj);
    //    Debug.Log("Added " + obj.name + " to disabler queue");
     //   GameManager.isControlEnabled = false;
    //    GameEvent.AnyEvent();
    }
    public static void RemoveFromDisablerQueue(Object obj)
    {
        controlDisablerQueue.Remove(obj);
    //    Debug.Log("Removed " + obj.name + " from disabler queue");
      //  if (controlDisablerQueue.Count == 0) { GameManager.isControlEnabled = true; }
     //   GameEvent.AnyEvent();
    }

}
