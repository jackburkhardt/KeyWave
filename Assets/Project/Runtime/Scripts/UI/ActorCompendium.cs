// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;

namespace Project.Runtime.Scripts.ActorCompendium
{

    public delegate ActorState StringToActorStateDelegate(string s);
    public delegate string ActorStateToStringDelegate(ActorState state);
    public delegate string CurrentActorStateDelegate(string Actor);
    public delegate void SetActorStateDelegate(string Actor, string state);
  

   

    /// <summary>
    /// A static class that manages a Actor log. It uses the Lua "Item[]" table, where each item in
    /// the table whose 'Is Item' field is false represents a Actor. This makes it easy to manage Actors 
    /// in Chat Mapper by adding, removing, and modifying items in the built-in Item[] table. The name 
    /// of the item is the title of the Actor. (Note that the Chat Mapper simulator doesn't have a 
    /// Actor system, so it treats elements of the Item[] table as items.)
    /// 
    /// This class uses the following fields in the Item[] table, which is also aliased as Actor[]:
    /// 
    /// - <b>Display Name</b>: (optional) Name to use in UIs. If blank or not present, UIs use Name field.
    /// - <b>State</b> (if using Chat Mapper, add this custom field or use the Dialogue System template 
    /// project)
    /// 	- Valid values (case-sensitive): <c>unassigned</c>, <c>active</c>, <c>success</c>, 
    /// <c>failure</c>, or <c>done</c>
    /// - <b>Description</b>: The description of the Actor
    /// - <b>Success Description</b> (optional): The description to be displayed when the Actor has been 
    /// successfully completed
    /// - <b>Failure Description</b> (optional): The description to be displayed when the Actor has ended 
    /// in failure
    /// 
    /// Note: <c>done</c> is essentially equivalent to </c>success</c>. In the remainder of the Dialogue 
    /// System's documentation,	either <c>done</c> or <c>success</c> may be used in examples, but when 
    /// using the ActorLog class, they both correspond to the same enum state, ActorState.Success.
    /// 
    /// As an example, you might define a simple Actor like this:
    /// 
    /// - Item["Kill 5 Rats"]
    /// 	- State = "unassigned"
    /// 	- Description = "The baker asked me to bring him 5 rat corpses to make a pie."
    /// 	- Success Description = "I brought the baker 5 dead rats, and we ate a delicious pie!"
    /// 	- Failure Description = "I freed the Pied Piper from jail. He took all the rats. No pie for me...."
    /// 
    /// This class provides methods to add and delete Actors, get and set their state, and get 
    /// their descriptions.
    /// 
    /// Note that Actor states are usually updated during conversations. In most cases, you will 
    /// probably set Actor states in Lua code during conversations, so you may never need to use
    /// many of the methods in this class.
    /// 
    /// The UnityActorLogWindow provides a Actor log window using Unity GUI. You can use it as-is 
    /// or use it as a template for implementing your own Actor log window in another GUI system 
    /// such as NGUI.
    /// </summary>
    public static class ActorCompendium
    {

        /// <summary>
        /// Constant state string for unassigned Actors.
        /// </summary>
        public const string UnidentifiedStateString = "unidentified";

        /// <summary>
        /// Constant state string for active Actors.
        /// </summary>
        public const string MentionedStateString = "mentioned";

        /// <summary>
        /// Constant state string for successfully-completed Actors.
        /// </summary>
        public const string AmicableStateString = "amicable";

        /// <summary>
        /// Constant state string for Actors ending in failure.
        /// </summary>
        public const string BotchedStateString = "botched";

        /// <summary>
        /// Constant state string for Actors that were abandoned.
        /// </summary>
        public const string UnapproachableStateString = "unapproachable";

        /// <summary>
        /// Constant state string for Actors that are grantable. 
        /// This state isn't used by the Dialogue System, but it's made available for those who want to use it.
        /// </summary>
        public const string ApproachableStateString = "approachable";

        /// <summary>
        /// Constant state string for Actors that are waiting to return to NPC.
        /// This state isn't used by the Dialogue System, but it's made available for those who want to use it.
        /// </summary>
        public const string ConfrontedStateString = "confronted";

        /// <summary>
        /// Constant state string for Actors that are done, if you want to track done instead of success/failure.
        /// This is essentially the same as success, and corresponds to the same enum value, ActorState.Success
        /// </summary>
        public const string DoneStateString = "done";

        /// <summary>
        /// You can reassign this delegate method to override the default conversion of
        /// strings to ActorStates.
        /// </summary>
        public static StringToActorStateDelegate StringToState = DefaultStringToState;

        public static ActorStateToStringDelegate StateToString = DefaultStateToString;

        /// <summary>
        /// You can assign a method to override the default CurrentActorState.
        /// </summary>
        public static CurrentActorStateDelegate CurrentActorStateOverride = null;

        /// <summary>
        /// You can assign a method to override the default SetActorState.
        /// </summary>
        public static SetActorStateDelegate SetActorStateOverride = null;

       
       

        public static void RegisterActorCompendiumFunctions()
        {
            // Unity 2017.3 bug IL2CPP can't do lambdas:
            //Lua.RegisterFunction("CurrentActorState", null, SymbolExtensions.GetMethodInfo(() => CurrentActorState(string.Empty)));
            //Lua.RegisterFunction("CurrentActorEntryState", null, SymbolExtensions.GetMethodInfo(() => CurrentActorEntryState(string.Empty, (double)0)));
            //Lua.RegisterFunction("SetActorState", null, SymbolExtensions.GetMethodInfo(() => SetActorState(string.Empty, string.Empty)));
            //Lua.RegisterFunction("SetActorEntryState", null, SymbolExtensions.GetMethodInfo(() => SetActorEntryState(string.Empty, (double)0, string.Empty)));

            Lua.RegisterFunction("CurrentActorState", null, typeof(ActorCompendium).GetMethod("CurrentActorState"));
            Lua.RegisterFunction("SetActorState", null, typeof(ActorCompendium).GetMethod("SetActorState", new[] { typeof(string), typeof(string) }));
        //    Lua.RegisterFunction("UpdateActorIndicators", null, typeof(ActorLog).GetMethod("UpdateActorIndicators", new[] { typeof(string) }));
        }

        /// <summary>
        /// Adds a Actor to the Lua Item[] table.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='description'>
        /// Description of the Actor when active.
        /// </param>
        /// <param name='successDescription'>
        /// Description of the Actor when successfully completed.
        /// </param>
        /// <param name='failureDescription'>
        /// Description of the Actor when completed in failure.
        /// </param>
        /// <param name='state'>
        /// Actor state.
        /// </param>
        /// <example>
        /// ActorLog.AddActor("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.", ActorState.Unassigned);
        /// </example>
        public static void AddActor(string actorName, string description, string successDescription, string failureDescription, ActorState state)
        {
            if (!string.IsNullOrEmpty(actorName))
            {
                Lua.Run(string.Format("Actor[\"{0}\"] = {{ Name = \"{1}\", IDescription = \"{2}\", Success_Description = \"{3}\", Failure_Description = \"{4}\", State = \"{5}\" }}",
                                      new System.Object[] { DialogueLua.StringToTableIndex(actorName),
                                      DialogueLua.DoubleQuotesToSingle(actorName),
                                      DialogueLua.DoubleQuotesToSingle(description),
                                      DialogueLua.DoubleQuotesToSingle(successDescription),
                                      DialogueLua.DoubleQuotesToSingle(failureDescription),
                                       StateToString(state) }),
                        DialogueDebug.logInfo);
            }
        }

        /// <summary>
        /// Adds a Actor to the Lua Item[] table.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='description'>
        /// Description of the Actor.
        /// </param>
        /// <param name='state'>
        /// Actor state.
        /// </param>
        /// <example>
        /// ActorLog.AddActor("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.", ActorState.Unassigned);
        /// </example>
        public static void AddActor(string actorName, string description, ActorState state)
        {
            if (!string.IsNullOrEmpty(actorName))
            {
                Lua.Run(string.Format("Actor[\"{0}\"] = {{ Name = \"{1}\", Description = \"{2}\", State = \"{3}\" }}",
                                      new System.Object[] { DialogueLua.StringToTableIndex(actorName),
                                      DialogueLua.DoubleQuotesToSingle(actorName),
                                      DialogueLua.DoubleQuotesToSingle(description),
                                      StateToString(state) }),
                        DialogueDebug.logInfo);
            }
        }

        /// <summary>
        /// Adds a Actor to the Lua Item[] table, and sets the state to Unassigned.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='description'>
        /// Description of the Actor.
        /// </param>
        /// <example>
        /// ActorLog.AddActor("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.");
        /// </example>
        public static void AddActor(string actorName, string description)
        {
            AddActor(actorName, description, ActorState.Unidentified);
        }

        /// <summary>
        /// Deletes a Actor from the Lua Item[] table. Use this method if you want to remove a Actor entirely.
        /// If you just want to set the state of a Actor, use SetActorState.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <example>
        /// ActorLog.RemoveActor("Kill 5 Rats");
        /// </example>
        public static void DeleteActor(string actorName)
        {
            if (!string.IsNullOrEmpty(actorName))
            {
                Lua.Run(string.Format("Actor[\"{0}\"] = nil", new System.Object[] { DialogueLua.StringToTableIndex(actorName) }), DialogueDebug.logInfo);
            }
        }

        /// <summary>
        /// Gets the Actor state.
        /// </summary>
        /// <returns>
        /// The Actor state.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <example>
        /// if (ActorLog.ActorState("Kill 5 Rats") == ActorState.Active) {
        ///     Smith.Say("Killing rats, eh? Here, take this hammer.");
        /// }
        /// </example>
        public static ActorState GetActorState(string actorName)
        {
            return StringToState(CurrentActorState(actorName)); //---Was: DialogueLua.GetActorField(actorName, "State").AsString);
        }

        /// <summary>
        /// Gets the Actor state.
        /// </summary>
        /// <param name="actorName">Name of the Actor.</param>
        /// <returns>The Actor state string ("unassigned", "success", etc.).</returns>
        public static string CurrentActorState(string actorName)
        {
            if (CurrentActorStateOverride != null)
            {
                return CurrentActorStateOverride(actorName);
            }
            else
            {
                return DefaultCurrentActorState(actorName);
            }
        }

        /// <summary>
        /// Default built-in version of CurrentActorState.
        /// </summary>
        public static string DefaultCurrentActorState(string actorName)
        {
            return DialogueLua.GetActorField(actorName, "State").asString;
        }

        /// <summary>
        /// Sets the Actor state.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='state'>
        /// New state.
        /// </param>
        /// <example>
        /// if (PiedPiperIsFree) {
        ///     ActorLog.SetActorState("Kill 5 Rats", ActorState.Failure);
        /// }
        /// </example>
        public static void SetActorState(string actorName, ActorState state)
        {
            SetActorState(actorName, StateToString(state));
        }

        /// <summary>
        /// Sets the Actor state, using the override delegate if assigned; otherwise
        /// using the default method DefaultSetActorState.
        /// </summary>
        /// <param name="actorName">Name of the Actor.</param>
        /// <param name="state">New state.</param>
        public static void SetActorState(string actorName, string state)
        {
            if (SetActorStateOverride != null)
            {
                SetActorStateOverride(actorName, state);
            }
            else
            {
                DefaultSetActorState(actorName, state);
            }
        }

        /// <summary>
        /// Default built-in method to set Actor state.
        /// </summary>
        public static void DefaultSetActorState(string actorName, string state)
        {
            if (DialogueLua.DoesTableElementExist("Actor", actorName))
            {
                DialogueLua.SetActorField(actorName, "State", state);
                SendUpdateTracker();
                InformActorStateChange(actorName);
            }
            else
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Actor '" + actorName + "' doesn't exist. Can't set state to " + state);
            }
        }

        private static void SendUpdateTracker()
        {
            DialogueManager.SendUpdateTracker();
        }

        public static void InformActorStateChange(string actorName)
        {
            DialogueManager.instance.BroadcastMessage(DialogueSystemMessages.OnActorStateChange, actorName, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Reports whether a Actor is unassigned.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the Actor is unassigned; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static bool IsActorUnidentified(string actorName)
        {
            return GetActorState(actorName) == ActorState.Unidentified;
        }

        /// <summary>
        /// Reports whether a Actor is active.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the Actor is active; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static bool IsActorMentioned(string actorName)
        {
            return GetActorState(actorName) == ActorState.Mentioned;
        }

        /// <summary>
        /// Reports whether a Actor was successfully completed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the Actor was successfully completed; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static bool IsActorAmicable(string actorName)
        {
            return GetActorState(actorName) == ActorState.Amicable;
        }

        /// <summary>
        /// Reports whether a Actor ended in failure.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the Actor ended in failure; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static bool IsActorBotched(string actorName)
        {
            return GetActorState(actorName) == ActorState.Botched;
        }

        /// <summary>
        /// Reports whether a Actor was abandoned (i.e., in the Abandoned state).
        /// </summary>
        /// <returns><c>true</c> if the Actor was abandoned; otherwise, <c>false</c>.</returns>
        /// <param name="actorName">Name of the Actor.</param>
        public static bool IsActorUnapproachable(string actorName)
        {
            return GetActorState(actorName) == ActorState.Unapproachable;
        }

        /// <summary>
        /// Reports whether a Actor is done, either successful or failed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the Actor is done; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static bool IsActorDone(string actorName)
        {
            ActorState state = GetActorState(actorName);
            return state is ActorState.Amicable or ActorState.Botched;
        }

        /// <summary>
        /// Reports whether a Actor's current state is one of the states marked in a state bit mask.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the Actor's current state is in the state bit mask.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='stateMask'>
        /// A ActorState bit mask (e.g., <c>ActorState.Success | ActorState.Failure</c>).
        /// </param>
        public static bool IsActorInStateMask(string actorName, ActorState stateMask)
        {
            ActorState state = GetActorState(actorName);
            return ((stateMask & state) == state);
        }

       

        /// <summary>
        /// Starts a Actor by setting its state to active.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <example>
        /// StartActor("Kill 5 Rats");
        /// </example>
        public static void MentionActor(string actorName)
        {
            SetActorState(actorName, ActorState.Mentioned);
        }

        /// <summary>
        /// Marks a Actor successful.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static void ConcludeActor(string actorName)
        {
            SetActorState(actorName, ActorState.Amicable);
        }

        /// <summary>
        /// Marks a Actor as failed.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static void BotchActor(string actorName)
        {
            SetActorState(actorName, ActorState.Botched);
        }

        /// <summary>
        /// Marks a Actor as abandoned (i.e., in the Abandoned state).
        /// </summary>
        /// <param name="actorName">Name of the Actor.</param>
        public static void AbandonActor(string actorName)
        {
            SetActorState(actorName, ActorState.Unapproachable);
        }

        /// <summary>
        /// Converts a string representation into a state enum value.
        /// </summary>
        /// <returns>
        /// The state (e.g., <c>ActorState.Active</c>).
        /// </returns>
        /// <param name='s'>
        /// The string representation (e.g., "active").
        /// </param>
        public static ActorState DefaultStringToState(string s)
        {
            if (string.Equals(s, UnidentifiedStateString)) return ActorState.Unidentified;
            if (string.Equals(s, MentionedStateString)) return ActorState.Mentioned;
            if (string.Equals(s, AmicableStateString) || string.Equals(s, DoneStateString)) return ActorState.Amicable;
            if (string.Equals(s, BotchedStateString)) return ActorState.Botched;
            if (string.Equals(s, UnapproachableStateString)) return ActorState.Unapproachable;
            if (string.Equals(s, ApproachableStateString)) return ActorState.Approachable;
            if (string.Equals(s, ConfrontedStateString)) return ActorState.Confronted;
            return ActorState.Unidentified;
            
        }
        
        
        

        public static string DefaultStateToString(ActorState state)
        {
            
            switch (state)
            {
                default:
                case ActorState.Unidentified: return UnidentifiedStateString;
                case ActorState.Mentioned: return MentionedStateString;
                case ActorState.Amicable: return AmicableStateString;
                case ActorState.Botched: return BotchedStateString;
                case ActorState.Unapproachable: return UnapproachableStateString;
                case ActorState.Approachable: return ApproachableStateString;
                case ActorState.Confronted: return ConfrontedStateString;
            }
        }

        ///// <summary>
        ///// Converts a state to its string representation.
        ///// </summary>
        ///// <returns>
        ///// The string representation (e.g., "active").
        ///// </returns>
        ///// <param name='state'>
        ///// The state (e.g., <c>ActorState.Active</c>).
        ///// </param>
        //public static string StateToString(ActorState state)
        //{
        //    switch (state)
        //    {
        //        case ActorState.Unassigned: return UnassignedStateString;
        //        case ActorState.Active: return ActiveStateString;
        //        case ActorState.Success: return SuccessStateString;
        //        case ActorState.Failure: return FailureStateString;
        //        case ActorState.Abandoned: return AbandonedStateString;
        //        case ActorState.Grantable: return GrantableStateString;
        //        case ActorState.ReturnToNPC: return ReturnToNPCStateString;
        //        default: return UnassignedStateString;
        //    }
        //}

        /// <summary>
        /// Gets the localized Actor display name.
        /// </summary>
        /// <returns>
        /// The Actor title (display name) in the current language.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        public static string GetActorName(string actorName)
        {
            //var name = DialogueLua.GetLocalizedActorField(actorName, "Display Name").asString;
            //if (string.IsNullOrEmpty(name)) name = DialogueLua.GetLocalizedActorField(actorName, "Name").asString;
            var name =  DialogueLua.GetLocalizedActorField(actorName, "Name").asString;
            return name;
        }

        /// <summary>
        /// Gets a Actor description, based on the current state of the Actor (i.e., SuccessDescription, FailureDescription, or just Description).
        /// </summary>
        /// <returns>
        /// The Actor description.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <example>
        /// GUILayout.Label("Objective: " + ActorLog.GetActorDescription("Kill 5 Rats"));
        /// </example>
        public static string GetActorDescription(string actorName)
        {
            switch (GetActorState(actorName))
            {
                case ActorState.Amicable:
                    return GetActorDescription(actorName, ActorState.Amicable) ?? GetActorDescription(actorName, ActorState.Mentioned);
                case ActorState.Botched:
                    return GetActorDescription(actorName, ActorState.Botched) ?? GetActorDescription(actorName, ActorState.Mentioned);
                default:
                    return GetActorDescription(actorName, ActorState.Mentioned);
            }
        }
        
        public static string GetActorImageAddress(string actorName, string fieldName = "Compendium Art")
        {
            var spriteAddress = DialogueLua.GetLocalizedActorField(actorName, fieldName).asString;
            return spriteAddress;
        }
        
        

        /// <summary>
        /// Gets the localized Actor description for a specific state.
        /// </summary>
        /// <returns>
        /// The Actor description.
        /// </returns>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='state'>
        /// State to check.
        /// </param>
        public static string GetActorDescription(string actorName, ActorState state)
        {
            string descriptionFieldName = GetDefaultDescriptionFieldForState(state);
            string result = DialogueLua.GetLocalizedActorField(actorName, descriptionFieldName).asString;
            return (string.Equals(result, "nil") || string.IsNullOrEmpty(result)) ? null : result;
        }

        private static string GetDefaultDescriptionFieldForState(ActorState state)
        {
            switch (state)
            {
                case ActorState.Amicable:
                    return "Amicable_Description";
                case ActorState.Botched:
                    return "Botched_Description";
                default:
                    return "Description";
            }
        }

        /// <summary>
        /// Sets the Actor description for a specified state.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='state'>
        /// Set the description for this state (i.e., regular, success, or failure).
        /// </param>
        /// <param name='description'>
        /// The description.
        /// </param>
        public static void SetActorDescription(string actorName, ActorState state, string description)
        {
            if (DialogueLua.DoesTableElementExist("Actor", actorName))
            {
                DialogueLua.SetActorField(actorName, GetDefaultDescriptionFieldForState(state), description);
            }
        }

        /// <summary>
        /// Gets the Actor abandon sequence. The ActorLogWindow plays this sequence when the player
        /// abandons a Actor.
        /// </summary>
        /// <returns>The Actor abandon sequence.</returns>
        /// <param name="actorName">Actor name.</param>
        public static string GetActorAbandonSequence(string actorName)
        {
            return DialogueLua.GetLocalizedActorField(actorName, "Abandon Sequence").asString;
        }

        /// <summary>
        /// Sets the Actor abandon sequence. The ActorLogWindow plays this sequence when the 
        /// player abandons a Actor.
        /// </summary>
        /// <param name="actorName">Actor name.</param>
        /// <param name="sequence">Sequence to play when the Actor is abandoned.</param>
        public static void SetActorAbandonSequence(string actorName, string sequence)
        {
            DialogueLua.SetLocalizedActorField(actorName, "Abandon Sequence", sequence);
        }

   

      

        /// <summary>
        /// Determines if a Actor is abandonable (that is, is has a field named "Abandonable" that's true.)
        /// </summary>
        /// <returns><c>true</c> if the Actor is abandonable; otherwise, <c>false</c>.</returns>
        /// <param name="actorName">Actor name.</param>
        public static bool IsActorAbandonable(string actorName)
        {
            return DialogueLua.GetActorField(actorName, "Abandonable").asBool;
        }

        /// <summary>
        /// Returns true if Actor has a field named "Notable" that is currently true or doesn't have the field.
        /// </summary>
        public static bool IsActorNotable(string actorName)
        {
            var result = Lua.Run($"return Actor[{DialogueLua.StringToTableIndex(actorName)}].Notable").asString;
            if (string.IsNullOrEmpty(result) || string.Equals(result, "nil")) return true;
            return string.Compare(result, "false", true) == 0;
        }

        /// <summary>
        /// Sets a Actor's Notable field true or false.
        /// </summary>
        public static void SetActorVisibility(string actorName)
        {
            if (DialogueLua.DoesTableElementExist("Actor", actorName))
            {
                DialogueLua.SetActorField(actorName, "Notable", true);
            }
        }

        /// <summary>
        /// Returns true if Actor has a field named "Viewed" that is currently true.
        /// Used if ActorLogWindow.newActorText is not blank.
        /// </summary>
        public static bool WasActorViewed(string actorName)
        {
            return DialogueLua.GetActorField(actorName, "Viewed").asBool;
        }

        /// <summary>
        /// Marks a Actor as viewed (i.e., in the Actor log window).
        /// Generally only set/used when ActorLogWindow.newActorText is not blank.
        /// </summary>
        /// <param name="actorName"></param>
        public static void MarkActorViewed(string actorName)
        {
            if (DialogueLua.DoesTableElementExist("Actor", actorName))
            {
                DialogueLua.SetActorField(actorName, "Viewed", true);
            }
        }

        /// <summary>
        /// Gets the group that a Actor belongs to.
        /// </summary>
        /// <returns>The Actor group name, or empty string if no group.</returns>
        /// <param name="actorName">Actor name.</param>
        public static string GetActorGroup(string actorName)
        {
            return DialogueLua.GetLocalizedActorField(actorName, "Group").asString;
        }

        public static string GetActorGroupDisplayName(string actorName)
        {
            var result = DialogueLua.GetLocalizedActorField(actorName, "Group Display Name").asString;
            if (string.IsNullOrEmpty(result) || result == "nil") result = GetActorGroup(actorName);
            return result;
        }

        /// <summary>
        /// Gets all Actor group names.
        /// </summary>
        /// <returns>The group names for active Actors, sorted by name.</returns>
        public static string[] GetAllGroups()
        {
            return GetAllGroups(ActorState.Mentioned, true);
        }

        /// <summary>
        /// Gets all Actor group names.
        /// </summary>
        /// <returns>The group names, sorted by name.</returns>
        /// <param name="flags">Flags for the Actor states to filter.</param>
        public static string[] GetAllGroups(ActorState flags)
        {
            return GetAllGroups(flags, true);
        }

        /// <summary>
        /// Gets all Actor group names.
        /// </summary>
        /// <returns>The group names.</returns>
        /// <param name="flags">Flags for the Actor states to filter.</param>
        /// <param name="sortByGroupName">If set to <c>true</c> sort by group name.</param>
        public static string[] GetAllGroups(ActorState flags, bool sortByGroupName)
        {
            List<string> groups = new List<string>();
            LuaTableWrapper actorTable = Lua.Run("return Actor").asTable;
            if (!actorTable.isValid)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Actor Log couldn't access Lua Item[] table. Has the Dialogue Manager loaded a database yet?", new System.Object[] { DialogueDebug.Prefix }));
                return groups.ToArray();
            }
            foreach (var actorTableValue in actorTable.values)
            {
                LuaTableWrapper fields = actorTableValue as LuaTableWrapper;
                if (fields == null) continue;
                string actorName = null;
                string group = null;
                try
                {
                    object actorNameObject = fields["Name"];
                    actorName = (actorNameObject != null) ? actorNameObject.ToString() : string.Empty;
                    object groupObject = fields["Group"];
                    group = (groupObject != null) ? groupObject.ToString() : string.Empty;
                   
                  
                    
                }
                catch { }
                
                if (!groups.Contains(group) && IsActorInStateMask(actorName, flags))
                {
                    groups.Add(group);
                }
            
            }
            if (sortByGroupName) groups.Sort();
            return groups.ToArray();
        }

        /// <summary>
        /// Gets an array of all active Actors.
        /// </summary>
        /// <returns>
        /// The names of all active Actors, sorted by Name.
        /// </returns>
        /// <example>
        /// string[] activeActors = ActorLog.GetAllActors();
        /// </example>
        public static string[] GetAllActors()
        {
            return GetAllActors(ActorState.Mentioned, true, null);
        }

        /// <summary>
        /// Gets an array of all Actors matching the specified state bitmask.
        /// </summary>
        /// <returns>The names of all Actors matching the specified state bitmask, sorted by Name.</returns>
        /// <param name="flags">A bitmask of ActorState values.</param>
        /// <example>
        /// string[] completedActors = ActorLog.GetAllActors( ActorState.Success | ActorState.Failure );
        /// </example>
        public static string[] GetAllActors(ActorState flags)
        {
            return GetAllActors(flags, true, null);
        }

        /// <summary>
        /// Gets an array of all Actors matching the specified state bitmask.
        /// </summary>
        /// <returns>The names of all Actors matching the specified state bitmask.</returns>
        /// <param name='flags'>A bitmask of ActorState values.</param>
        /// <param name='sortByName'>If `true`, sorts the names by name.</param>
        /// <example>
        /// string[] completedActors = ActorLog.GetAllActors( ActorState.Success | ActorState.Failure, true );
        /// </example>
        public static string[] GetAllActors(ActorState flags, bool sortByName)
        {
            return GetAllActors(flags, sortByName, null);
        }

        /// <summary>
        /// Gets an array of all Actors matching the specified state bitmask and in the specified group.
        /// </summary>
        /// <returns>The names of all Actors matching the specified state bitmask.</returns>
        /// <param name='flags'>A bitmask of ActorState values.</param>
        /// <param name='sortByName'>If `true`, sorts the names by name.</param>
        /// <param name='group'>If not null, return only Actors in the specified group.</param>
        /// <example>
        /// string[] completedActors = ActorLog.GetAllActors( ActorState.Success | ActorState.Failure, true );
        /// </example>
        public static string[] GetAllActors(ActorState flags, bool sortByName, string group)
        {
            List<string> actorNames = new List<string>();
            LuaTableWrapper actorTable = Lua.Run("return Actor").asTable;
            if (!actorTable.isValid)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Actor Compendium couldn't access Lua Actor[] table. Has the Dialogue Manager loaded a database yet?", new System.Object[] { DialogueDebug.Prefix }));
                return actorNames.ToArray();
            }
            var filterGroup = (group != null);
            foreach (var actorTableValue in actorTable.values)
            {
                LuaTableWrapper fields = actorTableValue as LuaTableWrapper;
                if (fields == null) continue;
                string actorName = null;
                string thisGroup = null;
               
                try
                {
                    object actorNameObject = fields["Name"];
                    actorName = (actorNameObject != null) ? actorNameObject.ToString() : string.Empty;
                    if (filterGroup)
                    {
                        object groupObject = fields["Group"];
                        thisGroup = (groupObject != null) ? groupObject.ToString() : string.Empty;
                    }
                
                    
                    
                }
                catch { }
                
                if (string.IsNullOrEmpty(actorName))
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: A Actor name (actor name in Actor[] table) is null or empty", new System.Object[] { DialogueDebug.Prefix }));
                }
                else if (!filterGroup || string.Equals(group, thisGroup))
                {
                    if (IsActorInStateMask(actorName, flags))
                    {
                        actorNames.Add(actorName);
                    }
                }
            }
            if (sortByName) actorNames.Sort();
            return actorNames.ToArray();
        }

        /// <summary>
        /// Gets all Actors (including their group names) in a specified state.
        /// </summary>
        /// <returns>An array of ActorGroupRecord elements.</returns>
        /// <param name="flags">A bitmask of ActorState values.</param>
        /// <param name="flags">Sort by group and name.</param>
        public static ActorGroupRecord[] GetAllGroupsAndActors(ActorState flags, bool sort = true)
        {
            List<ActorGroupRecord> list = new List<ActorGroupRecord>();
            LuaTableWrapper actorTable = Lua.Run("return Actor").asTable;
            if (!actorTable.isValid)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Actor Log couldn't access Lua Item[] table. Has the Dialogue Manager loaded a database yet?", new System.Object[] { DialogueDebug.Prefix }));
                return list.ToArray();
            }
            foreach (var actorTableValue in actorTable.values)
            {
                LuaTableWrapper fields = actorTableValue as LuaTableWrapper;
                if (fields == null) continue;
                string actorName = null;
                string group = null;
                
                try
                {
                    object actorNameObject = fields["Name"];
                    actorName = (actorNameObject != null) ? actorNameObject.ToString() : string.Empty;
                    object groupObject = fields["Group"];
                    group = (groupObject != null) ? groupObject.ToString() : string.Empty;

                }
                catch { }
                    if (string.IsNullOrEmpty(actorName))
                    {
                        if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: A Actor name (actor name in Actor[] table) is null or empty", new System.Object[] { DialogueDebug.Prefix }));
                    }
                    else if (IsActorInStateMask(actorName, flags))
                    {
                        list.Add(new ActorGroupRecord(group, actorName));
                    }
               
            }
            if (sort) list.Sort();
            return list.ToArray();
        }

        /// <summary>
        /// Actor changed delegate.
        /// </summary>
        public delegate void ActorChangedDelegate(string actorName, ActorState newState);

        /// <summary>
        /// The Actor watch item class is used internally by the ActorLog class to manage
        /// Lua observers on Actor states.
        /// </summary>
        public class ActorWatchItem
        {

            private string actorName;
            private int entryNumber;
            private LuaWatchFrequency frequency;
            private string luaExpression;
            private ActorChangedDelegate ActorChangedHandler;

            public ActorWatchItem(string actorName, LuaWatchFrequency frequency, ActorChangedDelegate ActorChangedHandler)
            {
                this.actorName = actorName;
              
                this.frequency = frequency;
                this.luaExpression = string.Format("return Actor[\"{0}\"].State", new System.Object[] { DialogueLua.StringToTableIndex(actorName) });
                this.ActorChangedHandler = ActorChangedHandler;
                DialogueManager.AddLuaObserver(luaExpression, frequency, OnLuaChanged);
            }

      

            public bool Matches(string actorName, LuaWatchFrequency frequency, ActorChangedDelegate ActorChangedHandler)
            {
                return string.Equals(actorName, this.actorName) && (frequency == this.frequency) && (ActorChangedHandler == this.ActorChangedHandler);
            }

    
            public void StopObserving()
            {
                DialogueManager.RemoveLuaObserver(luaExpression, frequency, OnLuaChanged);
            }

            private void OnLuaChanged(LuaWatchItem luaWatchItem, Lua.Result newResult)
            {
                if (string.Equals(luaWatchItem.luaExpression, this.luaExpression) && (ActorChangedHandler != null))
                {
                    ActorChangedHandler(actorName, StringToState(newResult.asString));
                }
            }
        }

        /// <summary>
        /// The Actor watch list.
        /// </summary>
        private static readonly List<ActorWatchItem> ActorWatchList = new List<ActorWatchItem>();

        /// <summary>
        /// Adds a Actor state observer.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='frequency'>
        /// Frequency to check the Actor state.
        /// </param>
        /// <param name='ActorChangedHandler'>
        /// Delegate to call when the Actor state changes. This should be in the form:
        /// <code>void MyDelegate(string actorName, ActorState newState) {...}</code>
        /// </param>
        public static void AddActorStateObserver(string actorName, LuaWatchFrequency frequency, ActorChangedDelegate ActorChangedHandler)
        {
            ActorWatchList.Add(new ActorWatchItem(actorName, frequency, ActorChangedHandler));
        }

        /// <summary>
        /// Adds a Actor state observer.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='entryNumber'>
        /// The entry number (1...Entry Count) in the Actor.
        /// </param>
        /// <param name='frequency'>
        /// Frequency to check the Actor state.
        /// </param>
        /// <param name='ActorChangedHandler'>
        /// Delegate to call when the Actor state changes. This should be in the form:
        /// <code>void MyDelegate(string actorName, ActorState newState) {...}</code>
        /// </param>


        /// <summary>
        /// Removes a Actor state observer. To be removed, the actorName, frequency, and delegate must
        /// all match.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='frequency'>
        /// Frequency that the Actor state is being checked.
        /// </param>
        /// <param name='ActorChangedHandler'>
        /// Actor changed handler delegate.
        /// </param>
        public static void RemoveActorStateObserver(string actorName, LuaWatchFrequency frequency, ActorChangedDelegate ActorChangedHandler)
        {
            foreach (var ActorWatchItem in ActorWatchList)
            {
                if (ActorWatchItem.Matches(actorName, frequency, ActorChangedHandler)) ActorWatchItem.StopObserving();
            }
            ActorWatchList.RemoveAll(ActorWatchItem => ActorWatchItem.Matches(actorName, frequency, ActorChangedHandler));
        }

        /// <summary>
        /// Removes a Actor state observer. To be removed, the actorName, frequency, and delegate must
        /// all match.
        /// </summary>
        /// <param name='actorName'>
        /// Name of the Actor.
        /// </param>
        /// <param name='entryNumber'>
        /// The entry number (1...Entry Count) in the Actor.
        /// </param>
        /// <param name='frequency'>
        /// Frequency that the Actor state is being checked.
        /// </param>
        /// <param name='ActorChangedHandler'>
        /// Actor changed handler delegate.
        /// </param>


        /// <summary>
        /// Removes all Actor state observers.
        /// </summary>
        public static void RemoveAllActorStateObservers()
        {
            foreach (var ActorWatchItem in ActorWatchList)
            {
                ActorWatchItem.StopObserving();
            }
            ActorWatchList.Clear();
        }


        /// <summary>
        /// Updates all Actor state listeners who are listening for actorName.
        /// </summary>
        public static void UpdateActorIndicators(string actorName)
        {
            var dispatcher = GameObjectUtility.FindFirstObjectByType<ActorStateDispatcher>();
            if (dispatcher != null) dispatcher.OnActorStateChange(actorName);
        }

    }

}
