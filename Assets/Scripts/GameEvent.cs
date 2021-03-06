using Apps;
using Interaction;
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
    
    
    // DELEGATES
    public delegate void InteractionStartDelegate(IInteractable interacObj);
    public delegate void InteractionEndDelegate(IInteractable interacObj);
    public delegate void PopupViewChangeDelegate();
    public delegate void PopupCreatedDelegate();
    public delegate void PopupClosedDelegate();
    public delegate void ChapterStartDelegate();
    public delegate void ChapterEndDelegate();
    public delegate void EmailDeliverDelegate();
    public delegate void GameSaveDelegate();
    public delegate void GameLoadDelegate();
    public delegate void TextSendDelegate(TextConversation convo, TextMessage message);
    public delegate void TextReceiveDelegate(TextConversation convo, TextMessage message);
    public delegate void PCEventDelegate();


    // EVENTS
    public static event InteractionStartDelegate OnInteractionStart;
    public static event InteractionEndDelegate OnInteractionEnd;
    public static event PopupViewChangeDelegate OnPopupViewChange;
    public static event PopupCreatedDelegate OnPopupCreate;
    public static event PopupClosedDelegate OnPopupClose;
    public static event ChapterStartDelegate OnChapterStart;
    public static event ChapterEndDelegate OnChapterEnd;
    public static event EmailDeliverDelegate OnEmailDeliver;
    public static event GameSaveDelegate OnGameSave;
    public static event GameLoadDelegate OnGameLoad;
    public static event TextSendDelegate OnTextSend;
    public static event TextReceiveDelegate OnTextReceive;
    public static event PCEventDelegate OnPCOpen;
    public static event PCEventDelegate OnPCClose;

    // TRIGGERS
    public static void InteractionStart(IInteractable interacObj) => OnInteractionStart?.Invoke(interacObj);
    public static void InteractionEnd(IInteractable interacObj) => OnInteractionEnd?.Invoke(interacObj);
    public static void ChangePopupView() => OnPopupViewChange?.Invoke();
    public static void PopupCreated() => OnPopupCreate?.Invoke();
    public static void PopupClose() => OnPopupClose?.Invoke();
    public static void StartChapter() => OnChapterStart?.Invoke();
    public static void EndChapter() => OnChapterEnd?.Invoke();
    public static void DeliverEmail(Email email) => OnEmailDeliver?.Invoke();
    public static void SaveGame() => OnGameSave?.Invoke();
    public static void LoadGame() => OnGameLoad?.Invoke();
    public static void SendText(TextConversation convo, TextMessage message) => OnTextSend?.Invoke(convo, message);
    public static void ReceiveText(TextConversation convo, TextMessage message) => OnTextReceive?.Invoke(convo, message);
    public static void OpenPC() => OnPCOpen?.Invoke();
    public static void ClosePC() => OnPCClose?.Invoke();
}
