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
        var splitIndex = sequencer.entrytag.LastIndexOf('_');
        var entryID = int.Parse(sequencer.entrytag.Substring(splitIndex + 1));
        var title = Sequencer.entrytag.Substring(0, splitIndex).Replace('_', '/');
        Sequencer.PlaySequence("SetContinueMode(false);");
        var entry = DialogueManager.instance.masterDatabase.GetConversation(title).GetDialogueEntry(entryID);

        var conversationType = title.Split("/").Length > 3 ? title.Split("/")[^2] : string.Empty;

        Sequencer.PlaySequence(entry.IsLastNode()
            ? $"WaitForMessage(Typed); SetActionPanel(true, {conversationType});"
            : "SetContinueMode(NotBeforeResponseMenu )@Message(Typed);");
    }
}
