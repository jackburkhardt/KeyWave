using UnityEngine;

namespace Project.Runtime.Scripts.Events
{
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
}