using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PixelCrushers.DialogueSystem;

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
    
