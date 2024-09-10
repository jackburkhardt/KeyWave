using System.Linq;
using Newtonsoft.Json;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

namespace Project.Runtime.Scripts.Events.Actions
{
    public class CustomConversationFunctions : MonoBehaviour
    {
        private string currentConversationTitle = string.Empty;

        public void OnConversationLine(Subtitle subtitle)
        {
           // if (subtitle.dialogueEntry.id == 0) 

            if (DialogueUtility.CurrentDialogueEntry.MenuText.Contains("Action"))
            {
                TutorialPanel.instance.PlayTutorial("ActionTutorial");
            }
        
            if (DialogueUtility.CurrentDialogueEntry.MenuText.Contains("Talk"))
            {
                TutorialPanel.instance.PlayTutorial("TalkTutorial");
            }
            
            if (DialogueUtility.CurrentDialogueEntry.Title.Contains("Options"))
            {
                TutorialPanel.instance.PlayTutorial("StartTutorial");
            }
        
            GameStateManager.instance.gameState.Clock += (DialogueUtility.CurrentNodeDuration);

            foreach (var actor in DialogueManager.masterDatabase.actors)
            {
                foreach (var name in actor.Name.Split(" "))
                {
                    if (subtitle.formattedText.text.Contains(name))
                    {
                        DialogueLua.SetActorField(actor.Name, "WasNameMentioned", true);
                    }
                }
                
            }

            
        }
        
        
        public void OnConversationLineEnd(Subtitle subtitle)
        {
            if (!subtitle.dialogueEntry.IsEmpty() && !subtitle.dialogueEntry.IsResponseChild())
            {
                PixelCrushers.SaveSystem.SaveToSlot(1);
            }
        }



        public void OnLoad()
        {
            Sequencer.Message("Load");
        }




        public void OnConversationStart(Transform t)
        {
            GameEvent.OnConversationStart();
        }

        public void OnConversationEnd(Transform t)
        {
        }
    }
}