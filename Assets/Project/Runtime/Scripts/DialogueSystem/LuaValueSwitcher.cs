using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class LuaValueSwitcher : MonoBehaviour
{
    public string targetLuaVariable;
    public List<string> luaValues;

    private int currentIndex = 0;
    public void Next()
    {
        currentIndex++;
        if (currentIndex >= luaValues.Count)
        {
            currentIndex = 0;
        }
        DialogueLua.SetVariable(targetLuaVariable, luaValues[currentIndex]);
    }

    public void Previous()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = luaValues.Count - 1;
        }
        DialogueLua.SetVariable(targetLuaVariable, luaValues[currentIndex]);
    }
    
    
}
