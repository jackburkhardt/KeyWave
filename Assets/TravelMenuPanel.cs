using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class TravelMenuPanel : LocationResponsePanel
{
    protected override void SetDestinationDialogueEntryFields(Location location, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        dialogueEntry.MenuText = location.Name;
        dialogueEntry.fields.Add(new Field("Location", location.id.ToString(), FieldType.Number));
        dialogueEntry.conditionsString = $"PlayerLocation() ~= \"{location.Name}\"";
    }
    
    protected override bool LocationIsValid(Location location)
    {
        return !location.IsSublocation;
    }
}
