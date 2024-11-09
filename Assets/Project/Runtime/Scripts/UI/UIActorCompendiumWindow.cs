// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.ActorCompendium
{

    /// <summary>
    /// This is the Standard UI implementation of the abstract ActorCompendiumWindow class.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UIActorCompendiumWindow : ActorCompendiumWindow, IEventSystemUser
    {

        #region Serialized Fields

        [Header("Main Panel")]

        public UIPanel mainPanel;
        public UITextField showingMentionedActorsHeading;
        public UITextField showingCompletedActorHeading;
        [Tooltip("Button to switch display to mentioned actors.")]
        public Button mentionedActorsButton;
        [Tooltip("Button to switch display to completed actors.")]
        public Button completedActorsButton;

        [Header("Selection Panel")]

        public RectTransform actorSelectionContentContainer;
        public StandardUIFoldoutTemplate actorGroupTemplate;
        [Tooltip("Use this template for mentioned actors.")]
        public UIActorNameButtonTemplate mentionedActorHeadingTemplate;
        [Tooltip("Use this template for the currently-selected mentioned actor.")]
        public UIActorNameButtonTemplate selectedMentionedActorHeadingTemplate;
        [Tooltip("Use this template for completed actors.")]
        public UIActorNameButtonTemplate completedActorHeadingTemplate;
        [Tooltip("Use this template for the currently-selected completed actor.")]
        public UIActorNameButtonTemplate selectedCompletedActorHeadingTemplate;
        [Tooltip("If there are no actors to show, show the No Mentioned/Completed Actors Text above.")]
        public bool showNoActorsText = true;
        [Tooltip("Select first actor in list when open. If unticked and Always Auto Focus is ticked, selects button assigned to main panel's First Selected field (Close button).")]
        public bool selectFirstActorOnOpen = false;
        [Tooltip("Show details when actor button is selected (highlighted/hovered), not when clicked.")]
        public bool showDetailsOnSelect = false;
        [Tooltip("Keep all groups expanded.")]
        public bool keepGroupsExpanded = false;

        [Header("Details Panel")]

        public RectTransform actorDetailsContentContainer;
        public StandardUITextTemplate actorHeadingTextTemplate;
        public StandardUITextTemplate actorDescriptionTextTemplate;
       
        public StandardUIButtonTemplate abandonButtonTemplate;
        
        public Image selectedActorImage;

        [Header("Abandon Actor Panel")]

        public UIPanel abandonActorPanel;
        public UITextField abandonActorNameText;

        [Header("Events")]
        public UnityEvent onOpen = new UnityEvent();
        public UnityEvent onClose = new UnityEvent();

        [Tooltip("Add an EventSystem if one isn't in the scene.")]
        public bool addEventSystemIfNeeded = true;

        public Button button;

        #endregion

        #region Runtime Properties

        private StandardUIInstancedContentManager m_selectionPanelContentManager = new StandardUIInstancedContentManager();
        protected StandardUIInstancedContentManager selectionPanelContentManager
        {
            get { return m_selectionPanelContentManager; }
            set { m_selectionPanelContentManager = value; }
        }

        private StandardUIInstancedContentManager m_detailsPanelContentManager = new StandardUIInstancedContentManager();
        protected StandardUIInstancedContentManager detailsPanelContentManager
        {
            get { return m_detailsPanelContentManager; }
            set { m_detailsPanelContentManager = value; }
        }

        private EventSystem m_eventSystem = null;
        public EventSystem eventSystem
        {
            get
            {
                if (m_eventSystem != null) return m_eventSystem;
                return EventSystem.current;
            }
            set { m_eventSystem = value; }
        }

        protected List<string> expandedGroupNames = new List<string>();
        protected Action confirmAbandonActorHandler = null;
        protected string mostRecentSelectedMentionedActor = null;
        protected string mostRecentSelectedCompletedActor = null;
        private Coroutine m_refreshCoroutine = null;
        private bool m_isAwake = false;

        #endregion

        #region Initialization

        public override void Awake()
        {
            m_isAwake = true;
            base.Awake();
            if (addEventSystemIfNeeded) UITools.RequireEventSystem();
            InitializeTemplates();
        }

        protected virtual void InitializeTemplates()
        {
            if (DialogueDebug.logWarnings)
            {
                if (mainPanel == null) Debug.LogWarning("Dialogue System: Main Panel is unassigned.", this);
                if (actorSelectionContentContainer == null) Debug.LogWarning("Dialogue System: Actor Selection Content Container is unassigned.", this);
                if (actorGroupTemplate == null) Debug.LogWarning("Dialogue System: Actor Group Template is unassigned.", this);
                if (mentionedActorHeadingTemplate == null) Debug.LogWarning("Dialogue System: Mentioned Actor Name Template is unassigned.", this);
                if (completedActorHeadingTemplate == null) Debug.LogWarning("Dialogue System: Completed Actor Name Template is unassigned.", this);
                if (actorDetailsContentContainer == null) Debug.LogWarning("Dialogue System: Actor Details Content Container is unassigned.", this);
                if (actorHeadingTextTemplate == null) Debug.LogWarning("Dialogue System: Actor Heading Text Template is unassigned.", this);
                if (actorDescriptionTextTemplate == null) Debug.LogWarning("Dialogue System: Actor Body Text Template is unassigned.", this);
                if (abandonActorPanel == null) Debug.LogWarning("Dialogue System: Abandon Actor Panel is unassigned.", this);
                if (abandonActorNameText == null) Debug.LogWarning("Dialogue System: Abandon Actor Name Text is unassigned.", this);
            }
            
            Tools.SetGameObjectActive(actorGroupTemplate, false);
            Tools.SetGameObjectActive(mentionedActorHeadingTemplate, false);
            Tools.SetGameObjectActive(completedActorHeadingTemplate, false);
            Tools.SetGameObjectActive(selectedMentionedActorHeadingTemplate, false);
            Tools.SetGameObjectActive(selectedCompletedActorHeadingTemplate, false);
            Tools.SetGameObjectActive(actorHeadingTextTemplate, false);
            Tools.SetGameObjectActive(actorDescriptionTextTemplate, false);
         
            Tools.SetGameObjectActive(abandonButtonTemplate, false);
        }

        #endregion

        #region Show & Hide

        /// <summary>
        /// Open the window by showing the main panel.
        /// </summary>
        /// <param name="openedWindowHandler">Opened window handler.</param>
        public override void OpenWindow(Action openedWindowHandler)
        {
            mainPanel.Open();
            openedWindowHandler();
            onOpen.Invoke();
        }

        /// <summary>
        /// Close the window by hiding the main panel. Re-enable the bark UI.
        /// </summary>
        /// <param name="closedWindowHandler">Closed window handler.</param>
        public override void CloseWindow(Action closedWindowHandler)
        {
            closedWindowHandler();
            mainPanel.Close();
            onClose.Invoke();
        }
        
        public void HideIfOpen()
        {
            if (IsOpen)
            {
                isOpen = false;
                mainPanel.Close();
            }
        }
        
        public void CloseIfOpen()
        {
            if (IsOpen)
            {
                var closeWindowDelegate = new Action(() => isOpen = false);
                mainPanel.Close();
                onClose.Invoke();
                CloseWindow(closeWindowDelegate);
            }
        }

        private Color _defaultButtonColor = Color.clear;

        public void KeepIconHighlighted(bool keep)
        {
            if (button == null) return;
            var colors = button.colors;
            if (keep)
            {
                _defaultButtonColor = colors.normalColor;
                colors.normalColor = colors.highlightedColor;
            }
            else
            {
                if (_defaultButtonColor != Color.clear ) colors.normalColor = _defaultButtonColor;
            }
            button.colors = colors;
        }

        protected override void PauseGameplay()
        {
            if (Time.timeScale == 0) return;
            base.PauseGameplay();
        }


        public virtual void Toggle()
        {
            if (isOpen) Close(); else Open();
        }

        /// <summary>
        /// True if the group is expanded in the UI.
        /// </summary>
        public virtual bool IsGroupExpanded(string groupName)
        {
            return keepGroupsExpanded || expandedGroupNames.Contains(groupName);
        }

        /// <summary>
        /// Toggles whether a group is expanded or not.
        /// </summary>
        /// <param name="groupName">Group to toggle.</param>
        public virtual void ToggleGroup(string groupName)
        {
            if (IsGroupExpanded(groupName))
            {
                expandedGroupNames.Remove(groupName);
            }
            else
            {
                expandedGroupNames.Add(groupName);
            }
        }

        protected void SetStateToggleButtons()
        {
            if (mentionedActorsButton != null) mentionedActorsButton.interactable = !isShowingMentionedActors;
            if (completedActorsButton != null) completedActorsButton.interactable = isShowingMentionedActors;
        }

        public virtual void Repaint()
        {
            if (!isOpen) return;
            if (m_refreshCoroutine == null) m_refreshCoroutine = StartCoroutine(RefreshAtEndOfFrame());
        }

        private IEnumerator RefreshAtEndOfFrame()
        {
            // Wait until end of frame so we only refresh once in case we receive multiple
            // reactors to refresh during the same frame.
            yield return CoroutineUtility.endOfFrame;
            m_refreshCoroutine = null;
            OnActorListUpdated();
        }

        public string foldoutToSelect = null;
        public string actorNameToSelect = null;

        public override void OnActorListUpdated()
        {
            if (!m_isAwake) return;
            Selectable elementToSelect = null;
            showingMentionedActorsHeading.SetActive(isShowingMentionedActors);
            showingCompletedActorHeading.SetActive(!isShowingMentionedActors);
            selectionPanelContentManager.Clear();

            // Get group names, and draw selected actor in its panel while we're at it:
            var groupNames = new List<string>();
            var groupDisplayNames = new Dictionary<string, string>();
            int numGroupless = 0;
            var repaintedActorDetails = false;
            if (actors.Length > 0)
            {
                foreach (var actor in actors)
                {
                    if (IsSelectedActor(actor))
                    {
                        RepaintSelectedActor(actor);
                        repaintedActorDetails = true;
                    }
                    var groupName = actor.Group;
                    var groupDisplayName = string.IsNullOrEmpty(actor.GroupDisplayName) ? actor.Group : actor.GroupDisplayName;
                    if (string.IsNullOrEmpty(groupName)) numGroupless++;
                    if (string.IsNullOrEmpty(groupName) || groupNames.Contains(groupName)) continue;
                    groupNames.Add(groupName);
                    groupDisplayNames[groupName] = groupDisplayName;
                }
            }
            if (!repaintedActorDetails) RepaintSelectedActor(null);

            // Add actors by group:
            foreach (var groupName in groupNames)
            {
                var groupFoldout = selectionPanelContentManager.Instantiate<StandardUIFoldoutTemplate>(actorGroupTemplate);
                selectionPanelContentManager.Add(groupFoldout, actorSelectionContentContainer);
                groupFoldout.Assign(groupDisplayNames[groupName], IsGroupExpanded(groupName));
                var targetGroupName = groupName;
                var targetGroupFoldout = groupFoldout;
                if (!keepGroupsExpanded)
                {
                    groupFoldout.foldoutButton.onClick.AddListener(() => { OnClickGroup(targetGroupName, targetGroupFoldout); });
                }
                if (string.Equals(foldoutToSelect, groupName))
                {
                    elementToSelect = groupFoldout.foldoutButton;
                    foldoutToSelect = null;
                }
                foreach (var actor in actors)
                {
                    if (string.Equals(actor.Group, groupName))
                    {
                        var template = IsSelectedActor(actor)
                            ? GetSelectedActorNameTemplate(actor)
                            : GetActorNameTemplate(actor);
                        var actorName = selectionPanelContentManager.Instantiate<UIActorNameButtonTemplate>(template);
                        actorName.Assign(actor.Name, actor.Heading.text, OnToggleTracking);
                        selectionPanelContentManager.Add(actorName, groupFoldout.interiorPanel);
                        var target = actor.Name;
                        actorName.button.onClick.AddListener(() => { OnClickActor(target); });
                        if (showDetailsOnSelect) AddShowDetailsOnSelect(actorName.button, target);
                        if (string.Equals(actor.Name, actorNameToSelect))
                        {
                            elementToSelect = actorName.button;
                            actorNameToSelect = null;
                        }
                    }
                }
            }

            // Add groupless actors:
            foreach (var actor in actors)
            {
                if (!string.IsNullOrEmpty(actor.Group)) continue;
                var template = IsSelectedActor(actor)
                    ? GetSelectedActorNameTemplate(actor)
                    : GetActorNameTemplate(actor);
                var actorName = selectionPanelContentManager.Instantiate<UIActorNameButtonTemplate>(template);
                actorName.Assign(actor.Name, actor.Heading.text, OnToggleTracking);
                selectionPanelContentManager.Add(actorName, actorSelectionContentContainer);
                var target = actor.Name;
                actorName.button.onClick.AddListener(() => { OnClickActor(target); });
                if (showDetailsOnSelect) AddShowDetailsOnSelect(actorName.button, target);
                if (string.Equals(actor.Name, actorNameToSelect))
                {
                    elementToSelect = actorName.button;
                    actorNameToSelect = null;
                }
            }

            // If no actors, add no actors text:
            if (actors.Length == 0 && showNoActorsText)
            {
                var actorName = selectionPanelContentManager.Instantiate<UIActorNameButtonTemplate>(completedActorHeadingTemplate);
                var dummyText = noActorsMessage;
                actorName.Assign(dummyText, dummyText, null);
                selectionPanelContentManager.Add(actorName, actorSelectionContentContainer);
            }

            // If no actor selected and Select First Actor On Open is ticked, select it:
            if (string.IsNullOrEmpty(selectedActor) && selectFirstActorOnOpen && actors.Length > 0)
            {
                selectedActor = actors[0].Name;
                RepaintSelectedActor(actors[0]);
                ActorCompendium.MarkActorViewed(selectedActor);
            }

            SetStateToggleButtons();
            mainPanel.RefreshSelectablesList();
            if (mainPanel != null) LayoutRebuilder.MarkLayoutForRebuild(mainPanel.GetComponent<RectTransform>());
            if (elementToSelect != null)
            {
                StartCoroutine(SelectElement(elementToSelect));
            }
            else if (eventSystem.currentSelectedGameObject == null && mainPanel != null && mainPanel.firstSelected != null && InputDeviceManager.autoFocus)
            {
                UITools.Select(mainPanel.firstSelected.GetComponent<Selectable>(), true, eventSystem);
            }
        }

        protected virtual UIActorNameButtonTemplate GetActorNameTemplate(ActorInfo actor)
        {
            return isShowingMentionedActors
                ? mentionedActorHeadingTemplate
                : completedActorHeadingTemplate;
        }

        protected virtual UIActorNameButtonTemplate GetSelectedActorNameTemplate(ActorInfo actor)
        {
            return isShowingMentionedActors
                ? (selectedMentionedActorHeadingTemplate ?? mentionedActorHeadingTemplate)
                : (selectedCompletedActorHeadingTemplate ?? completedActorHeadingTemplate);
        }

        protected IEnumerator SelectElement(Selectable elementToSelect)
        {
            yield return null;
            UITools.Select(elementToSelect, true, eventSystem);
        }

        protected virtual void AddShowDetailsOnSelect(Button button, string target)
        {
            var eventTrigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

            // On joystick navigation:
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Select;
            entry.callback.AddListener((eventData) => { ShowDetailsOnSelect(target); });
            eventTrigger.triggers.Add(entry);

            // On cursor hover:
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { ShowDetailsOnSelect(target); });
            eventTrigger.triggers.Add(entry);
        }

        protected virtual void OnClickGroup(string groupName, StandardUIFoldoutTemplate groupFoldout)
        {
            ToggleGroup(groupName);
            groupFoldout.ToggleInterior();
        }

        protected virtual void ShowDetailsOnSelect(string actorName)
        {
            if (!string.Equals(selectedActor, actorName)) SelectActor(actorName);
        }

        protected virtual void OnClickActor(string actorName)
        {
            SelectActor(actorName);
        }

        public virtual void SelectActor(string actorName)
        {
            actorNameToSelect = actorName;
            ClickActor(actorName);
        }

        public void SetActorImage(string spriteAddress)
        {
            if (string.IsNullOrEmpty(spriteAddress)) {
                selectedActorImage.enabled = false;
                return;
            }
            
            selectedActorImage.enabled = true;
            AddressableLoader.RequestLoad<Sprite>(spriteAddress, sprite =>
            {
                selectedActorImage.sprite = sprite;
            });
        }
        

        protected virtual void RepaintSelectedActor(ActorInfo actor)
        {
            detailsPanelContentManager.Clear();
            SetActorImage(null);
            RefreshLayoutGroups.Refresh(mainPanel.GetComponentInChildren<LayoutGroup>().gameObject);
                          
            if (actor != null)
            {
                // Name:
                SetActorImage(actor.SpriteAddress);
                var nameInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(actorHeadingTextTemplate);
                nameInstance.Assign(actor.Heading.text);
                detailsPanelContentManager.Add(nameInstance, actorDetailsContentContainer);

                // Description:
                var descriptionInstance = detailsPanelContentManager.Instantiate<StandardUITextTemplate>(actorDescriptionTextTemplate);
                descriptionInstance.Assign(actor.Description.text);
                detailsPanelContentManager.Add(descriptionInstance, actorDetailsContentContainer);

               

                // Abandon button:
                if (currentActorStateMask == ActorState.Mentioned && ActorCompendium.IsActorAbandonable(actor.Name))
                {
                    var abandonButtonInstance = detailsPanelContentManager.Instantiate<StandardUIButtonTemplate>(abandonButtonTemplate);
                    detailsPanelContentManager.Add(abandonButtonInstance, actorDetailsContentContainer);
                    abandonButtonInstance.button.onClick.AddListener(ClickAbandonActorButton);
                }
            }
        }


        /// <summary>
        /// Toggles actor tracking.
        /// </summary>
        /// <param name="value">Tracking on or off.</param>
        /// <param name="data">Actor name (string).</param>
        public virtual void OnToggleTracking(bool value, object data)
        {
            var actor = (string)data;
            if (string.IsNullOrEmpty(actor)) return;
            var previousSelected = selectedActor;
            selectedActor = actor;
          
            selectedActor = previousSelected;
        }

        /// <summary>
        /// Opens the abandon confirmation popup.
        /// </summary>
        /// <param name="name">Actor name.</param>
        /// <param name="confirmAbandonActorHandler">Confirm abandon actor handler.</param>
        public override void ConfirmAbandonActor(string name, Action confirmAbandonActorHandler)
        {
            if (abandonActorPanel == null || selectedActor == null) return;
            this.confirmAbandonActorHandler = confirmAbandonActorHandler;
            abandonActorNameText.text = ActorCompendium.GetActorName(selectedActor);
            abandonActorPanel.Open();
        }

        public virtual void AbandonActorConfirmed()
        {
            OnConfirmAbandonActor();
            detailsPanelContentManager.Clear();
        }

        protected override void ShowActors(ActorState actorStateMask)
        {
            if (actorStateMask != currentActorStateMask)
            {
                detailsPanelContentManager.Clear();

                // Record most recent selected actor in category for when we return to category:
                if (currentActorStateMask == MentionedActorStateMask)
                {
                    mostRecentSelectedMentionedActor = selectedActor;
                    selectedActor = mostRecentSelectedCompletedActor;
                }
                else
                {
                    mostRecentSelectedCompletedActor = selectedActor;
                    selectedActor = mostRecentSelectedMentionedActor;
                }
            }
            base.ShowActors(actorStateMask);
        }

        #endregion

    }

}
