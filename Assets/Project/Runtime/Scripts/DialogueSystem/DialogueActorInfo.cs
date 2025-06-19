using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class DialogueActorInfo : MonoBehaviour
{
    public Image actorImage;
    public UITextField actorName;

    public void SetActionInfo(string displayName)
    {
        actorName.text = displayName;
    }
    
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
