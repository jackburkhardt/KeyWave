using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Assignments;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
        
    public void SaveState(GameState state)
    {
        string destination = Application.persistentDataPath + "/assignments.dat";

        var file = File.Exists(destination) ? File.OpenWrite(destination) : File.Create(destination);
        BinaryFormatter bf = new BinaryFormatter();
        
        bf.Serialize(file, state);
        file.Close();
    }

    public GameState LoadState()
    {
        string destination = Application.persistentDataPath + "/assignments.dat";
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        try
        {
            file = File.OpenRead(destination);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }

        return (GameState)bf.Deserialize(file);
    }
}

[Serializable]
public struct GameState
{
    public int Chapter;
    public List<Assignment> Assignments;
    public Location LastLocation;
}
