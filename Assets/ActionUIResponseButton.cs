using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ActionUIResponseButton : StandardUIResponseButton
{
    public UITextField description;

    public override Response response
    {
        get { return base.response; }
        set
        {
            base.response = value;
            var actionField =  response.destinationEntry.fields.Find( f => f.title == "Action");
            var action = DialogueManager.masterDatabase.GetItem( int.Parse(actionField.value));
            description.text = action.LookupValue("Description");
            if (description.text == "") description.gameObject.SetActive(false);
        }
    }
}
