﻿using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

namespace Project.Runtime.Scripts.Events
{
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


        //UI SCREEN
        public delegate void OnUIScreen(Transform UIscreen);

        public delegate void PlayerEventDelegate(PlayerEvent playerEvent);


        // UI element

        public delegate void UIElementMouseAction(Transform element);

        public static event LoadDelegate OnLoadStart;
        public static event LoadDelegate OnLoadEnd;

        public static void LoadStart() => OnLoadStart?.Invoke();
        public static void LoadEnd() => OnLoadEnd?.Invoke();

        public static event PlayerEventDelegate OnPlayerEvent;

        public static event PlayerEventDelegate OnRegisterPlayerEvent;

        private static void RegisterPlayerEvent(string eventType, JObject data, int duration = 0)
        {
           
            var playerEvent = new PlayerEvent(eventType, data, duration);
            OnRegisterPlayerEvent?.Invoke(playerEvent);
        }

        public static void RunPlayerEvent(PlayerEvent playerEvent)
        {
            
            OnPlayerEvent?.Invoke(playerEvent);
        }


        public static void OnInteraction(GameObject interactable)
        {
            if (interactable.TryGetComponent(out DialogueSystemTrigger dialogueSystemTrigger))
            {
                var data = new JObject
                {
                    ["target"] = interactable.name,
                    ["trigger"] = dialogueSystemTrigger.ToString()
                };
                RegisterPlayerEvent("interact", data, Clock.SecondsPerInteract);
                dialogueSystemTrigger.OnUse();
            }
        }

        public static void OnMove(string locName, string lastLocation, int duration)
        {
            var data = new JObject
            {
                ["lastLocation"] = lastLocation,
                ["newLocation"] = locName
            };
            RegisterPlayerEvent("move", data, duration);
        }

        public static void OnPointsIncrease(Points.PointsField pointData, string source)
        {
            var data = new JObject
            {
                ["points"] = pointData.Points.ToString(),
                ["pointsType"] = pointData.Type.ToString(),
                ["source"] = source
            };
            RegisterPlayerEvent("points", data);
        }

        public static void OnQuestStateChange(string questName, QuestState state, Points.PointsField[] points, int duration)
        {
            var questData = new JObject
            {
                ["questName"] = questName,
                ["state"] = state.ToString(),
                ["points"] = JArray.FromObject(points.Where(pf => pf.Points != 0))
            };
            RegisterPlayerEvent("quest_state_change", questData, duration);
        }
        
        public static void OnActionStateChange(string actionName, QuestState state, Points.PointsField[] points, int duration)
        {
            var actionData = new JObject
            {
                ["actionName"] = actionName,
                ["state"] = state.ToString(),
                ["points"] = JArray.FromObject(points.Where(pf => pf.Points != 0))
            };
            RegisterPlayerEvent("action_state_change", actionData, duration);
        }

        public static void OnWait(int duration)
        {
            RegisterPlayerEvent("wait", null, duration);
        }

        public static void OnDayEnd()
        {
            var data = new JObject
            {
                ["day"] = GameManager.instance.dailyReport.Day,
                ["dailyReport"] = JsonConvert.SerializeObject(GameManager.instance.dailyReport)
            };
            RegisterPlayerEvent("end_day", data);
        }

        public static void OnConversationStart(string eventSender = "")
        {
            //get linked conversation start title
        
            int conversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
            var conversationTitle = DialogueManager.masterDatabase.GetConversation(conversationID).Title;
            var data = new JObject
            {
                ["conversationTitle"] = conversationTitle,
                ["currentEntry"] = DialogueManager.currentConversationState.subtitle.dialogueEntry.id
            };
            RegisterPlayerEvent("conversation_start",  data);
        }

        public static void OnConversationEnd()
        {
            if (DialogueManager.instance.activeConversation == null) return;
            var conversationTitle = DialogueManager.instance.activeConversation.conversationTitle;
            var currentEntry = DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.id;
            var data = new JObject
            {
                ["conversationTitle"] = conversationTitle,
                ["currentEntry"] = currentEntry
            };
            RegisterPlayerEvent("conversation_end",  data);
        }

        public static event OnUIScreen onUIScreenOpen;
        public static event OnUIScreen onUIScreenClose;

        public static void OpenUIScreen(Transform UIscreen) => onUIScreenOpen?.Invoke(UIscreen);
        public static void CloseUIScreen(Transform UIscreen) => onUIScreenClose?.Invoke(UIscreen);

        public static event UIElementMouseAction OnUIElementMouseClick;
        public static event UIElementMouseAction OnUIElementMouseHover;
        public static event UIElementMouseAction OnUIElementMouseExit;

        public static void UIElementMouseClick(Transform element) => OnUIElementMouseClick?.Invoke(element);
        public static void UIElementMouseHover(Transform element) => OnUIElementMouseHover?.Invoke(element);
        public static void UIElementMouseExit(Transform element) => OnUIElementMouseExit?.Invoke(element);
    }
}