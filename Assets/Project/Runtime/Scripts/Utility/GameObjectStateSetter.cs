using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectStateSetter : MonoBehaviour
{
    public enum Trigger
    {
        OnStart,
        OnEnable,
        OnDisable,
        OnAwake
    }
    
    
    [Serializable]
    public struct GameObjectState
    {
        public GameObject gameObject;
        public Trigger trigger;
        public bool state;
    }
    
    public List<GameObjectState> gameObjects = new List<GameObjectState>();

    void AddNew()
    {
        gameObjects.Add(new GameObjectState());
    }
    
    void Remove(int index)
    {
        gameObjects.RemoveAt(index);
    }
   
    // Start is called before the first frame update
    void Start()
    {
        foreach (var obj in gameObjects)
        {
            switch (obj.trigger)
            {
                case Trigger.OnStart:
                    if (obj.gameObject != null) obj.gameObject.SetActive(obj.state);
                    break;
            }
        }
    }
    
    private void OnEnable()
    {
        foreach (var obj in gameObjects)
        {
            switch (obj.trigger)
            {
                case Trigger.OnEnable:
                    if (obj.gameObject != null) obj.gameObject.SetActive(obj.state);
                    break;
            }
        }
    }

    private void Awake()
    {
        foreach (var obj in gameObjects)
        {
            switch (obj.trigger)
            {
                case Trigger.OnAwake:
                    if (obj.gameObject != null) obj.gameObject.SetActive(obj.state);
                    break;
            }
        }
    }
    
    private void OnDisable()
    {
        foreach (var obj in gameObjects)
        {
            switch (obj.trigger)
            {
                case Trigger.OnDisable:
                    if (obj.gameObject != null) obj.gameObject.SetActive(obj.state);
                    break;
            }
        }
    }

  
}
