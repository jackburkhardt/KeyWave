using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class TravelMenuPanel : LocationResponsePanel
{
    protected override void SetDestinationDialogueEntryFields(Location location, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        dialogueEntry.MenuText = location.Name;
        dialogueEntry.fields.Add(new Field("Location", location.id.ToString(), FieldType.Number));
        dialogueEntry.conditionsString = $"PlayerLocation() ~= \"{location.Name}\"";
        dialogueEntry.ActorID = DialogueManager.masterDatabase.actors.First(p => p.IsPlayer).id;
    }
    
    protected override bool LocationIsValid(Location location)
    {
        return !location.IsSublocation;
    }
}
