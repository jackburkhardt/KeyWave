using System;
using System.Collections;
using System.Collections.Generic;
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
        }

        var entry = sequencer.GetDialogueEntry();
        var title = entry.GetConversation().Title;

        var state = DialogueManager.instance.currentConversationState;
        var view = DialogueManager.instance.conversationView;
        var waitForTyped = entry.subtitleText.Length > 0 ? "@Message(Typed)" : string.Empty;
        
        conversationType ??= title.Split("/").Length > 2 ? title.Split("/")[^2] : string.Empty;
        
        if (conversationType is not "Action" and not "Walk" and not "Talk")
        {
            conversationType = string.Empty;
        }

        Sequencer.PlaySequence("SetContinueMode(false);");
        
        Sequencer.PlaySequence(entry.IsLastNode()
            ? $"WaitForMessage(Typed); SetActionPanel(true, {conversationType}){waitForTyped};"
            : "SetContinueMode(NotBeforeResponseMenu)@Message(Typed);");
        
        if (!entry.IsLastNode())
        {
            AnalyzePCResponses(state, view, out var isPCResponseMenuNext, out var isPCAutoResponseNext);
            var autoContinue = isPCResponseMenuNext || entry.IsEmpty();
        
            if (autoContinue)
            {
                if (entry.subtitleText.Length > 0)
                {
                    sequencer.PlaySequence("Continue()@Message(Typed);");
                }
                else sequencer.PlaySequence("Continue()");
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
