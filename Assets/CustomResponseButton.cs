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

    private void RefreshButtonColor()
    {
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
            button.RefreshButtonColor();
        }
    }

 
    private void OnEnable()
    {
        RefreshButtonColor();
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
        RefreshButtonColor();
    }

    public void OnButtonSubmit(CustomResponseButton button)
    {
        CustomResponsePanel.Instance.OnPlayerResponse(button);
    }


    public void OnButtonHover(CustomResponseButton button)
    {
        isHighlighted = button == this;
        RefreshButtonColor();
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
