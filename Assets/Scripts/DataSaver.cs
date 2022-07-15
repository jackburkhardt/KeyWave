using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Apps;
using Assignments;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    public static EmailManager EmailManager;
    public static CallManager CallManager;
    public static TextManager TextManager;
    
    private void Awake()
    {
        EmailManager = ScriptableObject.CreateInstance<EmailManager>();
        CallManager = ScriptableObject.CreateInstance<CallManager>();
        TextManager = ScriptableObject.CreateInstance<TextManager>();
        
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
