using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSwitcher : MonoBehaviour
{
  
    public void ShowObject(GameObject obj)
    {
        foreach (var transform in GetComponentsInChildren<Transform>())
        {
            if (transform != this.transform) transform.gameObject.SetActive(false);
        }
        
        obj.SetActive(true);
    }
    
    public void HideAll()
    {
        foreach (var transform in GetComponentsInChildren<Transform>())
        {
            if (transform != this.transform) transform.gameObject.SetActive(false);
        }
    }
}
