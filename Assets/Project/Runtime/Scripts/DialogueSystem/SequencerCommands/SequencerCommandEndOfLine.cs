using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class SequencerCommandEndOfLine : SequencerCommand
{
    public void Start()
    {
        var input = GetParameter(0, sequencer.entrytag);
        
        string conversationType = null;
        
        
        switch (input)
        {
            case "Action":
                conversationType = "Action";
                input = sequencer.entrytag;
                break;
            case "Walk":
                conversationType = "Walk";
                input = sequencer.entrytag;
                break;
            case "Talk":
                conversationType = "Talk";
                input = sequencer.entrytag;
                break;
            case "Map":
                conversationType = "Map";
                input = sequencer.entrytag;
                break;
        }

        var entry = sequencer.GetDialogueEntry();
        
        if (entry == null)
        {
            Debug.Log("EndOfLine: Entry is null");
        }
        
        entry ??= DialogueManager.instance.currentConversationState.subtitle.dialogueEntry;

        var title = entry != null
            ? entry.GetConversation().Title
            : DialogueManager.instance.activeConversation.conversationTitle;
        
       
       
        
        var debug = entry != null && Field.FieldExists(entry.fields, "Debug") && Field.LookupBool(entry.fields, "Debug");
        
    //    debug = true;
        
        if (debug) Debug.Log($"End of line: {input}");

        var state = DialogueManager.instance.currentConversationState;
        var view = DialogueManager.instance.conversationView;
        var waitForTyped = entry != null && entry.subtitleText.Length > 0 ? "@Message(Typed)" : string.Empty;



        conversationType ??=
            title.Split("/").Length > 2 ? title.Split("/")[^2] : title.EndsWith("Actions") ? "Actions" : title == "Map" ? "Map" :
            string.Empty;
        
        conversationType = string.Empty;
        
        if (conversationType is not "Actions" and not "Actions" and not "Walk" and not "Talk" and not "Map")
        {
            conversationType = string.Empty;
        }
        
        if (string.IsNullOrEmpty(conversationType)) Sequencer.PlaySequence("SetCustomPanel(SmartWatch, false);");

        
        Sequencer.PlaySequence("SetContinueMode(false);");

        if (sequencer.GetDialogueEntry() == null && conversationType != string.Empty && title.EndsWith("Base"))
        {
            Sequencer.PlaySequence($"WaitForMessage(Typed); SetCustomPanel(SmartWatch, true){waitForTyped};");
        }
        
        else if (entry != null)
        { 
            Sequencer.PlaySequence(entry.IsLastNode()
                ? $"WaitForMessage(Typed); SetCustomPanel(SmartWatch, true){waitForTyped};"
                : "SetContinueMode(true)@Message(Typed);");
        
            if (!entry.IsLastNode())
            {
                if (debug) Debug.Log("Not last node, will autocontinue or wait for response menu");
                
                AnalyzePCResponses(state, view, out var isPCResponseMenuNext, out var isPCAutoResponseNext);
                var autoContinue = isPCResponseMenuNext || entry.IsEmpty();
                
                if (debug) Debug.Log($"Auto-continue: {autoContinue} (isPCResponseMenuNext={isPCResponseMenuNext}, isPCAutoResponseNext={isPCAutoResponseNext}), entry.IsEmpty()={entry.IsEmpty()}");
        
                if (autoContinue)
                {
                    if (entry.subtitleText.Length > 0)
                    {
                        if (debug) Debug.Log("Auto-continuing to next subtitle after typed.");
                       // sequencer.PlaySequence("Continue()@Message(Typed);");
                    }
                    else
                    {
                        if (debug) Debug.Log("Auto-continuing to next entry immediately.");
                        sequencer.PlaySequence("Continue()");
                    }
                }
            }
        }
    }
    
    private void AnalyzePCResponses(ConversationState state,ConversationView view, out bool isPCResponseMenuNext, out bool isPCAutoResponseNext)
    {
        var alwaysForceMenu = view != null && view.displaySettings.GetAlwaysForceResponseMenu();
        var hasForceMenu = false;
        var hasForceAuto = false;
        var numPCResponses =  (state != null && state.pcResponses != null) ? state.pcResponses.Length : 0;
        for (int i = 0; i < numPCResponses; i++)
        {
            if (state.pcResponses[i].formattedText.forceMenu)
            {
                hasForceMenu = true;
            }
            if (state.pcResponses[i].formattedText.forceAuto)
            {
                hasForceAuto = true;
                break; // [auto] takes precedence over [f].
            }
        }
        isPCResponseMenuNext = !state.hasNPCResponse && !hasForceAuto &&
                               (numPCResponses > 1 || hasForceMenu || (numPCResponses == 1 && alwaysForceMenu && !string.IsNullOrEmpty(state.pcResponses[0].formattedText.text)));
        isPCAutoResponseNext = !state.hasNPCResponse && hasForceAuto || 
                               (numPCResponses == 1 && string.IsNullOrEmpty(state.pcResponses[0].formattedText.text)) ||
                               (numPCResponses == 1 && !hasForceMenu && (!alwaysForceMenu || state.pcResponses[0].destinationEntry.isGroup));
    }
}

public class SequencerCommandGoToConversation : SequencerCommand
{
    public void Awake()
    {
        var conversationName = GetParameter(0);
        var stop = GetParameterAsBool(1, false);
        if (string.IsNullOrEmpty(conversationName)) return;
        var database = DialogueManager.instance.masterDatabase;
        
        if (DialogueManager.instance.masterDatabase.GetConversation(conversationName) == null)
        {
            Debug.Log($"Conversation {conversationName} not found in database.");
            return;
        }
        
        if (!DialogueManager.instance.isConversationActive)
        {
            DialogueManager.instance.StartConversation(conversationName);
            return;
        }
        
        if (stop)
        {
            DialogueManager.StopConversation();
            DialogueManager.StartConversation(conversationName);
            return;
        }
        
            
        var conversation = database.GetConversation(conversationName);
        var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
        var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
        DialogueManager.conversationController.GotoState(state);
        DialogueManager.instance.PlaySequence("Continue()");
    }
}
