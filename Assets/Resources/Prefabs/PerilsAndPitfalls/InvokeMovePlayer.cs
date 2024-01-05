using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InvokeMovePlayer : MonoBehaviour
{
    [SerializeField] private SceneAsset unloadScene;
    private enum Room
    {
        Lobby,
        Beach,
        Airport
    };

    [SerializeReference] private Room room;

    public void MovePlayerTo(InvokeMovePlayer destination)
    {
        GameEvent.OnMove(gameObject.name, destination.room.ToString());
      //  if (unloadScene != null) SceneManager.UnloadSceneAsync(unloadScene.name);
      }
    
    
}
