using System;
using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("OnConversationLine");
        var conversation = subtitle.dialogueEntry.GetConversation().Title;
        if (conversation.Contains("Map") || conversation.EndsWith("Base")) 
        {
            Debug.Log(conversation + " contains Map, Action or Talk");
            if (isActionPanelActive) return;
            ShowPanel();
        }
        else
        {
            Debug.Log(conversation + " does not contain Map, Action or Talk");
            if (!isActionPanelActive) return;
            HidePanel();
        }
    }

    public void ShowPanel()
    {

        isActionPanelActive = true;
        //SendMessage("SetTrigger", "Show", SendMessageOptions.DontRequireReceiver);
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
