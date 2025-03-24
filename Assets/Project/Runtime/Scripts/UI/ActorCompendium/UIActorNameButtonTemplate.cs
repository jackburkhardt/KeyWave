// Copyright (c) Pixel Crushers. All rights reserved.

using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.ActorCompendium
{

    /// <summary>
    /// Unity UI template for a actor name button with a toggle for progress tracking.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UIActorNameButtonTemplate : StandardUIContentTemplate
    {

        [Header("Actor Name Button")]

        [Tooltip("Button UI element.")]
        public Button button;

        [Tooltip("Label text to set on button.")]
        public UITextField label;


        public virtual void Awake()
        {
            if (button == null && DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: UI Button is unassigned.", this);
          
        }

        public virtual void Assign(string actorName, string displayName, ToggleChangedDelegate trackToggleDelegate)
        {
            if (UITextField.IsNull(label)) label.uiText = button.GetComponentInChildren<Text>();
            name = actorName;
            label.text = displayName;
          
        }

    }
}