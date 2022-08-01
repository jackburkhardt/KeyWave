using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactLayerMask;
    public static bool _controlsEnabled = true;
    private GameObject lastHitGO;
    private static int time;
    private static int chapter;

    private void Awake()
    {
        GameEvent.OnInteractionStart += obj => _controlsEnabled = false;
        GameEvent.OnInteractionEnd += obj => _controlsEnabled = true;
    }

    private void Update()
    {
        if (!_controlsEnabled) return;

    }

    public static int Time => time;
    public static int Chapter => chapter;
    public static bool ControlsEnabled => _controlsEnabled;

}