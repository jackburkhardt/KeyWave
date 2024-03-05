using System.Collections;
using System.Collections.Generic;
using Interaction;
using UnityEngine;
using UnityEngine.UI;
public class InvokePlayerInteract : MonoBehaviour
{
    public void OnPlayerInteract()
    {
        GameEvent.OnInteraction(gameObject);
    }

}
