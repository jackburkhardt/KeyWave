using PixelCrushers.DialogueSystem;
using UnityEngine;

public class InvokeConversationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    public void BroadcastLine()
    {
        if (DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.id == 0) return; 
        
        GameEvent.OnConversationLine();
        
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

