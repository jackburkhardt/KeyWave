using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KeyWave.Runtime.Scripts.AssetLoading
{
    public class AddressableLoader : ScriptableObject
    {
        private List<object> _activeObjects = new();
        private List<object> _loadQueue = new();

        public void RequestLoad<T>(AssetReference reference, Action<T> callback)
        {
            if (_activeObjects.Contains(reference))
            {
                callback?.Invoke((T) _activeObjects.Find(x => x.ToString() == reference.ToString()));
            }
            else
            {
                _loadQueue.Add(reference);
                reference.LoadAssetAsync<T>().Completed += handle =>
                {
                    _activeObjects.Add(handle.Result);
                    _loadQueue.Remove(reference);
                    callback?.Invoke(handle.Result);
                };
            }
            
        }
        
        public void RequestLoad<T>(string address, Action<T> callback)
        {
            if (_activeObjects.Contains(address))
            {
                callback?.Invoke((T) _activeObjects.Find(x => x.ToString() == address));
            }
            else
            {
                _loadQueue.Add(address);
                Addressables.LoadAssetAsync<T>(address).Completed += handle =>
                {
                    _activeObjects.Add(handle.Result);
                    _loadQueue.Remove(address);
                    callback?.Invoke(handle.Result);
                };
            }
        }
        
        public void Release(object obj) => Addressables.Release(obj);
        
        public bool IsQueueEmpty() => _loadQueue.Count == 0;
        
        public void ClearQueue() => _loadQueue.Clear();

        private void OnDestroy()
        {
            foreach (var obj in _activeObjects)
            {
                Addressables.Release(obj);
            }

            Resources.UnloadUnusedAssets();
        }
    }
}