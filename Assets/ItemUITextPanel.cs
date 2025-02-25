using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;


public class ItemUITextPanel : UIPanel
{
    public UITextField itemName;
    public UITextField itemDescription;
    
    
    public UITextField actorField;
    private bool actorFieldVisible => actorField != null && actorField.gameObject != null;
    [ShowIf("actorFieldVisible")] public string actorFieldName;

    public void SetItem(Item item)
    {
        SetPanelInfo(item);
        Open();
    }
    
    private void SetPanelInfo(Item item)
    {

        itemName.text = FormattedText.Parse(item.Name).text;
        itemDescription.text = FormattedText.Parse(item.Description).text;
        if (actorFieldVisible)
        {
            var actor = item.LookupActor(actorFieldName, DialogueManager.masterDatabase);
            actorField.text = actor.Name;
        }
    }
}

