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