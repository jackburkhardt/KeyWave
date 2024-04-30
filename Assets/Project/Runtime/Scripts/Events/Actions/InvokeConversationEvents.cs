using PixelCrushers.DialogueSystem;
using UnityEngine;

public class InvokeConversationEvents : MonoBehaviour
{
    public void BroadcastLine()
    {
       
        if (DialogueUtility.CurrentDialogueEntry.id == 0) return; 
        
        GameEvent.OnConversationLine();
        
        GameStateManager.instance.AddTime(DialogueUtility.CurrentNodeDuration);

        if (DialogueUtility.Empty(DialogueUtility.CurrentDialogueEntry) &&
            DialogueUtility.CurrentDialogueEntry.Sequence == string.Empty) DialogueManager.PlaySequence("Continue()");

        // if (!Points.IsAnimating) Sequencer.Message("Animated");
       
        //   StartCoroutine(DelayedBroadcast("line"));
    }

    public void BroadcastConversationStart()
    {
        GameEvent.OnConversationStart();
        //  StartCoroutine(DelayedBroadcast("start"));

    }


    
    public void BroadcastConversationEnd()
    {
       GameEvent.OnConversationEnd();
       // StartCoroutine(DelayedBroadcast("end"));
    }

    public void BroadcastResponseMenu()
    {
        GameEvent.OnConversationResponseMenu();
    }
    
}

