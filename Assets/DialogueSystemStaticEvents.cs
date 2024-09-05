using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class DialogueSystemStaticEvents : MonoBehaviour
{
    public static Action<Subtitle> OnConversationLineEvent;
    // Start is called before the first frame update
    public void OnConversationLine(Subtitle subtitle)
    {
        OnConversationLineEvent?.Invoke(subtitle);
    }
}
