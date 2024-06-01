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
      
        if (DialogueUtility.CurrentDialogueEntry.IsEmpty())
        {
            //Debug.Log("Empty node: continuing...");
            DialogueManager.PlaySequence("Continue()");
        }
        
        
        var currentEntry = DialogueUtility.CurrentDialogueEntry;
        if (currentEntry.outgoingLinks.Count != 1) return;
        var nextEntry = currentEntry.outgoingLinks[0].GetDestinationEntry();
        
        if (nextEntry.Title.Contains("Show") && nextEntry.Title.Contains("Options") && !currentEntry.GetActor()!.IsPlayer && !currentEntry.IsEmpty())
        {
            DialogueManager.PlaySequence("Continue()@Message(Typed)");
        }
    }

    public void OnSequenceEnd()
    {
        
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

