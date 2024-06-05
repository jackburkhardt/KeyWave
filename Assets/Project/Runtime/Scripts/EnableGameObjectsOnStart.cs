using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
