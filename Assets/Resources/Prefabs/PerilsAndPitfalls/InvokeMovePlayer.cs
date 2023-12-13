using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeMovePlayer : MonoBehaviour
{
    private enum Room
    {
        Lobby,
        Beach,
        Airport
    };

    [SerializeReference] private Room room;

    public void MovePlayerTo(InvokeMovePlayer destination)
    {
        GameEvent.PlayerEvent("player_enter", destination.room.ToString());
      }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
