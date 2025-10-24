using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

[ DisallowMultipleComponent]

// This class manages the flow of conversations in the game.
// It dictates when to start different conversations like Base, and how to follow up with actions and other conversations.
// This class does NOT handle Actions or the SmartWatch, only the setup and follow up conversations. The SmartWatch is opened by the Base converesation itself.
// This class also does not generate conversations.

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
    
    /// <summary>
    /// A method to start the base or prebase conversation based on the player's location and visit count.
    /// A PreBase conversation is the Location's conversation. You can modify them in the Database Editor, usually generated in the Location's tab.
    /// If the PreBase conversation is already displayed, then it will be skipped.
    /// The Base conversation (i.e. "What will you do next?" immediately follows the PreBase conversation
    /// </summary>
        public void StartBaseOrPreBaseConversation()
        {

            state = State.PreBase;
            
            var playerLocation = LocationManager.instance.PlayerLocation;
            
             
            // contingency: check if the proper scene is loaded, and if not, load that scene
            var playerRootLocation = playerLocation.GetRootLocation();
            var activeScene = SceneManager.GetSceneAt(0).name;

            if (activeScene != playerRootLocation.Name)
            {
                Debug.LogWarning($"active scene {activeScene} != playerRootLocation ${playerRootLocation.Name}; reloading scene"); 
                App.Instance.ChangeScene(playerRootLocation.Name, activeScene, LoadingScreen.Transition.Default);
                return;
            }  //contingency end
            
            
            
            var visitCount = DialogueLua.GetLocationField(playerLocation.Name, "Visit Count").asInt;
                 var loopConversation = playerLocation.LookupBool("Loop Conversation");
            
            // locations are Dirty if their preBase conversation has been played and the player has not yet travelled to another location
            // (travelling will reset the Dirty flag)
            if (DialogueLua.GetLocationField( playerLocation.Name, "Dirty").asBool)
            {
                DialogueManager.StartConversation("Base");
                return;
            }


            // if the location is not associated with a prebase Conversation, then just start Base
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
            
            LocationManager.instance.MarkLocationAsDirty( playerLocation);
        }
        
        public void OnConversationStart()
        {
            
            
            var conversation = DialogueManager.masterDatabase.GetConversation(DialogueManager.currentConversationState
                .subtitle.dialogueEntry.conversationID);
            
            
            
            if (conversation.Title == "Base")
            {
                state = State.Base;
                
                // the base conversation acts as an autosave checkpoint
                GameManager.DoLocalSave();
                
                // Ensures that the right background music plays.
                var playerLocation = LocationManager.instance.PlayerLocation;
                if (playerLocation.IsFieldAssigned("Music"))
                {
                    var music = playerLocation.LookupValue("Music");
                    AudioEngine.Instance.PlayClipLooped(music);
                }
            }
            
            else if (conversation.Title == "Intro" || conversation.Title == "EndOfDay")
            {
                // these are story sequences, so we should not start the base conversation after them and let them handle their own flow.
                state = State.StorySequence;
            }
            
            if (state is State.Base or State.PreBase)
            {
                // ensures that the environmental sounds play immediately, instead of waiting for Base.
                
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
            //necessary as Linked Conversations (conversations that directly connect to other conversations) do not trigger OnConversationStart
            OnConversationStart(); 
        }

        public void OnGameSceneStart()
        {
            // starts the prebase or base conversation whenever a new scene loads
            StartCoroutine(QueueConversationEndEvent(StartBaseOrPreBaseConversation));
        }

        public void OnGameSceneEnd()
        {
            // game sometimes gets bugged otherwise
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
                //removing generated conversations to keep the database clean
                DialogueManager.masterDatabase.conversations.Remove(conversation);
            }
            
            
            
            if (conversation.IsFieldAssigned("Location"))
            {
                // if this conversation is attached to a Location, then play the location's music
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
                    //if we are in prebase, we should start the Base conversation
                    StartCoroutine(QueueConversationEndEvent(() =>
                    {
                        DialogueManager.StartConversation("Base"); 
                    }));
                    break;
                case State.Base:
                    // the base conversation opens the SmartWatch, so do nothing
                    break;
                case State.Travel: // do nothing
                    break;
                case State.StorySequence: 
                    // a storysequence conversation should handle its own flow.
                    break;
                default:
                    // probably an Action
                    StartCoroutine(QueueConversationEndEvent(StartBaseOrPreBaseConversation));
                    break;
            }
        }
        
        private void OnPhoneCallStart(string contactName)
        {
            // this is to ensure that we call StartBaseOrPreBase after the phone call ends
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
            
            // add time/seconds to the clock per line
            if (subtitle.dialogueEntry.fields.Any(p => p.title == "Duration"))
            {
                var duration = int.Parse(subtitle.dialogueEntry.fields.First(p => p.title == "Duration").value);
                GameEvent.OnWait( duration);
            }

        }
        
        /// <summary>
        /// Waits for a conversation to end, and then calls the callback.
        /// This is necessary because it is not possible to call StartConversation while one is already playing without stopping the conversation entirely.
        /// </summary>
        /// <returns></returns>
        IEnumerator QueueConversationEndEvent(Action callback)
        {
            Debug.Log( "queing event from state: " + state);
            yield return new WaitForEndOfFrame();
            while (DialogueManager.instance.isConversationActive || DialogueTime.isPaused) yield return new WaitForSecondsRealtime(0.25f);
            callback?.Invoke();
        }
    
}
