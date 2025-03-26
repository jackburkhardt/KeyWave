// Copyright (c) Pixel Crushers. All rights reserved.

using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.ActorCompendium
{

    /// <summary>
    /// Allows toggling of the quest log window using a key or button.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ActorCompendiumWindowHotkey : MonoBehaviour
    {
        [Tooltip("Toggle the actor compendium window when this key is pressed.")]
        public KeyCode key = KeyCode.C;

        [Tooltip("Toggle the actor compendium window when this input button is pressed.")]
        public string buttonName = string.Empty;

        [Tooltip("(Optional) Use this quest log window. If unassigned, will automatically find quest log window in scene. If you assign a window, assign a scene instance, not an uninstantiated prefab.")]
        public ActorCompendiumWindow actorCompendiumWindow;

        public ActorCompendiumWindow runtimeActorCompendiumWindow
        {
            get
            {
                if (actorCompendiumWindow == null) actorCompendiumWindow = GameObjectUtility.FindFirstObjectByType<ActorCompendiumWindow>();
                return actorCompendiumWindow;
            }
        }

        void Awake()
        {
            if (actorCompendiumWindow == null) actorCompendiumWindow = GameObjectUtility.FindFirstObjectByType<ActorCompendiumWindow>();
        }

        void Update()
        {
            if (InputDeviceManager.IsKeyDown(key) || (!string.IsNullOrEmpty(buttonName) && DialogueManager.getInputButtonDown(buttonName)))
            {
                ToggleActorCompendiumWindow();
            }
        }
        
        public void ToggleActorCompendiumWindow()
        {
            if (runtimeActorCompendiumWindow == null) return;
            if (DialogueManager.IsDialogueSystemInputDisabled()) return;
            if (runtimeActorCompendiumWindow.isOpen) runtimeActorCompendiumWindow.Close(); else runtimeActorCompendiumWindow.Open();
           
        }

    }

}