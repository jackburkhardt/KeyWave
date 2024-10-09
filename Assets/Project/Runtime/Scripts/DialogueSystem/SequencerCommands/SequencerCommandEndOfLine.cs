using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class SequencerCommandEndOfLine : SequencerCommand
{
    public static DialogueEntry mostRecentDialogueEntry;
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

        var title = entry != null
            ? entry.GetConversation().Title
            : DialogueManager.instance.activeConversation.conversationTitle;
        
        var debug = entry != null && Field.FieldExists(entry.fields, "Debug") && Field.LookupBool(entry.fields, "Debug");
        
        if (debug) Debug.Log($"End of line: {input}");

        var state = DialogueManager.instance.currentConversationState;
        var view = DialogueManager.instance.conversationView;
        var waitForTyped = entry != null && entry.subtitleText.Length > 0 ? "@Message(Typed)" : string.Empty;
        
        conversationType ??= title.Split("/").Length > 2 ? title.Split("/")[^2] : title == "Map" ? "Map" :
            string.Empty;
        
        if (conversationType is not "Action" and not "Walk" and not "Talk" and not "Map")
        {
            conversationType = string.Empty;
        }

        
        Sequencer.PlaySequence("SetContinueMode(false);");
        
        if (entry != null)
        {
            if (debug && entry.IsLastNode()) Debug.Log("Last node (End of conversation)");
            Sequencer.PlaySequence(entry.IsLastNode()
                ? $"WaitForMessage(Typed); SetActionPanel(true, {conversationType}){waitForTyped};"
                : "SetContinueMode(NotBeforeResponseMenu)@Message(Typed);");
        
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
                        sequencer.PlaySequence("Continue()@Message(Typed);");
                    }
                    else
                    {
                        if (debug) Debug.Log("Auto-continuing to next entry immediately.");
                        sequencer.PlaySequence("Continue()");
                    }
                }
            }
        }

        else
        {
            Sequencer.PlaySequence($"WaitForMessage(Typed); SetActionPanel(true, {conversationType}){waitForTyped};");
        }
        
        mostRecentDialogueEntry = entry ?? mostRecentDialogueEntry;
       
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
