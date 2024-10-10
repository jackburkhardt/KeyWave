using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class WindowsButtonPanel : MonoBehaviour
{
    // Start is called before the first frame update

    private void OnEnable()
    {
        GameManager.OnMapOpen += OnMapOpen;
    }
    
    private void OnDisable()
    {
        GameManager.OnMapOpen -= OnMapOpen;
    }
    

    public void OnConversationStart()
    {
        if (DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.GetConversation().Title != "Intro")
        GetComponent<Animator>().SetTrigger("Show");
    }
    
    public void OnMapOpen()
    {
        GetComponent<Animator>().SetTrigger("Hide");
    }
    
    
}
