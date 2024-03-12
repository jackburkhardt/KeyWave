using UnityEngine;

public class InvokePlayerInteract : MonoBehaviour
{
    public void OnPlayerInteract()
    {
        GameEvent.OnInteraction(gameObject);
    }

}
