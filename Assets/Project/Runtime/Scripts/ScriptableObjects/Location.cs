using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

namespace Project.Runtime.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Location", menuName = "Location")]
    public class Location : ScriptableObject
    {
        public enum Area
        {
            Hotel,
            Promenade,
            Neighborhood,
            Mall,
            Park,
            Harbor,
            Café,
            Store
        }

        private const float _trafficMultiplier = 3f;

        // formula for time to distance: minutes * 4 (5 mins * 4 = 20)
        public const int DistanceToNearestCafe = 300;

        public Color responseMenuButtonColor;

        public Area area;
        public string description;
        public List<Item> objectives;
        public bool unlocked;
        [SerializeField] private Vector2 coordinates;
        
        
        public static Action<Location> OnLocationLeave;
        public static Action<Location> OnLocationEnter;

        public Vector2 Coordinates
        {
            get
            {
                var coords = area != Area.Café ? coordinates : Vector2.MoveTowards(LastNonCaféLocation.coordinates, Vector2.zero, 200);
                return coords;
            }
            
        }
        
        public string Name => area.ToString();

        public List<Field> luaFields
        {
            get => DialogueManager.instance.masterDatabase.locations.Find(n => n.Name == Name).fields;
            set {
                var fields = DialogueManager.instance.masterDatabase.locations.Find(n => n.Name == Name).fields;
                foreach (var field in value) Field.SetValue(fields, field.title, field.value);
            }
        }
        
        public string Description => DialogueManager.instance.masterDatabase.locations.Find(n => n.Name == Name).Description;

        private static Location LastNonCaféLocation
        {
            get {return FromString(GameManager.gameState.LastNonCaféLocation); }
            set { GameManager.gameState.LastNonCaféLocation = value.Name; }
        }

        private int Distance
        {
            get
            {
                if (PlayerLocation == this) return 0;
                // if cafe, distance is relative to current location
                if (this.area == Area.Café || (this == LastNonCaféLocation && PlayerLocation.area == Area.Café))
                {
                    return DistanceToNearestCafe;
                }
                var playerCoordinates = PlayerLocation.coordinates;
           
                return (int)(Vector2.Distance(playerCoordinates, coordinates) * Traffic.CurrentTrafficMultiplier);
            }
        }

        public int TravelTime {
            get
            {
                return Distance;
            }
        }
        
        public void MoveHere()
        {
            LastLocation = FromString(GameManager.gameState.PlayerLocation);
            if (LastLocation.area != Area.Café) LastNonCaféLocation = LastLocation;
            
            GameEvent.OnMove(this.Name, LastLocation, Distance);
            
            GameManager.instance.TravelTo(this.name,
                onStart: () =>
                {
                    OnLocationLeave?.Invoke(PlayerLocation);
                },
                onComplete: () =>
                {
                    OnLocationEnter?.Invoke(this);
                }
                );
        }
        
        public void FadeHere()
        {
            LastLocation = FromString(GameManager.gameState.PlayerLocation);
            if (LastLocation.area != Area.Café) LastNonCaféLocation = LastLocation;
            //GameEvent.OnMove(this.Name, LastLocation, Distance);

            GameManager.instance.TravelTo(this, LoadingScreen.Transition.Black);
        }

        public void MoveHereImmediate()
        {
            App.App.Instance.LoadSceneImmediate(name, "StartMenu");
        }

        

        public override string ToString()
        {
            return Name;
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

        public static Location PlayerLocation => FromString(GameManager.gameState.PlayerLocation);

        public static string PlayerLocationWithSublocation
        {
            get
            {
                var location = PlayerLocation.name;
                var sublocation = DialogueLua.GetLocationField(location, "Current Sublocation").asString;
        
                if (!string.IsNullOrEmpty(sublocation)) location += "/" + sublocation;
                return location;
            }
        }

        public static Location LastLocation;

        #endregion
    }
}