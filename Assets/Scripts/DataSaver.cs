using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Apps;
using Apps.Phone;
using Assignments;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    public static EmailBackend EmailBackend;
    public static CallBackend CallBackend;
    public static TextBackend TextBackend;
    
    private void Awake()
    {
        EmailBackend = ScriptableObject.CreateInstance<EmailBackend>();
        CallBackend = ScriptableObject.CreateInstance<CallBackend>();
        TextBackend = ScriptableObject.CreateInstance<TextBackend>();
        
        GameEvent.LoadGame();
    }

    private void OnDestroy()
    {
        GameEvent.SaveGame();
    }
   
}

[Serializable]
public struct GameState
{
    public int Chapter;
    public List<Assignment> Assignments;
    public Location LastLocation;
}
