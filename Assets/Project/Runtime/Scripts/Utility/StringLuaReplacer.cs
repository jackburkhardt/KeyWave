using System;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using HelpBoxMessageType = PixelCrushers.HelpBoxMessageType;

public class StringLuaReplacer : MonoBehaviour
{
    
    
    
    public UITextField text;

    [SerializeField] private bool overrideText = true;
    [ShowIf("overrideText")]
    [SerializeField] string startString;
    
    [SerializeField] private bool onlyVariables;
    
    [ShowIf("onlyVariables")]
    [HelpBox("This script replaces the following substrings with Lua variables used in the Dialogue System by indexing Variable[\"{i}\"]", HelpBoxMessageType.Info)]
       
    [Label("{0}")]
    public string _0_asVariable;

    [HideIf("onlyVariables")]
    [HelpBox("This script replaces the following substrings with a result of a 'Lua.Run(return {i})' command.",
        HelpBoxMessageType.Info)]
    [Label("{0}")]
    public string _0_asLua;

    private string _0 => onlyVariables ? _0_asVariable : _0_asLua;
    
    [Label("{1}")]
    public string _1;
    [Label("{2}")]
    public string _2;

    private string _text;
    

    private void OnEnable()
    {
        if (overrideText)
        {

            //text.text = startString;
            _text = startString;

        }

        else _text = text.text;
      
      
    }
    
    private void OnDisable()
    {
       
    }

    private bool _isRunning;

    private void Update()
    {
        UpdateText();
        return;
      //  if (_isRunning) 
        //else if (FindObjectOfType(typeof(CustomLuaFunctions)) == null) return;
       // _isRunning = true;

    }

   

    private void UpdateText()
    {
        
        //return;
        if (onlyVariables) {
            if (_0 != string.Empty) text.text = _text.Replace("{0}", DialogueLua.GetVariable(_0).asString);
            if (_1 != string.Empty) text.text = text.text.Replace("{1}", DialogueLua.GetVariable(_1).asString);
            if (_2 != string.Empty) text.text = text.text.Replace("{2}", DialogueLua.GetVariable(_2).asString);
        }

        else
        {
          
            if (_0 != string.Empty) text.text = _text.Replace("{0}", Lua.Run("return " + _0).asString);
            if (_1 != string.Empty) text.text = text.text.Replace("{1}", Lua.Run("return " + _1).asString);
            if (_2 != string.Empty) text.text = text.text.Replace("{2}", Lua.Run("return " + _2).asString);
        }
    }
}
