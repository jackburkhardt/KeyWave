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

public class ItemUIMenuPanel : UIPanel
{

    public ItemUIPanel itemUIPanel;
    public ItemUIButton itemButtonTemplate;
    
    public string itemType;
    public string itemValidField;
    public bool flipValidField;
    
    public List<Item> items => DialogueManager.masterDatabase.items.Where(item => item.LookupValue("Item Type") == itemType).ToList();
    
    
    public void ShowItemButtons()
    {
        itemButtonTemplate.gameObject.SetActive(false);
        
        foreach (var item in items)
        {
            var itemVisible = item.LookupBool(itemValidField);
            if (flipValidField) itemVisible = !itemVisible;
            if (!itemVisible) continue;
            
            var itemButton = Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
            itemButton.SetButtonFromItem(item, itemUIPanel);
            itemButton.gameObject.SetActive(true);
        }
    }
    
    public override void Open()
    {
        ShowItemButtons();
        base.Open();
    }
    
    
    
    
    
}
