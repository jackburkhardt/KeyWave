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
            
            
            description.text = GetConditionalDisplayDescription(action);
            if (description.text == "") description.gameObject.SetActive(false);
        }
    }
    
    
    private string GetConditionalDisplayDescription(Item action)
    {
        
        var conditionalDisplayEntryCount = action.LookupInt("Conditional Display Entry Count");
            
        if (conditionalDisplayEntryCount > 0)
        {
            for (int i = 1; i < conditionalDisplayEntryCount + 1; i++)
            {
                var displayEntry = action.AssignedField($"Conditional Display Entry {i}");
                if (displayEntry == null) continue;
                    
                    
                var condition = action.LookupValue( $"Conditional Display Entry {i} Conditions");
                    
                if (Lua.IsTrue(condition) && !string.IsNullOrEmpty(condition) && condition != "true")
                {
                    return action.LookupValue($"Conditional Display Entry {i} Description");
                }
            }
        }

        return action.Description;

    }
}
