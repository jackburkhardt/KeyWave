using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIButton : MonoBehaviour
{
    [ShowIf("showInfoBox")]
    [HelpBox("Make sure that the button has an OnClick event that calls the OnClick method.",
        HelpBoxMessageType.Warning)]
    [ReadOnly]
    public string infoBox;
    
    public Button button;
    
    public List< FieldLabel> fieldLabels;
    
    private ItemUIPanel _panel;
    private Item _item;

    public void SetButtonFromItem(Item item, ItemUIPanel panel)
    {
        _panel = panel;

        foreach (var fieldLabel in fieldLabels)
        {
            var field = item.fields.Find(f => f.title == fieldLabel.fieldName);
            if (field == null) continue;
            fieldLabel.fieldLabel.text = field.value;
        }
    }
    
    public void OnClick()
    {
        _panel.SetPanel(_item);
    }

    private bool showInfoBox => button.onClick.GetPersistentEventCount() == 0;
 

    public void OnEnable()
    {
        if (button.onClick.GetPersistentEventCount() == 0)
        {
            button.onClick.AddListener(OnClick);
        }
    }
}
