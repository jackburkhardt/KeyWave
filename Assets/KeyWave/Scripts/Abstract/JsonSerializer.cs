using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Recorder;
using UnityEngine;

public class JsonSerializer : PlayerEventHandler
{
    
    protected string FileName;
    protected string Path;

  
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        throw new NotImplementedException();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvent.OnSerializePlayerEvent += OnSerializePlayerEvent;
        SetPath(FileName);
        OnLoad();
    }

    protected override void OnPlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
        
    }

    protected override void OnDisable()
    {
        GameEvent.OnSerializePlayerEvent -= OnSerializePlayerEvent;
    }
    
    protected virtual void OnLoad()
    {
        throw new NotImplementedException();
    }
    
    protected virtual void OnSerializePlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
        throw new NotImplementedException();
    }

    protected void Deserialize<T>(ref T rootObject)
    {
        rootObject = DataManager.DeserializeData<T>(Path);
    }
    
    protected void Serialize<T>(T rootObject)
    {
        DataManager.SerializeData(rootObject, Path);
    }

    protected void SetPath(string file)
    {
        Path = $"{Application.dataPath}/Resources/GameData/PerilsAndPitfalls/{file}";
    }
}
