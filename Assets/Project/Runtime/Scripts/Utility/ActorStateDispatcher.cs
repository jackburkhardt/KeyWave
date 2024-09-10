using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;
using PixelCrushers.DialogueSystem;


namespace Project.Runtime.Scripts.ActorCompendium
{
    [AddComponentMenu("")] // Added automatically by QuestStateListener.
    public class ActorStateDispatcher : MonoBehaviour
    {

        private List<ActorStateListener> m_listeners = new List<ActorStateListener>();
        public List<ActorStateListener> listeners => m_listeners;

        protected virtual void OnEnable()
        {
            PixelCrushers.SaveSystem.saveDataApplied += UpdateListeners;
        }

        protected virtual void OnDisable()
        {
            PixelCrushers.SaveSystem.saveDataApplied -= UpdateListeners;
        }

        public virtual void AddListener(ActorStateListener listener)
        {
            if (listener == null) return;
            m_listeners.Add(listener);
        }

        public virtual void RemoveListener(ActorStateListener listener)
        {
            m_listeners.Remove(listener);
        }

        private void UpdateListeners()
        {
            for (int i = 0; i < m_listeners.Count; i++)
            {
                var listener = m_listeners[i];
                if (listener == null) continue;
                listener.UpdateIndicator();
            }
        }

        public virtual void OnActorStateChange(string actorName)
        {
            for (int i = 0; i < m_listeners.Count; i++)
            {
                var listener = m_listeners[i];
                if (listener == null) continue;
                if (string.Equals(actorName, listener.actorName))
                {
                    listener.OnChange();
                }
            }
        }

    }
}


