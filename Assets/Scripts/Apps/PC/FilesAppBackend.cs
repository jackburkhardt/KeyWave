using System.Collections.Generic;
using UnityEngine;

namespace Apps.PC
{
    public class FilesAppBackend : ScriptableObject
    {
        private SimFolder _rootFolder;
        private string _filesLocation;
        
        private void Awake()
        {
            _filesLocation = Application.streamingAssetsPath + "/GameData/SimFiles/tree.json";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
            
        }

        private void Save() => DataManager.SerializeData(_rootFolder, _filesLocation);
        private void Load() => _rootFolder = DataManager.DeserializeData<SimFolder>(_filesLocation);
        
        private struct SimFolder
        {
            public string Name;
            public bool Locked;
            public string Password; // yes we are storing passwords as a string but it's a sim okay
            public List<SimFolder> ContainedFolders;
            public List<SimFile> ContainedFiles;

            public SimFolder(string name, bool locked = false, string password = "")
            {
                Name = name;
                Locked = locked;
                Password = password;
                ContainedFolders = new List<SimFolder>();
                ContainedFiles = new List<SimFile>();
            }
        }
        
        private struct SimFile
        {
            public string Name;
            public string Path;

            public SimFile(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }

        private void OnDestroy()
        {
            GameEvent.OnGameSave -= Save;
            GameEvent.OnGameLoad -= Load;
        }
    }
}