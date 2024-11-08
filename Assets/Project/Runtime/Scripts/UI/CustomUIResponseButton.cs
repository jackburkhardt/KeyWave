#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Utility;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIButtonKeyTrigger = PixelCrushers.UIButtonKeyTrigger;

namespace Project.Runtime.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class CustomUIResponseButton : StandardUIResponseButton, IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        //private StandardUIResponseButton StandardUIResponseButton => GetComponent<StandardUIResponseButton>();

        protected static CustomUIResponseButton _hoveredButton;

        [SerializeField] private Graphic nodeColorChameleon;
        
        private bool chameleonNotNull => nodeColorChameleon != null;

        [ShowIf("chameleonNotNull")] [SerializeField] private string chameleonField = "UseNodeColorInMenu";

        [SerializeField] private Image icon;
        
        private bool iconNotNull => icon != null;
        [ShowIf("chameleonNotNull")] [SerializeField] private string iconField = "Icon";
        
        
        private Color defaultImageColor;

        [SerializeField] protected string _autoNumberFormat = "{0}. {1}";

        [SerializeField] protected CustomUIMenuPanel MenuPanelContainer;

        [SerializeField] protected UITextField actorNameText;
        
        [SerializeField] protected UITextField conversantNameText;
        
        [SerializeField] protected UITextField autonumberText;

        

         [SerializeField] private Image _notificationBadge;

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


        public virtual void Refresh()
        {
       
            if (MenuPanelContainer.buttonTemplate == this) return;

            if (DialogueEntryInvalid)
            {
               // if ( !Field.LookupBool(response?.destinationEntry?.fields, "Show If Invalid")) Destroy(gameObject);
                if (Field.FieldExists(response?.destinationEntry?.fields, "Invalid Text"))
                {
                    var invalidText = Field.LookupValue(response?.destinationEntry?.fields, "Invalid Text");
                
                   //abel.text = invalidText;
                }
            }

            else if (response != null && response.destinationEntry != null)
            {
                if (nodeColorChameleon != null)
                {
                    var color = defaultImageColor;

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
        }
    }
}