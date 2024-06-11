using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.ScriptableObjects;
using UnityEngine;

namespace Project.Runtime.Scripts.Events.Actions
{
    public class InvokeMovePlayer : MonoBehaviour
    {
        [SerializeReference] private Location _destination;

        public void MovePlayer()
        {
            _destination.MoveHere();
            GameManager.instance.CloseMap(false);
        }

        public void SetDestination(Location location)
        {
            _destination = location;
        }

        public void SetDestination(string locationName)
        {
            SetDestination(Location.FromString(locationName));

        }
    }
}