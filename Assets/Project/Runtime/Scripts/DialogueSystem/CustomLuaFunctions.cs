using System;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts.DialogueSystem
{
    public class CustomLuaFunctions : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            RegisterLuaFunctions();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnEnable()
        {
            RegisterLuaFunctions();
        }

        private void OnDisable()
        {
            DeregisterLuaFunctions();
        }

        private void RegisterLuaFunctions()
        {
            Lua.RegisterFunction(nameof(SurpassedTime), this, SymbolExtensions.GetMethodInfo(() => SurpassedTime(string.Empty)));
            Lua.RegisterFunction(nameof(BeforeTimeslot), this, SymbolExtensions.GetMethodInfo(() => BeforeTimeslot(string.Empty)));
            Lua.RegisterFunction(nameof(AfterTimeslot), this, SymbolExtensions.GetMethodInfo(() => AfterTimeslot(string.Empty)));
            Lua.RegisterFunction(nameof(WithinTimeslotRange), this, SymbolExtensions.GetMethodInfo(() => WithinTimeslotRange(string.Empty, string.Empty)));
            Lua.RegisterFunction(nameof(WithinSeconds), this, SymbolExtensions.GetMethodInfo(() => WithinSeconds(string.Empty, 0)));
            Lua.RegisterFunction(nameof(WithinMinutes), this, SymbolExtensions.GetMethodInfo(() => WithinMinutes(string.Empty, 0)));
            Lua.RegisterFunction(nameof(FreezeClock), this, SymbolExtensions.GetMethodInfo(() => FreezeClock(false)));
            Lua.RegisterFunction(nameof(QuestInProgress), this, SymbolExtensions.GetMethodInfo(() => QuestInProgress(string.Empty)));
            Lua.RegisterFunction(nameof(QuestPartiallyComplete), this, SymbolExtensions.GetMethodInfo(() => QuestPartiallyComplete(string.Empty)));
            Lua.RegisterFunction(nameof(QuestInProgressButNascent), this, SymbolExtensions.GetMethodInfo(() => QuestInProgressButNascent(string.Empty)));
            Lua.RegisterFunction(nameof(LocationIDToName), this, SymbolExtensions.GetMethodInfo(() => LocationIDToName(0)));
            Lua.RegisterFunction(nameof(HourMinuteToTime), this, SymbolExtensions.GetMethodInfo(() => HourMinuteToTime(0, 0)));
            Lua.RegisterFunction(nameof(UnlockLocation), this, SymbolExtensions.GetMethodInfo(() => UnlockLocation(string.Empty)));
            Lua.RegisterFunction(nameof(IsLocationUnlocked), this, SymbolExtensions.GetMethodInfo(() => IsLocationUnlocked(string.Empty)));
            Lua.RegisterFunction(nameof(EmailState), this, SymbolExtensions.GetMethodInfo(() => EmailState(string.Empty)));
            Lua.RegisterFunction(nameof(CartItem), this, SymbolExtensions.GetMethodInfo(() => CartItem(string.Empty)));
            Lua.RegisterFunction(nameof(Not), this, SymbolExtensions.GetMethodInfo(() => Not(false)));
            Lua.RegisterFunction(nameof(ToggleInventoryItem), this, SymbolExtensions.GetMethodInfo(() => ToggleInventoryItem(string.Empty)));
            Lua.RegisterFunction(nameof(ClearInventory), this, SymbolExtensions.GetMethodInfo(() => ClearInventory(string.Empty)));
            Lua.RegisterFunction(nameof(MainQuestCount), this, SymbolExtensions.GetMethodInfo(() => MainQuestCount()));
            Lua.RegisterFunction(nameof(ActiveMainQuestCount), this, SymbolExtensions.GetMethodInfo(() => ActiveMainQuestCount()));
            Lua.RegisterFunction(nameof(CompletedMainQuestCount), this, SymbolExtensions.GetMethodInfo(() => CompletedMainQuestCount()));
            Lua.RegisterFunction(nameof(CompletedActionQuestCount), this, SymbolExtensions.GetMethodInfo(() => CompletedActionQuestCount()));
            Lua.RegisterFunction(nameof(LocationETA), this, SymbolExtensions.GetMethodInfo(() => LocationETA(string.Empty)));
            Lua.RegisterFunction(nameof(LocationDistanceInMinutes), this, SymbolExtensions.GetMethodInfo(() => LocationDistanceInMinutes(string.Empty)));
            Lua.RegisterFunction(nameof(SetExclusiveQuestEntryState), this , SymbolExtensions.GetMethodInfo(() => SetExclusiveQuestEntryState(string.Empty, 0, string.Empty)));
            Lua.RegisterFunction(nameof(SkipTime), this, SymbolExtensions.GetMethodInfo(() => SkipTime(0)));
            Lua.RegisterFunction(nameof(LikedThat), this, SymbolExtensions.GetMethodInfo(() => LikedThat(string.Empty, true)));
            Lua.RegisterFunction(nameof(SetConversationConditions), this,
                SymbolExtensions.GetMethodInfo(() => SetConversationConditions(0, string.Empty)));
            Lua.RegisterFunction(nameof(AddMinutes), this, SymbolExtensions.GetMethodInfo(() => AddMinutes(0)));
            Lua.RegisterFunction(nameof(Increment), this, SymbolExtensions.GetMethodInfo(() => Increment(null)));
            Lua.RegisterFunction(nameof(SecondsToTime), this, SymbolExtensions.GetMethodInfo(() => SecondsToTime(0)));
            
            Lua.RegisterFunction(nameof(WithinMinutesOfVariable), this, SymbolExtensions.GetMethodInfo(() => WithinMinutesOfVariable(string.Empty, 0)));
            Lua.RegisterFunction(nameof(AddToCurrentTime), this, SymbolExtensions.GetMethodInfo(() => AddToCurrentTime(0)));
            Lua.RegisterFunction(nameof(ShowIntroductionText), this,
                SymbolExtensions.GetMethodInfo(() => ShowIntroductionText()));
            Lua.RegisterFunction(nameof(MinutesUntilNextScheduledEvent), this,
                SymbolExtensions.GetMethodInfo(() => MinutesUntilNextScheduledEvent(string.Empty)));
            Lua.RegisterFunction(nameof(TimeOfNextScheduledEvent), this,
                SymbolExtensions.GetMethodInfo(() => TimeOfNextScheduledEvent(string.Empty)));
            Lua.RegisterFunction(nameof(Length), this,
                SymbolExtensions.GetMethodInfo(() => Length(string.Empty)));
            Lua.RegisterFunction(nameof(SaveGame), this, SymbolExtensions.GetMethodInfo(() => SaveGame()));
            
            Lua.RegisterFunction(nameof(PlayerLocation), this, SymbolExtensions.GetMethodInfo(() => PlayerLocation()));

            Lua.RegisterFunction(nameof(SetRootApp), this,
                SymbolExtensions.GetMethodInfo(() => SetRootApp(string.Empty)));
        }

        private void DeregisterLuaFunctions()
        {
            Lua.UnregisterFunction(nameof(SurpassedTime));
            Lua.UnregisterFunction(nameof(BeforeTimeslot));
            Lua.UnregisterFunction(nameof(AfterTimeslot));
            Lua.UnregisterFunction(nameof(WithinTimeslotRange));
            Lua.UnregisterFunction(nameof(WithinSeconds));
            Lua.UnregisterFunction(nameof(WithinMinutes));
            Lua.UnregisterFunction(nameof(FreezeClock));
            Lua.UnregisterFunction(nameof(QuestInProgress));
            Lua.UnregisterFunction(nameof(QuestPartiallyComplete));
            Lua.UnregisterFunction(nameof(QuestInProgressButNascent));
            Lua.UnregisterFunction(nameof(LocationIDToName));
            Lua.UnregisterFunction(nameof(HourMinuteToTime));
            Lua.UnregisterFunction(nameof(UnlockLocation));
            Lua.UnregisterFunction(nameof(IsLocationUnlocked));
            Lua.UnregisterFunction(nameof(EmailState));
            Lua.UnregisterFunction(nameof(CartItem));
            Lua.UnregisterFunction(nameof(Not));
            Lua.UnregisterFunction(nameof(ToggleInventoryItem));
            Lua.UnregisterFunction(nameof(ClearInventory));
            Lua.UnregisterFunction(nameof(MainQuestCount));
            Lua.UnregisterFunction(nameof(ActiveMainQuestCount));
            Lua.UnregisterFunction(nameof(CompletedMainQuestCount));
            Lua.UnregisterFunction(nameof(CompletedActionQuestCount));
            Lua.UnregisterFunction(nameof(LocationETA));
            Lua.UnregisterFunction(nameof(LocationDistanceInMinutes));
            Lua.UnregisterFunction(nameof(SetExclusiveQuestEntryState));
            Lua.UnregisterFunction(nameof(SkipTime));
            Lua.UnregisterFunction(nameof(LikedThat));
            Lua.UnregisterFunction(nameof(SetConversationConditions));
            Lua.UnregisterFunction(nameof(AddMinutes));
            Lua.UnregisterFunction(nameof(Increment));
            Lua.UnregisterFunction(nameof(SecondsToTime));
            Lua.UnregisterFunction(nameof(WithinMinutesOfVariable));
            Lua.UnregisterFunction(nameof(AddToCurrentTime));
            Lua.UnregisterFunction(nameof(ShowIntroductionText));
            Lua.UnregisterFunction(nameof(MinutesUntilNextScheduledEvent));
            Lua.UnregisterFunction(nameof(TimeOfNextScheduledEvent));
            Lua.UnregisterFunction(nameof(Length));
            Lua.RegisterFunction(nameof(SaveGame), this, SymbolExtensions.GetMethodInfo(() => SaveGame()));
            Lua.UnregisterFunction(nameof(PlayerLocation));
            Lua.UnregisterFunction(nameof(SetRootApp));
        }
        
        
        


        //lua functions

        public string HourMinuteToTime(double hour, double minute)
        {
            var hourString = hour.ToString();
            if (hourString.Length == 1) hourString = "0" + hourString;
        
            var minuteString = minute.ToString();
            if (minuteString.Length == 1) minuteString = "0" + minuteString;

            return hourString + ":" + minuteString;
        }

        public string LocationIDToName(Single locationID)
        {
            return DialogueManager.DatabaseManager.masterDatabase.GetLocation((int)locationID).Name;
        }

        public void SkipTime(double seconds)
        {
            GameEvent.OnWait((int)seconds);
        }

        public string EmailState(string itemName)
        {
            var emailState = QuestLog.GetQuestState(itemName);
        
            switch (emailState) {
                case QuestState.Success:
                    return "[OPENED]";
                case QuestState.Failure:
                    return "[OPENED]";
                default:
                    return "[UNREAD]";
            }
        }

        public string CartItem(string itemName)
        {
            var item = DialogueManager.DatabaseManager.masterDatabase.items.Find(i => i.Name == itemName);
            if (item == null) return string.Empty;
        
            var itemDisplayName = DialogueLua.GetItemField(itemName, "Display Name").asString;
        
            var itemCost = DialogueLua.GetItemField(itemName, "Cost").asInt;
        
            var isItemInCart =  DialogueLua.GetItemField(itemName, "In Inventory").asBool;
        
            var prefaceText = isItemInCart ? "[REMOVE]" : "[ADD]";

            return $"{prefaceText} {itemDisplayName} - ${itemCost}";

        }

        public void ClearInventory(string cartName)
        {
            var cart = DialogueManager.DatabaseManager.masterDatabase.items.Find(i => i.Name == cartName);
            if (cart == null) return;
        
            // get all items that have an "Inventory" field and check if that field is equal to the cart name
        
            var cartItems = DialogueManager.DatabaseManager.masterDatabase.items.FindAll(i => i.fields.Exists(f => f.title == "Inventory" && f.value == cart.id.ToString()));
        
   
            foreach (var item in cartItems)
            {
                //if the item has a field "In Inventory" and it's true, set it to false
                if (item.FieldExists("In Inventory") && item.LookupBool("In Inventory"))
                {
                    DialogueLua.SetItemField(item.Name, "In Inventory", false);
                }
            }
        
            DialogueLua.SetItemField(cartName, "Total", 0);
        }

        public void ToggleInventoryItem(string itemName)
        {
            var item = DialogueManager.DatabaseManager.masterDatabase.items.Find(i => i.Name == itemName);
            if (item == null) return;
        
            var isItemInCart =  DialogueLua.GetItemField(itemName, "In Inventory").asBool;
        
            var itemCost = DialogueLua.GetItemField(itemName, "Cost").asInt;
        
            DialogueLua.SetItemField(itemName, "In Inventory", !isItemInCart);

            var inventory = DialogueLua.GetItemField(itemName, "Inventory").asInt;
        
            var inventoryItem = DialogueManager.DatabaseManager.masterDatabase.items.Find(i => i.id == inventory);
        
            if (inventoryItem == null) return;
        
            var totalInventoryCost = DialogueLua.GetItemField(inventoryItem.Name, "Total").asInt;
        
            DialogueLua.SetItemField(inventoryItem.Name, "Total", isItemInCart ? totalInventoryCost - itemCost : totalInventoryCost + itemCost);
        
        }

        public bool Not(bool value) => !value;

        public void UnlockLocation(string location)
        {
            Location.FromString(location).unlocked = true;
        }

        public bool IsLocationUnlocked(string location)
        {
            return Location.FromString(location).unlocked;
        }

        public bool QuestInProgressButNascent(string quest) => QuestUtility.QuestInProgressButNascent(quest);

        public bool QuestInProgress(string quest)
        {
            return QuestUtility.QuestInProgress(quest);
        }

        public bool QuestPartiallyComplete(string quest)
        {
            return QuestUtility.QuestPartiallyComplete(quest);
        }


        public void FreezeClock(bool freeze)
        {
          //  Clock.Freeze(freeze);
        }

        public bool SurpassedTime(string time)
        {
       
            var timeInSeconds = Clock.ToSeconds(time);

            return Clock.CurrentTimeRaw > timeInSeconds;
        }

        public bool BeforeTimeslot(string time)
        {
       
            var timeInSeconds = Clock.ToSeconds(time);

            return Clock.CurrentTimeRaw < timeInSeconds;
        }

        public bool AfterTimeslot(string time)
        {
       
            var timeInSeconds = Clock.ToSeconds(time);

            return Clock.CurrentTimeRaw >= timeInSeconds;
        }

        public bool WithinTimeslotRange(string time1, string time2)
        {
        
            var time1InSeconds = Clock.ToSeconds(time1);
            var time2InSeconds = Clock.ToSeconds(time2);

            return Clock.CurrentTimeRaw > time1InSeconds && Clock.CurrentTimeRaw < time2InSeconds;
        }

        public bool WithinSeconds(string time, double gracePeriod)
        {
        
            var timeInSeconds = Clock.ToSeconds(time);
            return Clock.CurrentTimeRaw > timeInSeconds - (int)gracePeriod && Clock.CurrentTimeRaw < timeInSeconds + (int)gracePeriod;
        }

        public bool WithinMinutes(string time, double gracePeriod)
        {
            return WithinSeconds(time, gracePeriod * 60);
        }

        public int MainQuestCount()
        {
            return DialogueManager.masterDatabase.GetQuests(group: "Main Task").Count;
        }

        public int ActiveMainQuestCount()
        {
            return DialogueManager.masterDatabase.GetQuests(group: "Main Task").FindAll(i => i.GetQuestState() != QuestState.Unassigned).Count;
        }

        public int CompletedMainQuestCount()
        {
            return DialogueManager.masterDatabase.GetQuests(group: "Main Task").FindAll(i => i.GetQuestState() == QuestState.Success).Count;
        }

        public int CompletedActionQuestCount()
        {
            return DialogueManager.masterDatabase.GetQuests(group: "Action").FindAll(i => i.GetQuestState() == QuestState.Success).Count;
        }

        public string LocationETA(string location)
        {
            var playerLocation = Location.FromString(location);
            return Clock.EstimatedTimeOfArrival(playerLocation);
        }

        public int LocationDistanceInMinutes(string location)
        {
            var playerLocation = Location.FromString(location);
            return playerLocation.TravelTime / 60;
        }

        public void SetExclusiveQuestEntryState(string questName, double entry, string questState)
        {
            
            for (int i = 1; i <= QuestLog.GetQuestEntryCount(questName); i++)
            {
                if (i == (int)entry) QuestLog.SetQuestEntryState(questName, i, questState);
                else
                {
                    if (QuestLog.GetQuestEntryState(questName, i) != QuestState.Active) continue;
                    QuestLog.SetQuestEntryState(questName, i, QuestState.Success);
                }
            }
        }
        
        public void LikedThat(string actorName, bool liked)
        {
            var offset = liked ? 0.25f : -0.25f;
            var relationship = DialogueLua.GetActorField(actorName, "PlayerRelationship").asFloat;
            var newRelationship = Mathf.Clamp(relationship + offset, -1f, 1f);
            DialogueLua.SetActorField(actorName, "PlayerRelationship", relationship + offset);
        }
        
        public void SetConversationConditions(double conversationID, string fieldValue)
        {
           // Debug.Log($"Setting conversation '{DialogueManager.instance.GetConversationTitle((int)conversationID)}' field {fieldName} to {fieldValue}");

            var conversation =
                DialogueManager.masterDatabase.GetConversation(
                    DialogueManager.instance.GetConversationTitle((int)conversationID));
            
            var availableFieldExists = Field.FieldExists(conversation.fields, "Conditions");
            if (!availableFieldExists)
            {
                Field.SetValue(conversation.fields, "Conditions", fieldValue);
            }
            
            DialogueLua.SetConversationField((int)conversationID, "Conditions", fieldValue);
            //Debug.Log($"Field set to {DialogueLua.GetConversationField((int)conversationID, "Conditions").}");
        }
        
        public void AddMinutes(double minutes)
        {
            Clock.AddSeconds((int)minutes * 60);
        }

        public void Increment(string var)
        {
            var value = DialogueLua.GetVariable(var).asInt;
            DialogueLua.SetVariable(var, value + 1);
        }
        
        public string SecondsToTime(double seconds)
        {
            return Clock.To24HourClock((int)seconds);
        }
        
        public bool WithinMinutesOfVariable(string var, double rangeInMinutes)
        {
            if (string.IsNullOrEmpty(var)) return false;
            var range = (int)rangeInMinutes * 60;
            var timeInSeconds = Clock.ToSeconds(var);
            return Clock.CurrentTimeRaw > timeInSeconds - range && Clock.CurrentTimeRaw < timeInSeconds + range;
        }
        
        public string AddToCurrentTime(double minutes)
        {
            var time = Clock.CurrentTimeRaw + (int)minutes * 60;
            return Clock.To24HourClock(time);
        }

        public bool ShowIntroductionText()
        {
            return Lua.IsTrue(
                "Dialog[thisID].SimStatus ~= \"WasDisplayed\" and (Variable[\"skip_location_intros\"] == false) or Dialog[thisID].SimStatus ~= \"WasDisplayed\" and (Variable[\"skip_content\"] == false)");
        }
        
        public string TimeOfNextScheduledEvent(string eventName)
        {
            var item = DialogueManager.DatabaseManager.masterDatabase.items.Find(i => i.Name == eventName);
            var scheduledEventType = DialogueLua.GetItemField(item.Name, "Scheduled Event Type").asString;
            if (scheduledEventType == "Repeating")
            {
                var startTimeInSeconds = Clock.ToSeconds(DialogueLua.GetItemField(item.Name, "Start Time").asString);
                var frequency = DialogueLua.GetItemField(item.Name, "Frequency").asInt;
                var endTimeInSeconds = Clock.ToSeconds(DialogueLua.GetItemField(item.Name, "End Time").asString);
              
                while (Clock.CurrentTimeRaw > startTimeInSeconds)
                {
                    startTimeInSeconds += frequency * 60;
                    if (startTimeInSeconds > endTimeInSeconds)
                    {
                        Debug.LogError("Scheduled event start time is greater than end time");
                        return "ERROR";
                    }
                }

                return Clock.To24HourClock(startTimeInSeconds);
            }
            else
            {
                Debug.LogError($"Scheduled event type {scheduledEventType} not supported");
                return "ERROR";
            }
        }
        
        public int MinutesUntilNextScheduledEvent(string eventName)
        {
            if (TimeOfNextScheduledEvent(eventName) == "ERROR") return -1;
            return (Clock.ToSeconds(TimeOfNextScheduledEvent(eventName)) - Clock.CurrentTimeRaw)/60;
        }
        
        public int Length(string var)
        {
            return var.Length;
        }
        
        public void SaveGame()
        {
            PixelCrushers.SaveSystem.SaveToSlot(1);
        }


        public string PlayerLocation()
        {
            return GameManager.gameState.PlayerLocation;
        }
        
        public void SetRootApp(string app)
        {
            GameManager.MostRecentApp = app;
        }
    }
}