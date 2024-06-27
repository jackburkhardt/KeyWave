using Newtonsoft.Json;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

namespace Project.Runtime.Scripts.Events.Actions
{
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
                SaveDataStorer.LocalStoreGameData(PixelCrushers.SaveSystem.RecordSavedGameData());
               
               
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
    }
}