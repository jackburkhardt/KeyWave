using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;


[CreateAssetMenu(fileName = "New Location", menuName = "Location")]
public class GameLocation : ScriptableObject
{

    public GameManager.Region region;
    public GameManager.Locations location;
    public string description;
    public Sprite pin;
    public Color buttonTint;
    public List<Scene> scenes;
    public List<Objectives> objectives;
    public Vector2 coordinates;
    public bool isUnlocked;


    public enum Objectives
    {
        WatchFilm,
        BeMysteryShopper,
        MeetDouglas,
        MeetPrado,
        WinBowling,
        GetHaircut
    }
    
    
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
