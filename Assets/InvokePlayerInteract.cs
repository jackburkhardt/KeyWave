using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokePlayerInteract : MonoBehaviour
{
    public void OnPlayerInteract()
    {
        GameEvent.PlayerEvent("player_interact", gameObject.name);
    }

}
