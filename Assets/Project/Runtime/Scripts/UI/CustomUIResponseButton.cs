#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIButtonKeyTrigger = PixelCrushers.UIButtonKeyTrigger;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CircularUIButton))]
public class CustomUIResponseButton : StandardUIResponseButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    //private StandardUIResponseButton StandardUIResponseButton => GetComponent<StandardUIResponseButton>();
    
    private static CustomUIResponseButton _hoveredButton;
    private Button UnityButton => GetComponent<Button>();
    private CircularUIButton CircularUIButton => GetComponent<CircularUIButton>();
    private Vector2 Position => label.gameObject.transform.position;
    
    private Item? AssociatedQuest => DialogueManager.Instance.masterDatabase.items.Find(item => item.Name == FollowupEntry?.GetConversation().Title);

    private UIButtonKeyTrigger[] ButtonKeyTriggers => GetComponents<UIButtonKeyTrigger>();

    [SerializeField] private UITextField autonumberText;

    public void SetResponseText()
    {
        //StandardUIResponseButton.label.text = StandardUIResponseButton.response.formattedText.text;
      }

    private Color HighlightColor
    {
        get => UnityButton.colors.highlightedColor;
        set
        {
            var block = UnityButton.colors;
            block.highlightedColor = value;
            UnityButton.colors = block;
        }
    }

   

    public void SetAutonumber()
    {
        int GetAutonumber()
        {
            var buttons = transform.parent.GetComponentsInChildren<CustomUIResponseButton>().ToList();
            // remove all buttons that are invalid
            buttons.RemoveAll(button => !button.transform.gameObject.activeSelf || button.DestinationEntry != null && button.DestinationEntry.conditionsString.Length != 0 &&
                                        !Lua.IsTrue(button.DestinationEntry.conditionsString));
            
            if (!buttons.Contains(this)) return -1;

            var siblingIndices = buttons.Select(button => button.transform.GetSiblingIndex()).OrderBy(n => n).ToList();

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
           if (trigger.key.ToString().Contains("Keypad")) {trigger.key = intToKeyCodeKeypad(autoNumber); continue; }
           trigger.key = intToKeyCodeAlpha(autoNumber);
        }

        if (autoNumber < 0) autonumberText.text = "";
        else if (autoNumber < 10) autonumberText.text = $"{autoNumber}.";
        else autonumberText.text = extraKeys[autoNumber - 10].ToString();
    }
    
    private Color DisabledColor
    {
        get => UnityButton.colors.disabledColor;
        set
        {
            var block = UnityButton.colors;
            block.disabledColor = value;
            UnityButton.colors = block;
        }
    }

    private DialogueEntry? DestinationEntry => response?.destinationEntry;
    private DialogueEntry? FollowupEntry {
        get
        {
            var entry = DestinationEntry?.outgoingLinks.Count == 1
                ? DestinationEntry?.outgoingLinks[0].GetDestinationEntry()
                : null;
            return entry;
        }}

    private bool DialogueEntryInvalid => DestinationEntry != null && DestinationEntry.conditionsString.Length != 0 &&
                                         !Lua.IsTrue(DestinationEntry.conditionsString);


    public Points.Type PointsType
    {
        get
        {
            if (DestinationEntry != null && FollowupEntry != null)
            {
                var conversation = FollowupEntry.GetConversation().Title;
                return QuestUtility.GetPoints(conversation).Type;
                
            }
            return Points.Type.Null;
        }
    } 
    
   // public string TimeEstimate => TimeEstimateText(DialogueEntry);

    private bool isHighlighted;

    public Color ButtonColor
    {
        get => CircularUIButton.image.color;
        set => CircularUIButton.image.color = value;
    }

    private Color BaseColor
    {
        get
        {
            if (DestinationEntry != null) return  DialogueUtility.NodeColor(DestinationEntry);
            return Color.white;
        }
    }

    private void RefreshButton()
    {

        if (DialogueEntryInvalid)
        {
            if ( !Field.LookupBool(DestinationEntry?.fields, "Show If Invalid")) Destroy(gameObject);
            
            
            if (Field.FieldExists(DestinationEntry?.fields, "Invalid Text"))
            {
                var invalidText = Field.LookupValue(DestinationEntry?.fields, "Invalid Text");
                
                label.text = invalidText;
            }
        } 
      
        if (PointsType != Points.Type.Null && isHighlighted && AssociatedQuest?.GetQuestState() == QuestState.Active)
        {
            ButtonColor = Points.Color(PointsType);
        }

        else ButtonColor = BaseColor;
        
        SetAutonumber();
    }
    
    public static void RefreshButtonColors()
    {
        foreach (var button in FindObjectsOfType<CustomUIResponseButton>())
        {
            button.RefreshButton();
        }
    }

 
    private void OnEnable()
    {
        RefreshButton();
        CustomUIMenuPanel.OnCustomResponseButtonClick += OnButtonSubmit;
    }
    
    private void OnDisable()
    {
        CustomUIMenuPanel.OnCustomResponseButtonClick -= OnButtonSubmit;
    }
    
    

    public void OnButtonSubmit(CustomUIResponseButton button)
    {
        if (button == this)
        {
            DisabledColor = HighlightColor;
        }

        {
            label.color = Color.clear;
        }
    }
  
    public string TimeEstimateText
    {
        get
        {
            if (FollowupEntry == null) return "";
            
            if (AssociatedQuest == null) return "";
            
            
            if (DestinationEntry?.GetConversation().Title == AssociatedQuest.Name) return "";
            if (AssociatedQuest.GetQuestState() != QuestState.Active) return "";
            
            var timespan = AssociatedQuest.Timespan("Duration");
            if (timespan <= 0) return "";
            var unit = timespan > 60 ? "minutes" : "seconds";
            var duration = timespan > 60 ? timespan / 60 : timespan;
            return $"{duration} {unit}";
        }
     }


    public void OnPointerEnter(PointerEventData eventData)
    {
        
        isHighlighted = true;
        _hoveredButton = this;
        RefreshButton();
        CustomUIMenuPanel.OnButtonHover(_hoveredButton);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlighted = false;
        RefreshButton();
        StartCoroutine(HoveredButtonCheck());
    }
    
    private IEnumerator HoveredButtonCheck()
    {
        yield return new WaitForEndOfFrame();
        if (_hoveredButton == this) _hoveredButton = null;
        CustomUIMenuPanel.OnButtonHover(_hoveredButton);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CustomUIMenuPanel.ButtonClick(this);
    }
}
