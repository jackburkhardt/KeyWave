using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Manager;

public class SequencerCommandAddSeconds : SequencerCommand
{
    public void Awake()
    {
        var time = GetParameterAsInt(0);
        Clock.AddSeconds(time);
        Stop();
    }
}

public class SequencerCommandAddMinutes: SequencerCommand
{
    public void Awake()
    {
        var time = GetParameterAsInt(0);
        Clock.AddSeconds(time * 60);
        Stop();
    }
}

public class SequencerCommandBlackOut : SequencerCommand
{
    private void Start()
    {
        var time = GetParameterAsInt(0);
        var unit = GetParameter(1, "minutes");

        int timeToAdd;
        
        switch (unit)
        {
            case "seconds":
                timeToAdd = time;
                break;
            case "minutes":
                timeToAdd = time * 60;
                break;
            case "hours":
                timeToAdd = time * 3600;
                break;
            default:
                timeToAdd = time * 60;
                break;
        }

        sequencer.PlaySequence($"Fade(stay, 1);" +
                               $"SetContinueMode(false);" + 
                               $"AddSeconds({timeToAdd})@1;" +
                               $"Delay(1)@Message(ClockUpdated)->Message(FadeOut);" +
                               $"Fade(unstay, 1)@Message(FadeOut)->Message(Continue);");
        
        Stop();
    }
}
