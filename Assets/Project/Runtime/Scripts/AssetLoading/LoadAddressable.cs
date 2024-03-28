using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace KeyWave.Runtime.Scripts.AssetLoading
{
    public static class AddressableLoader
    {
        private static Dictionary<string, object> _activeObjects = new();
        private static List<object> _loadQueue = new();
        private static bool _isInitialized = false;
        
        private static void Initialize()
        {
            _isInitialized = true;
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                Debug.Log("AddressableLoader: unloading unused assets");
                Resources.UnloadUnusedAssets();
            };
        }

        /// <summary>
        ///  Loads an asset of type T or returns the asset if it is already loaded.
        ///  Assets are loaded asynchronously, and once ready the callback is invoked.
        /// </summary>
        public static void RequestLoad<T>(AssetReference reference, Action<T> callback)
        {
            if (!_isInitialized) Initialize();
            
            if (_activeObjects.TryGetValue(reference.AssetGUID, out var obj))
            {
                callback?.Invoke((T) obj);
            }
            else
            {
                _loadQueue.Add(reference);
                reference.LoadAssetAsync<T>().Completed += handle =>
                {
                    _activeObjects[reference.AssetGUID] = handle.Result;
                    _loadQueue.Remove(reference);
                    callback?.Invoke(handle.Result);
                };
            }
            
        }
        
        /// <summary>
        ///  Loads an asset of type T or returns the asset if it is already loaded.
        ///  Assets are loaded asynchronously, and once ready the callback is invoked.
        /// </summary>
        public static void RequestLoad<T>(string address, Action<T> callback)
        {
            if (!_isInitialized) Initialize();
            
            if (_activeObjects.TryGetValue(address, out var obj))
            {
                callback?.Invoke((T) obj);
            }
            else
            {
                _loadQueue.Add(address);
                var operation = Addressables.LoadAssetAsync<T>(address);
                operation.Completed += handle =>
                {
                    _activeObjects[address] = handle.Result;
                    _loadQueue.Remove(address);
                    callback?.Invoke(handle.Result);
                };
            }
        }
        
        public static void Release(object obj) => Addressables.Release(obj);
        
        public static bool IsQueueEmpty() => _loadQueue.Count == 0;
        
        public static void ClearQueue() => _loadQueue.Clear();
    }
}