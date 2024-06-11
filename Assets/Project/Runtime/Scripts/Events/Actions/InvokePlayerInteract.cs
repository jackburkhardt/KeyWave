using UnityEngine;

namespace Project.Runtime.Scripts.Events.Actions
{
    public class InvokePlayerInteract : MonoBehaviour
    {
        public void OnPlayerInteract()
        {
            GameEvent.OnInteraction(gameObject);
        }
    }
}