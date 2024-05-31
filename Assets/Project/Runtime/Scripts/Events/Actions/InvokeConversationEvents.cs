using PixelCrushers.DialogueSystem;
using UnityEngine;

public class InvokeConversationEvents : MonoBehaviour
{
    public void BroadcastLine()
    {
        if (DialogueUtility.CurrentDialogueEntry.id == 0) return; 
        
        GameEvent.OnConversationLine();
        
        GameStateManager.instance.AddTime(DialogueUtility.CurrentNodeDuration);
        
     //   Debug.Log("Current Node Duration: " + DialogueUtility.CurrentNodeDuration);

       // if (DialogueUtility.CurrentDialogueEntry.IsEmpty()) DialogueManager.PlaySequence("Continue()");
    }

    public void OnSequenceStart()
    {
        if (DialogueUtility.CurrentDialogueEntry.IsEmpty()) DialogueManager.PlaySequence("Continue()");
    }

    public void OnSequenceEnd()
    {
       // if (DialogueUtility.CurrentDialogueEntry.outgoingLinks.Count == 1 && DialogueUtility.CurrentDialogueEntry.outgoingLinks[0].GetDestinationEntry().IsResponseRoot())
       // {
     //       DialogueManager.PlaySequence("Continue()");
       // }
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

