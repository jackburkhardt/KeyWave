using PixelCrushers.DialogueSystem;
using UnityEngine;

public class CustomLuaFunctions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      RegisterLuaFunctions();
    }

    private void OnEnable()
    {
        RegisterLuaFunctions();
    }

    private void OnDisable()
    {
        DeregisterLuaFunctions();
    }

    // Update is called once per frame
    void Update()
    {
        
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
    
    public string LocationIDToName(System.Single locationID)
    {
        return DialogueManager.DatabaseManager.masterDatabase.GetLocation((int)locationID).Name;
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
        Clock.Freeze(freeze);
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
}
