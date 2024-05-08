using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;
using UnityEditor;
using NaughtyAttributes;


[CreateAssetMenu(fileName = "New Location", menuName = "Location")]
public class Location : ScriptableObject
{
    public string Name => area.ToString();

    public Color responseMenuButtonColor;
    
    public enum Area {
        Hotel,
        Beach,
        Neighborhood,
        Mall,
        Park,
        Island,
        Café,
        Store
    }

    public Area area;
    public string description;
    public List<Item> objectives;
    public bool unlocked;
    public Vector2 coordinates;
 
    // formula for time to distance: minutes * 4 (5 mins * 4 = 20)
    public const int DistanceToNearestCafe = 20;

    public void MoveHere()
    {
        GameEvent.OnMove(this.Name, this, this.TravelTime);
        GameManager.instance.TravelTo(this);
    }

    private int Distance
    {
        get
        {
            var playerCoordinates = PlayerLocation.coordinates;
            return (int)Vector2.Distance(playerCoordinates, coordinates);
        }
    }
    
    public int TravelTime {
        get
        {
            if (area == Area.Café || GameManager.gameState.player_location == "Café")
            {
                return DistanceToNearestCafe * Clock.TimeScales.GlobalTimeScale;
            } 
            return Distance * Clock.TimeScales.GlobalTimeScale;
        }
    }

    #region Static Methods
    
    public static int GetTravelTime(string location) => FromString(location).TravelTime;
    
    public static int GetTravelTime(Area area) => FromArea(area).TravelTime;
    
    public static List<Item> Objectives(string location)
    {
        Location loc = FromString(location);
        return loc.objectives;
    }
    
    public static List<Item> Objectives(Area area) => Objectives(area.ToString());
    
    public static int GetDistanceFromPlayer(string location)
    {
        var playerCoordinates = PlayerLocation.coordinates;
        var targetLocation = FromString(location);
        var targetCoordinates = targetLocation.coordinates;
        return (int)Vector2.Distance(playerCoordinates, targetCoordinates);
    }
    
    public static Location FromString(string location)
    {
        // get all locations and return the one with the same name
        var locations = GameManager.instance.locations;
        foreach (Location loc in locations) if (loc.area.ToString() == location) return loc;
        return null;
    }
    
    public static Location FromArea(Area area) =>  FromString(area.ToString());
    
    public static Location PlayerLocation => FromString(GameManager.gameState.player_location);
    
    #endregion
}
