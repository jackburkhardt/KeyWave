using System;
using System.Collections.Generic;
using System.IO;
using Apps;
using Apps.PC;
using Apps.Phone;
using Assignments;
using Newtonsoft.Json;
using UnityEngine;

public class DataManager : MonoBehaviour
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
    public static T DeserializeData<T>(string path)
    {
        return File.Exists(path) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) : default(T);
    }

    public static async void SerializeData(object obj, string path)
    {
        StreamWriter sw = new StreamWriter(path, false);
        string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        await sw.WriteAsync(json);
        sw.Close();
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
