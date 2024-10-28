using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
using DialogueActor = PixelCrushers.DialogueSystem.DialogueActor;

public class CharacterMenuPanel : CustomUIMenuPanel
{
    
    
    
    public new static List<string> CustomFields = new List<string>
    {
        "characterTemplates",
        "responseField"
    };
    
    [SerializeField] private GameObject characterTemplates;
    
    public string responseField = "Character Panel";
    
    public DialogueActor GetActor(string actorName)
    {
        foreach (var actor in characterTemplates.transform.GetComponentsInChildren<DialogueActor>(true))
        {
            if (actor.actor == actorName)
            {
                return actor;
            }
        }

        return null;
    }
    
    protected override void OnContentChanged()
    {
        RefreshLayoutGroups.Refresh(this.gameObject);
        BroadcastMessage("OnContentChange", SendMessageOptions.DontRequireReceiver);
    }
    
    
    
}
