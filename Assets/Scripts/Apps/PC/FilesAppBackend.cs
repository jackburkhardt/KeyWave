using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Apps.PC
{
    public class FilesAppBackend : ScriptableObject
    {
        private SimFolder _rootFolder;
        private string _filesLocation;
        
        private void Awake()
        {
            _filesLocation = Application.dataPath + "/GameData/SimFiles/tree.json";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
            
            GenerateOneShot();
        }

        private void GenerateOneShot()
        {
            SimFolder newRoot = new SimFolder("root"), folder1 = new SimFolder("folder1"), folder2 = new SimFolder("folder2"), folder3 = new SimFolder("folder3"), folder4 = new SimFolder("folder4"), folder5 = new SimFolder("folder5");
            folder4.Locked = true;
            folder4.Password = "test";
            folder5.Locked = true;
            folder5.Password = "superSecurePassword";
            folder1.ContainedFiles.Add(new SimFile("Finances.doc","document.png"));
            folder1.ContainedFiles.Add(new SimFile("HiringList.pdf", "otherDoc.png"));
            folder2.ContainedFiles.Add(new SimFile("Screenshot.png", "specialimage.png"));
            newRoot.ContainedFolders = new List<SimFolder> {folder1, folder2, folder3, folder4, folder5};
            _rootFolder = newRoot;
            Save();
        }

        private void Save()
        {
            StreamWriter sw = new StreamWriter(_filesLocation, false);
            string json = JsonConvert.SerializeObject(_rootFolder, Formatting.Indented);
            sw.Write(json);
            sw.Close();
        }

        private void Load()
        {
            if (File.Exists(_filesLocation))
            {
                _rootFolder = JsonConvert.DeserializeObject<SimFolder>(File.ReadAllText(_filesLocation));
            }
        }

        [Serializable]
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

        [Serializable]
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