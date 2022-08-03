using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Apps.PC
{
    public class SearchBackend : ScriptableObject
    {
        private string _searchFilePath;
        private static Dictionary<string, Texture2D> _searchMap = new Dictionary<string, Texture2D>(); // TODO: check sprite vs raw

        private void Awake()
        {
            _searchFilePath = Application.streamingAssetsPath + "/GameData/Search/entries.json";

            GameEvent.OnGameLoad += Load;
        }

        // returning null this time so that frontend can handle however it chooses
        public static Texture2D Search(string key) =>
            _searchMap.TryGetValue(key, out var tex) ? tex : null;
        
        private void Load()
        {
            List<SearchItem> textEntries = DataManager.DeserializeData<List<SearchItem>>(_searchFilePath);

            foreach (var textEntry in textEntries)
            {
                var image = LoadImage(textEntry.ImageName);
                foreach (var searchEntry in textEntry.SearchEntries)
                {
                    _searchMap.Add(searchEntry, image);
                }
            }
        }

        private Texture2D LoadImage(string fileName)
        {
            if (!File.Exists(_searchFilePath + fileName)) throw new FileNotFoundException(); // TODO: load oops?

            var fileData = File.ReadAllBytes(_searchFilePath + fileName);
            var tex = new Texture2D(2, 2); // Create new "empty" texture
            tex.LoadImage(fileData);
            return tex;
        }

        private struct SearchItem
        {
            public List<string> SearchEntries;
            public string ImageName;

            public SearchItem(List<string> searchEntries, string imageName)
            {
                SearchEntries = searchEntries;
                ImageName = imageName;
            }
        }
    }
}