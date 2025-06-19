using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Project.Runtime.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "danieltype", menuName = "Danieltype", order = 0)]
    public class Danieltype : ScriptableObject
    {
        [SerializeField] private string baseLuaVariable;
        public string displayName;
        [TextArea] public string description;
        public int threshold;

        public int Value => DialogueLua.GetVariable($"reputation.{baseLuaVariable}", 0);
    }
}