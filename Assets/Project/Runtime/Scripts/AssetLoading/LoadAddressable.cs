using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Project.Runtime.Scripts.AssetLoading
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
                var operation = reference.LoadAssetAsync<T>();
                operation.Completed += handle =>
                {
                    _loadQueue.Remove(reference);
                    if (operation.Status == AsyncOperationStatus.Failed)
                    {
                        Debug.LogError($"AddressableLoader: failed to load asset at address {reference}!");
                        Debug.LogError(operation.OperationException);
                        return;
                    }
                    _activeObjects[reference.AssetGUID] = handle.Result;
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
                    _loadQueue.Remove(address);
                    if (operation.Status == AsyncOperationStatus.Failed)
                    {
                        Debug.LogError($"AddressableLoader: failed to load asset at address {address}!");
                        Debug.LogError(operation.OperationException);
                        return;
                    }
                    _activeObjects[address] = handle.Result;
                    callback?.Invoke(handle.Result);
                };
            }
        }

        public static void Release(string path)
        { 
            if (_activeObjects.Remove(path, out var obj))
            {
                Addressables.Release(obj);
            }
        }

        public static bool IsQueueEmpty() => _loadQueue.Count == 0;

        public static void ClearQueue() => _loadQueue.Clear();
    }
}