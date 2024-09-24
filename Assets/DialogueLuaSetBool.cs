using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class DialogueLuaSetBool : MonoBehaviour
{
    public string targetLuaVariable;
    public void SetBool(bool value)
    {
        DialogueLua.SetVariable(targetLuaVariable, value);
    }
}
