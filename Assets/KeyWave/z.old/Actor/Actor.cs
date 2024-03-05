using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.UI;

///<summmary>
///Actors are in-game characters. To be honest I just like the word Actor better than Character or NPC. 
///Every character, including the player's character, is an actor.
///If you're an Unreal Engine dev I sowy.
///</summmary>
///

public class Actor : MonoBehaviour
{


    public float travelTime = 0.75f;
    [SerializeField]
    public Transform location;
    [SerializeField]
    private string _fullName;
    protected Image currentEmotion;

    private void Awake()
    {
        if (!location) location = transform.parent;
    }

    //RelocateToRoom moves the actor from its current location to a new Transform location
    public void RelocateToRoom(Transform destination) {
        StartCoroutine(Relocation(destination)); 
    }

    IEnumerator Relocation(Transform destination)
    {
    //    GameEvent.RemoveActorFromRoom(this);
        yield return new WaitForSeconds(0.75f); // number should change depending on distance from new location
     //   GameEvent.MoveActorToRoom(this, destination);
    }

    

    public void ShowPortrait()
    {
        currentEmotion.enabled = true;
        currentEmotion.color -= new Color(0, 0, 0, currentEmotion.color.a);
        StartCoroutine(CrossFadeAlpha(1, 1));
    }

    IEnumerator CrossFadeAlpha(float target, float speed)
    {
        int sign = 1;
        if (target < currentEmotion.color.a) sign = -1;

        while (target * sign > currentEmotion.color.a * sign)
        {
            float fadeAmount = currentEmotion.color.a + speed * sign * Time.deltaTime;
            currentEmotion.color = new Color(currentEmotion.color.r, currentEmotion.color.g, currentEmotion.color.b, fadeAmount);
            yield return null;
        }


    }

    public void HidePortrait() => StartCoroutine("HidePortraitHelper");


    IEnumerator HidePortraitHelper()
    {
        yield return StartCoroutine(CrossFadeAlpha(0, 1));
        currentEmotion.enabled = false;
    }


    public void SwitchMood(Image newMood)
    {
        currentEmotion = newMood;
    }



}


