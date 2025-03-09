using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    
    [LocationPopup]
    public string location;
    
    private void Awake()
    {
        var locationAsset = DialogueManager.masterDatabase.GetLocation(location);

        var spawnPoint =
            DialogueManager.masterDatabase.GetLocation(DialogueLua.GetLocationField(location, "Spawn Point").asInt);
        
        if (spawnPoint.id != locationAsset.id)
        {
          var locationGameObject = GetComponentsInChildren<RectTransform>(true).First( p => p.gameObject.name == location);
          var spawnPointGameObject = GetComponentsInChildren<RectTransform>(true).First( p => p.gameObject.name == spawnPoint.Name);
          
          spawnPointGameObject.gameObject.SetActive( true);
          locationGameObject.gameObject.SetActive( false);
        }


    }
}
