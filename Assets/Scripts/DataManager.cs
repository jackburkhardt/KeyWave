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
    public static AssignmentManager AssignmentManager;
    public static CharacterManager CharacterManager;
    
    public static SaveData SaveData;
    private string savePath;
    
    private void Awake()
    {
        savePath = Application.streamingAssetsPath + "/GameData/save.json";
        SaveData = DeserializeData<SaveData>(savePath);
        RealtimeManager.Chapter = SaveData.CurrentChapter;
        
        EmailBackend = ScriptableObject.CreateInstance<EmailBackend>();
        CallBackend = ScriptableObject.CreateInstance<CallBackend>();
        TextBackend = ScriptableObject.CreateInstance<TextBackend>();
        FilesAppBackend = ScriptableObject.CreateInstance<FilesAppBackend>();
        SearchBackend = ScriptableObject.CreateInstance<SearchBackend>();
        AssignmentManager = gameObject.AddComponent<AssignmentManager>();
        CharacterManager = ScriptableObject.CreateInstance<CharacterManager>();
    }

    private void Start()
    {
        GameEvent.LoadGame();
    }

    public static T DeserializeData<T>(string path)
    {
        if (File.Exists(path))
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }
        else
        {
            Debug.LogError($"DataManager: The path \"{path}\" was not able to be loaded.");
            return default(T);
        }
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
        SerializeData(SaveData, savePath);
    }

}

public struct SaveData
{
    public bool IsPCUnlocked;
    public int CurrentChapter;
    public TimeSpan CurrentTime;
    
    SaveData Default(){
        return new SaveData{
            IsPCUnlocked = false,
            CurrentChapter = 1,
            CurrentTime = new TimeSpan(9, 0, 0)
        };
    }
}
