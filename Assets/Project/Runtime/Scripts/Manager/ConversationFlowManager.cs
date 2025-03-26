using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

public class ConversationFlowManager : MonoBehaviour
{
    
    private void Awake()
    {
        state = State.PreBase;
    }

    private void OnEnable()
    {
        PhoneCallPanel.OnPhoneCallStart += OnPhoneCallStart;
    }

    private void OnDisable()
    {
        PhoneCallPanel.OnPhoneCallStart -= OnPhoneCallStart;
    }
    
  


    public enum State
    {
        Travel,
        PreBase,
        Base,
        Action,
        StorySequence,
        PhoneCall
    }
    
    State state = State.PreBase;
    
        public void StartBaseOrPreBaseConversation()
        {

            state = State.PreBase;
            
            var playerLocation = GameManager.instance.locationManager.PlayerLocation;
            
            
            var visitCount = DialogueLua.GetLocationField(playerLocation.Name, "Visit Count").asInt;
                 var loopConversation = playerLocation.LookupBool("Loop Conversation");
            
            if (DialogueLua.GetLocationField( playerLocation.Name, "Dirty").asBool)
            {
                DialogueManager.StartConversation("Base");
                return;
            }


            if (!playerLocation.FieldExists("Conversation"))
            {
                DialogueManager.StartConversation("Base");
                return;
            }

            if (visitCount == 0)
            {
                if (playerLocation.IsFieldAssigned("Conversation"))
                    DialogueManager.StartConversation(
                        playerLocation.LookupValue("Conversation"));

                else
                {
                    var generatedConversation =
                        KeyWaveUtility.GenerateConversation(playerLocation);
                    // SequencerCommandGoToConversatio
                    DialogueManager.StartConversation(generatedConversation.Title);
                }
            }
            
            else if (visitCount > 0 && loopConversation)
                if (playerLocation.IsFieldAssigned("Conversation"))
                    DialogueManager.StartConversation(
                        playerLocation.LookupValue("Conversation"));

                else
                {
                    var generatedConversation =
                        KeyWaveUtility.GenerateConversation(playerLocation, true);
                    // SequencerCommandGoToConversatio
                    DialogueManager.StartConversation(generatedConversation.Title);
                }

            else DialogueManager.StartConversation("Base");
            
            GameManager.instance.locationManager.MarkLocationAsDirty( playerLocation);
        }
        
        public void OnConversationStart()
        {
            
            var conversation = DialogueManager.masterDatabase.GetConversation(DialogueManager.currentConversationState
                .subtitle.dialogueEntry.conversationID);
            
            
            if (conversation.Title == "Base")
            {
                GameManager.DoLocalSave();
                state = State.Base;
                var playerLocation = LocationManager.instance.PlayerLocation;

                if (playerLocation.IsFieldAssigned("Music"))
                {
                    var music = playerLocation.LookupValue("Music");
                    AudioEngine.Instance.PlayClipLooped(music);
                }
            }
            
            else if (conversation.Title == "Intro" || conversation.Title == "EndOfDay")
            {
                state = State.StorySequence;
            }
            
            if (state is State.Base or State.PreBase)
            {
                var playerLocation = LocationManager.instance.PlayerLocation;
                if (playerLocation.IsFieldAssigned("Environment"))
                {
                    var environment = playerLocation.LookupValue("Environment");
                    AudioEngine.Instance.PlayClipLooped(environment);
                }
            }
        }

        public void OnLinkedConversationStart()
        {
            OnConversationStart();
        }

        public void OnGameSceneStart()
        {
            StartCoroutine(QueueConversationEndEvent(StartBaseOrPreBaseConversation));
        }

        public void OnGameSceneEnd()
        {
            StopAllCoroutines();
        }

        public void OnGameActionStart()
        {
            state = State.Action;
        }
        
        public void OnConversationEnd()
        {
            
            var conversationID = DialogueManager.currentConversationState.subtitle.dialogueEntry.conversationID;
            var conversation = DialogueManager.masterDatabase.GetConversation(conversationID);
           
        
            if (conversation.Title.Contains("/GENERATED/"))
            {
                DialogueManager.masterDatabase.conversations.Remove(conversation);
            }

            if (conversation.IsFieldAssigned("Location"))
            {
                var location = DialogueManager.masterDatabase.GetLocation(conversation.LookupInt("Location"));
                if (location.IsFieldAssigned("Music"))
                {
                    var music = location.LookupValue("Music");
                    AudioEngine.Instance.PlayClipLooped(music);
                }
            }


            switch (state)
            {
                case State.PreBase:
                    StartCoroutine(QueueConversationEndEvent(() =>
                    {
                        DialogueManager.StartConversation("Base"); 
                    }));
                    break;
                case State.Base:
                    break;
                case State.Travel: // do nothing
                    break;
                case State.StorySequence:  // do nothing
                    break;
                default:
                    StartCoroutine(QueueConversationEndEvent(StartBaseOrPreBaseConversation));
                    break;
            }
        }
        
        private void OnPhoneCallStart(string contactName)
        {
            state = State.PhoneCall;
        }
        
        public void OnConversationLine(Subtitle subtitle)
        {
           


            var conversation = DialogueManager.masterDatabase.GetConversation(subtitle.dialogueEntry.conversationID);
            
            var subtitleActor = DialogueManager.masterDatabase.GetActor(subtitle.dialogueEntry.ActorID);
            if (subtitleActor != null && subtitleActor.Name.Split(" ").Any( p => subtitle.formattedText.text.Split(" ").Contains(p)))
            {
                DialogueLua.SetActorField(subtitleActor.Name, "Introduced", true);
            }

            var conversationActor = DialogueManager.masterDatabase.GetActor(conversation.ActorID);
            
            if (conversationActor != null && conversationActor.Name.Split(" ").Any( p => subtitle.formattedText.text.Split(" ").Contains(p)))
            {
                DialogueLua.SetActorField(conversationActor.Name, "Introduced", true);
            }
            
            
            if (subtitle.dialogueEntry.fields.Any(p => p.title == "Duration"))
            {
                var duration = int.Parse(subtitle.dialogueEntry.fields.First(p => p.title == "Duration").value);
                GameEvent.OnWait( duration);
            }

        }
        
        IEnumerator QueueConversationEndEvent(Action callback)
        {
            Debug.Log( "queing event from state: " + state);
            yield return new WaitForEndOfFrame();
            while (DialogueManager.instance.isConversationActive || DialogueTime.isPaused) yield return new WaitForSecondsRealtime(0.25f);
            callback?.Invoke();
        }
    
}
