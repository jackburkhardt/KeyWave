using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.UI
{
    public class CustomUIMenuPanel : StandardUIMenuPanel
    {
        public static List<string> CustomFields = new List<string>
        {
          "buttonStyles"
        };
        


        [SerializeField] Animator responseMenuAnimator;

        public List<CustomUIResponseButton> ResponseButtons => GetComponentsInChildren<CustomUIResponseButton>().ToList();
        

        #region overrides

            public new void DestroyInstantiatedButtons()
            {
                //Debug.Log("Destroying instantiated buttons. Count: " + instantiatedButtons.Count);
            // Return buttons to pool:
                for (int i = instantiatedButtons.Count - 1; i >= 0; i--)
                {
                    Destroy(instantiatedButtons[i].gameObject);
                }

                instantiatedButtons.Clear();
                NotifyContentChanged();
            }
            
            
            
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
                DestroyInstantiatedButtons();
            }



            private Response[] _currentResponses;
            private int _currentResponseIndex;
            

            protected virtual GameObject InstantiateButton(Response response)
            {
                if (response.destinationEntry.outgoingLinks.Count == 0)
                {
                    
                    var style = buttonStyles.Find(x => x.condition == ResponseButtonStyle.styleCondition.EntryHasNoOutgoingLinks);
                    if (style != null && style.responseButton != null)
                    {
                        var button = Instantiate(style.responseButton.gameObject);
                        button.SetActive(true);
                        return button;
                    }
                }
                
                if (response.destinationEntry.GetNextDialogueEntry()?.GetConversation().Title == "SmartWatch/Actions")
                {
                    var style = buttonStyles.Find(x => x.condition == ResponseButtonStyle.styleCondition.EntryLeadsToSmartWatchActions);
                    if (style != null && style.responseButton != null)
                    {
                        var button = Instantiate(style.responseButton.gameObject);
                        button.SetActive(true);
                        return button;
                    }
                }
                
                var defaultButton = Instantiate(buttonTemplate.gameObject);
                defaultButton.SetActive(true);
                return defaultButton;
            }
            
            
            protected override void SetResponseButtons(Response[] responses, Transform target)
        {
            firstSelected = null;
            DestroyInstantiatedButtons();
            var hasDisabledButton = false;

            if ((buttons != null) && (responses != null))
            {
                // Add explicitly-positioned buttons:
                int buttonNumber = 0;
                for (int i = 0; i < responses.Length; i++)
                {
                    if (responses[i].formattedText.position != FormattedText.NoAssignedPosition)
                    {
                        int position = responses[i].formattedText.position;
                        if (0 <= position && position < buttons.Length && buttons[position] != null)
                        {
                            SetResponseButton(buttons[position], responses[i], target, buttonNumber++);
                        }
                        else
                        {
                            Debug.LogWarning("Dialogue System: Buttons list doesn't contain a button for position " + position + ".", this);
                        }
                    }
                }

                if ((buttonTemplate != null) && (buttonTemplateHolder != null))
                {
                    if (scrollbarEnabler != null) CheckScrollbar();

                    // Instantiate buttons from template:
                    for (int i = 0; i < responses.Length; i++)
                    {
                        if (responses[i].formattedText.position != FormattedText.NoAssignedPosition) continue;
                        GameObject buttonGameObject = InstantiateButton(responses[i]);
                        if (buttonGameObject == null)
                        {
                            Debug.LogError("Dialogue System: Couldn't instantiate response button template.");
                        }
                        else
                        {
                            instantiatedButtons.Add(buttonGameObject);
                            
                            Debug.Log("Instantiated button count : " + instantiatedButtons.Count);
                            
                            buttonGameObject.transform.SetParent(buttonTemplateHolder.transform, false);
                            buttonGameObject.transform.SetAsLastSibling();
                            buttonGameObject.SetActive(true);
                            StandardUIResponseButton responseButton = buttonGameObject.GetComponent<StandardUIResponseButton>();
                            SetResponseButton(responseButton, responses[i], target, buttonNumber++);
                            if (responseButton != null)
                            {
                                buttonGameObject.name = "Response: " + responseButton.text;
                                if (explicitNavigationForTemplateButtons && !responseButton.isClickable) hasDisabledButton = true;
                            }
                            if (firstSelected == null) firstSelected = buttonGameObject;

                        }
                    }
                }
                else
                {
                    // Auto-position remaining buttons:
                    if (buttonAlignment == ResponseButtonAlignment.ToFirst)
                    {
                        // Align to first, so add in order to front:
                        for (int i = 0; i < Mathf.Min(buttons.Length, responses.Length); i++)
                        {
                            if (responses[i].formattedText.position == FormattedText.NoAssignedPosition)
                            {
                                int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(0, 1), 0, buttons.Length - 1);
                                SetResponseButton(buttons[position], responses[i], target, buttonNumber++);
                                if (firstSelected == null) firstSelected = buttons[position].gameObject;
                            }
                        }
                    }
                    else
                    {
                        // Align to last, so add in reverse order to back:
                        for (int i = Mathf.Min(buttons.Length, responses.Length) - 1; i >= 0; i--)
                        {
                            if (responses[i].formattedText.position == FormattedText.NoAssignedPosition)
                            {
                                int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(buttons.Length - 1, -1), 0, buttons.Length - 1);
                                SetResponseButton(buttons[position], responses[i], target, buttonNumber++);
                                firstSelected = buttons[position].gameObject;
                            }
                        }
                    }
                }
            }

            if (explicitNavigationForTemplateButtons) SetupTemplateButtonNavigation(hasDisabledButton);

            NotifyContentChanged();
        }
            
            
            
        public override void Awake()
        {
            Tools.SetGameObjectActive(buttonTemplate, false);
            foreach (var button in buttonStyles)
            {
                Tools.SetGameObjectActive(button.responseButton.gameObject, false);
            }
        }
            

        #endregion
       
        [Serializable]
        public class ResponseButtonStyle
        {
            public enum styleCondition
            {
                EntryHasNoOutgoingLinks,
                EntryLeadsToSmartWatchActions
            }
            
            public styleCondition condition;
            public StandardUIResponseButton responseButton;
            
        }
        
        public List<ResponseButtonStyle> buttonStyles = new List<ResponseButtonStyle>();

        
        
     
       
        public virtual void OnChoiceSelection(CustomUIResponseButton customUIResponseButton)
        {
        
       if (customUIResponseButton.simStatus == "WasDisplayed")
       {
           Clock.Freeze(true);
       }

       var destinationEntry = customUIResponseButton.response.destinationEntry;

       if (destinationEntry.outgoingLinks.Count == 0)
       {
           var title = destinationEntry.GetConversation().Title;
           var baseConversation = Location.PlayerLocationWithSublocation + "/Base";
           if (title != baseConversation)
           {
               Debug.Log("Last node, going to base conversation.");
               DialogueManager.PlaySequence("GoToConversation(" + baseConversation + ", true);");
           }
           else Debug.Log("Last node, not going to base conversation.");
       }

   
    
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
        
        public override void Close()
        {
            base.Close();
            DialogueManager.instance.BroadcastMessage("OnUIPanelClose", this);
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