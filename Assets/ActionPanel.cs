using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class ActionPanel : MonoBehaviour
{
    public bool isActionPanelActive = false;

    public static ActionPanel instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    public void OnConversationLine(Subtitle subtitle)
    {
        var conversation = subtitle.dialogueEntry.GetConversation().Title;
        if (conversation.Contains("Map") || conversation.EndsWith("Action/Base") || conversation.EndsWith("Talk/Base")) 
        {
           if (isActionPanelActive) return;
            ShowPanel();
        }
        else
        {
            if (!isActionPanelActive) return;
            HidePanel();
        }
        
        if (Field.FieldExists(subtitle.dialogueEntry.fields, "Track") && Field.LookupBool(subtitle.dialogueEntry.fields, "Track").Equals(true))
        {
       //     if (subtitle.dialogueEntry.)
        }
    }
    
    public void OnQuestStateChange(string questTitle, QuestState questState)
    {
       
    }

    public void ShowPanel()
    {

        isActionPanelActive = true;
       
        GetComponent<Animator>().SetTrigger("Show");
    }

    public void HidePanel()
    {
        isActionPanelActive = false;
        GetComponent<Animator>().SetTrigger("Hide");
    }

}

public class SequencerCommandSetActionPanel : SequencerCommand
{
    private void Awake()
    {
        var value = GetParameterAsBool(0);
        if (value)
        {
           ActionPanel.instance.ShowPanel();
        }
        else
        {
            ActionPanel.instance.HidePanel();
        }
    }
}
