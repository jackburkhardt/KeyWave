using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class PlaySequenceOnMessage : MonoBehaviour, IMessageHandler
{
    public string message;
    public string sequence;
    public bool once = true;
    private bool hasPlayed = false;
   

    public void OnMessage(MessageArgs messageArgs)
    {
        if (messageArgs.message == message)
        {
            hasPlayed = true;
            DialogueManager.PlaySequence(sequence);
        }
    }
}
