/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace blackboard {

    public class RoomBlackboard : MonoBehaviour
    {
        public int previousTotalPlayerVisits = 0;
        public bool isPlayerHere = false;


        private void OnEnable()
        {
            GameEvent.OnActorEnterRoom += UpdateBlackboardOnActorEnter;
            GameEvent.OnActorExitRoom += UpdateBlackboardOnActorExit;
        }


        private void OnDisable()
        {
            GameEvent.OnActorEnterRoom -= UpdateBlackboardOnActorEnter;
            GameEvent.OnActorExitRoom -= UpdateBlackboardOnActorExit;
        }

        private void UpdateBlackboardOnActorEnter(Actor actor, Transform destination)
        {
            if (actor == GameManager.playerActor)
            {
                if (transform != destination) { isPlayerHere = false; return; };
                isPlayerHere = true;
            }
           
        }

        private void UpdateBlackboardOnActorExit(Actor actor, Transform room)
        {
            if (actor == GameManager.playerActor)
            {
                if (isPlayerHere) previousTotalPlayerVisits++;
            }
                
        }

    }
    }

*/
