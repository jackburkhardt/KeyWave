using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.ScriptableObjects;
using UnityEditor;

namespace Project.Editor.Scripts.Tools
{
    public class SkipTime
    {
        [MenuItem("Tools/Perils and Pitfalls/Game/SkipTime/1 Hour")]
        private static void Hotel()
        {
           GameEvent.OnWait(Clock.HoursToSeconds(1));
        }

        
    }
}