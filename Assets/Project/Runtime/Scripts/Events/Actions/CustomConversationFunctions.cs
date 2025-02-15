using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

namespace Project.Runtime.Scripts.Events.Actions
{
    public class CustomConversationFunctions : MonoBehaviour
    {
        private string currentConversationTitle = string.Empty;

        public void OnConversationLine(Subtitle subtitle)
        {
            
        }
        
        
        public void OnConversationLineEnd(Subtitle subtitle)
        {
            
            foreach (var actor in DialogueManager.masterDatabase.actors)
            {
                foreach (var name in actor.Name.Split(" "))
                {
                    if (subtitle.formattedText.text.Contains(name))
                    {
                        Field.SetValue(actor.fields, "Introduced", true);
                        DialogueLua.SetActorField(actor.Name, "Introduced", true);
                    }
                }
                
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