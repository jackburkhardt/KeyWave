using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayerEvent = PlayerEvents.PlayerEvent;


public class InvokeMovePlayer : PlayerEventHandler
{

   
    
    protected void Start()
    {
        /*
        _playerLocationName = GameManager.instance.gameStateManager.gameState.player_location;
        _playerLocationTransform =  FindTransformMatchingString(_playerLocationName);
        UpdateText();
        */
    }
    
    /*

    private void UpdateText()
    {
        minutesAway = GetDistanceToTransform(_playerLocationTransform);
        _minutesAwayText.text = transform == _playerLocationTransform
            ? "You are here."
            : $"{minutesAway} minutes away.";
    }
    
   
    
    Transform FindTransformMatchingString(string locationName)
    {
        Transform currentPlayerLocation = null;
        var allMapButtons = FindObjectsOfType<InvokeMovePlayer>();
        foreach (var mapButton in allMapButtons)
        {
            if (mapButton._locationNameText.text == locationName) currentPlayerLocation = mapButton.transform;
        }
        return currentPlayerLocation;
    }


    public int GetDistanceToTransform(Transform currentPlayerCoordinates)
    {
        var distance = Vector2.Distance(currentPlayerCoordinates.position, transform.position);
        var minutesAway = Mathf.RoundToInt(distance / 10);
        return minutesAway;
    }
    
    */
    
    protected override void OnPlayerEvent(PlayerEvent playerEvent)
    {
       
        if (playerEvent.Type != "move" || playerEvent.Value != _destination.ToString()) return;
        GameManager.instance.TravelTo(_destination.ToString());
         /*
        _playerLocationName = playerEvent.Value;
       _playerLocationTransform = FindTransformMatchingString(_playerLocationName);


       */
    }

    [SerializeReference] private GameManager.Locations _destination;

    
    

    public void MovePlayer()
    {
        var distanceInSeconds =  GameManager.instance.GetPlayerDistanceFromLocation(_destination);
        GameEvent.OnMove(gameObject.name, _destination.ToString(), distanceInSeconds);
    }

    public void SetDestination(GameManager.Locations location)
    {
        _destination = location;
    }
    
}
