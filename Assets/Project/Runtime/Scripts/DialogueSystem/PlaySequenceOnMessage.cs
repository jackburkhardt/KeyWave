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
        if (once && hasPlayed) return;
        if (messageArgs.message == message)
        {
            hasPlayed = true;
            DialogueManager.PlaySequence(sequence);
        }
    }
}
