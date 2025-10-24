using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;


[DisallowMultipleComponent]
/// <summary>
/// The LocationManager is responsible for managing the player's location and the distance between locations. It also handles the transition between locations.
/// </summary>
public class LocationManager : MonoBehaviour
{
    
    public const float DistanceToCafé = 300f;

    public static LocationManager instance;

    public static Action<Location> OnLocationEnter;
    public static Action<Location> OnLocationExit;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(this);
        }
    }

    public Location PlayerLocation
    {
        get => DialogueManager.masterDatabase.GetLocation(DialogueLua.GetActorField(GameManager.instance.PlayerActor.Name, "Location").asInt);
        private set
        {
            DialogueLua.SetActorField(GameManager.instance.PlayerActor.Name, "Location", value.id);
        
            var rootLocation = value.GetRootLocation();
            if (rootLocation.Name != "Café")
                LastNonCaféLocation = rootLocation.Name;
        }
    }
    
    public int MostRecentSublocation
    {
        get => (DialogueLua.GetVariable("game.player.mostRecentSublocation").asInt);
        set => DialogueLua.SetVariable("game.player.mostRecentSublocation", value);
    }



    public void OnGameSceneEnd()
    {
        UnmarkLocationAsDirty( PlayerLocation);
    }

    public void OnGameActionStart(Item item)
    {
        if (item.FieldExists("New Sublocation"))
        {
            var sublocationSwitcherMethod = item.AssignedField("Sublocation Switcher Method");
            if (sublocationSwitcherMethod != null && sublocationSwitcherMethod.value.StartsWith("MoveBeforeConversation"))
            {
                var currentSublocation = PlayerLocation;
                var sublocation = DialogueManager.masterDatabase.GetLocation(item.LookupInt("New Sublocation"));
                SetPlayerLocation(sublocation);
                        
                if (sublocationSwitcherMethod.value.Contains( "ReturnWhenDone"))
                {
                    if (MostRecentSublocation == -1) MostRecentSublocation = currentSublocation.id;
                }
            }
        }
    }

    public void OnGameActionEnd(Item item)
    {
        if (item.IsFieldAssigned("New Sublocation"))
        {
            if (!item.FieldExists("Conversation") || item.AssignedField("Sublocation Switcher Method").value == "MoveAfterConversation")
            {
                var location = DialogueManager.masterDatabase.GetLocation(item.LookupInt("New Sublocation"));
                SetPlayerLocation(location);
            }
                        
            else if (item.AssignedField("Sublocation Switcher Method").value.Contains("ReturnWhenDone"))
            {
                SetPlayerLocation( DialogueManager.masterDatabase.GetLocation(MostRecentSublocation));
                MostRecentSublocation = -1;
            }
        }
    }
    
    public static void SetPlayerLocation(Location newLocation,  LoadingScreen.Transition transition = LoadingScreen.Transition.Default)
    {
        var currentLocation = instance.PlayerLocation;
        
        if (currentLocation == newLocation)
        {
            GameManager.instance.StartGameScene(newLocation.Name);
        }

        else
        {
            if (currentLocation != null && currentLocation.GetRootLocation() == newLocation.GetRootLocation()) // root location is the same, which means we are only changing sublocation
            {
                instance.StartCoroutine(instance.SwitchSublocationHandler(newLocation)); 
            }

            else
            {
                instance.StartCoroutine(instance.SwitchLocationHandler(newLocation, transition));
            }
        }
    }
    
    
    IEnumerator SwitchLocationHandler(Location newLocation,  LoadingScreen.Transition transition)
    {
        var currentLocation = instance.PlayerLocation.GetRootLocation();
        if (PlayerLocation != null)
        {
            OnLocationExit?.Invoke(PlayerLocation);
            GameEvent.OnMove(newLocation.Name, currentLocation.Name, (int)DistanceToLocation(newLocation));
        }
        
        if (newLocation.FieldExists("Spawn Point")) // if a location has a spawn point, PlayerLocation must be set to that spawn point
        {
            var spawnPoint = DialogueLua.GetLocationField(newLocation.Name, "Spawn Point").asInt;
            if (spawnPoint < 0) spawnPoint = newLocation.id;
            PlayerLocation = DialogueManager.masterDatabase.GetLocation(spawnPoint);
        }

        else
        {
            PlayerLocation = newLocation;
        }
        
        yield return App.Instance.ChangeScene(newLocation.Name, SceneManager.GetActiveScene().name, transition); // scene transition
        AudioEngine.Instance.StopAllAudioOnChannel("Music");
        
        OnLocationEnter?.Invoke(newLocation.GetRootLocation());
                
    }
    
    IEnumerator SwitchSublocationHandler(Location location, float transitionDuration = 3f)
    {
        DialogueManager.instance.gameObject.BroadcastMessageExt( "OnSublocationChange");
        var locationScene = SceneManager.GetSceneByName(DialogueManager.masterDatabase.GetLocation(location.RootID).Name);
        DialogueManager.Pause();
                
        yield return KeyWaveUtility.Fade( "stay", transitionDuration/4);
        yield return new WaitForSeconds(transitionDuration/4);
                
        var destinationSublocationGameObject = locationScene.FindGameObject(location.Name);
        if (destinationSublocationGameObject != null) destinationSublocationGameObject.SetActive(true);
                
        if (PlayerLocation.IsSublocation)
        {
            var currentSublocationGameObject = locationScene.FindGameObject(PlayerLocation.Name);
            if (currentSublocationGameObject != null) currentSublocationGameObject.SetActive(false);
        }
                
        yield return KeyWaveUtility.Fade( "unstay", transitionDuration/4);
        yield return new WaitForSeconds(transitionDuration/4);
        DialogueManager.Unpause();
                
        PlayerLocation = location;
        yield return null;
    }
    
    public static IEnumerator SwitchLocationImmediate(Location location)
    {
        instance.PlayerLocation = location;
        yield return App.Instance.ChangeScene(location.Name, SceneManager.GetActiveScene().name, LoadingScreen.Transition.None);
    }


    public static float DistanceToLocation(Location location)
    {
        if (instance.PlayerLocation.GetRootLocation() == location) return 0;
      
        if (location.Name == "Café")
        {
            return DistanceToCafé;
        }
        
        // if cafe, distance is relative to current location

        if (instance.PlayerLocation.GetRootLocation().Name == "Café" && location.Name == instance.LastNonCaféLocation)
        {
            return DistanceToCafé;
        }
                
        var locationCoordinates = location.LookupVector2("Coordinates");
        var playerCoordinates = instance.PlayerLocation.GetRootLocation().LookupVector2("Coordinates");

        return Vector2.Distance(playerCoordinates, locationCoordinates) * Traffic.CurrentTrafficMultiplier;
    }
    
    public string LastNonCaféLocation
    {
        get
        {
            var lastLocation = DialogueLua.GetVariable("game.player.lastNonCaféLocation").asString;
            return lastLocation == string.Empty || lastLocation == "nil" ? PlayerLocation.Name : lastLocation;
        }
        set => DialogueLua.SetVariable("game.player.lastNonCaféLocation", value);
    }
    
    
    /// <summary>
    /// Locations are marked as Dirty in order to ensure that the Visit Count is not incremented multiple times before the player leaves the location.
    /// </summary>
    /// <param name="location"></param>
    public void MarkLocationAsDirty(Location location)
    {
        DialogueLua.SetLocationField(PlayerLocation.Name, "Dirty", true);
        var visitCount = DialogueLua.GetLocationField(PlayerLocation.Name, "Visit Count").asInt;
        visitCount += 1;
        DialogueLua.SetLocationField( PlayerLocation.Name, "Visit Count", visitCount);
        PlayerLocation.AssignedField("Visit Count").value = (visitCount).ToString();
    }
        
    public void UnmarkLocationAsDirty(Location location)
    {
        DialogueLua.SetLocationField(PlayerLocation.Name, "Dirty", false);
    }
}

// this sequence command is used in the Intro cutscene to change the player's location.

public class SequencerCommandSetLocationImmediate : SequencerCommand
{
    IEnumerator Start()
    {
        var location = parameters[0];
        yield return LocationManager.SwitchLocationImmediate(DialogueManager.masterDatabase.GetLocation(location));
        Stop();
    }

}
