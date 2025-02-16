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

namespace Project.Runtime.Scripts.UI
{
    public class CustomUIMenuPanel : StandardUIMenuPanel
    {
        public static List<string> CustomFields = new List<string>
        {
          "buttonStyles",
          "persistentMenuPanel",
          "accumulateResponse",
          "accumulatedResponseContainer"
        };
        


        [SerializeField] Animator responseMenuAnimator;

        public List<CustomUIResponseButton> ResponseButtons => GetComponentsInChildren<CustomUIResponseButton>().ToList();
        
        public bool persistentMenuPanel = false;

        public bool accumulateResponse;
        
        [ShowIf("accumulateResponse")]
        public RectTransform accumulatedResponseContainer;
        
        
        

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
                
                var unmarkedInstantiatedButtons = transform.GetComponentsInChildren<StandardUIResponseButton>(true).ToList();
                foreach (var button in unmarkedInstantiatedButtons)
                {
                    if (button.gameObject.name.Contains("Response:") && button.gameObject != buttonTemplate.gameObject)
                    {
                        Destroy(button.gameObject);
                    }
                }
                
                
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
                if (GetButtonStyle(response, out var stylePrefab))
                {
                    var button = Instantiate(stylePrefab.gameObject);
                    button.SetActive(true);
                    return button;
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


        protected override void ShowResponsesNow(Subtitle subtitle, Response[] responses, Transform target)
        {
            
            if (responses == null || responses.Length == 0)
            {
                if (TryGetComponent<SmartWatchApp>(out var app))
                {
                    app.OnEnable();
                }
            }
            
            base.ShowResponsesNow(subtitle, responses, target);
        }
            
            
            
        public override void Awake()
        {
            Tools.SetGameObjectActive(buttonTemplate, false);
            foreach (var button in buttonStyles)
            {
                Tools.SetGameObjectActive(button.responseButton.gameObject, false);
            }
        }
        
        public override void Open()
        {
            base.Open();
            
            if (!deactivateOnHidden && TryGetComponent<SmartWatchApp>(out var app))
            {
                app.OnEnable();
            }
            ;
            
            DialogueManager.instance.BroadcastMessage("OnUIPanelOpen", this);
            RefreshLayoutGroups.Refresh(gameObject);
            StartCoroutine(DelayedRefresh());
        }
        
        public override void Close()
        {
            if (!persistentMenuPanel)
            {
                CloseNow();
            }
            
        }
        
        public override void MakeButtonsNonclickable()
        {
            if (persistentMenuPanel) return;
            base.MakeButtonsNonclickable();
        }
            

        #endregion

        private void CloseNow()
        {
            base.Close();
            DialogueManager.instance.BroadcastMessage("OnUIPanelClose", this);
            DestroyInstantiatedButtons();
        }
       
        [Serializable]
        public class ResponseButtonStyle
        {
            public enum styleCondition
            {
                EntryHasNoOutgoingLinks,
                EntryLeadsToSmartWatchActions,
                EntryLeadsToSmartWatchPhone,
                EntryHasTextField,
                EntryGoesToSublocation
            }
            
            public styleCondition condition;
            public StandardUIResponseButton responseButton;
            
            [ShowIf("condition", styleCondition.EntryHasTextField)]
            [AllowNesting]
            public string fieldName;
            [ShowIf("condition", styleCondition.EntryHasTextField)]
            [AllowNesting]
            public string fieldValue;

        }
        
        public List<ResponseButtonStyle> buttonStyles = new List<ResponseButtonStyle>();

        
        
     
       
        public virtual void OnChoiceClick(StandardUIResponseButton responseButton)
        {

            
           var destinationEntry = responseButton.response.destinationEntry;
           
           if (destinationEntry.SimStatus() == "WasDisplayed")
           {
            //   Clock.Freeze(true);
           }

           if (destinationEntry.outgoingLinks.Count == 0)
           {
               
               var title = destinationEntry.GetConversation().Title;
               var baseConversation = GameManager.gameState.GetPlayerLocation(true).BaseConversation();
               if (title != baseConversation)
               {
                   Debug.Log("No outgoing links. Going to base conversation :" + baseConversation);
                   DialogueManager.PlaySequence("GoToConversation(" + baseConversation + ", true);");
               }
           }

           else
           {
               if (accumulateResponse && accumulatedResponseContainer != null)
               {
                   var text = responseButton.text;
                   var button = Instantiate(responseButton, accumulatedResponseContainer);
                   button.gameObject.SetActive(true);
                   button.button.interactable = false;
                   button.enabled = false;
                   button.text = text;
            
                   RefreshLayoutGroups.Refresh(accumulatedResponseContainer.gameObject);
               }

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

        public bool GetButtonStyle(Response response, out StandardUIResponseButton stylePrefab)
        {
            stylePrefab = null;
            
            foreach (var style in buttonStyles)
            {
                if (style.responseButton == null) continue;
                
                switch ( style.condition)
                {
                    case ResponseButtonStyle.styleCondition.EntryHasNoOutgoingLinks:
                        if (response.destinationEntry.outgoingLinks.Count == 0)
                        {
                            stylePrefab = style.responseButton;
                            return true;
                        }
                        break;
                    case ResponseButtonStyle.styleCondition.EntryLeadsToSmartWatchActions:
                        if (response.destinationEntry.GetNextDialogueEntry()?.GetConversation().Title == "SmartWatch/Actions")
                        {
                            stylePrefab = style.responseButton;
                            return true;
                        }
                        break;
                    case ResponseButtonStyle.styleCondition.EntryLeadsToSmartWatchPhone:
                        if (response.destinationEntry.GetNextDialogueEntry()?.GetConversation().Title == "SmartWatch/Phone/Base")
                        {
                            stylePrefab = style.responseButton;
                            return true;
                        }
                        break;
                    case ResponseButtonStyle.styleCondition.EntryHasTextField:
                        if (response.destinationEntry.fields.Any(field => field.title == style.fieldName && field.value == style.fieldValue))
                        {
                            stylePrefab = style.responseButton;
                            return true;
                        }
                        break;
                    case ResponseButtonStyle.styleCondition.EntryGoesToSublocation:
                        if (response.destinationEntry.Sequence.Contains("Sublocation"))
                        {
                            stylePrefab = style.responseButton;
                            return true;
                        }
                        break;
                }

               
            }
            
            return false;
            
        }
        
        private IEnumerator DelayedRefresh()
        {
            yield return new WaitForSeconds(0.1f);
            RefreshLayoutGroups.Refresh(gameObject);
        }
        
        public void ForceClose()
        {
           CloseNow();
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