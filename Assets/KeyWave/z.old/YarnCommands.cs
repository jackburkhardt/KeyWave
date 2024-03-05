using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using System;
using UnityEngine.Playables;

///Whole buncha custom commands for yarn scripts.
namespace YarnEvents {
    public class YarnCommands : MonoBehaviour

    {

        //  public static YarnEvents.YarnEvent currentYarnEvent;

        private void OnEnable()
        {
            //    GameEvent.OnYarnEventActive += SetCurrentYarnEvent;
        }

        private void OnDisable()
        {
            //    GameEvent.OnYarnEventActive -= SetCurrentYarnEvent;
        }

        /*

        [YarnCommand("travel")]
        public static IEnumerator travelYarn(GameObject destination)
        {
            yield return new WaitForSeconds(0.25f);
            GameManager.playerActor.RelocateToRoom(destination.transform);
            yield return new WaitForSeconds(1.5f);
            //GameEvent.MoveActorToRoom(Player.playerActor, destination.transform); 
        }

        */


        void SetCurrentYarnEvent(YarnEvents.YarnEvent yarnEvent)
        {
            //   currentYarnEvent = yarnEvent;
        }




        // start conversation with an actor
        [YarnCommand("show")]
        public static IEnumerator StartConversation(GameObject actorObj)
        {
            actorObj.GetComponent<Actor>().ShowPortrait();
            yield return new WaitForSeconds(1.25f);
        }

        [YarnCommand("hide")]
        public static IEnumerator EndConversation(GameObject actorObj)
        {
            yield return new WaitForSeconds(0.25f);
            actorObj.GetComponent<Actor>().HidePortrait();
            yield return new WaitForSeconds(1f);
        }

        // goob what the fuck does signal do

        [YarnCommand("signal")]
        public static void Signal(string signal)
        {
           // GameEvent.YarnSignal(signal);
        }

        [YarnFunction("first_visit")]
        public static bool FirstVisit()
        {
            string name = GameObject.FindObjectOfType<DialogueRunner>().gameObject.name;

            foreach (YarnEvent yarnEvent in YarnEventManager.ChapterYarnEvents)
            {
                if (yarnEvent.Name == name && yarnEvent.ActivationCount == 1) return true;
            }

            return false;
        }

        [YarnFunction("activated")]

        public static bool Activated(string name)
        {
            foreach (YarnEvent yarnEvent in YarnEventManager.ChapterYarnEvents)
            {
                if (yarnEvent.Name == name && yarnEvent.ActivationCount > 0) return true;
            }

            return false;
        }

        [YarnCommand("play_camera_animation")] 
        public static void PlayCameraAnimation(string name)
        {
            Camera.main.GetComponent<PlayableDirector>().Play();
        }
        

    }
}

