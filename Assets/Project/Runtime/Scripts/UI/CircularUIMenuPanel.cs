using System;
using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            "useHideAnimationTrigger"
        };

        [SerializeField] private WatchHandCursor mousePointerHand;
        [SerializeField] public AnimationCurve offsetCurve;
        [SerializeField] private UITextField timeEstimate;
        [SerializeField] private Image outerBackground, innerBackground;
        [SerializeField] private bool useHideAnimationTrigger;

        private Color defaultOuterColor, defaultInnerColor;

        private bool _firstFocus = false;
        
        private bool _pointerInside = false;
        
        
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

        public override void Focus()
        {
           // Animator!.SetBool("Focus", true);
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
            
            Clock.Freeze(false);
            
        }
        
        public override void Unfocus()
        {
            //Animator!.SetBool("Focus", false);
            base.Unfocus();
            WatchHandCursor.Freeze();
        }

        public void OnDialogueSystemPause()
        {
         //   if (!Animator!.GetBool("Active")) return;
           // WatchHandCursor.GlobalFreeze();
         }

        public void OnDialogueSystemUnpause()
        {
           // WatchHandCursor.GlobalUnfreeze();
          }

        public void OnConversationLine()
        {
            
        }


        public override void OnChoiceSelection(CustomUIResponseButton customUIResponseButton)
        {
            if (!Animator!.GetBool("Active")) return;
            WatchHandCursor.Freeze();
            Animator!.SetBool("Frozen", true);
            
            var destinationEntry = customUIResponseButton.response.destinationEntry;
            
            if (destinationEntry.outgoingLinks.Count == 0)
            {
                var entrytag = $"{destinationEntry.GetConversation().Title}_{destinationEntry.id}";
                DialogueManager.instance.PlaySequence($"EndOfLine({entrytag})");
            }
        }

        private Color _color;

        public void SetColor(Color color)
        {
            _color = color;
            
            outerBackground.color = Color.Lerp(defaultOuterColor, new Color(_color.r, _color.g, _color.b ,outerBackground.color.a), 0.2f);
                
            innerBackground.color = Color.Lerp(defaultInnerColor, new Color(_color.r, _color.g, _color.b ,innerBackground.color.a), 0.4f);
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
            Focus();
            _pointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Unfocus();
            _pointerInside = false;
        }
    }
}