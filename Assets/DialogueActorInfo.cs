using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueActorInfo : MonoBehaviour
{
    public Image actorImage;
    public UITextField actorName;
    
    public void SetActorInfo(DialogueActor actor, string suffix = "")
    {
        actorImage.sprite = actor.spritePortrait;
        actorName.text = actor.name;
        if (!string.IsNullOrEmpty(suffix))
        {
            actorName.text += " " + suffix;
        }
    }
    
    public void SetActorInfo(Actor actor, string suffix = "")
    {
        actorImage.sprite = actor.spritePortrait;
        actorName.text = actor.Name;
        if (!string.IsNullOrEmpty(suffix))
        {
            actorName.text += " " + suffix;
        }
    }
}
