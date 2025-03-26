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

    /// <summary>
    /// Checks if an asset is valid. If it is not, it will not be displayed in the response menu, regardless of other conditions.
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    protected abstract bool AssetIsValid(Asset asset);
    
    /// <summary>
    /// Sets up the followup conversation or dialogue entries that will be linked to the response.
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="dialogueEntry"></param>
    protected abstract void SetupFollowupConversationOrDialogueEntries(Asset asset, ref DialogueEntry dialogueEntry);
    
    /// <summary>
    ///  Sets the fields of the dialogue entries that will be created as responses.
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="originSubtitle">The subtitle that precedes the response menu.</param>
    /// <param name="dialogueEntry">The entry that will appear as a response.</param>
    protected abstract void SetDestinationDialogueEntryFields(Asset asset, Subtitle originSubtitle, ref DialogueEntry dialogueEntry);
   
    public override void Open()
    {
        customDialogueUI ??= FindObjectOfType<CustomDialogueUI>();
        customDialogueUI.ForceOverrideMenuPanel( this);
        base.Open();
    }

    public override void Close()
    {
        customDialogueUI ??= FindObjectOfType<CustomDialogueUI>();
        customDialogueUI.ClearForcedMenuOverride(this);
        base.Close();
    }
    
    /// <summary>
    /// Generates the responses that will be displayed in the response menu. This overrides the default method in the Dialogue System, ignoring other responses that are already present in the Dialogue Editor.
    /// </summary>
    /// <param name="subtitle"></param>
    /// <returns></returns>
    protected virtual Response[] GetAssetResponses(Subtitle subtitle)
    {
        customDialogueUI ??= FindObjectOfType<CustomDialogueUI>();
        var newResponses = new List<Response>();
            
        foreach (var asset in assetList.Where(AssetIsValid))
        {
            var template = Template.FromDefault();
            var newDialogueEntry = template.CreateDialogueEntry( template.GetNextDialogueEntryID( subtitle.dialogueEntry.GetConversation()), subtitle.dialogueEntry.conversationID, string.Empty);
                
            SetDestinationDialogueEntryFields( asset, subtitle, ref newDialogueEntry);
            SetupFollowupConversationOrDialogueEntries( asset, ref newDialogueEntry);
                
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
    
    /// <summary>
    /// Gets the display name of the asset. If the asset has a conditional display entry, it will return the display name of the first entry that meets the conditions.
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Checks if the item has a valid conversation field. If it does, it returns the conversation title. Used for generating new conversations or linking to existing ones.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="conversationTitle"></param>
    /// <returns></returns>
    protected virtual bool ItemConversationIsValid(Item item, out string conversationTitle)
    {
        var conversation = item.FieldExists("Conversation");
        if (!conversation)
        {
            conversationTitle = null;
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
    
    /// <summary>
    /// Sets the fields of the dialogue entries that will be created as responses. 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="originSubtitle">The subtitle that precedes the response menu.</param>
    /// <param name="dialogueEntry">The entry that will appear as a response.</param>
    
    protected abstract void SetDestinationDialogueEntryFields(Item item, Subtitle originSubtitle, ref DialogueEntry dialogueEntry);
    
    protected override bool AssetIsValid(Asset asset)
    {
        return ItemIsValid(asset as Item);
    }
    
    /// <summary>
    /// Sets the condition for whether the item is valid or not. An invalid item will not be displayed in the response menu regardless of other conditions.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    
    protected abstract bool ItemIsValid(Item item);

    /// <summary>
    /// Sets up the followup conversation or dialogue entries that will be linked to the response.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="dialogueEntry"></param>
    protected virtual void SetupFollowupConversationOrDialogueEntries(Item item, ref DialogueEntry dialogueEntry)
    {
        if (ItemConversationIsValid(item, out var conversationTitle))
        {
            dialogueEntry.outgoingLinks = new List<Link>();

            //  Debug.Log("Action conversation is valid: " + conversationTitle);

            if (conversationTitle == string.Empty)
            {
                var newConversation = KeyWaveUtility.GenerateConversation(item, item.RepeatCount > 0);
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
    
    protected override void SetupFollowupConversationOrDialogueEntries(Asset asset, ref DialogueEntry dialogueEntry)
    {
        SetupFollowupConversationOrDialogueEntries( asset as Item, ref dialogueEntry);
    }
}


public abstract class LocationResponsePanel : AbstractAssetPanel
{
    protected override List<Asset> assetList => new List<Asset>(DialogueManager.masterDatabase.locations);
    
    protected override void SetDestinationDialogueEntryFields(Asset asset, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        SetDestinationDialogueEntryFields(asset as Location, originSubtitle, ref dialogueEntry);
    }
    
    /// <summary>
    /// Sets the fields of the dialogue entries that will be created as responses.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="originSubtitle"></param>
    /// <param name="dialogueEntry"></param>
    protected abstract void SetDestinationDialogueEntryFields(Location location, Subtitle originSubtitle, ref DialogueEntry dialogueEntry);
    
    protected override bool AssetIsValid(Asset asset)
    {
        return LocationIsValid(asset as Location);
    }
    
    /// <summary>
    /// Sets the condition for whether the location is valid or not. An invalid location will not be displayed in the response menu regardless of other conditions.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    protected abstract bool LocationIsValid(Location location);

    protected override void SetupFollowupConversationOrDialogueEntries(Asset asset, ref DialogueEntry dialogueEntry)
    {
        // do nothing
    }
}

