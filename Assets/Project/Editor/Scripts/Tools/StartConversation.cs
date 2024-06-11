using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts.Tools
{
    public class StartConversation : MonoBehaviour
    {
        [MenuItem("Tools/Perils and Pitfalls/Game/Start Conversation/Intro")]
        private static void Intro()
        {
            DialogueManager.StartConversation("Intro");
        }

        [MenuItem("Tools/Perils and Pitfalls/Game/Start Conversation/Hotel")]
        private static void Hotel()
        {
            DialogueManager.StartConversation("Hotel");
        }


        [MenuItem("Tools/Perils and Pitfalls/Game/Start Conversation/Store")]
        private static void Store()
        {
            DialogueManager.StartConversation("Store");
        }
    }
}