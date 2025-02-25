using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.Wrappers;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class SmartWatchPanel : UIPanel
{
   
    public string focusAnimationTrigger;
    public string unfocusAnimationTrigger;


    protected override void OnEnable()
    {
        base.OnEnable();

        var smartWatchApps = FindObjectsByType<SmartWatchApp>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var smartWatchApp in smartWatchApps)
        {
            SmartWatch.OnAppOpen += smartWatchApp.OnAppOpen;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        var smartWatchApps = FindObjectsByType<SmartWatchApp>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var smartWatchApp in smartWatchApps)
        {
            SmartWatch.OnAppOpen -= smartWatchApp.OnAppOpen;
        }
    }
    
    protected override void OnVisible()
    {
        base.OnVisible();
        
        
        if (DialogueManager.instance.IsConversationActive)
        {
            var currentSubtitle = DialogueManager.instance.currentConversationState.subtitle;
            var conversation =
                DialogueManager.instance.masterDatabase.GetConversation(currentSubtitle.dialogueEntry.conversationID);

            if (conversation.Name == "SmartWatch/Home") return;
            
            DialogueManager.instance.StopConversation();
            
            if (conversation.Name != "SmartWatch/Home")
            {
                DialogueManager.instance.StartConversation("SmartWatch/Home");
            }

            else
            {
                PopFromPanelStack();
                CheckFocus();
                DialogueManager.instance.StopConversation();
            }
        }
          
        else if (!DialogueManager.instance.IsConversationActive)
        {
            DialogueManager.instance.StartConversation("SmartWatch/Home");
        }
    }

    public void OnConversationStart()
    {
        var currentApp = SmartWatch.GetCurrentApp();
        var conversation = DialogueManager.instance.activeConversation.conversationTitle;
        var conversationTitle = DialogueManager.instance.masterDatabase.GetConversation(conversation).Title;

        var appConversations = SmartWatch.GetAllApps().ConvertAll(app => app.dialogueSystemConversationTitle);
        
        Debug.Log("conversation title: " + conversationTitle);

        if (!string.IsNullOrEmpty(conversationTitle)) return;

        if (appConversations.Contains(conversationTitle) || conversationTitle == "Base")
            GetComponent<Animator>().SetTrigger("Focus");

        else
        {

            switch (currentApp.name)
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
                  
                    Debug.Log( "unfocusing for conversation " + conversationTitle);
                    GetComponent<Animator>().SetTrigger(unfocusAnimationTrigger);
                    break;
            }
        }
    }

    public void OnLinkedConversationStart()
    {
        OnConversationStart();
    }

   

    public void OnConversationEnd()
    {
        var currentApp = SmartWatch.GetCurrentApp();
        if (currentApp.name == "Travel")
        {
            GetComponent<Animator>().SetTrigger(unfocusAnimationTrigger);
        }
    }
    
    
    
}
