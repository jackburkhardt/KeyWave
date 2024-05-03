using Project.Runtime.Scripts.App;
using UnityEngine;

public class InvokeMovePlayer : PlayerEventHandler
{
    [SerializeReference] private Location _destination;
    
    protected override void OnPlayerEvent(PlayerEvent playerEvent)
    {
        if (playerEvent.EventType != "move" || ((Location)playerEvent.Data).Name != _destination.Name) return;
        GameManager.instance.TravelTo(_destination.Name);
    }
    
    public void MovePlayer()
    {
        GameEvent.OnMove("InvokeMovePlayer", _destination, _destination.TravelTime);
        App.Instance.UnloadScene("Map");
    }

    public void SetDestination(Location location)
    {
        _destination = location;
    }
    
}
