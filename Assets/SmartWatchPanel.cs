using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class SmartWatchPanel : UIPanel
{
    
    public string focusAnimationTrigger;
    public string unfocusAnimationTrigger;

    private static SmartWatchAppPanel _currentApp;
    public static Action<SmartWatchAppPanel> onAppOpen;
    
    [ConversationPopup] public string conversation;
    
    public List<SmartWatchAppPanel> appPanels =>
        GetComponentsInChildren<SmartWatchAppPanel>(true).ToList();

    public override void Open()
    {
        DialogueManager.StartConversation(conversation);
        if (_currentApp == null)
        {
            var defaultApp = GetAllApps().Find(x => x.LookupBool("Is Default"));
            Debug.Log(defaultApp.Name);
            _currentApp = appPanels.Find(p => p.Name == defaultApp.Name);
        }
        
        OpenApp( _currentApp.Name);
        base.Open();
    }
    
    public void OpenApp( string appName)
    {
        var appPanel = appPanels.FirstOrDefault(p => p.Name == appName);
        appPanel.panel.Open();
    }


    protected override void OnEnable()
    {
        base.OnEnable();

        var smartWatchApps = FindObjectsByType<SmartWatchAppPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var smartWatchApp in smartWatchApps)
        {
            onAppOpen += smartWatchApp.OnAppOpen;
        }
        
        onAppOpen += OnAppOpen;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        var smartWatchApps = FindObjectsByType<SmartWatchAppPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var smartWatchApp in smartWatchApps)
        {
            onAppOpen -= smartWatchApp.OnAppOpen;
        }
        
        onAppOpen -= OnAppOpen;
    }
    
    protected override void OnVisible()
    {
        base.OnVisible();
        
        if (DialogueManager.instance.IsConversationActive)
        {
            var currentSubtitle = DialogueManager.instance.currentConversationState.subtitle;
            var conversation =
                DialogueManager.instance.masterDatabase.GetConversation(currentSubtitle.dialogueEntry.conversationID);
        }
    }

    public void OnConversationStart()
    {

        if (_currentApp == null) return;
        switch (_currentApp.Name)
        {
            case "Phone":
            {
                GetComponent<Animator>().SetTrigger("Down");
                break;
            }

            case "Home":
            {
                //  GetComponent<Animator>().SetTrigger(showAnimationTrigger);
                break;
            }

            default:
                
                GetComponent<Animator>().SetTrigger(unfocusAnimationTrigger);
                break;
        }
    }

    public static List<Item> GetAllApps()
    {
        return GameManager.settings.dialogueDatabase.items.FindAll(p => p.IsApp);
    }

    public void ForceDefaultApp()
    {
        _currentApp = null;
        DialogueManager.instance.StopConversation();
        Open();
    }

    public void ResetCurrentApp()
    {
        _currentApp = null;
    }

    public void OnLinkedConversationStart()
    {
        OnConversationStart();
    }

    private void OnAppOpen(SmartWatchAppPanel app)
    {
        animatorMonitor.SetTrigger(focusAnimationTrigger, null);
        FindObjectOfType<CustomDialogueUI>().ClearAllDefaultOverrides();
    }

    public void OnConversationEnd()
    {
        if (_currentApp != null)
        {
            if (_currentApp.Name == "Travel")
            {
                GetComponent<Animator>().SetTrigger(unfocusAnimationTrigger);
            }
        }
        
        
    }
    

    private void ForceResponseMenuPanel()
    {
        DialogueManager.StopConversation();
        if (DialogueManager.masterDatabase.GetConversation("GENERATED/SmartWatch") != null) return;
        
        var template = Template.FromDefault();
        var newConversation =
            template.CreateConversation(template.GetNextConversationID(DialogueManager.masterDatabase), "GENERATED/SmartWatch");
        var startEntry = template.CreateDialogueEntry(template.GetNextDialogueEntryID(newConversation), newConversation.id,
            "Start");
        startEntry.Title = "START";
        startEntry.Sequence = "(None)";
        startEntry.ActorID = DialogueManager.masterDatabase.actors.Find(p => p.IsPlayer).id;
        startEntry.isRoot = true;
        
        var followupEntry = template.CreateDialogueEntry(template.GetNextDialogueEntryID(newConversation), newConversation.id,
            "Followup");
        followupEntry.Title = "FOLLOWUP";
        followupEntry.ActorID = startEntry.ActorID;
        
        startEntry.outgoingLinks.Add(new Link(newConversation.id, startEntry.id, newConversation.id, followupEntry.id));
        
        var responseMenuEntry = template.CreateDialogueEntry(template.GetNextDialogueEntryID(newConversation), newConversation.id,
            "Response Menu");
        responseMenuEntry.Title = "RESPONSE MENU";
        responseMenuEntry.MenuText = "[f]()"; // this text will force a response menu to show.
        responseMenuEntry.ActorID = startEntry.ActorID;
        
        followupEntry.outgoingLinks.Add(new Link(newConversation.id, followupEntry.id, newConversation.id, responseMenuEntry.id));
        
        newConversation.dialogueEntries.Add(startEntry);
        newConversation.dialogueEntries.Add(followupEntry);
        newConversation.dialogueEntries.Add(responseMenuEntry);
        
        newConversation.ActorID = startEntry.ActorID;
        
        DialogueManager.masterDatabase.AddConversation( newConversation);
        
        Debug.Log("Conversation generated: " + newConversation.Title);
        DialogueManager.instance.StartConversation( "GENERATED/SmartWatch");
    }
    
}
