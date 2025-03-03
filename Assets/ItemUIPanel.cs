using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class ItemUIPanel : UIPanel
{

    public ItemUITextPanel itemUIPanel;
    public ItemUIButton itemButtonTemplate;
    
    public string itemType;
    [Tooltip("If true, the quest state will be used to determine if the item should be shown.")]
    public bool useQuestStateForItemValidity;
    [HideIf("useQuestStateForItemValidity")]
    [Tooltip( "Field to check if the item should be shown.")]
    public string itemValidField;
    [HideIf("useQuestStateForItemValidity")]
    [Tooltip( "If true, the item will be shown if the field is false.")]
    public bool flipValidField;

    public List<UIPanel> panelsToOffset;
    public string offsetAnimatorTrigger;
    public string revertAnimatorTrigger;
    
    public List<Item> items => DialogueManager.masterDatabase.items.Where(item => item.LookupValue("Item Type") == itemType).ToList();
    
    
    public void ShowItemButtons()
    {
        itemButtonTemplate.gameObject.SetActive(false);
        itemUIPanel.Open();
        
        foreach (var item in items)
        {

            if (useQuestStateForItemValidity)
            {
                var questState = QuestLog.GetQuestState(item.Name);
                if (questState == QuestState.Unassigned) continue;
            }

            else
            {
                var itemVisible = item.LookupBool(itemValidField);
                if (flipValidField) itemVisible = !itemVisible;
                if (!itemVisible) continue;
            }
            
            
            var itemButton = Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
            itemButton.SetItem(item);
            itemButton.gameObject.SetActive(true);
            itemButton.target = this.transform;
        }
    }
    
    public override void Open()
    {
        ShowItemButtons();
        base.Open();
        
        foreach (var panel in panelsToOffset)
        {
            panel.GetComponent<Animator>().SetTrigger(offsetAnimatorTrigger);
        }
    }
    
    public override void Close()
    {
        base.Close();
        
        foreach (var panel in panelsToOffset)
        {
            panel.GetComponent<Animator>().SetTrigger(revertAnimatorTrigger);
        }
    }

    protected override void OnHidden()
    {
        foreach (Transform child in itemButtonTemplate.transform.parent)
        {
            if (child == itemButtonTemplate.transform) continue;
            Destroy(child.gameObject);
        }
        base.OnHidden();
    }
    
    public void OnClick(Item item)
    {
        
        if (item.FieldExists("State"))
        {
            QuestLog.SetQuestState(item.Name, QuestState.Success);
        }
        
        
        itemUIPanel.SetItem(item);
    }
    
}

