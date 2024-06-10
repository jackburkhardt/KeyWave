using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PixelCrushers.DialogueSystem;
using UnityEngine;

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
    
    public delegate void PlayerEventDelegate(PlayerEvent playerEvent);
    
    public static event PlayerEventDelegate OnPlayerEvent;

    public static event PlayerEventDelegate OnRegisterPlayerEvent;
    
    private static void RegisterPlayerEvent(string eventType, string source, string target, string data = null, int duration = 0)
    {
        Debug.Log("Registering player event: " + eventType);
        var playerEvent = new PlayerEvent(eventType, source, target, data, duration);
        OnRegisterPlayerEvent?.Invoke(playerEvent);
    }
    
    public static void RunPlayerEvent(PlayerEvent playerEvent)
    {
        Debug.Log("Running player event: " + playerEvent.EventType);
        OnPlayerEvent?.Invoke(playerEvent);
    }
    
    
    public static void OnInteraction(GameObject interactable)
    {
        if (interactable.TryGetComponent(out DialogueSystemTrigger dialogueSystemTrigger))
        {
            RegisterPlayerEvent("interact", "player", interactable.name, dialogueSystemTrigger.ToString(), Clock.TimeScales.SecondsPerInteract);
            dialogueSystemTrigger.OnUse();
        }
    }
   
   public static void OnMove(string locName, Location lastLocation, int duration)
   {
       RegisterPlayerEvent("move", "player", locName, "{\"previous_location\":\""+ lastLocation.ToString() +"\"}", duration);
   }

   public static void OnPointsIncrease(Points.PointsField pointData, string source)
   {
       RegisterPlayerEvent("points", source, "player", pointData.ToString());
   }
   
   public static void OnQuestStateChange(string questName, QuestState state, int duration)
   {
       var quest = DialogueUtility.GetQuestByName(questName);
       RegisterPlayerEvent("quest_state_change", questName, state.ToString(), JsonConvert.SerializeObject(quest), duration);
   }
   
   public static void OnWait(int duration)
   {
       RegisterPlayerEvent("wait", "player", "player", duration.ToString(), duration);
   }

   public static void OnDayEnd()
   {
       RegisterPlayerEvent("end_day", "", "");
   }

    public static void OnConversationStart(string eventSender = "")
    {
        //get linked conversation start title
        
        int conversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
        var conversationTitle = DialogueManager.masterDatabase.GetConversation(conversationID).Title;
        RegisterPlayerEvent("conversation_start",  eventSender, "", conversationTitle);
    }
    
    public static void OnConversationEnd()
    {
        var conversationTitle = DialogueManager.instance.activeConversation.conversationTitle;
        var currentEntry = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.id;
        RegisterPlayerEvent("conversation_end",  "DialogueManager", conversationTitle, currentEntry.ToString());
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
        RegisterPlayerEvent("awaiting_response", String.Join(",", responses), "", dialogueEntryNodeTitle);
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
