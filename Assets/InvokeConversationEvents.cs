using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using WaitUntil = UnityEngine.WaitUntil;

public class InvokeConversationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    public void BroadcastLine()
    {
        if (DialogueManager.instance.currentConversationState.subtitle.dialogueEntry.id == 0) return; 
        GameEvent.OnConversationLine();
    //   StartCoroutine(DelayedBroadcast("line"));
    }

    public void BroadcastConversationStart()
    {
      GameEvent.OnConversationStart(GameManager.MostRecentDialogueTrigger);
     //  StartCoroutine(DelayedBroadcast("start"));

    }
    
   
    
    IEnumerator DelayedBroadcast(string type)
    {
        switch (type)

        {
            case "line":
                yield return null;
                GameEvent.OnConversationLine();
                break;
            case "start":
                GameEvent.OnConversationStart(GameManager.MostRecentDialogueTrigger);
                break;
            case "end":
                GameEvent.OnConversationEnd();
                break;
        }

    }
    

    
    public void BroadcastConversationEnd()
    {
       GameEvent.OnConversationEnd();
       // StartCoroutine(DelayedBroadcast("end"));
    }

    public void BroadcastResponseMenu()
    {
        GameEvent.OnConversationResponseMenu();
    }
    
}

