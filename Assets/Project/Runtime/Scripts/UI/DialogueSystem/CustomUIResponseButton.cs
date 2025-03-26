#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UIButtonKeyTrigger = PixelCrushers.UIButtonKeyTrigger;

namespace Project.Runtime.Scripts.UI
{
    public class CustomUIResponseButton : StandardUIResponseButton
    {
        //private StandardUIResponseButton StandardUIResponseButton => GetComponent<StandardUIResponseButton>();
        
      


        
        public string autoNumberFormat = "{0}. {1}";
        public UITextField? autoNumberText;
        
        
        
        protected Color defaultImageColor;
        public Color DefaultImageColor => defaultImageColor;
        
       
        
        [Foldout("Animation")]
        [SerializeField] private Animator? _animator;
        
        [Foldout("Animation")]
        [SerializeField] private string _showAnimationTrigger = "Show";
        [Foldout("Animation")]
        [SerializeField] private string _hideAnimationTrigger = "Hide";
        
        
        
        [FormerlySerializedAs("_waitForHideAnimation")]
        [Foldout("Animation")]
        [Tooltip("Wait for the hide animation to finish before firing the StandardUIResponseButton's OnClick event.")]
        [SerializeField] private bool waitForHideAnimation;
        
        
        protected UIButtonKeyTrigger[] ButtonKeyTriggers => GetComponents<UIButtonKeyTrigger>();

        protected List<CustomUIResponseButton>? SiblingButtons =>
            transform != null && transform.parent != null ? transform.parent.GetComponentsInChildren<CustomUIResponseButton>().ToList() : null;

        private bool DialogueEntryInvalid => response?.destinationEntry?.conditionsString?.Length != 0 &&
                                             !Lua.IsTrue(response?.destinationEntry?.conditionsString);


        protected virtual void OnEnable()
        {
            Refresh();
            
            if (_animator != null && !string.IsNullOrEmpty(_showAnimationTrigger))
            {
                _animator.SetTrigger(_showAnimationTrigger);
            }
            
        }
        
        public void SetAsLastSibling()
        {
            transform.SetAsLastSibling();
        }


        public void SetAutonumber()
        {
        
        
            int GetAutonumber()
            {
            
                if (SiblingButtons == null) return -1;
                var buttons = new List<CustomUIResponseButton>(SiblingButtons);
            
                // remove all buttons that are invalid
                buttons.RemoveAll(localButton => !localButton.transform.gameObject.activeSelf || 
                                                 localButton.response?.destinationEntry?.conditionsString?.Length != 0 &&
                                                 !Lua.IsTrue(localButton.response?.destinationEntry?.conditionsString));

                if (!buttons.Contains(this)) return -1;

                var siblingIndices = buttons.Select(localButton => localButton.transform.GetSiblingIndex()).OrderBy(n => n).ToList();

                var siblingIndex = transform.GetSiblingIndex();


                var autoNumber = siblingIndices.IndexOf(siblingIndex) == 9 ? 0 : siblingIndices.IndexOf(siblingIndex) + 1;

                return autoNumber;

            }

            string extraKeys = "QWERTYUIOPASDFGHJKLZXCVBNM";


            KeyCode intToKeyCodeKeypad(int i)
            {
                if (i < 0 || i > 9) return KeyCode.None;
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), "Keypad" + i);
                return key;
            }

            KeyCode intToKeyCodeAlpha(int i)
            {
                if (i < 0) return KeyCode.None;
                if (i < 10)
                {
                    var key = (KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + i);
                    return key;
                }

                else
                {
                    var key = (KeyCode)Enum.Parse(typeof(KeyCode), extraKeys[i - 10].ToString());
                    return key;
                }
            }

            var autoNumber = GetAutonumber();

            foreach (var trigger in ButtonKeyTriggers)
            {
                if (trigger.key.ToString().Contains("Keypad"))
                {
                    trigger.key = intToKeyCodeKeypad(autoNumber);
                    continue;
                }

                trigger.key = intToKeyCodeAlpha(autoNumber);
            }

            if (autoNumberText == null || autoNumberText.gameObject == null)
            {
                if (autoNumberFormat.Contains("{0}")) label.text = autoNumberFormat.Replace("{0}", $"{autoNumber}");
                if (autoNumberFormat.Contains("{1}")) label.text = label.text.Replace("{1}", response?.formattedText.text);
            }

            else
            {
                if (autoNumber < 0) autoNumberText.text = "";
                else if (autoNumber < 10) autoNumberText.text = autoNumberFormat.Replace("{0}", $"{autoNumber}");
                else autoNumberText.text = autoNumberFormat.Replace("{0}", $"{extraKeys[autoNumber - 10]}");
                autoNumberText.text.Replace("{1}", string.Empty);
            }
        }


        public virtual void Refresh()
        {
            if (DialogueEntryInvalid)
            {
               // if ( !Field.LookupBool(response?.destinationEntry?.fields, "Show If Invalid")) Destroy(gameObject);
                if (Field.FieldExists(response?.destinationEntry?.fields, "Invalid Text"))
                {
                    var invalidText = Field.LookupValue(response?.destinationEntry?.fields, "Invalid Text");
                
                   //abel.text = invalidText;
                }
            }

            if (response != null && response.destinationEntry != null)
            {
               
                
                label.text = response.formattedText.text;
                
            }
        
            SetAutonumber();
        }


        public override void OnClick()
        {
            if (_animator != null && !string.IsNullOrEmpty(_hideAnimationTrigger))
            {
                if (waitForHideAnimation)
                    StartCoroutine(SetAnimationTriggerAndWait(_hideAnimationTrigger, DoClick));
                else DoClick();
            }
            else DoClick();

            void DoClick()
            {
                var customMenuPanel = GetComponentInParent<CustomUIMenuPanel>();

                if (customMenuPanel != null && customMenuPanel.accumulateResponse &&
                    customMenuPanel.accumulatedResponseContainer != null)
                {
                    customMenuPanel.OnChoiceClick( this);
                }
                
                base.OnClick();
            }

        }
        
        private IEnumerator SetAnimationTriggerAndWait(string trigger, Action callback, int stateIndex = 0)
        {
            if (_animator == null) yield break;
            _animator.SetTrigger(_hideAnimationTrigger);
            yield return new WaitForEndOfFrame();
            var clip = _animator.GetCurrentAnimatorClipInfo(stateIndex);
            var state = _animator.GetCurrentAnimatorStateInfo(stateIndex);
            var transition = _animator.GetAnimatorTransitionInfo(0);
            
            var longestClip = clip.Max(c => c.clip.length) * state.speed;
            var animationLength = Mathf.Max(longestClip, transition.duration);
            yield return new WaitForSeconds(animationLength);
            callback.Invoke();
        }
        
    }
}