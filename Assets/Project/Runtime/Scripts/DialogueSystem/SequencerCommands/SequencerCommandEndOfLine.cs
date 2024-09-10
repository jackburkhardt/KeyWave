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
        var entrytag = GetParameter(0, sequencer.entrytag);
        var splitIndex = entrytag.LastIndexOf('_');
        var entryID = int.Parse(entrytag.Substring(splitIndex + 1));
        var title = entrytag.Substring(0, splitIndex).Replace('_', '/');
        Sequencer.PlaySequence("SetContinueMode(false);");
        var entry = DialogueManager.instance.masterDatabase.GetConversation(title).GetDialogueEntry(entryID);

        var conversationType = title.Split("/").Length > 3 ? title.Split("/")[^2] : string.Empty;

        var state = DialogueManager.instance.currentConversationState;
        var view = DialogueManager.instance.conversationView;
        
        var waitForTyped = entry.subtitleText.Length > 0 ? "@Message(Typed)" : string.Empty;
        
        if (entry.IsLastNode()) Debug.Log("is last node: " + entry.IsLastNode());
        
        Sequencer.PlaySequence(entry.IsLastNode()
            ? $"WaitForMessage(Typed); SetActionPanel(true, {conversationType}){waitForTyped};"
            : "SetContinueMode(NotBeforeResponseMenu)@Message(Typed);");
        
        if (!entry.IsLastNode())
        {
            AnalyzePCResponses(state, view, out var isPCResponseMenuNext, out var isPCAutoResponseNext);
            var autoContinue = isPCResponseMenuNext || entry.IsEmpty();
        
            Debug.Log(entryID);
        
            if (autoContinue)
            {
                Debug.Log("PC Response Menu Next");
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
