using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StandardUIResponseButton))]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CircularUIButton))]
public class CustomResponseButton : MonoBehaviour
{
    
    private StandardUIResponseButton StandardUIResponseButton => GetComponent<StandardUIResponseButton>();
    private Button UnityButton => GetComponent<Button>();
    private CircularUIButton CircularUIButton => GetComponent<CircularUIButton>();
    private Vector2 Position => StandardUIResponseButton.label.gameObject.transform.position;
    
    
    private DialogueEntry DestinationEntry
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

    private void Start()
    {
      
    }

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
        CustomResponsePanel.OnCustomResponseButtonClick += OnButtonClick;
    }
    
    private void OnDisable()
    {
        CustomResponsePanel.OnCustomResponseButtonClick -= OnButtonClick;
    }
    
    
    private void OnButtonClick(CustomResponseButton button)
    {
        if (button == this)
        {
            
        }

        else
        {
            StandardUIResponseButton.label.color = Color.clear;
        }
    }
    
    public void OnButtonExit(CustomResponseButton button)
    {
        isHighlighted = false;
        RefreshButton();
    }

    public void OnButtonSubmit(CustomResponseButton button)
    {
        CustomResponsePanel.Instance.OnPlayerResponse(button);
    }


    public void OnButtonHover(CustomResponseButton button)
    {
        isHighlighted = button == this;
        RefreshButton();
    }
    
    private static string TimeEstimateText(DialogueEntry dialogueEntry)
    {
        if (!Field.FieldExists(dialogueEntry.fields, "Time Estimate")) return "";
        
        var estimate = DialogueUtility.TimeEstimate(dialogueEntry);
        if (estimate.Item1 > estimate.Item2) return "";
        if (estimate.Item1 == estimate.Item2) return $"{estimate.Item1 / 60} minutes";
        return $"{estimate.Item1 / 60}-{estimate.Item2 / 60} minutes";
    }
    
    
}
