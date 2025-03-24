// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Editor.Utility.Attributes;
using UnityEngine;
using TextTable = PixelCrushers.Wrappers.TextTable;

namespace Project.Runtime.Scripts.ActorCompendium
{

    /// <summary>
    /// This is the abstract base class for actor log windows. You can implement a actor log
    /// window in any GUI system by creating a subclass.
    /// 
    /// When open, the window displays mentioned and completed actors. It gets the names, 
    /// descriptions, and states of the actors from the ActorCompendium class.
    /// 
    /// The window allows the player to abandon actors (if the actor's Abandonable field is
    /// true) and toggle tracking (if the actor's Trackable field is true).
    /// </summary>
    /// <remarks>
    /// If pauseWhileOpen is set to <c>true</c>, the actor log window pauses the game by setting 
    /// <c>Time.timeScale</c> to <c>0</c>. When closed, it restores the previous time scale.
    /// </remarks>
    public abstract class ActorCompendiumWindow : MonoBehaviour
    {

        [Tooltip("Optional localized text table to use to localize no mentioned/completed actors.")]
        public TextTable textTable = null; // v2: changed from LocalizedTextTable.

        [Tooltip("Text to show (or localize) when there are no shown actors")]
        public string noMentionedActorsText = "No characters discovered.";
        
        [Tooltip("Text to show (or localize) when there are no completed actor relationships.")]
        public string noCompletedActorsText = "No Completed Actor descriptions.";

        [Tooltip("Check if actor has a field named 'Notable'. If field is false, don't show actor.")]
        public bool checkNotableField = false;

        public enum ActorHeadingSource
        {
            /// <summary>
            /// Use the name of the item for the actor heading.
            /// </summary>
            Name,

            /// <summary>
            /// Use the item's Description field for the actor heading.
            /// </summary>
            Description
        };

        /// <summary>
        /// The a source.
        /// </summary>
        public ActorHeadingSource actorHeadingSource = ActorHeadingSource.Name;

        /// <summary>
        /// The state to assign abandoned actors.
        /// </summary>
        [Tooltip("State to assign to actors when player abandons then.")]
        [ActorState]
        public ActorState abandonActorState = ActorState.Unidentified;

        /// <summary>
        /// If <c>true</c>, the window sets <c>Time.timeScale = 0</c> to pause the game while 
        /// displaying the actor log window.
        /// </summary>
        public bool pauseWhileOpen = true;

        /// <summary>
        /// If <c>true</c>, the cursor is unlocked while the actor log window is open.
        /// </summary>
        public bool unlockCursorWhileOpen = true;

        /// <summary>
        /// If <c>true</c>, organize the actors by group.
        /// </summary>
        [Tooltip("Organize actors by the values of their Group fields.")]
        public bool useGroups = false;

        [Tooltip("If not blank, show this text next to actor names that haven't been viewed yet. Will be localized if text has entry in Dialogue Manager's Text Table.")]
        public string newActorText = string.Empty;

      

        [Tooltip("Clicking again on selected actor name deselects actor.")]
        public bool deselectActorOnSecondClick = true;

        [Serializable]
        public class ActorInfo
        {
            public string Group { get; set; }
            public string GroupDisplayName { get; set; }
            public string Name { get; set; }
            public FormattedText Heading { get; set; }
            public FormattedText Description { get; set; }
            
            public string SpriteAddress { get; set; }
            public bool Abandonable { get; set; }
            public ActorInfo(string group, string groupDisplayName, string name, FormattedText heading, FormattedText description, string spriteAddress, bool abandonable)
            {
                this.Group = group;
                this.GroupDisplayName = groupDisplayName;
                this.Name = name;
                this.Heading = heading;
                this.Description = description;
                this.SpriteAddress = spriteAddress;
                this.Abandonable = abandonable;
            }
            public ActorInfo(string group, string name, FormattedText heading, FormattedText description, string spriteAddress, bool abandonable)
            {
                this.Group = group;
                this.GroupDisplayName = string.Empty;
                this.Name = name;
                this.Heading = heading;
                this.Description = description;
                this.SpriteAddress = spriteAddress;
                this.Abandonable = abandonable;
            }
            public ActorInfo(string name, FormattedText heading, FormattedText description, string spriteAddress, bool abandonable)
            {
                this.Group = string.Empty;
                this.GroupDisplayName = string.Empty;
                this.Name = name;
                this.Heading = heading;
                this.Description = description;
                this.SpriteAddress = spriteAddress;
                this.Abandonable = abandonable;
            }
        }

        /// <summary>
        /// Indicates whether the actor log window is currently open.
        /// </summary>
        /// <value>
        /// <c>true</c> if open; otherwise, <c>false</c>.
        /// </value>
        public bool isOpen { get; protected set; }

        /// <summary>
        /// The current list of actors. This will change based on whether the player is
        /// viewing mentioned or completed actors.
        /// </summary>
        /// <value>The actors.</value>
        public ActorInfo[] actors { get; protected set; }

        /// <summary>
        /// The current list of actor groups.
        /// </summary>
        /// <value>The actor group names.</value>
        public string[] groups { get; protected set; }

        /// <summary>
        /// The name of the currently-selected actor.
        /// </summary>
        /// <value>The selected actor.</value>
        public string selectedActor { get; protected set; }

        /// <summary>
        /// The message to show if Actors[] is empty.
        /// </summary>
        /// <value>The no actors message.</value>
        public string noActorsMessage { get; protected set; }

        /// <summary>
        /// Indicates whether the window is showing mentioned actors or completed actors.
        /// </summary>
        /// <value><c>true</c> if showing mentioned actors; otherwise, <c>false</c>.</value>
        public virtual bool isShowingMentionedActors { get { return currentActorStateMask == MentionedActorStateMask; } }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsOpen { get { return isOpen; } protected set { isOpen = value; } }
        public ActorInfo[] Actors { get { return actors; } protected set { actors = value; } }
        public string[] Groups { get { return groups; } protected set { groups = value; } }
        public string SelectedActor { get { return selectedActor; } protected set { selectedActor = value; } }
        public string NoActorsMessage { get { return noActorsMessage; } protected set { noActorsMessage = value; } }
        public bool IsShowingMentionedActors { get { return isShowingMentionedActors; } }
        /// @endcond

        protected const ActorState MentionedActorStateMask = ActorState.Mentioned | ActorState.Confronted;

        /// <summary>
        /// The current actor state mask.
        /// </summary>
        protected ActorState currentActorStateMask = MentionedActorStateMask;

        /// <summary>
        /// The previous time scale prior to opening the window.
        /// </summary>
        protected float previousTimeScale = 1;

        protected Coroutine refreshCoroutine = null;

        protected bool started = false;

        public virtual void Awake()
        {
            isOpen = false;
            actors = new ActorInfo[0];
            groups = new string[0];
            selectedActor = string.Empty;
            noActorsMessage = string.Empty;
        }

        protected virtual void Start()
        {
            started = true;
            RegisterForUpdateTrackerEvents();
        }

        protected virtual void OnEnable()
        {
            if (started) RegisterForUpdateTrackerEvents();
        }

        protected virtual void OnDisable()
        {
            refreshCoroutine = null;
            UnregisterFromUpdateTrackerEvents();
        }

        protected void RegisterForUpdateTrackerEvents()
        {
            if (!started || DialogueManager.instance == null) return;
            if (GetComponentInParent<DialogueSystemController>() != null) return; // Children of Dialogue Manager automatically receive UpdateTracker; no need to register.
            DialogueManager.instance.receivedUpdateTracker -= UpdateTracker;
            DialogueManager.instance.receivedUpdateTracker += UpdateTracker;
        }

        protected void UnregisterFromUpdateTrackerEvents()
        {
            if (!started || DialogueManager.instance == null) return;
            DialogueManager.instance.receivedUpdateTracker -= UpdateTracker;
        }

        /// <summary>
        /// Opens the window. Your implementation should override this to handle any
        /// window-opening activity, then call openedWindowHandler at the end.
        /// </summary>
        /// <param name="openedWindowHandler">Opened window handler.</param>
        public virtual void OpenWindow(Action openedWindowHandler)
        {
            openedWindowHandler();
        }

        /// <summary>
        /// Closes the window. Your implementation should override this to handle any
        /// window-closing activity, then call closedWindowHandler at the end.
        /// </summary>
        /// <param name="openedWindowHandler">Closed window handler.</param>
        public virtual void CloseWindow(Action closedWindowHandler)
        {
            
            closedWindowHandler();
        }
        
        /// <summary>
        /// Gets the actor's image address for use in the compendium window.
        /// </summary>
        
       
        

        /// <summary>
        /// Called when the actor list has been updated -- for example, when switching between
        /// mentioned and completed actors. Your implementation may override this to do processing.
        /// </summary>
        public virtual void OnActorListUpdated() { }

        /// <summary>
        /// Asks the player to confirm abandonment of a actor. Your implementation should override
        /// this to show a modal dialogue box or something similar. If confirmed, it should call
        /// confirmedAbandonActorHandler.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="confirmedAbandonActorHandler">Confirmed abandon actor handler.</param>
        public virtual void ConfirmAbandonActor(string name, Action confirmedAbandonActorHandler) { }

        /// <summary>
        /// Opens the actor window.
        /// </summary>
        public virtual void Open()
        {
           
            PauseGameplay();
            OpenWindow(OnOpenedWindow);
        }

        protected virtual void OnOpenedWindow()
        {
            isOpen = true;
            ShowActors(currentActorStateMask);
        }

        /// <summary>
        /// Closes the actor log window. While you can call this manually in your own script, this
        /// method is normally called internally when the player clicks the close button. You can 
        /// call it manually to support alternate methods of closing the window.
        /// </summary>
        /// <example>
        /// if (Input.GetKeyDown(KeyCode.L) && myActorCompendiumWindow.IsOpen) {
        ///     myActorCompendiumWindow.Close();
        /// }
        /// </example>
        public virtual void Close()
        {
            //--- No need to clear it: selectedActor = string.Empty;
            CloseWindow(OnClosedWindow);
        }

        protected virtual void OnClosedWindow()
        {
            isOpen = false;
            ResumeGameplay();
        }

        private bool wasCursorActive = false;

        protected virtual void PauseGameplay()
        {
            if (pauseWhileOpen)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            if (unlockCursorWhileOpen)
            {
                wasCursorActive = Tools.IsCursorActive();
                Tools.SetCursorActive(true);
            }
        }

        protected virtual void ResumeGameplay()
        {
            if (pauseWhileOpen) Time.timeScale = previousTimeScale;
            if (unlockCursorWhileOpen && !wasCursorActive) Tools.SetCursorActive(false);
        }

        public virtual bool IsActorNotable(string actorName)
        {
            return !checkNotableField || Lua.IsTrue("Actor[\"" + DialogueLua.StringToTableIndex(actorName) + "\"].Notable == true");
        }

        protected virtual void ShowActors(ActorState actorStateMask)
        {
            currentActorStateMask = actorStateMask;
            noActorsMessage = GetNoActorsMessage(actorStateMask);
            List<ActorInfo> actorList = new List<ActorInfo>();
            if (useGroups)
            {
                var records = ActorCompendium.GetAllGroupsAndActors(actorStateMask, true);
                foreach (var record in records)
                {
                    if (!IsActorNotable(record.actorName)) continue;
                    actorList.Add(GetActorInfo(record.groupName, record.actorName));
                }
            }
            else
            {
                string[] names = ActorCompendium.GetAllActors(actorStateMask, true, null);
                foreach (var name in names)
                {
                    if (!IsActorNotable(name)) continue;
                    actorList.Add(GetActorInfo(string.Empty, name));
                }
            }
            actors = actorList.ToArray();
            OnActorListUpdated();
        }

        protected virtual ActorInfo GetActorInfo(string group, string name)
        {            
            FormattedText description = FormattedText.Parse(ActorCompendium.GetActorDescription(name), DialogueManager.masterDatabase.emphasisSettings);
            FormattedText localizedName = FormattedText.Parse(ActorCompendium.GetActorName(name), DialogueManager.masterDatabase.emphasisSettings);
            FormattedText heading = (actorHeadingSource == ActorHeadingSource.Description) ? description : localizedName;
            string localizedGroup = string.IsNullOrEmpty(group) ? string.Empty : ActorCompendium.GetActorGroup(name);
            string localizedGroupDisplayName = string.IsNullOrEmpty(group) ? string.Empty : ActorCompendium.GetActorGroupDisplayName(name);
            bool abandonable = ActorCompendium.IsActorAbandonable(name) && isShowingMentionedActors;
            string spriteAddress = ActorCompendium.GetActorImageAddress(name);
          
     

            // Check if need to show [new]:
            if (!string.IsNullOrEmpty(newActorText))
            {
                if (!ActorCompendium.WasActorViewed(name))
                {
                    heading.text += " " + FormattedText.Parse(DialogueManager.GetLocalizedText(newActorText)).text;
                }
            }

            return new ActorInfo(localizedGroup, localizedGroupDisplayName, name, heading, description, spriteAddress, abandonable);
        }

        /// <summary>
        /// Gets the "no actors" message for a actor state (mentioned or success|failure). This
        /// method uses the strings "No Mentioned Actors" and "No Completed Actors" or their
        /// localized equivalents if you've set the localized text table.
        /// </summary>
        /// <returns>The "no actors" message.</returns>
        /// <param name="actorStateMask">Actor state mask.</param>
        protected virtual string GetNoActorsMessage(ActorState actorStateMask)
        {
            return (actorStateMask == MentionedActorStateMask) ? GetLocalizedText(noMentionedActorsText) : GetLocalizedText(noCompletedActorsText);
        }

        /// <summary>
        /// Gets the localized text for a field name.
        /// </summary>
        /// <returns>The localized text.</returns>
        /// <param name="fieldName">Field name.</param>
        public virtual string GetLocalizedText(string fieldName)
        {
            if ((textTable != null) && textTable.HasFieldTextForLanguage(fieldName, Localization.GetCurrentLanguageID(textTable)))
            {
                return textTable.GetFieldTextForLanguage(fieldName, Localization.GetCurrentLanguageID(textTable));
            }
            else
            {
                return DialogueManager.GetLocalizedText(fieldName);
            }
        }

        /// <summary>
        /// Determines whether the specified actorInfo is for the currently-selected actor.
        /// </summary>
        /// <returns><c>true</c> if this is the selected actor; otherwise, <c>false</c>.</returns>
        /// <param name="actorInfo">Actor info.</param>
        public virtual bool IsSelectedActor(ActorInfo actorInfo)
        {
            return string.Equals(actorInfo.Name, selectedActor);
        }

        /// <summary>
        /// Your GUI close button should call this.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public void ClickClose(object data)
        {
            Close();
        }

        /// <summary>
        /// Your GUI "show mentioned actors" button should call this.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public virtual void ClickShowMentionedActors(object data)
        {
            ShowActors(MentionedActorStateMask);
        }

        /// <summary>
        /// Your GUI "show completed actors" button should call this.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public virtual void ClickShowCompletedActors(object data)
        {
            ShowActors(ActorState.Amicable | ActorState.Botched);
        }

        /// <summary>
        /// Your GUI should call this when the player clicks on a actor to expand
        /// or close it.
        /// </summary>
        /// <param name="data">The actor name.</param>
        public virtual void ClickActor(object data)
        {
            if (!IsString(data)) return;
            string clickedActor = (string)data;
            selectedActor = (deselectActorOnSecondClick && string.Equals(selectedActor, clickedActor)) ? string.Empty : clickedActor;

            // Mark viewed:
            if (!string.IsNullOrEmpty(newActorText) && !string.IsNullOrEmpty(selectedActor))
            {
                ActorCompendium.MarkActorViewed(selectedActor);
                foreach (var actor in actors)
                {
                    if (IsSelectedActor(actor))
                    {
                        var newActorInfo = GetActorInfo(actor.Group, actor.Name);
                        actor.Heading = newActorInfo.Heading;
                        break;
                    }
                }
            }

            OnActorListUpdated();
        }

        /// <summary>
        /// Your GUI should call this when the player clicks to abandon a actor.
        /// </summary>
        /// <param name="data">Ignored.</param>
        public virtual void ClickAbandonActor(object data)
        {
            if (string.IsNullOrEmpty(selectedActor)) return;
            ConfirmAbandonActor(selectedActor, OnConfirmAbandonActor);
        }

        /// <summary>
        /// Your GUI should call this when the player confirms abandonment of a actor.
        /// </summary>
        protected virtual void OnConfirmAbandonActor()
        {
            ActorCompendium.SetActorState(selectedActor, abandonActorState);
            selectedActor = string.Empty;
            ShowActors(currentActorStateMask);
            string sequence = ActorCompendium.GetActorAbandonSequence(selectedActor);
            if (!string.IsNullOrEmpty(sequence)) DialogueManager.PlaySequence(sequence);
        }



        private bool IsString(object data)
        {
            return (data != null) && (data.GetType() == typeof(string));
        }

        // Parameter-less versions of methods for GUI systems that require them for button hookups:
        public virtual void ClickShowMentionedActorsButton()
        {
            ClickShowMentionedActors(null);
        }

        public void ClickShowCompletedActorsButton()
        {
            ClickShowCompletedActors(null);
        }

        public void ClickCloseButton()
        {
            ClickClose(null);
        }

        public void ClickAbandonActorButton()
        {
            ClickAbandonActor(null);
        }


        public void UpdateTracker()
        {
            if (isOpen)
            {
                if (refreshCoroutine == null)
                {
                    refreshCoroutine = StartCoroutine(UpdateActorDisplayAtEndOfFrame());
                }
            }
        }

        protected IEnumerator UpdateActorDisplayAtEndOfFrame()
        {
            yield return CoroutineUtility.endOfFrame;
            refreshCoroutine = null;
            ShowActors(currentActorStateMask);
        }

    }

}
