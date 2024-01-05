using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.ChatMapper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeEvent : MonoBehaviour
{
    
    /*
    // Start is called before the first frame update
    public void OnPlayerInteract(string gameobject)
    {
        GameEvent.PlayerEvent("player_interact", gameobject);
    }

    public void OnConversationDecision(string decision)
    {
        string conversation = DialogueManager.instance.activeConversation.conversationTitle;

        //

        string node_title = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.Title;
        GameEvent.PlayerEvent("conversation_decision", $"{conversation}_{node_title}_{decision}");

    }

    public void OnConversationStart()
    {
        string conversation = DialogueManager.instance.activeConversation.conversationTitle;
        GameEvent.PlayerEvent("conversation_start", conversation);
    }

    public void OnConversationEnd() { 
    
        string conversation = DialogueManager.instance.activeConversation.conversationTitle;
        GameEvent.PlayerEvent("conversation_end", conversation);

    }
    
    */
}
