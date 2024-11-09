using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

[RequireComponent(typeof(Button))]
public class ActionPanelButton : MonoBehaviour
{
    public enum ActionPanelButtonType
    {
        Walk,
        Talk,
        Action,
        Map
    }
    
    public Color responseMenuHue;
    
    public ActionPanelButtonType type;
    
    public Transform label;
    
    public Color defaultColor;

    private void Awake()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        defaultColor = button.colors.normalColor;
    }

    public void DarkenButton(bool darken)
    {
        var button = GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = darken ? Color.gray : defaultColor;
        button.colors = colors;
    }

    public Action StartConversation
    {
        get
        {
            switch (type)
            {
                case ActionPanelButtonType.Walk:
                    return StartWalkConversation;
                case ActionPanelButtonType.Talk:
                    return StartTalkConversation;
                case ActionPanelButtonType.Action:
                    return StartActionConversation;
                case ActionPanelButtonType.Map:
                    return StartMapConversation;
                default:
                    return null;
            }
        }
    }

    public void OnClick()
    {
        ActionPanel.ShowLabelAndHideAllOthers(this);
      //  ActionPanel.instance.circularUIMenuPanel.SetColor(responseMenuHue);
        StartConversation.Invoke();
    }
    
    public bool ConversationCheck()
    {
        
        bool ConversationExistsAndIsAvailable(string conversationName)
        {
            var conversationExists = DialogueManager.masterDatabase.GetConversation(conversationName) != null;
        
            if (!conversationExists) return false;
        
            var conditionsCheckExists = Field.FieldExists(DialogueManager.masterDatabase.GetConversation(conversationName).fields, "Conditions");

            if (!conditionsCheckExists) return true;
        
            var check = Field.LookupValue(DialogueManager.masterDatabase.GetConversation(conversationName).fields, "Conditions");
            return check == string.Empty || Lua.Run($"return {check}").asBool;
            
        }
        
        if (Location.PlayerLocation == null) return false;
        var location = Location.PlayerLocation.name;
        var sublocation = DialogueLua.GetLocationField(location, "Current Sublocation").asString;
        
        if (!string.IsNullOrEmpty(sublocation)) location += "/" + sublocation;
        
        switch (type)
        {
            case ActionPanelButtonType.Walk:
                return ConversationExistsAndIsAvailable($"{Location.PlayerLocation.name}/Walk/Base");
                break;
            case ActionPanelButtonType.Talk:
                return ConversationExistsAndIsAvailable($"{location}/Talk/Base");
                break;
            case ActionPanelButtonType.Action:
                return ConversationExistsAndIsAvailable($"{location}/Action/Base");
                break;
            case ActionPanelButtonType.Map:
                return ConversationExistsAndIsAvailable("Map");
                break;
        }
        
        return false;
    }

    public void StartWalkConversation()
    {
          
        var database = DialogueManager.instance.masterDatabase;
        var playerLocation = Location.PlayerLocation.name;
        var conversationName = playerLocation + "/Walk/Base";

        GoToConversation(conversationName);
        return;
    }

    public void GoToConversation(string conversationName)
    {
        var database = DialogueManager.instance.masterDatabase;
        if (!DialogueManager.instance.isConversationActive)
        {
            DialogueManager.instance.StartConversation(conversationName);
            return;
        }
            
        var conversation = database.GetConversation(conversationName);
        var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
        var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
        DialogueManager.conversationController.GotoState(state);
        DialogueManager.instance.PlaySequence("Continue()");
    }
    
    public void StartActionConversation()
    {
        var database = DialogueManager.instance.masterDatabase;
        var playerLocation = Location.PlayerLocation.name;
        var sublocation = DialogueLua.GetLocationField(playerLocation, "Current Sublocation").asString;
        if (!string.IsNullOrEmpty(sublocation)) playerLocation += "/" + sublocation;
        var conversationName = playerLocation + "/Action/Base";
        GoToConversation(conversationName);
        return;
       
    }
    
    public void StartTalkConversation()
    {
          
        var database = DialogueManager.instance.masterDatabase;
        var playerLocation = Location.PlayerLocation.name;
        var sublocation = DialogueLua.GetLocationField(playerLocation, "Current Sublocation").asString;
        if (!string.IsNullOrEmpty(sublocation)) playerLocation += "/" + sublocation;
        var conversationName = playerLocation + "/Talk/Base";

        GoToConversation(conversationName);
        return;
        
    }
    
    public void StartMapConversation()
    {
        var database = DialogueManager.instance.masterDatabase;
        var conversationName = "Map";

        GoToConversation(conversationName);
        
        return;
    }

}
