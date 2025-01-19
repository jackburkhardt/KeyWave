using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class SequencerCommandEndOfDay : SequencerCommand
{
    public void Awake()
    {
        GameManager.instance.EndOfDay();
        
        DialogueManager.instance.PlaySequence("HideCustomPanel(SmartWatch)");
        
    }
}
