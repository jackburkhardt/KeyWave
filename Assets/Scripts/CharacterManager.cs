using System;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using UnityEngine;

public class CharacterManager : ScriptableObject
{
    private static List<Character> characters = new List<Character>();
    private string charactersPath;

    private void Awake()
    {
        charactersPath = Application.dataPath + "/GameData/Characters/Characters.json";
        GameEvent.OnGameSave += Save;
        GameEvent.OnGameLoad += Load;
    }

    public static Character Find(string characterName)
    {
        return Characters.FirstOrDefault(character => character.Name == characterName);
    }

    public static List<Character> Characters => characters;
    private void Save() => DataManager.SerializeData(characters, charactersPath);
    private void Load() => characters = DataManager.DeserializeData<List<Character>>(charactersPath);
    
    private void OnDestroy()
    {
        GameEvent.OnGameSave -= Save;
        GameEvent.OnGameLoad -= Load;
    }
    
}