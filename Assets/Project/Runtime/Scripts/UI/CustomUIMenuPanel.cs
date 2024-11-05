using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.UI
{
    public class CustomUIMenuPanel : StandardUIMenuPanel
    {
        public static List<string> CustomFields = new List<string>
        {
          
        };
        


        [SerializeField] Animator responseMenuAnimator;

        public List<CustomUIResponseButton> ResponseButtons => GetComponentsInChildren<CustomUIResponseButton>().ToList();

        public bool openOnEnable = false;
        public bool closeOnDisable = false;

        protected override void OnEnable()
        {   
            base.OnEnable();
            onContentChanged.AddListener(OnContentChanged);
//            BroadcastMessage("OnCustomUIMenuPanel", DialogueManager.instance.gameObject);
            RefreshLayoutGroups.Refresh(gameObject);
        }

        protected override void OnDisable()
        { 
            base.OnDisable();
            onContentChanged.RemoveListener(OnContentChanged);
        }
        
     
        public virtual void OnChoiceSelection(CustomUIResponseButton customUIResponseButton)
        {
            if (customUIResponseButton.simStatus == "WasDisplayed")
            {
                Clock.Freeze(true);
            }
            
            var destinationEntry = customUIResponseButton.response.destinationEntry;

            if (destinationEntry.outgoingLinks.Count == 0)
            {
                var conversationTitle = destinationEntry.GetConversation().Title;
                var conversationType = conversationTitle.Split("/").Length > 2 ? conversationTitle.Split("/")[^2] : string.Empty;
                Debug.Log($"Conversation type: {conversationType}");
                DialogueManager.instance.PlaySequence($"SetDialoguePanel(false); SetActionPanel(true, {conversationType})");
            }

         //   DialogueManager.instance.PlaySequence("Message(Choice)");
        }

        protected virtual void OnContentChanged()
        {
           
            var responseButtons = GetComponentsInChildren<CustomUIResponseButton>();
            foreach (var button in responseButtons)
            {
                if (!button.gameObject.activeSelf && button != buttonTemplate) return;
                button.Refresh();
            }
            
            RefreshLayoutGroups.Refresh(gameObject);
            
          //  Clock.Freeze(false);

        }

        public void OnQuestStateChange(string questTitle)
        {
       
        }

        public void OnConversationEnd()
        {
     
        }
        
        private IEnumerator DelayedRefresh()
        {
            yield return new WaitForSeconds(0.1f);
            RefreshLayoutGroups.Refresh(gameObject);
        }

        public override void Open()
        {
            base.Open();
            DialogueManager.instance.BroadcastMessage("OnUIPanelOpen", this);
            RefreshLayoutGroups.Refresh(gameObject);
            StartCoroutine(DelayedRefresh());
        }
        
        

        public void StartConversationWithPlayerLocationPrefix(string conversationName)
        {
            DialogueManager.StopConversation();
            DialogueManager.StartConversation(Location.PlayerLocationWithSublocation + "/" + conversationName);
        }
        
        public void StartConversation(string conversationName)
        {
            DialogueManager.StopConversation();
            DialogueManager.StartConversation(conversationName);
        }

       
    }
    
    
}


public class SequencerCommandSetMenuPanelTrigger : SequencerCommand
{
    private void Awake()
    {
        var panelID = GetParameterAsInt(0);
        var show = GetParameterAsBool(1 );
        var standardDialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
        if (standardDialogueUI == null) return;
        
        var panel = PanelNumberUtility.IntToMenuPanelNumber(panelID);
        var menuPanels = standardDialogueUI.conversationUIElements.menuPanels;
        var i = PanelNumberUtility.GetMenuPanelIndex(panel);
        
        var menuPanel = menuPanels[i];
        if (show)
        {
            menuPanel.gameObject.GetComponent<Animator>().SetTrigger(menuPanel.showAnimationTrigger);
        }
        else
        {
            menuPanel.gameObject.GetComponent<Animator>().SetTrigger(menuPanel.hideAnimationTrigger);
        }
        
        
        
        Stop();
    }
}