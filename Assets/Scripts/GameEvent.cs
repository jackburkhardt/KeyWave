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
    
    public delegate void InteractionStartDelegate(IInteractable interacObj);
    public delegate void InteractionEndDelegate(IInteractable interacObj);
    public delegate void PopupViewChangeDelegate();
    public delegate void PopupCreatedDelegate();

    public delegate void PopupClosedDelegate();
    public static event InteractionStartDelegate OnInteractionStart;
    public static event InteractionEndDelegate OnInteractionEnd;
    public static event PopupViewChangeDelegate OnPopupViewChange;
    public static event PopupCreatedDelegate OnPopupCreate;
    public static event PopupClosedDelegate OnPopupClose;

    public static void InteractionStart(IInteractable interacObj) => OnInteractionStart?.Invoke(interacObj);
    public static void InteractionEnd(IInteractable interacObj) => OnInteractionEnd?.Invoke(interacObj);
    public static void ChangePopupView() => OnPopupViewChange?.Invoke();
    public static void PopupCreated() => OnPopupCreate?.Invoke();
    public static void PopupClose() => OnPopupClose?.Invoke();
}
