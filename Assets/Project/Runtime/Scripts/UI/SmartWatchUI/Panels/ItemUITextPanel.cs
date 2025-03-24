using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;


public class ItemUITextPanel : UIPanel
{
    public UITextField itemName;
    public UITextField itemDescription;
    public UITextField actorField;
    private bool actorFieldVisible => actorField != null && actorField.gameObject != null;
    [ShowIf("actorFieldVisible")] public string actorFieldName;
    
    
    public bool fadeTextOnContentChange = true;
    [ShowIf("fadeTextOnContentChange")] public float fadeDuration = 0.15f;
    [ShowIf("fadeTextOnContentChange")] public CanvasGroup textCanvasGroup;
    

    public void SetItem(Item item)
    {
        SetPanelInfo(item);
        Open();
    }
    
    private void SetPanelInfo(Item item)
    {
        
        
        itemName.text = FormattedText.Parse(item.Name).text;
           
        if (actorFieldVisible)
        {
            var actor = item.LookupActor(actorFieldName, DialogueManager.masterDatabase);
            actorField.text = actor.Name;
        }

        if (fadeTextOnContentChange)
        {
            textCanvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
            {
                itemDescription.text = FormattedText.Parse(item.Description).text;
                RefreshLayoutGroups.Refresh(transform.gameObject);
                textCanvasGroup.DOFade(1, fadeDuration);
            });
        }
        
        else
        {
            itemDescription.text = FormattedText.Parse(item.Description).text;
            RefreshLayoutGroups.Refresh(transform.gameObject);
        }
    }
}

