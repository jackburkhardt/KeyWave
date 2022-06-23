
using UnityEngine;


public class Location : MonoBehaviour
{
    public Vector3 Position { get; private set; }
    [SerializeField] private bool _isRealTime;

    private void Awake()
    {
        Position = transform.position;
    }
}
