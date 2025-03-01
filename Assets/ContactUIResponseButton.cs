using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ContactUIResponseButton : StandardUIResponseButton
{
    public UITextField contactDescription;
    public UIPanel callScreenPanel;
    
    public override Response response
    {
        get { return base.response; }
        set
        {
            base.response = value;
            var itemField = response.destinationEntry.fields.Find( f => f.title == "Contact");
            var item = DialogueManager.masterDatabase.GetItem(int.Parse(itemField.value));
            label.text = item.Name;
            contactDescription.text = item.Description;
        }
    }

    public override void OnClick()
    {
        callScreenPanel.Open();
        base.OnClick();
    }
}
