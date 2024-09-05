using System;
using System.Collections;
using System.Collections.Generic;
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
        ActionPanel.instance.circularUIMenuPanel.SetColor(responseMenuHue);
        StartConversation.Invoke();
    }

    public void StartWalkConversation()
    {
          
        var database = DialogueManager.instance.masterDatabase;
        var playerLocation = Location.PlayerLocation.name;
        var conversationName = playerLocation + "/Walk/Base";

        if (!DialogueManager.instance.isConversationActive)
        {
            Debug.Log("Starting conversation: " + conversationName);
            DialogueManager.instance.StartConversation(conversationName);
            return;
        }
            
        var conversation = database.GetConversation(conversationName);
        var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
        var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
        // DialogueManager.instance.BroadcastMessage("OnConversationBase", dialogueEntry);
        DialogueManager.conversationController.GotoState(state);
    }
    
    public void StartActionConversation()
    {
        var database = DialogueManager.instance.masterDatabase;
        var playerLocation = Location.PlayerLocation.name;
        var sublocation = DialogueLua.GetLocationField(playerLocation, "Current Sublocation").asString;
        if (!string.IsNullOrEmpty(sublocation)) playerLocation += "/" + sublocation;
        var conversationName = playerLocation + "/Action/Base";
            
            
        if (!DialogueManager.instance.isConversationActive)
        {
            DialogueManager.instance.StartConversation(conversationName);
            return;
        }
            
        var conversation = database.GetConversation(conversationName);
        var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
            
        //DialogueManager.instance.BroadcastMessage("OnConversationBase", dialogueEntry);
            
        var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
        DialogueManager.conversationController.GotoState(state);
    }
    
    public void StartTalkConversation()
    {
          
        var database = DialogueManager.instance.masterDatabase;
        var playerLocation = Location.PlayerLocation.name;
        var sublocation = DialogueLua.GetLocationField(playerLocation, "Current Sublocation").asString;
        if (!string.IsNullOrEmpty(sublocation)) playerLocation += "/" + sublocation;
        var conversationName = playerLocation + "/Talk/Base";

        if (!DialogueManager.instance.isConversationActive)
        {
            DialogueManager.instance.StartConversation(conversationName);
            return;
        }
            
        var conversation = database.GetConversation(conversationName);
        var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
        var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
        // DialogueManager.instance.BroadcastMessage("OnConversationBase", dialogueEntry);
        DialogueManager.conversationController.GotoState(state);
    }
    
    public void StartMapConversation()
    {
        var database = DialogueManager.instance.masterDatabase;
        var conversationName = "Map";

        if (!DialogueManager.instance.isConversationActive)
        {
            DialogueManager.instance.StartConversation(conversationName);
            return;
        }
            
        var conversation = database.GetConversation(conversationName);
        var dialogueEntry = database.GetDialogueEntry(conversation.id, 0);
        var state = DialogueManager.instance.conversationModel.GetState(dialogueEntry);
        // DialogueManager.instance.BroadcastMessage("OnConversationBase", dialogueEntry);
        DialogueManager.conversationController.GotoState(state);
    }

}
