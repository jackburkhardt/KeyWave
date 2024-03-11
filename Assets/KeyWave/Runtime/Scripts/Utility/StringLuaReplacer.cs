using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using PixelCrushers;
using UnityEngine;
using UnityEngine.UIElements;
using PixelCrushers.DialogueSystem;
using HelpBoxMessageType = PixelCrushers.HelpBoxMessageType;
using PlayerEvent = PlayerEvents.PlayerEvent;

public class StringLuaReplacer : PlayerEventHandler
{
    
    public UITextField text;
    [HelpBox("This script replaces the following substrings with Lua variables used in the Dialogue System.", HelpBoxMessageType.Info)]
    [Label("{0}")]
    public string _0;
    [Label("{1}")]
    public string _1;
    [Label("{2}")]
    public string _2;

    void OnEnable()
    {
        UpdateText();
    }

    protected override void OnPlayerEvent(PlayerEvent playerEvent)
    {
        switch (playerEvent.Type)
        {
            case "conversation_script":
                UpdateText();
                break;
        }
    }

    void UpdateText()
    {
        text.text = text.text.Replace("{0}", DialogueLua.GetVariable(_0).asString);
        text.text = text.text.Replace("{1}", DialogueLua.GetVariable(_1).asString);
        text.text = text.text.Replace("{2}", DialogueLua.GetVariable(_2).asString);
    }
}
