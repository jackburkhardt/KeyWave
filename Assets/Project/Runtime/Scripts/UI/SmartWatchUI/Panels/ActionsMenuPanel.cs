using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class ActionsMenuPanel : ItemResponsePanel
{
    
    protected override bool ItemIsValid(Item item)
    {
        if (!item.IsAction) return false;
        return ActionLocationIsValid(item) && ActionRequiredActorsAreValid(item);
    }

    protected override void SetDestinationDialogueEntryFields(Item item, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        dialogueEntry.Title = "ACTION";
        dialogueEntry.MenuText = GetAssetDisplayName(item);
        dialogueEntry.DialogueText = string.Empty;
        dialogueEntry.conditionsString = item.IsFieldAssigned("Conditions") ? item.AssignedField("Conditions").value : string.Empty;
        dialogueEntry.fields.Add( new Field(customDialogueUI.showInvalidFieldName, item.LookupValue(customDialogueUI.showInvalidFieldName), FieldType.Boolean));
        dialogueEntry.ActorID = originSubtitle.dialogueEntry.ActorID;
        dialogueEntry.ConversantID = originSubtitle.dialogueEntry.ConversantID;
        
        dialogueEntry.fields.Add(new Field("Action", item.id.ToString(), FieldType.Number));
    }

    private bool ActionLocationIsValid(Item item)
    {
        var actionLocation = item.AssignedField("Location");
        if (actionLocation == null) return true;
            
        var location = DialogueManager.masterDatabase.GetLocation(int.Parse(actionLocation.value));


        var playerLocation = LocationManager.instance.PlayerLocation;

        if (item.IsFieldAssigned("New Sublocation"))
        {
            var rootLocation = DialogueManager.masterDatabase.GetLocation(location.RootID);
            var rootPlayerLocation = LocationManager.instance.PlayerLocation.GetRootLocation();

            return rootLocation == rootPlayerLocation;
        }

        return location == playerLocation;

    }
    private bool ActionRequiredActorsAreValid(Item item)
    {
        var actionRequiredActors = item.fields.Where(p => p.title == "Required Nearby Actor");
            
        foreach (var actionRequiredActor in actionRequiredActors)
        {
            if (actionRequiredActor.value == string.Empty) continue;
            var actor = DialogueManager.masterDatabase.GetActor(int.Parse(actionRequiredActor.value));
            if (actor == null) continue;
                
            var actorLocation = actor.AssignedField("Location");
            if (actorLocation == null) continue;
            if (DialogueManager.masterDatabase.GetLocation(int.Parse(actorLocation.value)) != LocationManager.instance.PlayerLocation) return false;
        }
        return true;
    }
    
    
    
}
