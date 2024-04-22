using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class CustomSequencerShortcuts : MonoBehaviour
{
    private void Awake()
    {
        Sequencer.RegisterShortcut("fade", "Fade(stay, 0.5); Fade(unstay, 0.5)@1; Delay(3)");
        
        Sequencer.RegisterShortcut("typed", "Continue()@Message(Typed)");
        
    }
}
