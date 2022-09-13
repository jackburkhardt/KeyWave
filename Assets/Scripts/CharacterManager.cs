using System.Collections.Generic;
using System.Linq;
using Interaction;
using UnityEngine;

public class CharacterManager : ScriptableObject
{
    public static List<Character> SceneCharacters = new List<Character>();

    public static Character Find(string characterName)
    {
        return SceneCharacters.FirstOrDefault(character => character.Name == characterName);
    }
}