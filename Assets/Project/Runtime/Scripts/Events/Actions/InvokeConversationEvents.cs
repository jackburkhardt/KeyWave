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
    }

    public void BroadcastConversationStart()
    {
        GameEvent.OnConversationStart();
    }
    
    public void BroadcastConversationEnd()
    {
       GameEvent.OnConversationEnd();
    }

    public void BroadcastResponseMenu()
    {
        GameEvent.OnConversationResponseMenu();
    }
    
}

