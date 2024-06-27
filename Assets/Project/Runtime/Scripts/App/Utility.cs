using System.IO;

namespace Project.Runtime.Scripts.App
{
    public class Utility
    {
        public static void PipeToEditorAndOpen(string data)
        {
            // pipe this to a text file in Logs and open
            File.WriteAllText($"{UnityEngine.Application.dataPath}/DebugSaves/last_load.json", data);
            System.Diagnostics.Process.Start($"{UnityEngine.Application.dataPath}/DebugSaves/last_load.json");
        }
    }
}