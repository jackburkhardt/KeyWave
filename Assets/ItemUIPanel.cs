using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ItemUIPanel : UIPanel
{
    public UITextField itemName;
    public UITextField itemDescription;
    
    [SerializeField]
    public List<FieldLabel> fieldLabels;

    public Action onHidden;
    private bool _busy = false;

    public void SetPanel(Item item)
    {
        if (_busy) return;
        
        Close();
        
        _busy = true;
        
        onHidden += () =>
        {
            SetPanelInfo(item);
            Open();
        };
    }
    
    protected override void OnVisible()
    {
        _busy = false;
        base.OnVisible();
    }

    protected override void OnHidden()
    {
        onHidden?.Invoke();
        onHidden = null;
        base.OnHidden();
    }
    
    private void SetPanelInfo(Item item)
    {
        itemName.text = item.Name;
        itemDescription.text = item.Description;
        
        foreach (var fieldLabel in fieldLabels)
        {
            var field = item.fields.Find(f => f.title == fieldLabel.fieldName);
            if (field == null) continue;
            fieldLabel.fieldLabel.text = field.value;
        }
    }
}


[Serializable] public class FieldLabel
{
    public string fieldName;
    public UITextField fieldLabel;
}



    
[Serializable] public class FieldComponent
{
    public string fieldName;
    public string fieldValue;
    public GameObject fieldObject;
}
    