using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class NotificationBadge : MonoBehaviour
{
    public enum Type
    {
        Talk,
        Action
    }
    
    private static List<DialogueEntry> trackedActionNodes = new List<DialogueEntry>();
    private static List<DialogueEntry> trackedTalkNodes = new List<DialogueEntry>();
    public Image badge;
    
    public Type type;
    
    // Start is called before the first frame update
    public void OnNewOptionAvailable(DialogueEntry dialogueEntry)
    {
        var entryType = dialogueEntry.Title.Split("/")[1];
        switch (type, entryType)
        {
            case (Type.Action, "Action"):
                trackedActionNodes.Add(dialogueEntry);
                badge.enabled = true;
                break;
            case (Type.Talk, "Talk"):
                trackedTalkNodes.Add(dialogueEntry);
                badge.enabled = true;
                break;
            default:
                break;
        }
    }
    
    public void OnNewOptionSelected(DialogueEntry dialogueEntry)
    {
        var entryType = dialogueEntry.Title.Split("/")[1];
        switch (type, entryType)
        {
            case (Type.Action, "Action"):
                trackedActionNodes.Remove(dialogueEntry);
                if (trackedActionNodes.Count == 0) badge.enabled = false;
                break;
            case (Type.Talk, "Talk"):
                trackedTalkNodes.Remove(dialogueEntry);
                if (trackedTalkNodes.Count == 0) badge.enabled = false;
                break;
            default:
                break;
        }
    }

    private void OnEnable()
    {
        switch (type)
        {
            case Type.Action:
                badge.enabled = trackedActionNodes.Count != 0;
                break;
            case Type.Talk:
                badge.enabled = trackedTalkNodes.Count != 0;
                break;
        }
    }

    public void OnConversationLine()
    {
        
    }
}
