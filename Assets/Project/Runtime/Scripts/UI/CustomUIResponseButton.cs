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
using UnityEngine.UI;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;
using UIButtonKeyTrigger = PixelCrushers.UIButtonKeyTrigger;

namespace Project.Runtime.Scripts.UI
{
    public class CustomUIResponseButton : StandardUIResponseButton, IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        //private StandardUIResponseButton StandardUIResponseButton => GetComponent<StandardUIResponseButton>();
        
      

        protected static CustomUIResponseButton _hoveredButton;

        [Foldout("Custom Fields")]
        [SerializeField] protected Graphic nodeColorChameleon;
        
        public Graphic NodeColorChameleon => nodeColorChameleon;
        
        private bool chameleonNotNull => nodeColorChameleon != null;

        [ShowIf("chameleonNotNull")] [SerializeField] private string chameleonField = "UseNodeColorInMenu";

        [Foldout("Custom Fields")]
        [SerializeField] private Image icon;
        
        
        public Image Icon => icon;
        
        private bool iconNotNull => icon != null;
        [ShowIf("chameleonNotNull")] [SerializeField] private string iconField = "Icon";
        
        
        protected Color defaultImageColor;
        public Color DefaultImageColor => defaultImageColor;

        [Foldout("Custom Fields")]
        [SerializeField] protected string _autoNumberFormat = "{0}. {1}";

        [Foldout("Custom Fields")]
        [SerializeField] protected CustomUIMenuPanel MenuPanelContainer;

        [Foldout("Custom Fields")]
        [SerializeField] protected UITextField actorNameText;
        
        public string ActorName => actorNameText.text;
        
        [Foldout("Custom Fields")]
        [SerializeField] protected UITextField conversantNameText;
        
        public string ConversantName => conversantNameText.text;
        
        [Foldout("Custom Fields")]
        [SerializeField] protected UITextField autonumberText;
        
        [Foldout("Custom Fields")]
        [SerializeField] private Image _notificationBadge;
        
        [Foldout("Custom Fields")]
        [SerializeField] private bool useLocation;
        
        [ShowIf("useLocation")]
        [Foldout("Custom Fields")]
        [SerializeField] private string locationField = "Location";
        
        [ShowIf("useLocation")]
        [Foldout("Custom Fields")]
        [SerializeField] private bool useCoordinates = true;
        
        [ShowIf("useLocation")]
        [Foldout("Custom Fields")]
        [SerializeField] private UITextField locationLabel;
        
        [ShowIf("useLocation")]
        [Foldout("Custom Fields")]
        [SerializeField] private UITextField ETALabel;
        
        
        
        
        [Foldout("Animation")]
        [SerializeField] private Animator _animator;
        
        [Foldout("Animation")]
        [SerializeField] private string _showAnimationTrigger = "Show";
        [Foldout("Animation")]
        [SerializeField] private string _hideAnimationTrigger = "Hide";
        
        
        
        [Foldout("Animation")]
        [Tooltip("Wait for the hide animation to finish before firing the StandardUIResponseButton's OnClick event.")]
        [SerializeField] private bool _waitForHideAnimation;
        
        

        public string simStatus
        {
            get;
            private set;
        }
        
      
       
        protected Button UnityButton => GetComponent<Button>();
        protected Vector2 Position => label.gameObject.transform.position;

        
       


        protected UIButtonKeyTrigger[] ButtonKeyTriggers => GetComponents<UIButtonKeyTrigger>();

        protected List<CustomUIResponseButton>? SiblingButtons =>
            transform != null && transform.parent != null ? transform.parent.GetComponentsInChildren<CustomUIResponseButton>().ToList() : null;

        private bool DialogueEntryInvalid => response?.destinationEntry?.conditionsString?.Length != 0 &&
                                             !Lua.IsTrue(response?.destinationEntry?.conditionsString);


        protected virtual void OnEnable()
        {
            Refresh();
            if (MenuPanelContainer == null) MenuPanelContainer = GetComponentInParent<CustomUIMenuPanel>(true);
            
            if (_animator != null && !string.IsNullOrEmpty(_showAnimationTrigger))
            {
                _animator.SetTrigger(_showAnimationTrigger);
            }
            
        }
        
        private void Awake()
        {
            defaultImageColor = nodeColorChameleon != null ? nodeColorChameleon.color : defaultColor;
          
        }

        private void OnDisable()
        {
          //  if (this != MenuPanelContainer.buttonTemplate && this.transform.parent == MenuPanelContainer.buttonTemplate.transform.parent) Destroy(gameObject);
        }


        public virtual void OnPointerClick(PointerEventData eventData)
        {
       
        }


        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _hoveredButton = this;
            simStatus = response?.destinationEntry?.SimStatus()!;
            Refresh();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            StartCoroutine(DelaydRefreshOnExit());
        }

        private IEnumerator DelaydRefreshOnExit()
        {
            yield return new WaitForEndOfFrame();
            if (_hoveredButton != this)
            {
                Field.SetValue(response?.destinationEntry?.fields, "Show Badge", false);
                if (_notificationBadge != null && _notificationBadge.gameObject.activeSelf) DialogueManager.instance.gameObject.BroadcastMessage("OnNewOptionSelected", response?.destinationEntry, SendMessageOptions.DontRequireReceiver);
            }
            Refresh();
        }
        
        

        public void GoToResponse()
        {
            var entry = response.destinationEntry.GetNextDialogueEntry();
            if (DialogueManager.instance == null || DialogueManager.instance.conversationModel == null)
            {
                var nextConversation = entry.GetConversation();
                DialogueManager.StartConversation(nextConversation.Title);
            }

            else
            {
                var state = DialogueManager.instance.conversationModel.GetState(entry);
                DialogueManager.conversationController.GotoState(state);
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

            if (autonumberText.gameObject == null)
            {
                if (_autoNumberFormat.Contains("{0}")) label.text = _autoNumberFormat.Replace("{0}", $"{autoNumber}");
                if (_autoNumberFormat.Contains("{1}")) label.text = label.text.Replace("{1}", response?.formattedText.text);
            }

            else
            {
                if (autoNumber < 0) autonumberText.text = "";
                else if (autoNumber < 10) autonumberText.text = _autoNumberFormat.Replace("{0}", $"{autoNumber}");
                else autonumberText.text = _autoNumberFormat.Replace("{0}", $"{extraKeys[autoNumber - 10]}");
                autonumberText.text.Replace("{1}", string.Empty);
            }
        }

        protected void OnValidate()
        {
            Refresh();
            
            
            MenuPanelContainer ??= GetComponentInParent<CustomUIMenuPanel>(true);
            
        }

        public virtual void Refresh()
        {
       
            if (MenuPanelContainer != null && MenuPanelContainer.buttonTemplate == this) return;

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
                if (nodeColorChameleon != null)
                {
                    var color = defaultImageColor;
                    
                    if (!response.enabled && Field.FieldExists(response.destinationEntry.fields, "DisabledColor"))
                    {
                        color = Tools.WebColor(
                            Field.LookupValue(response.destinationEntry.fields, "DisabledColor"));
                    }
                    
                    else

                    if (Field.FieldExists(response.destinationEntry.fields, chameleonField)
                        && Field.LookupBool(response.destinationEntry.fields, chameleonField))
                    {
                        if (Field.FieldExists(response.destinationEntry.fields, "NodeColor"))
                        {
                            color = Tools.WebColor(
                                Field.LookupValue(response.destinationEntry.fields, "NodeColor"));
                        }
                
                        else if (Field.FieldExists(response.destinationEntry.GetActor().fields, "NodeColor"))
                        {
                            color = Tools.WebColor(
                                Field.LookupValue(response.destinationEntry.GetActor().fields, "NodeColor"));
                        }
                    }
                
                    nodeColorChameleon.color = color;
                    
                    if (!button.interactable) nodeColorChameleon.color = Color.Lerp(nodeColorChameleon.color, Color.black, 0.5f);
                }
                
                if (icon != null)
                {
                    if (Field.FieldExists(response.destinationEntry.fields, iconField))
                    {
                        var iconPath = Field.LookupValue(response.destinationEntry.fields, iconField);
                        if (string.IsNullOrEmpty(iconPath)) icon.gameObject.SetActive(false);
                        else
                        {
                            icon.gameObject.SetActive(true);
                            AddressableLoader.RequestLoad<Sprite>(iconPath, sprite =>
                            {
                                icon.sprite = sprite;
                            });
                        }
                    }
                }
                
                if (actorNameText != null)
                {
                    if (response.destinationEntry.GetActor() != null)
                    {
                        actorNameText.text = response.destinationEntry.GetActor().Name;
                    }
                    else actorNameText.text = string.Empty;
                }
                
                if (conversantNameText != null)
                {
                    if (response.destinationEntry.GetConversant() != null)
                    {
                        conversantNameText.text = response.destinationEntry.GetConversant().Name;
                    }
                    else conversantNameText.text = string.Empty;
                }
                
                label.text = response.formattedText.text;
                
                
                if (useLocation && !string.IsNullOrEmpty(locationField) && Field.FieldExists(response.destinationEntry.fields, locationField))
                {
                    var locationIndex = Field.LookupInt(response.destinationEntry.fields, locationField);
                        
                    var location = Location.FromString(DialogueManager.instance.masterDatabase.locations[locationIndex].Name); 
                    
                    //Debug.Log(Field.LookupValue(response.destinationEntry.fields, locationField));
                    
                    if (location != null)
                    {
                        if (locationLabel != null) locationLabel.text = location.Name;
                        if (ETALabel != null) ETALabel.text = $"ETA {Clock.EstimatedTimeOfArrival(location)}";
                        if (useCoordinates)
                        {
                            transform.localPosition = location.coordinates;
                        }
                    }
                    
                    else Debug.LogWarning($"Location not found: {locationIndex}");
                }
                
                
                
              //  Debug.Log($"Response: { label.text}");
            }
        
            SetAutonumber();


            if (Field.FieldExists(response?.destinationEntry?.fields, "Show Badge") && Field.LookupBool(response?.destinationEntry?.fields, "Show Badge"))
            {
                if (_notificationBadge != null) _notificationBadge.gameObject.SetActive(true);
            }
            else
            {
                if (_notificationBadge != null) _notificationBadge.gameObject.SetActive(false);
            }
            
            
            if (MenuPanelContainer == null) MenuPanelContainer = GetComponentInParent<CustomUIMenuPanel>(true);
            
            
            //buttonStyles
            
            
            
           
            
            
        }


        public override void OnClick()
        {




            if (_animator != null && !string.IsNullOrEmpty(_hideAnimationTrigger))
            {
                if (_waitForHideAnimation)
                    StartCoroutine(SetAnimationTriggerAndWait(_hideAnimationTrigger, DoClick));
                else DoClick();
            }
            else DoClick();

            void DoClick()
            {
                 
                if (MenuPanelContainer != null && MenuPanelContainer.accumulateResponse && MenuPanelContainer.accumulatedResponseContainer != null)
                {
                    MenuPanelContainer.OnChoiceClick(this);
                }
                
                base.OnClick();
            }
            
            

        }
        
        private IEnumerator SetAnimationTriggerAndWait(string trigger, Action callback, int stateIndex = 0)
        {
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