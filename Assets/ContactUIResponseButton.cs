using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ContactUIResponseButton : StandardUIResponseButton
{
    public UITextField contactDescription;
    public PhoneCallPanel callScreenPanel;
    private Item _contact;
    
    public override Response response
    {
        get { return base.response; }
        set
        {
            base.response = value;
            var itemField = response.destinationEntry.fields.Find( f => f.title == "Contact");
            _contact = DialogueManager.masterDatabase.GetItem(int.Parse(itemField.value));
            label.text = _contact.Name;
            contactDescription.text = _contact.Description;
        }
    }

    public override void OnClick()
    {
        callScreenPanel.Open();
        callScreenPanel.SetContactInfo( _contact);
        base.OnClick();
    }
}
