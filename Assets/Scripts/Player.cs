using UnityEngine;

public class Player : MonoBehaviour
{
    private bool _isInteracting;
    public static int Time = 1200;

    private void Awake()
    {
        GameEvent.OnInteractionStart += obj => _isInteracting = true;
        GameEvent.OnInteractionEnd += obj => _isInteracting = false;
    }
}