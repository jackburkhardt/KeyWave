using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class SequencerCommandEndOfLine : SequencerCommand
{
   
    public void Start()
    {

        DialogueEntry entry;
        try
        {
            entry = sequencer.GetDialogueEntry();
        }
        
        catch
        {
            entry = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry;
        }

        if (entry.fields.Exists(f => f.title == "Actions")) return;
        
        if (entry == null)
        {
            Debug.Log("EndOfLine: Entry is null");
        }
        
        Sequencer.PlaySequence("SetContinueMode(false);");
      
        if (entry != null)
        { 

            Sequencer.PlaySequence("SetContinueMode(true)@Message(Typed);");
            
            var state = DialogueManager.instance.currentConversationState;
            var view = DialogueManager.instance.conversationView;
                
            AnalyzePCResponses(state, view, out var isPCResponseMenuNext, out var isPCAutoResponseNext);
            
            if (isPCResponseMenuNext || entry.IsEmpty())
            {
                if (entry.subtitleText.Length > 0)
                {
                    //if (debug) Debug.Log("Auto-continuing to next subtitle after typed.");
                    sequencer.PlaySequence("Continue()@Message(Typed);");
                }
                else
                {
                    //if (debug) Debug.Log("Auto-continuing to next entry immediately.");
                    sequencer.PlaySequence("Continue()");
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
        isPCResponseMenuNext = state != null && !state.hasNPCResponse && !hasForceAuto &&
                               (numPCResponses > 1 || hasForceMenu || (numPCResponses == 1 && alwaysForceMenu && !string.IsNullOrEmpty(state.pcResponses[0].formattedText.text)));
        isPCAutoResponseNext = state != null && !state.hasNPCResponse && hasForceAuto || 
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
