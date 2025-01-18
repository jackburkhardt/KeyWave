using System;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.UI
{
    public class CircularUIMenuPanel : CustomUIMenuPanel, IPointerEnterHandler, IPointerExitHandler
    {
        private const float MaxVisibleDegreeSum = 85;


        public new static List<string> CustomFields = new List<string>
        {
            "offsetCurve", 
            "timeEstimate",
            "responseMenuAnimator",
            "mousePointerHand",
            "outerBackground",
            "innerBackground",
            "useHideAnimationTrigger",
            "MainComponentSwitcher"
        };
        

        [SerializeField] private WatchHandCursor mousePointerHand;
        [SerializeField] public AnimationCurve offsetCurve;
        [SerializeField] private UITextField timeEstimate;
        [SerializeField] private Image outerBackground, innerBackground;
        [SerializeField] private bool useHideAnimationTrigger;
        
        public ComponentSwitcher MainComponentSwitcher;

        private Color defaultOuterColor, defaultInnerColor;

        private bool _firstFocus = false;
        
        private bool _pointerInside = false;

        [Serializable]
        public class ResponseMenuHue
        {
            public string conversationType;
            public Color color;
        }
        
        public List<ResponseMenuHue> responseMenuHues;
        
        
        private Animator? Animator => GetComponent<Animator>();
        
       // protected void
        public override void Awake()
        {
            base.Awake();
            {
                defaultOuterColor = outerBackground.color;
                defaultInnerColor = innerBackground.color;
            }
        }


        protected override void Update()
        {
            base.Update();
        
            var circularButtons = GetComponentsInChildren<CircularUIButton>();

            foreach (var circularButton in circularButtons)
            {
                if (circularButton.CircularUIDegreeSum < MaxVisibleDegreeSum)  circularButton.Offset = 0;
                else
                {
                    var normalizedPointerAngle = mousePointerHand.AngleCenteredSouth / MaxVisibleDegreeSum * 2f; 
                    var offsetRange = circularButton.CircularUIDegreeSum - MaxVisibleDegreeSum;
                    var offset = offsetRange * - (offsetCurve.Evaluate(Mathf.Abs(normalizedPointerAngle)) *  MathF.Sign(normalizedPointerAngle) * 0.5f); 
                    circularButton.Offset = offset;
                }
            }
        
            var normalizedMousePosition = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
        
            var normalizedMenuPosition = new Vector2(transform.position.x / Screen.width, transform.position.y / Screen.height);
        
            var distance = Vector2.Distance(normalizedMousePosition, normalizedMenuPosition);
            
           
        
            var active = Animator!.GetBool("Active");

            if (!active) return;
            
            
            if (distance > 1.65f && _firstFocus)
            {
                Animator!.SetBool("Focus", false);
            
            }
            else if (distance < 1.65f && !WatchHandCursor.Frozen)
            {
                Animator!.SetBool("Focus", true);
                _firstFocus = true;
            }
      
        }
        
        /*
        public override void Close()
        {
            PopFromPanelStack();
            if (gameObject == null) return;
            if (gameObject.activeInHierarchy) CancelInvoke();
            if (panelState == PanelState.Closed || panelState == PanelState.Closing) return;
            panelState = PanelState.Closing;
            onClose.Invoke();
            var myAnimator = GetComponent<Animator>();
            if (myAnimator != null && myAnimator.isInitialized && !string.IsNullOrEmpty(showAnimationTrigger))
            {
                myAnimator.ResetTrigger(showAnimationTrigger);
            }
            if (useHideAnimationTrigger) animatorMonitor.SetTrigger(hideAnimationTrigger, OnHidden, true);

            // Deselect ours:
            if (eventSystem != null && selectables.Contains(eventSystem.currentSelectedGameObject))
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
*/
        public override void Focus()
        {
            DialogueManager.instance.StopConversation();
            DialogueManager.instance.StartConversation(Location.PlayerLocationWithSublocation + "/Actions");
            base.Focus();
            WatchHandCursor.Unfreeze();
        }
        
        protected override void ShowResponsesNow(Subtitle subtitle, Response[] responses, Transform target)
        {
            if (responses == null || responses.Length == 0)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: StandardDialogueUI ShowResponses received an empty list of responses.", this);
                return;
            }
            ClearResponseButtons();
            SetResponseButtons(responses, target);
            ActivateUIElements();
            Open();
            RefreshSelectablesList();
            if (blockInputDuration > 0)
            {
                DisableInput();
                if (InputDeviceManager.autoFocus) SetFocus(firstSelected);
                Invoke(nameof(EnableInput), blockInputDuration);
            }
            else
            {
                if (InputDeviceManager.autoFocus) SetFocus(firstSelected);
                if (s_isInputDisabled) EnableInput();
            }
#if TMP_PRESENT
            DialogueManager.instance.StartCoroutine(CheckTMProAutoScroll());
#endif
            
        }
        
        public override void Unfocus()
        {
            //Animator!.SetBool("Focus", false);
            if (DialogueManager.instance.conversationModel.conversationTitle.EndsWith("Actions"))
            {
                DialogueManager.PlaySequence("GoToConversation(" + Location.PlayerLocationWithSublocation + "/Talk/Base)");
            }
            base.Unfocus();
            WatchHandCursor.Freeze();
        }


        public override void OnChoiceClick(StandardUIResponseButton customUIResponseButton)
        {
            //if (!Animator!.GetBool("Active")) return;
            WatchHandCursor.Freeze();
            Animator!.SetBool("Frozen", true);
            
            var destinationEntry = customUIResponseButton.response.destinationEntry;
            
            if (destinationEntry.outgoingLinks.Count == 0)
            {
                var conversationTitle = destinationEntry.GetConversation().Title;
                var conversationType = conversationTitle.Split("/").Length > 2 ? conversationTitle.Split("/")[^2] : string.Empty;
                Debug.Log($"Conversation type: {conversationType}");
                DialogueManager.instance.PlaySequence($"SetDialoguePanel(false); SetActionPanel(true, {conversationType})");
            }
            
            else Debug.Log("links: " + destinationEntry.outgoingLinks.Count);

        }

        private Color _color;

        public void SetColor(Color color)
        {
            _color = color;
            
            outerBackground.color = Color.Lerp(defaultOuterColor, new Color(_color.r, _color.g, _color.b ,outerBackground.color.a), 0.2f);
                
            innerBackground.color = Color.Lerp(defaultInnerColor, new Color(_color.r, _color.g, _color.b ,innerBackground.color.a), 0.4f);
        }

        public void OnLinkedConversationStart()
        {
            var currentConversation =
                (DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.GetConversation());
            
            var conversationType = currentConversation.Title == "Map" ? "Map" : currentConversation.Title.Split("/").Length > 2 ? currentConversation.Title.Split("/")[^2] : string.Empty;
            
            switch (conversationType) 
            {
                case "Talk":
                    MainComponentSwitcher.SwitchTo(0);
                    break;
                case "Action":
                    MainComponentSwitcher.SwitchTo(1);
                    break;
                case "Walk":
                    MainComponentSwitcher.SwitchTo(2);
                    break;
                case "Map":
                    MainComponentSwitcher.SwitchTo(3);
                    break;
            }
        }


        protected override void OnContentChanged()
        {
            base.OnContentChanged();
            _firstFocus = false;
           
          
            //WatchHandCursor.Unfreeze();
       
            Animator!.SetBool("Frozen", false);
            
  
            var currentConversation =
                (DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.GetConversation());
            

            if (_pointerInside) Focus();
           
        }

        public void Show()
        {
            animatorMonitor.SetTrigger(showAnimationTrigger, OnVisible, waitForShowAnimation);
        }

        public void Hide()
        {
            animatorMonitor.SetTrigger(showAnimationTrigger, OnHidden, waitForShowAnimation);
        }

        public void ShowActions()
        {
           
           // animatorMonitor.SetTrigger(showAnimationTrigger, OnVisible, waitForShowAnimation);
        }

        public void SetPropertiesFromButton(CircularUIResponseButton button)
        {
            if (button == null)
            {
                timeEstimate.text = "";
            }

            else
            {
                timeEstimate.text = button.TimeEstimateText;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
          //  Focus();
          //  _pointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Unfocus();
            //_pointerInside = false;
        }
    }

    public class SequencerCommandSetCircularUIPanel : SequencerCommand
    {
        private void Awake()
        {
            
            var value = GetParameterAsBool(0);
            var type = GetParameter(1, "");
            
            /*

            var panel = FindObjectOfType<CircularUIMenuPanel>();
            
            if (value)
            {
                panel.Open();
            }
            else
            {
                panel.Close();
            }

            return;
            */
        
           
        }
    }
}