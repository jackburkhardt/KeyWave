// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.ActorCompendium
{

    /// <summary>
    /// Add this to a GameObject such as an NPC that wants to know about Actor state changes
    /// to a specific Actor. You can add multiple ActorStateListener components to listen
    /// to multiple Actors.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ActorStateListener : MonoBehaviour
    {

        [ActorPopup(true)]
        public string actorName;

        [Serializable]
        public class ActorStateIndicatorLevel
        {
            [Tooltip("Actor state to listen for.")]
            public ActorState actorState;

            [Tooltip("Conditions that must also be true.")]
            public Condition condition;

            [Tooltip("Indicator level to use when this Actor state is reached.")]
            public int indicatorLevel;

            public UnityEvent onEnterState = new UnityEvent();
        }

        public ActorStateIndicatorLevel[] actorStateIndicatorLevels = new ActorStateIndicatorLevel[0];

     
       

       

        [Tooltip("When starting component, do not invoke any OnEnterState() events.")]
        public bool suppressOnEnterStateEventsOnStart = false;

        protected ActorStateDispatcher m_actorStateDispatcher;
        protected ActorStateDispatcher actorStateDispatcher
        {
            get
            {
                if (m_actorStateDispatcher == null)
                {
                    if (DialogueManager.instance != null)
                    {
                        m_actorStateDispatcher = DialogueManager.instance.GetComponent<ActorStateDispatcher>();
                        if (m_actorStateDispatcher == null)
                        {
                            m_actorStateDispatcher = GameObjectUtility.FindFirstObjectByType<ActorStateDispatcher>();
                            if (m_actorStateDispatcher == null)
                            {
                                m_actorStateDispatcher = DialogueManager.instance.gameObject.AddComponent<ActorStateDispatcher>();
                            }
                        }
                    }
                    else
                    {
                        m_actorStateDispatcher = GameObjectUtility.FindFirstObjectByType<ActorStateDispatcher>();
                        if (m_actorStateDispatcher == null)
                        {
                            var go = new GameObject("ActorStateDispatcher");
                            DontDestroyOnLoad(go);
                            m_actorStateDispatcher = go.AddComponent<ActorStateDispatcher>();
                        }
                    }
                }
                return m_actorStateDispatcher;
            }
        }
        protected ActorStateIndicator m_actorStateIndicator;
        protected ActorStateIndicator actorStateIndicator
        {
            get
            {
                if (m_actorStateIndicator == null) m_actorStateIndicator = GetComponent<ActorStateIndicator>();
                return m_actorStateIndicator;
            }
        }
        private bool m_started = false;
        protected bool started
        {
            get { return m_started; }
            set { m_started = value; }
        }

        protected bool m_suppressOnEnterStateEvent = false;

        protected virtual void OnApplicationQuit()
        {
            enabled = false;
        }

        protected virtual IEnumerator Start()
        {
            if (enabled)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: " + name + ": Listening for state changes to Actor '" + actorName + "'.", this);
                started = true;
                if (actorStateDispatcher == null)
                {
                    if (DialogueDebug.logErrors) Debug.LogWarning("Dialogue System: Unexpected error. Actor State Listener on " + name + " can't find or create a Actor State Dispatcher.", this);
                }
                else
                {
                    actorStateDispatcher.AddListener(this);
                }
                yield return null;
                m_suppressOnEnterStateEvent = suppressOnEnterStateEventsOnStart;
                UpdateIndicator();
                m_suppressOnEnterStateEvent = false;
            }
        }

        protected virtual void OnEnable()
        {
            if (started) actorStateDispatcher.AddListener(this);
        }

        protected virtual void OnDisable()
        {
            if (m_actorStateDispatcher != null) m_actorStateDispatcher.RemoveListener(this); // Use private; don't create new Actor state dispatcher.
        }

        public virtual void OnChange()
        {
            UpdateIndicator();
        }

        /// <summary>
        /// Update the current Actor state indicator based on the specified Actor state indicator 
        /// levels and Actor entry state indicator levels.
        /// </summary>
        public virtual void UpdateIndicator()
        {
            // Check Actor state:
            var actorState = ActorCompendium.GetActorState(actorName);
            for (int i = 0; i < actorStateIndicatorLevels.Length; i++)
            {
                var actorStateIndicatorLevel = actorStateIndicatorLevels[i];
                if (((actorState & actorStateIndicatorLevel.actorState) != 0) && actorStateIndicatorLevel.condition.IsTrue(null))
                {
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: " + name + ": Actor '" + actorName + "' changed to state " + actorState + ".", this);
                    if (actorStateIndicator != null) actorStateIndicator.SetIndicatorLevel(this, actorStateIndicatorLevel.indicatorLevel);
                    if (!m_suppressOnEnterStateEvent)
                    {
                        actorStateIndicatorLevel.onEnterState.Invoke();
                    }
                }
            }

        }

    }
}