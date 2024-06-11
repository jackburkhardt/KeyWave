using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(CircularUIButton))]

public class CircularUIResponseButton : CustomUIResponseButton
{
    private CircularUIButton CircularUIButton => GetComponent<CircularUIButton>();
    protected CircularUIMenuPanel CircularPanelContainer => (CircularUIMenuPanel)GetComponentInParent(typeof(CircularUIMenuPanel));
    
    public Color ButtonColor
    {
        get => CircularUIButton.image.color;
        set => CircularUIButton.image.color = value;
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
    
    protected override void OnEnable()
    {
        base.OnEnable();
        Refresh();
        StartCoroutine(SetButtonColor());
    }
    
    private IEnumerator SetButtonColor()
    {
        yield return new WaitForEndOfFrame();
        ButtonColor = BaseColor;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        DisabledColor = HighlightColor;
        label.color = Color.Lerp(label.color, Color.clear, 0.25f);
        
        foreach (var customUIResponseButton in SiblingButtons)
        {
            if (customUIResponseButton != this) customUIResponseButton.label.color = Color.clear;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        Refresh();

        if (PointsData.Type != Points.Type.Null && AssociatedQuest?.GetQuestState() == QuestState.Active && PointsData.Points > 0)
        {
            ButtonColor = Points.Color(PointsData.Type);
        }

        else ButtonColor = BaseColor;
        
        CircularPanelContainer.SetPropertiesFromButton(this);
    }
    
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        Refresh();
        ButtonColor = BaseColor;
        StartCoroutine(HoveredButtonCheck());
    }
    
    private IEnumerator HoveredButtonCheck()
    {
        yield return new WaitForEndOfFrame();
        if (_hoveredButton == this) CircularPanelContainer.SetPropertiesFromButton(null);
    }

    private Color BaseColor
    {
        get
        {
            if (response?.destinationEntry != null) return DialogueUtility.NodeColor(response?.destinationEntry);
            return Color.white;
        }
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
    
    private Item? AssociatedQuest => DialogueManager.Instance.masterDatabase.items.Find(item => item.Name == response?.destinationEntry.GetNextDialogueEntry()?.GetConversation().Title);

   private Points.PointsField PointsData
    {
        get
        {
            if (response?.destinationEntry != null && response?.destinationEntry.GetNextDialogueEntry() != null)
            {
                var conversation = response?.destinationEntry.GetNextDialogueEntry()?.GetConversation().Title;
                return QuestUtility.GetPoints(conversation);
                
            }
            return new Points.PointsField();
        }
    }
    
    public string TimeEstimateText
    {
        get
        {
            if (response.destinationEntry.GetNextDialogueEntry() == null) return "";
            
            if (AssociatedQuest == null) return "";
            
            
            if (response.destinationEntry?.GetConversation().Title == AssociatedQuest.Name) return "";
            if (AssociatedQuest.GetQuestState() != QuestState.Active) return "";
            
            var timespan = AssociatedQuest.Timespan("Duration");
            if (timespan <= 0) return "";
            var unit = timespan > 60 ? "minutes" : "seconds";
            var duration = timespan > 60 ? timespan / 60 : timespan;
            return $"{duration} {unit}";
        }
    }
    
   

}
