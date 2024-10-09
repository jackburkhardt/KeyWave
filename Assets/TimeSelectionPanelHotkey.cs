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
    public class TimeSelectionPanelHotkey : MonoBehaviour
    {
        private bool isOpen = false;

        [Tooltip("Toggle the actor compendium window when this key is pressed.")]
        public KeyCode key = KeyCode.T;

        [Tooltip("Toggle the actor compendium window when this input button is pressed.")]
        public string buttonName = string.Empty;
        
        [Tooltip("Play this sequence when clicking on Submit when opened by this button.")]
        public string playSequence = string.Empty;

        [Tooltip("(Optional) Use this quest log window. If unassigned, will automatically find quest log window in scene. If you assign a window, assign a scene instance, not an uninstantiated prefab.")]
        public TimeSelectionInputPanel timeSelectionInputPanel;
        
        public string luaVariable = "TimeSelectionPanelHotkeyValue";
        

        public TimeSelectionInputPanel runtimeTimeSelectionInputPanel
        {
            get
            {
                if (timeSelectionInputPanel == null) timeSelectionInputPanel = GameObjectUtility.FindFirstObjectByType<TimeSelectionInputPanel>();
                return timeSelectionInputPanel;
            }
        }

        void Awake()
        {
            if (timeSelectionInputPanel == null) timeSelectionInputPanel = GameObjectUtility.FindFirstObjectByType<TimeSelectionInputPanel>();
        }

        void Update()
        {
           
            if (InputDeviceManager.IsKeyDown(key) || (!string.IsNullOrEmpty(buttonName) && DialogueManager.getInputButtonDown(buttonName)))
            {
                ToggleTimeSelectionPanel();
            }
        }
        
        public void ToggleTimeSelectionPanel()
        {
            if (runtimeTimeSelectionInputPanel == null) return;
            if (DialogueManager.IsDialogueSystemInputDisabled()) return;
            if (runtimeTimeSelectionInputPanel.isOpen)
            {
                runtimeTimeSelectionInputPanel.PlaySequenceOnSubmit = string.Empty;
                runtimeTimeSelectionInputPanel.Close();
            }
            else
            {
                runtimeTimeSelectionInputPanel.PlaySequenceOnSubmit = playSequence;
                runtimeTimeSelectionInputPanel.luaVariableName = luaVariable;
                runtimeTimeSelectionInputPanel.Open();
                
            }
           
        }

    }

}