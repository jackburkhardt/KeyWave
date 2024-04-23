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
    private DialogueEntry DialogueEntry
    {
        get
        {
            if (StandardUIResponseButton != null && StandardUIResponseButton.response != null) return StandardUIResponseButton.response.destinationEntry;
            return null;
        }

}

    private bool DialogueEntryInvalid => DialogueEntry != null && DialogueEntry.conditionsString.Length != 0 &&
                                         !Lua.IsTrue(DialogueEntry.conditionsString);

    private void Start()
    {
      
    }

    public Points.Type PointsType
    {
        get
        {
            if (DialogueEntry != null) return DialogueUtility.GetPointsField(DialogueEntry).Type;
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
            if (DialogueEntry != null) return  DialogueUtility.NodeColor(DialogueEntry);
            return Color.white;
        }
    }

    private void RefreshButton()
    {

        if (DialogueEntryInvalid)
        {
            if ( !Field.LookupBool(DialogueEntry.fields, "Show If Invalid")) Destroy(gameObject);
            
            
            if (Field.FieldExists(DialogueEntry.fields, "Invalid Text"))
            {
                var invalidText = Field.LookupValue(DialogueEntry.fields, "Invalid Text");
                
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
