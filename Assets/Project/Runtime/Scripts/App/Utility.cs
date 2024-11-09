using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Project.Runtime.Scripts.App
{
    public class Utility
    {
        public static void PipeToEditorAndOpen(string data)
        {
            // pipe this to a text file in Logs and open
            File.WriteAllText($"{Application.dataPath}/DebugSaves/last_load.json", data);
            Process.Start($"{Application.dataPath}/DebugSaves/last_load.json");
        }
    }
}