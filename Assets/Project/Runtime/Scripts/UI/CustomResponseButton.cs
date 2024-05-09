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

[RequireComponent(typeof(StandardUIResponseButton))]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CircularUIButton))]
public class CustomResponseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    private StandardUIResponseButton StandardUIResponseButton => GetComponent<StandardUIResponseButton>();
    private Button UnityButton => GetComponent<Button>();
    private CircularUIButton CircularUIButton => GetComponent<CircularUIButton>();
    private Vector2 Position => StandardUIResponseButton.label.gameObject.transform.position;

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
            var buttons = transform.parent.GetComponentsInChildren<CustomResponseButton>().ToList();
            // remove all buttons that are invalid
            buttons.RemoveAll(button => !button.transform.gameObject.activeSelf || button.DestinationEntry != null && button.DestinationEntry.conditionsString.Length != 0 &&
                                        !Lua.IsTrue(button.DestinationEntry.conditionsString));
            
            if (!buttons.Contains(this)) return -1;

            var siblingIndices = buttons.Select(button => button.transform.GetSiblingIndex()).OrderBy(n => n).ToList();

            var siblingIndex = transform.GetSiblingIndex();
            
            
            var autoNumber = siblingIndices.IndexOf(siblingIndex) == 9 ? 0 : siblingIndices.IndexOf(siblingIndex) + 1;
          
            return autoNumber;

        }
        
        KeyCode intToKeyCodeAlpha(int i)
        {
            if (i < 0 || i > 9) return KeyCode.None;
            var key = (KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + i);
            return key;
        }

        KeyCode intToKeyCodeKeypad(int i)
        {
            if (i < 0 || i > 9) return KeyCode.None;
            var key = (KeyCode)Enum.Parse(typeof(KeyCode), "Keypad" + i);
            return key;
        }
        
        
        var autoNumber = GetAutonumber();
        
        foreach (var trigger in ButtonKeyTriggers)
        { 
           if (trigger.key.ToString().Contains("Alpha")) trigger.key = intToKeyCodeAlpha(autoNumber);
           if (trigger.key.ToString().Contains("Keypad")) trigger.key = intToKeyCodeKeypad(autoNumber);
        }

        if (autoNumber < 0 || autoNumber > 9) autonumberText.text = "";
        else autonumberText.text = $"{autoNumber}.";
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

    public DialogueEntry DestinationEntry
    {
        get
        {
            if (StandardUIResponseButton != null && StandardUIResponseButton.response != null) return StandardUIResponseButton.response.destinationEntry;
            return null;
        }

}

    private DialogueEntry FollowupEntry
    {
        get
        {
            if (DestinationEntry != null && DestinationEntry.outgoingLinks.Count == 1) {
                return DialogueUtility.GetDialogueEntryByLink(DestinationEntry.outgoingLinks[0]);
            }
            return null;
        }
    }

    private bool DialogueEntryInvalid => DestinationEntry != null && DestinationEntry.conditionsString.Length != 0 &&
                                         !Lua.IsTrue(DestinationEntry.conditionsString);


    public Points.Type PointsType
    {
        get
        {
            if (DestinationEntry != null && FollowupEntry != null)
            {
                var conversation = DialogueUtility.GetConversationByDialogueEntry(FollowupEntry).Title;
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
            if ( !Field.LookupBool(DestinationEntry.fields, "Show If Invalid")) Destroy(gameObject);
            
            
            if (Field.FieldExists(DestinationEntry.fields, "Invalid Text"))
            {
                var invalidText = Field.LookupValue(DestinationEntry.fields, "Invalid Text");
                
                StandardUIResponseButton.label.text = invalidText;
            }
        } 
        
        
        
        if (PointsType != Points.Type.Null && isHighlighted)
        {
            ButtonColor = Points.Color(PointsType);
        }

        else ButtonColor = BaseColor;
        
        SetAutonumber();
    }
    
    public static void RefreshButtonColors()
    {
        foreach (var button in FindObjectsOfType<CustomResponseButton>())
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
    
    

    public void OnButtonSubmit(CustomResponseButton button)
    {
        if (button == this)
        {
            DisabledColor = HighlightColor;
        }

        else
        {
            StandardUIResponseButton.label.color = Color.clear;
        }
    }


  
    private static string TimeEstimateText(DialogueEntry dialogueEntry)
    {
        if (!Field.FieldExists(dialogueEntry.fields, "Time Estimate")) return "";
        
        var estimate = DialogueUtility.TimeEstimate(dialogueEntry);
        if (estimate.Item1 > estimate.Item2) return "";
        if (estimate.Item1 == estimate.Item2) return $"{estimate.Item1 / 60} minutes";
        return $"{estimate.Item1 / 60}-{estimate.Item2 / 60} minutes";
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        isHighlighted = true;
        RefreshButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlighted = false;
        RefreshButton();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CustomUIMenuPanel.ButtonClick(this);
    }
}
