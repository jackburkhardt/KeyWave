using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;


[CreateAssetMenu(fileName = "New Location", menuName = "Location")]
public class Location : ScriptableObject
{
    public new string name => area.ToString();

    public static Location PlayerLocation
    {
        get
        {
            return FromString(GameManager.gameState.player_location);
        }
    }
    
    public enum Area {
        Hotel,
        Beach,
        Airport,
        Mall,
        Park,
        Island,
        Café,
        Store
    }

    public Area area;
    public string description;
    public Sprite pin;
    public Color buttonTint;
    public List<Scene> scenes;
    public List<Objective> objectives;
    public Vector2 coordinates;
    public bool isUnlocked;


    public enum Objective
    {
        WatchFilm,
        BeMysteryShopper,
        MeetDouglas,
        MeetPrado,
        WinBowling,
        GetHaircut
    }
    
    public static Location FromString(string location)
    {
        // get all locations and return the one with the same name
        var locations = GameManager.instance.locations;
        foreach (Location loc in locations) if (loc.area.ToString() == location) return loc;
        return null;
    }
    
    public static Location FromArea(Area area) =>  FromString(area.ToString());

    
    public static List<Objective> Objectives(string location)
    {
        Location loc = FromString(location);
        return loc.objectives;
    }
    
    public static List<Objective> Objectives(Area area) => Objectives(area.ToString());
    
    
    public static int GetDistanceFromPlayer(string location)
    {
        var playerCoordinates = PlayerLocation.coordinates;
        var targetLocation = FromString(location);
        var targetCoordinates = targetLocation.coordinates;
        return (int)Vector2.Distance(playerCoordinates, targetCoordinates);
    }

    private int Distance
    {
        get
        {
            var playerCoordinates = PlayerLocation.coordinates;
            return (int)Vector2.Distance(playerCoordinates, coordinates);
        }
    }
    
    public int TravelTime => Distance * Clock.TimeScales.GlobalTimeScale;

    public static int GetTravelTime(string location) => FromString(location).TravelTime;
    
    public static int GetTravelTime(Area area) => FromArea(area).TravelTime;
    
}
