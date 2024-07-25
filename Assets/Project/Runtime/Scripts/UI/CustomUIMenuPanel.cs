using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

namespace Project.Runtime.Scripts.UI
{
    public class CustomUIMenuPanel : StandardUIMenuPanel
    {
        public static List<string> CustomFields = new List<string>
        {
       
        };


        [SerializeField] Animator responseMenuAnimator;


        public List<CustomUIResponseButton> ResponseButtons => GetComponentsInChildren<CustomUIResponseButton>().ToList();


        protected override void OnEnable()
        { 
            base.OnEnable();
            onContentChanged.AddListener(OnContentChanged);
        }

        protected override void OnDisable()
        { 
            base.OnDisable();
            onContentChanged.RemoveListener(OnContentChanged);
        }

        protected virtual void OnContentChanged()
        {
           
            var responseButtons = GetComponentsInChildren<CustomUIResponseButton>();
            foreach (var button in responseButtons)
            {
                if (!button.gameObject.activeSelf && button != buttonTemplate) return;
                button.Refresh();
            }
            
            /*

            var allResponses = response
            foreach (var response in allResponses)
            {
                Debug.Log("Response: " + response.destinationEntry.fields[0].value);
                if (response.destinationEntry == null) continue;
                if (!response.destinationEntry.EvaluateConditions() &&
                    Field.FieldExists(response.destinationEntry.fields, "Show Invalid") &&
                    Field.Lookup(response.destinationEntry.fields, "Show Invalid").value == "true")
                {
                    var invalidButton = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                    invalidButton.gameObject.SetActive(true);
                    invalidButton.response = response;
                    invalidButton.button.interactable = false;
                    invalidButton.text = "???";
                }

            }
            
            
            */

        }

        public void OnQuestStateChange(string questTitle)
        {
       
        }

        public void OnConversationEnd()
        {
     
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