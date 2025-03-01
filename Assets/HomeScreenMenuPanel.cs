using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class HomeScreenMenuPanel : ItemResponsePanel
{

    protected override void SetDestinationDialogueEntryFields(Item item, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        dialogueEntry.MenuText = GetAssetDisplayName(item);
        dialogueEntry.fields.Add( new Field("App", item.id.ToString(), FieldType.Number));
        dialogueEntry.ActorID = originSubtitle.dialogueEntry.ActorID;
    }

    protected override bool ItemIsValid(Item item)
    {
        return item.IsApp && !item.LookupBool("Is Default");
    }
    
    protected override void SetFollowupConversationOrDialogueEntries(Item item, ref DialogueEntry dialogueEntry)
    {
        var conversation = DialogueManager.masterDatabase.GetConversation(dialogueEntry.conversationID);
        var followUpEntry = Template.FromDefault().CreateDialogueEntry(Template.FromDefault().GetNextDialogueEntryID(conversation), dialogueEntry.conversationID, string.Empty);

        followUpEntry.ActorID = dialogueEntry.ActorID;
        followUpEntry.userScript = item.LookupValue("Script");
        
        dialogueEntry.outgoingLinks.Add(new Link(dialogueEntry.conversationID, dialogueEntry.id, dialogueEntry.conversationID, followUpEntry.id));
        conversation.dialogueEntries.Add(followUpEntry);

        if (item.LookupBool("Force Response Menu"))
        {
            var responseMenuEntry = Template.FromDefault().CreateDialogueEntry(
                Template.FromDefault().GetNextDialogueEntryID(conversation), dialogueEntry.conversationID,
                string.Empty);
            responseMenuEntry.Title = "RESPONSE MENU";
            responseMenuEntry.MenuText = "[f]()";
            responseMenuEntry.ActorID = dialogueEntry.ActorID;

            followUpEntry.outgoingLinks.Add(new Link(followUpEntry.conversationID, followUpEntry.id,
                followUpEntry.conversationID, responseMenuEntry.id));
            conversation.dialogueEntries.Add(responseMenuEntry);
        }

        else
        {
            dialogueEntry.fields.Add(new Field("Style", "Inbox", FieldType.Number));
        }

    }
    
}
