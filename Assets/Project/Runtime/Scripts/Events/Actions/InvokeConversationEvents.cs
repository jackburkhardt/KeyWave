using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;

public class InvokeConversationEvents : MonoBehaviour
{
    private string currentConversationTitle = string.Empty;
    public void BroadcastLine(Subtitle subtitle)
    {
        if (DialogueUtility.CurrentDialogueEntry.id == 0) return; 
        
        
       

        if (DialogueUtility.CurrentDialogueEntry.MenuText.Contains("Action"))
        {
            TutorialPanel.instance.PlayTutorial("ActionTutorial");
        }
        
        if (DialogueUtility.CurrentDialogueEntry.MenuText.Contains("Talk"))
        {
            TutorialPanel.instance.PlayTutorial("TalkTutorial");
        }
        
        GameStateManager.instance.gameState.Clock += (DialogueUtility.CurrentNodeDuration);
        
        if (!subtitle.dialogueEntry.IsEmpty() && !subtitle.dialogueEntry.IsResponseChild())
        {
            PixelCrushers.SaveSystem.SaveToSlot(1);
        }
        
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

   
    
    public void OnConversationLine()
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

