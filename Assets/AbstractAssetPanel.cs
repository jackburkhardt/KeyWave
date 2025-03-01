using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public abstract class AbstractAssetPanel : CustomUIMenuPanel
{
    protected CustomDialogueUI customDialogueUI;
    protected abstract List<Asset> assetList { get; }

    protected abstract bool AssetIsValid(Asset asset);
    protected abstract void SetFollowupConversationOrDialogueEntries(Asset asset, ref DialogueEntry dialogueEntry);
    protected abstract void SetDestinationDialogueEntryFields(Asset asset, Subtitle originSubtitle, ref DialogueEntry dialogueEntry);
   
    public override void Open()
    {
        customDialogueUI ??= FindObjectOfType<CustomDialogueUI>();
        customDialogueUI.ForceOverrideMenuPanel( this);
        base.Open();
    }

    public override void Close()
    {
        customDialogueUI.ClearForcedMenuOverride(this);
        base.Close();
    }
    
    protected virtual Response[] GetAssetResponses(Subtitle subtitle)
    {
        customDialogueUI ??= FindObjectOfType<CustomDialogueUI>();
        var newResponses = new List<Response>();
            
        foreach (var asset in assetList.Where(AssetIsValid))
        {
            var template = Template.FromDefault();
            var newDialogueEntry = template.CreateDialogueEntry( template.GetNextDialogueEntryID( subtitle.dialogueEntry.GetConversation()), subtitle.dialogueEntry.conversationID, string.Empty);
                
            SetDestinationDialogueEntryFields( asset, subtitle, ref newDialogueEntry);
            SetFollowupConversationOrDialogueEntries( asset, ref newDialogueEntry);
                
            var newResponse = new Response(new FormattedText(newDialogueEntry.MenuText), newDialogueEntry,
                newDialogueEntry.conditionsString == string.Empty || Lua.IsTrue($"{newDialogueEntry.conditionsString}"));
            newResponses.Add(newResponse);
        }
        return newResponses.ToArray();
    }

    
    
    protected override void ShowResponsesNow(Subtitle subtitle, Response[] responses, Transform target)
    {
        
        var generatedResponses = GetAssetResponses(subtitle);
        
        foreach (var newResponse in generatedResponses)
        {
            subtitle.dialogueEntry.outgoingLinks.Add(new Link(subtitle.dialogueEntry.conversationID, subtitle.dialogueEntry.id, newResponse.destinationEntry.conversationID, newResponse.destinationEntry.id));
        }

        responses = generatedResponses;
        responses = customDialogueUI.CheckInvalidResponses(responses);
        
        base.ShowResponsesNow(subtitle, responses, target);
    }
    
    protected virtual string GetAssetDisplayName(Asset asset)
    {
        var conditionalDisplayEntryCount = asset.LookupInt("Conditional Display Entry Count");
            
        if (conditionalDisplayEntryCount > 0)
        {
            for (int i = 1; i < conditionalDisplayEntryCount + 1; i++)
            {
                var displayEntry = asset.AssignedField($"Conditional Display Entry {i}");
                if (displayEntry == null) continue;
                    
                    
                var condition = asset.LookupValue( $"Conditional Display Entry {i} Conditions");
                    
                if (Lua.IsTrue(condition) && !string.IsNullOrEmpty(condition) && condition != "true")
                {
                    return displayEntry.value;
                }
            }
        }
            
        return asset.IsFieldAssigned( "Display Name") ? asset.LookupValue("Display Name") : asset.Name;
    }
}

public abstract class ItemResponsePanel : AbstractAssetPanel
{
    protected virtual bool ItemConversationIsValid(Item item, out string conversationTitle)
    {
        var conversation = item.FieldExists("Conversation");
        if (!conversation)
        {
            conversationTitle = null;
            //     Debug.Log("No conversation assigned to action: " + item.Name);
            return false;
        }

        if (item.IsFieldAssigned("Entry Count")) conversationTitle = string.Empty;
        else conversationTitle = item.LookupValue("Conversation");
            
        return true;
    }

    protected override List<Asset> assetList => new List<Asset>(DialogueManager.masterDatabase.items);
    
    protected override void SetDestinationDialogueEntryFields(Asset asset, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        SetDestinationDialogueEntryFields(asset as Item, originSubtitle, ref dialogueEntry);
    }
    
    protected abstract void SetDestinationDialogueEntryFields(Item item, Subtitle originSubtitle, ref DialogueEntry dialogueEntry);
    
    protected override bool AssetIsValid(Asset asset)
    {
        return ItemIsValid(asset as Item);
    }
    
    protected abstract bool ItemIsValid(Item item);

    protected virtual void SetFollowupConversationOrDialogueEntries(Item item, ref DialogueEntry dialogueEntry)
    {
        if (ItemConversationIsValid(item, out var conversationTitle))
        {
            dialogueEntry.outgoingLinks = new List<Link>();

            //  Debug.Log("Action conversation is valid: " + conversationTitle);

            if (conversationTitle == string.Empty)
            {
                var newConversation = GameManager.GenerateConversation(item, item.RepeatCount > 0);
                DialogueManager.masterDatabase.conversations.Add(newConversation);
                dialogueEntry.outgoingLinks.Add(new Link(dialogueEntry.conversationID,
                    dialogueEntry.id, newConversation.id, 0));
            }

            else if (conversationTitle != null)
            {
                var conversation = DialogueManager.masterDatabase.GetConversation(conversationTitle);
                dialogueEntry.outgoingLinks.Add(new Link(dialogueEntry.conversationID,
                    dialogueEntry.id, conversation.id, 0));
            }
        }
    }
    
    protected override void SetFollowupConversationOrDialogueEntries(Asset asset, ref DialogueEntry dialogueEntry)
    {
        SetFollowupConversationOrDialogueEntries( asset as Item, ref dialogueEntry);
    }
}


public abstract class LocationResponsePanel : AbstractAssetPanel
{
    protected override List<Asset> assetList => new List<Asset>(DialogueManager.masterDatabase.locations);
    
    protected override void SetDestinationDialogueEntryFields(Asset asset, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        SetDestinationDialogueEntryFields(asset as Location, originSubtitle, ref dialogueEntry);
    }
    
    protected abstract void SetDestinationDialogueEntryFields(Location location, Subtitle originSubtitle, ref DialogueEntry dialogueEntry);
    
    protected override bool AssetIsValid(Asset asset)
    {
        return LocationIsValid(asset as Location);
    }
    
    protected abstract bool LocationIsValid(Location location);

    protected override void SetFollowupConversationOrDialogueEntries(Asset asset, ref DialogueEntry dialogueEntry)
    {
        // do nothing
    }
}

