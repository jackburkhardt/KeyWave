using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;

public class CustomDialogueUI : StandardDialogueUI
{

    [Tooltip("Dialogue Manager's Include Sim Status must be ticked. This checkbox omits Sim Status from saved games to keep them smaller.")]
    public bool includeSimStatusInSavedGames = false;

    public string showInvalidFieldName = "Show Invalid";

    [HideIf("_showInvalidAlways")]
    [SerializeField]
    private bool _showInvalidByDefault;
    
    public bool ShowInvalidByDefault
    {
        get => _showInvalidByDefault && !_showInvalidAlways;
        set => _showInvalidByDefault = value;
    }
    
    [HideIf("_showInvalidByDefault")]
    [SerializeField]
    private bool _showInvalidAlways = false;


    public bool ShowInvalidAlways
    {
        get => _showInvalidAlways && !_showInvalidByDefault;
        set => _showInvalidAlways = value;
    }
    
   // private new CustomUIDialogueControls conversationUIElements;

    public override void Awake()
    {
        conversationUIElements = new CustomUIDialogueControls(conversationUIElements);
        base.Awake();
    }
    
    private void VerifyAssignments()
    {
        if (addEventSystemIfNeeded) UITools.RequireEventSystem();
        if (DialogueDebug.logWarnings && verifyPanelAssignments)
        {
            if (alertUIElements.alertText.gameObject == null) Debug.LogWarning("Dialogue System: No UI text element is assigned to Standard Dialogue UI's Alert UI Elements.", this);
            if (conversationUIElements.subtitlePanels.Length == 0) Debug.LogWarning("Dialogue System: No subtitle panels are assigned to Standard Dialogue UI.", this);
            if (conversationUIElements.menuPanels.Length == 0) Debug.LogWarning("Dialogue System: No response menu panels are assigned to Standard Dialogue UI.", this);
        }
    }


    #region Custom

    public override void Start()
    {
        PersistentDataManager.includeSimStatus = includeSimStatusInSavedGames;
        base.Start();
        
    }
    

    public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
    {
       
        bool ActionLocationIsValid(Item item)
        {
            var actionLocation = item.AssignedField("Location");
            if (actionLocation == null) return true;
            
            var location = DialogueManager.masterDatabase.GetLocation(int.Parse(actionLocation.value));

            if (item.IsFieldAssigned("New Sublocation"))
            {
                var newSublocation =
                    DialogueManager.masterDatabase.GetLocation(int.Parse(item.LookupValue("New Sublocation")));

                if (newSublocation == GameManager.gameState.PlayerLocation(true))
                {
                    return false;
                }
            }
            
            var playerLocation = GameManager.gameState.PlayerLocation(true);

            if (item.LookupBool("Ignore Sublocations"))
            {
                var rootLocation = DialogueManager.masterDatabase.GetLocation(location.RootID);
                var rootPlayerLocation = DialogueManager.masterDatabase.GetLocation(playerLocation.RootID);

                return rootLocation == rootPlayerLocation;
            }

            return location == GameManager.gameState.PlayerLocation(true);
        }

        bool ActionConversationIsValid(Item item, out string conversationTitle)
        {
            var conversation = item.AssignedField("Conversation");
            if (conversation == null)
            {
                conversationTitle = null;
                return false;
            }

            if (item.IsFieldAssigned("Entry Count")) conversationTitle = string.Empty;
            
            else conversationTitle = item.LookupValue("Conversation");
            
            return true;
        }
        
        bool ActionRequiredActorsAreValid(Item item)
        {
            var actionRequiredActors = item.fields.Where(p => p.title == "Required Nearby Actor");
            
            foreach (var actionRequiredActor in actionRequiredActors)
            {
                if (actionRequiredActor.value == string.Empty) continue;
                var actor = DialogueManager.masterDatabase.GetActor(int.Parse(actionRequiredActor.value));
                if (actor == null) continue;
                
                var actorLocation = actor.AssignedField("Location");
                if (actorLocation == null) continue;
                if (DialogueManager.masterDatabase.GetLocation(int.Parse(actorLocation.value)) != GameManager.gameState.PlayerLocation(true)) return false;
            }
            return true;
        }

        List<DialogueEntry> GeneratedDialogueEntries(Item item, int ConversationID, int actorID)
        {
            var dialogueEntries = new List<DialogueEntry>();
            var template = Template.FromDefault();
            
            var repeatCount = item.IsRepeatable && item.fields.Any(p => p.title.StartsWith("Repeat Entry ")) ? int.Parse(item.LookupValue("Repeat Count")) : 0;
            var entryFieldLabelStart = repeatCount > 0 ? "Repeat Entry " : "Entry ";
            var entryCount = item.LookupInt(entryFieldLabelStart + "Count");
            
            var startNode = template.CreateDialogueEntry( 0, ConversationID, "START");
            
            startNode.ActorID = actorID;
            startNode.Sequence = "None()";
            startNode.outgoingLinks = new List<Link> { new Link( ConversationID, 0, ConversationID, 1) };
            dialogueEntries.Add(startNode);
            

            for (int i = 1; i < entryCount + 1; i++)
            {
                var menuText = item.LookupValue(entryFieldLabelStart + i + " Menu Text");
                var dialogueText = item.LookupValue(entryFieldLabelStart + i + " Dialogue Text");
                
                var newDialogueEntry = template.CreateDialogueEntry( i, ConversationID, "ACTION");
                newDialogueEntry.MenuText = menuText;
                newDialogueEntry.DialogueText = dialogueText;
                
                newDialogueEntry.ActorID = actorID;
                
                if (i < entryCount) newDialogueEntry.outgoingLinks = new List<Link>() { new Link( ConversationID, i, ConversationID, i + 1) };
                
                dialogueEntries.Add(newDialogueEntry);
            };
            
           
            
            
            return dialogueEntries;
        }
        
        
        
        //generate new responses

        if (subtitle.dialogueEntry.GetConversation().Title == "SmartWatch/Actions")
        {
            var newResponses = new List<Response>();
            
            foreach (var action in DialogueManager.masterDatabase.items.Where(p => !p.IsItem && p.IsAction))
            {

              
                if (!ActionLocationIsValid(action)) continue;
                if (!ActionRequiredActorsAreValid(action)) continue;
                
                var template = Template.FromDefault();
                var newDialogueEntry = template.CreateDialogueEntry( template.GetNextDialogueEntryID( subtitle.dialogueEntry.GetConversation()), subtitle.dialogueEntry.conversationID, "ACTION");
                
                
                newDialogueEntry.MenuText = action.IsFieldAssigned("Display Name") ? action.LookupValue("Display Name") : action.Name;
                newDialogueEntry.DialogueText = string.Empty;
                newDialogueEntry.conditionsString = action.IsFieldAssigned("Conditions") ? action.AssignedField("Conditions").value : string.Empty;
                
                newDialogueEntry.fields.Add( new Field(showInvalidFieldName, action.LookupValue(showInvalidFieldName), FieldType.Boolean));
                
                newDialogueEntry.ActorID = subtitle.dialogueEntry.ActorID;
                newDialogueEntry.ConversantID = subtitle.dialogueEntry.ConversantID;



                if (ActionConversationIsValid(action, out var conversationTitle))
                {
                    newDialogueEntry.outgoingLinks = new List<Link>();

                    if (conversationTitle == string.Empty)
                    {
                        var newConversation = template.CreateConversation(
                            template.GetNextConversationID(DialogueManager.masterDatabase),
                            $"/GENERATED/{action.Name}");
                        var entryActorID = action.IsFieldAssigned("Entry Actor")
                            ? int.Parse(action.LookupValue("Entry Actor"))
                            : -1;

                        newConversation.ActorID = entryActorID;
                        newConversation.dialogueEntries =
                            GeneratedDialogueEntries(action, newConversation.id, entryActorID);
                        newConversation.fields.Add(new Field("Action", action.id.ToString(), FieldType.Number));
                        DialogueManager.masterDatabase.conversations.Add(newConversation);
                        newDialogueEntry.outgoingLinks.Add(new Link(newDialogueEntry.conversationID,
                            newDialogueEntry.id, newConversation.id, 0));
                    }

                    else if (conversationTitle != null)
                    {
                        Debug.Log(conversationTitle);
                        var conversation = DialogueManager.masterDatabase.GetConversation(conversationTitle);
                        Debug.Log(conversation);
                        conversation.fields.Add(new Field("Action", action.id.ToString(), FieldType.Number));
                        newDialogueEntry.outgoingLinks.Add(new Link(newDialogueEntry.conversationID,
                            newDialogueEntry.id, conversation.id, 0));
                    }
                }

                else
                {
                    newDialogueEntry.fields.Add(new Field("Action", action.id.ToString(), FieldType.Number));
                }
                
                var newResponse = new Response(new FormattedText(newDialogueEntry.MenuText), newDialogueEntry);
                newResponse.enabled = Lua.Run($"return {newDialogueEntry.conditionsString}").asBool;
                
                if (!newResponse.enabled) Debug.Log("action not available: " + newDialogueEntry.conditionsString);
                
                
                newResponses.Add(newResponse);
                
            }

            foreach (var newResponse in newResponses)
            {
                subtitle.dialogueEntry.outgoingLinks.Add(new Link(subtitle.dialogueEntry.conversationID, subtitle.dialogueEntry.id, newResponse.destinationEntry.conversationID, newResponse.destinationEntry.id));
            }
            
            responses = responses.Concat(newResponses).ToArray();
        }
        
        responses = CheckInvalidResponses(responses);
        
        base.ShowResponses(subtitle, responses, timeout);
    }

    private Response[] CheckInvalidResponses(Response[] responses)
    {
        if (!HasAnyInvalid(responses)) return responses;
        var list = new List<Response>();
        for (int i = 0; i < responses.Length; i++)
        {
            var response = responses[i];
            //--- Was: if (response.enabled || Field.LookupBool(response.destinationEntry.fields, showInvalidFieldName))
            //--- To allow runtime changes, we need to use a more sophisticated AllowShowInvalid() method:
            if (response.enabled || AllowShowInvalid(response) || ShowInvalidAlways)
            {
                list.Add(response);
            }
        }
        return list.ToArray();
    }

    private bool HasAnyInvalid(Response[] responses)
    {
       // Debug.Log("Checking for invalid responses");
        if (responses == null)
        {
          //  Debug.Log("No responses");
            return false;
        }
        for (int i = 0; i < responses.Length; i++)
        {
        //    Debug.Log("Checking response: " + responses[i].formattedText.text);
         //   Debug.Log("Response condition: " + responses[i].destinationEntry.conditionsString);
            if (!responses[i].enabled)
            {
            //    Debug.Log("Found invalid response: " + responses[i].formattedText.text);
                return true;
            }
        }
        
        
//        Debug.Log("No invalid responses");
      //  Debug.Log(DialogueManager.displaySettings.inputSettings.includeInvalidEntries);
        return false;
    }

    private bool AllowShowInvalid(Response response)
    {
        
        if (Field.FieldExists(response.destinationEntry.fields, showInvalidFieldName))
        {
            return Field.LookupBool(response.destinationEntry.fields, showInvalidFieldName);
        }
        else
        {
            // Not sure how to get this
            return ShowInvalidByDefault;
        }
    }

    public override void ShowSubtitle(Subtitle subtitle)
    {
        //subtitle.formattedText.text = Regex.Replace(subtitle.formattedText.text, @"[^\x20-\x7F]", "");
        base.ShowSubtitle(subtitle);
    }

    public void OnConversationLine(Subtitle subtitle)
    {
        
        if (Field.FieldExists(subtitle.dialogueEntry.fields, "Randomize Next Entry") && Field.LookupBool(subtitle.dialogueEntry.fields, "Randomize Next Entry"))
        {
            Debug.Log("Randomizing next entry");
            DialogueManager.instance.conversationController.randomizeNextEntry = true;
            DialogueManager.instance.conversationController.randomizeNextEntryNoDuplicate = true;
        }

        
    }



    
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindObjectOfType<StandardUIContinueButtonFastForward>(true).OnFastForward();
        }
    }

    #endregion
}

public class CustomUIDialogueControls : StandardUIDialogueControls
{
    StandardUIDialogueControls _standardUIDialogueControls;

    public new CustomUISubtitleControls standardSubtitleControls;
    
    public CustomUIDialogueControls(StandardUIDialogueControls standardUIDialogueControls) 
    {
        _standardUIDialogueControls = standardUIDialogueControls;
        
        mainPanel = standardUIDialogueControls.mainPanel;
        dontDeactivateMainPanel = standardUIDialogueControls.dontDeactivateMainPanel;
        waitForMainPanelOpen = standardUIDialogueControls.waitForMainPanelOpen;
        subtitlePanels = standardUIDialogueControls.subtitlePanels;
        defaultNPCSubtitlePanel = standardUIDialogueControls.defaultNPCSubtitlePanel;
        defaultPCSubtitlePanel = standardUIDialogueControls.defaultPCSubtitlePanel;
        allowOpenSubtitlePanelsOnStartConversation = standardUIDialogueControls.allowOpenSubtitlePanelsOnStartConversation;
        allowDialogueActorCustomPanels = standardUIDialogueControls.allowDialogueActorCustomPanels;
        menuPanels = standardUIDialogueControls.menuPanels;
        defaultMenuPanel = standardUIDialogueControls.defaultMenuPanel;
        useFirstResponseForMenuPortrait = standardUIDialogueControls.useFirstResponseForMenuPortrait;
        waitForClose = standardUIDialogueControls.waitForClose;
        
        standardSubtitleControls = new CustomUISubtitleControls(_standardUIDialogueControls.standardSubtitleControls);
        typeof(StandardUIDialogueControls).GetField("m_standardSubtitleControls", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, standardSubtitleControls);
    }
}                   




public class CustomUISubtitleControls : StandardUISubtitleControls
{
    StandardUISubtitleControls _standardUISubtitleControls;
    
    public CustomUISubtitleControls(StandardUISubtitleControls standardUISubtitleControls) 
    {
        _standardUISubtitleControls = standardUISubtitleControls;
    }

    protected override void SupercedeOtherPanelsInList(List<StandardUISubtitlePanel> list, StandardUISubtitlePanel newPanel)
    {
       
        for (int i = 0; i < list.Count; i++)
        {
            var panel = list[i];
            if (panel == null || panel == newPanel) continue;
            if (panel.isOpen)
            {
                if (UITools.CanBeSuperceded(panel.visibility))
                {
                    panel.Close();
                    panel.SendMessage("OnSuperceded", SendMessageOptions.DontRequireReceiver);
                   
                }
                else
                {
                    panel.Unfocus();
                }
            }
        }
    }
}
