using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.Events;
using System;

/// <summary>
/// A YarnEvent is a scripted Yarn sequence that is triggered when a set of requirements are matched. The name's not great.
/// GOOB PLEASE: I should probably just have it set so that instead of checking requirements on any kind of event, it just does it when blackboards are updated. hmmm. 
/// </summary>

public class YarnEvent : MonoBehaviour
{
    // Start is called before the first frame update

    private DialogueRunner dialogueRunner;
    public string conversationStartNode;
    [SerializeField]
    private bool onAnyEvent;


    private EventRequirementsCheck[] requirements;


    public void Start()
    {
        dialogueRunner = FindObjectOfType<Yarn.Unity.DialogueRunner>();
        requirements = GetComponents<EventRequirementsCheck>();
    }

    private void OnEnable()
    {
     //  if (onAnyEvent) GameEvent.OnAnyEvent += RunDialogue;

    }

    private void OnDisable()
    {
       // if (onAnyEvent) GameEvent.OnAnyEvent -= RunDialogue;

    }

    private bool CheckRequirements()
    {
        bool doRequirementsMatch = true;
        foreach (EventRequirementsCheck requirement in requirements)
        {
            if (!requirement.requirementsMatched) doRequirementsMatch = false;
        }

        return doRequirementsMatch;
    }

    public void RunDialogue()
    {
        StartCoroutine("Dialogue", conversationStartNode);         
    }


    IEnumerator Dialogue(string node)
    {
        yield return new WaitForEndOfFrame();
        if (!CheckRequirements()) yield break;
        dialogueRunner.Stop();
      //  GameEvent.YarnEvent(this);
        dialogueRunner.StartDialogue(node);
    }



}
