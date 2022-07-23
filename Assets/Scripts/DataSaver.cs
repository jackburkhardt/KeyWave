using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Apps;
using Apps.PC;
using Apps.Phone;
using Assignments;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    public static EmailBackend EmailBackend;
    public static CallBackend CallBackend;
    public static TextBackend TextBackend;
    public static FilesAppBackend FilesAppBackend;
    public static SearchBackend SearchBackend;
    
    private void Awake()
    {
        EmailBackend = ScriptableObject.CreateInstance<EmailBackend>();
        CallBackend = ScriptableObject.CreateInstance<CallBackend>();
        TextBackend = ScriptableObject.CreateInstance<TextBackend>();
        FilesAppBackend = ScriptableObject.CreateInstance<FilesAppBackend>();
        SearchBackend = ScriptableObject.CreateInstance<SearchBackend>();
        
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
