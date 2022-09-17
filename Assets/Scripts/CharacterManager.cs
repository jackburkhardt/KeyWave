using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

public class CharacterManager : ScriptableObject
{
    private static List<Character> characters = new List<Character>();
    private string charactersPath;

    private void Awake()
    {
        charactersPath = Application.streamingAssetsPath + "/GameData/Characters/Characters.json";
        GameEvent.OnGameSave += Save;
        GameEvent.OnGameLoad += Load;
    }

    /// <summary>
    /// Enables/disables the ability for a character to be delegated assignments.
    /// </summary>
    /// <param name="characterName"></param>
    /// <param name="enabled"></param>
    [YarnCommand("toggle_delegation")]
    public static void ToggleCharacterDelegation(string characterName, bool enabled)
    {
        var c = Find(characterName);
        if (c.Equals(default))
        {
            Debug.LogError($"ToggleCharacterDelegation: Character {characterName} not found.");
            return;
        }
        
        c.CanReceiveDelegations = enabled;
    }

    public static Character Find(string characterName) => characters.FirstOrDefault(character => character.Name == characterName);
    public static List<Character> Characters => characters;
    private void Save() => DataManager.SerializeData(characters, charactersPath);
    private void Load() => characters = DataManager.DeserializeData<List<Character>>(charactersPath);
    
    private void OnDestroy()
    {
        GameEvent.OnGameSave -= Save;
        GameEvent.OnGameLoad -= Load;
    }
    
}