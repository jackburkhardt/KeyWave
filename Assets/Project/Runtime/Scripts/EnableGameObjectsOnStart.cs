using System.Collections.Generic;
using UnityEngine;

namespace Project.Runtime.Scripts
{
    public class EnableGameObjectsOnStart : MonoBehaviour
    {
        [SerializeReference] private List<GameObject> _gameObjectsToEnable;

        // Start is called before the first frame update
        void Start()
        {
            foreach (var gameObject in _gameObjectsToEnable)
            {
                gameObject!.SetActive(true);
            }
        }
    }
}