using System.Collections.Generic;
using UnityEngine;

namespace Apps.PC
{
    public class FilesAppBackend : ScriptableObject
    {
        private SimFolder _rootFolder;


        private struct SimFolder
        {
            public bool Locked;
            public string Password; // yes we are storing passwords as a string but it's a sim okay
            public List<SimFolder> ContainedFolders;
            public List<SimFile> ContainedFiles;
        }

        private struct SimFile
        {
            public string Path;
        }
    }
}