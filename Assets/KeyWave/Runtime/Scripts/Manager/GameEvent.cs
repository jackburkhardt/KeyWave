using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using PlayerEvent = PlayerEvents.PlayerEvent;

public static class GameEvent
{
    /*
     How to use events 101!! Events are nice because they can allow your code to react to stuff happening in other
     parts of your code without having to manually check/wait or constantly reference other systems.
        
        - First, you have to make a delegate. The arguments of the delegate are passed whenever the event is invoked,
        so it can be useful to include stuff that event subscribers may need!
        - Second, you want to create the event. Kinda just follow like the examples here.
        - Third, you want to create a method to invoke the event. Other parts of code will call these functions when
        they have an event to share, and this will send out the event to all of its subscribers.
        - Now go put in some subscribers!! Subscribers will "listen" for events and then run some code in response,
        such as a function or a lambda. See Awake() in Player.cs.
        
    The above explanation is probably using terms wrong or isn't explaining very well. Here's some online resources:
        https://youtu.be/k4JlFxPcqlg
        https://www.monkeykidgc.com/2020/07/how-to-use-events-in-unity-with-c.html
        https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/
        https://www.tutorialsteacher.com/csharp/csharp-event
     */

    //ANYEVENT

    // pls goob: use a generic interface instead of "Button"

    public delegate void LoadDelegate();
    
    public static event LoadDelegate OnLoadStart;
    public static event LoadDelegate OnLoadEnd;
    
    public static void LoadStart() => OnLoadStart?.Invoke();
    public static void LoadEnd() => OnLoadEnd?.Invoke();
    
    public delegate void PlayerEventDelegate(PlayerEvents.PlayerEvent playerEvent);
    
    public static event PlayerEventDelegate OnPlayerEvent;

    public static event PlayerEventDelegate OnSerializePlayerEvent;

    private static void SerializePlayerEvent(string type, string sender, string value, int duration = 0,
        string log = "") => SerializePlayerEvent(type, sender, string.Empty, value, duration, log);
    private static void SerializePlayerEvent(string type, string sender, string receiver, string value, int duration = 0, string log = "")
    {
        var playerEvent = new PlayerEvent(type, sender, receiver, value, duration, log);
        OnSerializePlayerEvent?.Invoke(playerEvent);
    }
    
    public static void RunPlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
        OnPlayerEvent?.Invoke(playerEvent);
    }

    public static void OnGameStateChange(string state)
    {
        SerializePlayerEvent("state_change", "GameStateManager", state);
    }
    
    
    public static void OnInteraction(GameObject interactable)
    {
        if (interactable.TryGetComponent(out DialogueSystemTrigger dialogueSystemTrigger))
        {
            SerializePlayerEvent("interact", "player", interactable.name, "DialogueSystemTrigger", Clock.TimeScales.SecondsPerInteract);
            dialogueSystemTrigger.OnUse();
        }
    }
    
   public static void OnMove(string sender, string value, int duration = 0)
   {
       SerializePlayerEvent("move", sender, "player", value, duration);
   }
   
   public static void OnMove(string sender, Location location)
   {
       SerializePlayerEvent("move", sender, "player", location.name, location.TravelTime);
   }

   public static void OnPointsIncrease(Points.Type type, int points)
   {
         if (Points.IsAnimating == false) Points.AnimationStart(type);
         SerializePlayerEvent("points", DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.Title, type.ToString(), points.ToString());
   }
   
   
   public static void OnWait(int duration)
    {
        SerializePlayerEvent("wait", "player", duration.ToString(), duration);
    }
   
    public static void OnConversationDecision(string decision)
    {
        var conversationTitle = DialogueManager.instance.activeConversation.conversationTitle;
        var currentNode = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.Title;
        SerializePlayerEvent("decision", conversationTitle, currentNode, decision);
    }

    public static void OnAction(int index, string title)
    {
        SerializePlayerEvent("action", "player", title, index.ToString());
    }

    public static void OnConversationStart(string eventSender = "")
    {
        //get linked conversation start title
        
        int conversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
        var conversationTitle = DialogueManager.masterDatabase.GetConversation(conversationID).Title;
        SerializePlayerEvent("conversation_start",  eventSender, "", conversationTitle);
    }
    
    public static void OnConversationEnd()
    {
        var conversationTitle = DialogueManager.instance.activeConversation.conversationTitle;
        var currentEntry = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.id;
        SerializePlayerEvent("conversation_end",  "DialogueManager", conversationTitle, currentEntry.ToString());
    }

    public static void OnConversationResponseMenu()
    {
        var dialogueEntryNodeTitle = GameManager.GetHighestDialogueNodeValue();
        string responses = String.Empty;
        foreach (var response in DialogueManager.instance.currentConversationState.pcResponses)
        {
            if (responses.Length != 0) responses += ",";
            responses += response.destinationEntry.Title; 
        }
        SerializePlayerEvent("awaiting_response", String.Join(",", responses), "", dialogueEntryNodeTitle);
    }

    public static void OnConversationLine()
    {
        
        var text = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.currentDialogueText;
        var mostRecentResponseNode = GameManager.instance.gameStateManager.gameState.most_recent_response_node;
        var nodeValue = GameManager.GetHighestDialogueNodeValue();

        if (mostRecentResponseNode != string.Empty)
        {  
            SerializePlayerEvent("conversation_decision", "player", mostRecentResponseNode, nodeValue);
        }

        var nodeScript = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.userScript;
        
        if (nodeScript != string.Empty)
        {
            SerializePlayerEvent("conversation_script", nodeValue, nodeScript);
        }
            
        if (text == string.Empty) return;
        
       // var conversationTitle = DialogueManager.instance.activeConversation.conversationTitle;
        var dialogueEntryID = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.id;
        var conversationID = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.conversationID;
        var actorName = DialogueManager.instance.conversationController.actorInfo.Name;
        var conversantName = DialogueManager.instance.conversationController.conversantInfo.Name;
        var durationSeconds = DialogueUtility.GetNodeDuration(conversationID, dialogueEntryID);
        SerializePlayerEvent("conversation_line", actorName, conversantName, dialogueEntryID.ToString(), durationSeconds, $"{actorName}: {text}");
        
        
 }
    

  

    //UI SCREEN
    public delegate void OnUIScreen(Transform UIscreen);
    public static event OnUIScreen onUIScreenOpen;
    public static event OnUIScreen onUIScreenClose;

    public static void OpenUIScreen(Transform UIscreen) => onUIScreenOpen?.Invoke(UIscreen);
    public static void CloseUIScreen(Transform UIscreen) => onUIScreenClose?.Invoke(UIscreen);


    // UI element

    public delegate void UIElementMouseAction(Transform element);

    public static event UIElementMouseAction OnUIElementMouseClick;
    public static event UIElementMouseAction OnUIElementMouseHover;
    public static event UIElementMouseAction OnUIElementMouseExit;

    public static void UIElementMouseClick(Transform element) => OnUIElementMouseClick?.Invoke(element);
    public static void UIElementMouseHover(Transform element) => OnUIElementMouseHover?.Invoke(element);
    public static void UIElementMouseExit(Transform element) => OnUIElementMouseExit?.Invoke(element);



}
