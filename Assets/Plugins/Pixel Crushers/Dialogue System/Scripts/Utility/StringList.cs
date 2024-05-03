using UnityEngine;

namespace Plugins.Pixel_Crushers.Dialogue_System.Scripts.Utility
{
    [CreateAssetMenu(fileName = "String List", menuName = "String List Asset", order = 0)]
    public class StringList : ScriptableObject
    {
        public string[] strings;
    }
}