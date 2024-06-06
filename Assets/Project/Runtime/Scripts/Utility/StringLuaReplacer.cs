using System;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using HelpBoxMessageType = PixelCrushers.HelpBoxMessageType;

public class StringLuaReplacer : PlayerEventHandler
{
    
    
    
    public UITextField text;

    [SerializeField] private bool overrideText;
    [ShowIf("overrideText")]
    [SerializeField] string startString;
    
    [SerializeField] private bool onlyVariables;
    
    [ShowIf("onlyVariables")]
    [HelpBox("This script replaces the following substrings with Lua variables used in the Dialogue System by indexing Variable[\"{i}\"]", HelpBoxMessageType.Info)]
       
    [Label("{0}")]
    public string _0_asVariable;

    [HideIf("onlyVariables")]
    [HelpBox("This script replaces the following substrings with a result of a 'Lua.Run({i})' command.",
        HelpBoxMessageType.Info)]
    [Label("{0}")]
    public string _0_asLua;

    private string _0 => onlyVariables ? _0_asVariable : _0_asLua;
    
    [Label("{1}")]
    public string _1;
    [Label("{2}")]
    public string _2;

    private string _text;
    

    void OnEnable()
    {
        if (overrideText)
        {
            
            text.text = startString;
        }
        
        _text = text.text;
        
      // s  Language.Lua.Assignment.

      //  var b = new Language.Lua.Assignment();
        
       // DialogueManager.masterDatabase.variables.ForEach(variable => D
        //   DialogueManager.masterDatabase.variables.ForEach(variable => Language.Lua.Assignment.Add(DialogueLua.StringToTableIndex(variable.Name)));
        // Language.Lua.Assignment.VariableChanged += OnVariableChanged;
    }

    protected override void OnPlayerEvent(PlayerEvent playerEvent)
    {
        
    }

    private void OnValidate()
    {
        TextUpdate();
    }

    private void TextUpdate()
    {
        
        if (onlyVariables) {
            text.text = _text.Replace("{0}", DialogueLua.GetVariable(_0).asString);
            text.text = _text.Replace("{1}", DialogueLua.GetVariable(_1).asString);
            text.text = _text.Replace("{2}", DialogueLua.GetVariable(_2).asString);
        }

        else
        {
            text.text = _text.Replace("{0}", Lua.Run("return " + _0).asString);
          //  Debug.Log(Lua.Run("return " + _0).asString);
            text.text = _text.Replace("{1}", Lua.Run("return " + _1).asString);
            text.text = _text.Replace("{2}", Lua.Run("return " + _2).asString);
        }
        
    }
}
