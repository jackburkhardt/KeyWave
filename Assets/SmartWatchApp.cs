using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;


namespace Project.Runtime.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SmartWatchApp", menuName = "SmartWatch/App")]
    public class SmartWatchApp : ScriptableObject
    {
        public Sprite icon;
        public Color color;
        
        public string appName;
        public enum OpenAppMethod
        {
            StartDialogueSystemConversation
        }
        
        public OpenAppMethod openAppMethod;

        [ShowIf("openAppMethod", OpenAppMethod.StartDialogueSystemConversation)]
        [EnableIf("manuallySetConversation")]
        public string dialogueSystemConversationTitle;
        
        [ShowIf("openAppMethod", OpenAppMethod.StartDialogueSystemConversation)]
        public bool manuallySetConversation = false;
        
        public void OpenApp()
        {
            switch (openAppMethod)
            {
                case OpenAppMethod.StartDialogueSystemConversation:
                    DialogueManager.instance.GoToConversation(dialogueSystemConversationTitle, true);
                    break;
            }
        }

        private void OnValidate()
        {
           SetConversationName();

           foreach (var subcomponent in persistentSubcomponents)
           {
               subcomponent.SetName();
           }

           foreach (var subcomponent in conditionalSubcomponents)
           {
               subcomponent.SetName();
           }
        }

        private void SetConversationName()
        {
            if (string.IsNullOrEmpty(appName)) return;
            
            if (openAppMethod == OpenAppMethod.StartDialogueSystemConversation && !manuallySetConversation)
            {
                var possibleConversations = Settings.Instance.dialogueDatabase.conversations.FindAll(p => p.Title.Contains($"SmartWatch/{appName}"));
                
                
                
                if (possibleConversations.Count == 0)
                {
                    Debug.LogWarning($"No conversation found for SmartWatch app {appName}");
                    return;
                }
                
                else if (possibleConversations.Count == 1)
                {
                    dialogueSystemConversationTitle = possibleConversations[0].Title;
                }

                else
                {
                    possibleConversations = possibleConversations.OrderBy(p => p.Title).ToList();
                    
                    if (possibleConversations.Any(p => p.Title.Contains("Base"))) dialogueSystemConversationTitle = possibleConversations.First(p => p.Title.Contains("Base")).Title;
                    
                    else dialogueSystemConversationTitle = possibleConversations[0].Title;
                    
                    
                }
            }
        }
        
        [Serializable]
        public class AppSubcomponent
        {
            [ReadOnly]
            public string name;

            public enum LuaFieldBool
            {
                True,
                False
            }
          
            public enum StaticProperty
            {
                None,
                Text = 1 << 0,
                Sprite = 1 << 1,
                Color = 1 << 2,
                Prefab = 1 << 3,
            }
            
            [AllowNesting]
            [EnumFlags]
            [Header( "Static Properties")]
            [Label("Properties")]
            public StaticProperty staticProperties;
            
            private bool showSprite => staticProperties.HasFlag(StaticProperty.Sprite);
            [AllowNesting]
            [ShowIf("showSprite")]
            [ShowAssetPreview]
            public Sprite sprite;
            
            private bool showText => staticProperties.HasFlag(StaticProperty.Text);
            [AllowNesting]
            [ShowIf("showText")]
            public string text;
            
            private bool showColor => staticProperties.HasFlag(StaticProperty.Color) && !seperateColorsPerGraphicProperty;
            [AllowNesting]
            [ShowIf("showColor")]
            public Color color;
            
             
            private bool showSeperateColors => (showColorSeparator && seperateColorsPerGraphicProperty);
            [AllowNesting]
            [ShowIf("showSeperateColors")]
            public Color spriteColor;
            
            [AllowNesting]
            [ShowIf("showSeperateColors")]
            public Color textColor;
            
            
            private bool showColorSeparator => (staticProperties.HasFlag(StaticProperty.Color) && (showSprite && showText));
            [AllowNesting]
            [ShowIf("showColorSeparator")]
            public bool seperateColorsPerGraphicProperty;
            
            
            private bool showPrefab => staticProperties.HasFlag(StaticProperty.Prefab);
            [AllowNesting]
            [ShowAssetPreview]
            [ShowIf("showPrefab")]
            public GameObject prefab;

            public virtual void SetName()
            {
                throw new NotImplementedException();
            }
           
        }
        
        [Serializable]
        public class PersistentSubcomponent : AppSubcomponent
        {
            public override void SetName()
            {
                name = "Persistent Subcomponent";
            }
        }
        [Serializable]
        public class ConditionalSubcomponent : AppSubcomponent
        {
            public override void SetName()
            {
                name = string.Empty;
                EvaluateConditions(true);
            }
            
            public void EvaluateConditions(bool setName = false)
            {
                List<bool> conditions = new List<bool>();
                var propertyName = string.Empty;
                
                if (showRepeatable) AddCondition(questIsRepeatable, "QUEST", "REPEATABLE");
                if (showRepeatCount) AddCondition(questHasPreviouslyCompleted, "QUEST","PREVIOUSLY_COMPLETED");
                
                if (showActorPresentInLocation) AddCondition(actorIsPresentInLocation, "LOCATION", "ACTOR_PRESENT");
                if (showSpecificLocation) AddCondition(actorIsPresentInLocation, "LOCATION", "SPECIFIC_LOCATION");
                
                if (showPointsCondition)
                {
                    if (pointsCondition == PointsCondition.WillEarnAnyPoints) AddCondition(LuaFieldBool.True, "POINTS", "WILL_EARN_ANY_POINTS");
                    if (willEarnSpecificPointTypes) AddCondition(LuaFieldBool.True, "POINTS", "WILL_EARN_SPECIFIC_TYPES");
                }
                
                void AddCondition(LuaFieldBool condition, string group, string property)
                {
                    var conditionResult = condition == LuaFieldBool.True;
                    conditions.Add(conditionResult);

                    if (!setName) return;
                    if (!propertyName.Contains($"{group}_"))
                    {
                        if (string.IsNullOrEmpty(propertyName)) propertyName += $"{group}_";
                        else propertyName += $"_AND_{group}_";
                    }
                    else propertyName += "_AND_";
                    propertyName += conditionResult ? property : "NOT_" + property;
                    
                }
                
                if (setName) name = propertyName;
                
            }
            
            
           
            public enum SubcomponentDependency
            {
                None,
                Location = 1 << 0,
                Quest = 1 << 1,
                Points = 1 << 2,
            }
            
            [AllowNesting] [EnumFlags]
            [Header("Conditions")]
            [Label("Condition Dependencies")]
            public SubcomponentDependency subcomponentDependencies;


            #region Quest Conditions
            
            public enum QuestCondition
            {
                None = 0,
                Repeatable =  1 << 0,
                RepeatCount = 1 << 1
            }
            
            private bool showQuestCondition => subcomponentDependencies.HasFlag(SubcomponentDependency.Quest);
            [ShowIf("showQuestCondition")] [AllowNesting] [EnumFlags]
            public QuestCondition questCondition;
            
            private bool showRepeatable => (showQuestCondition && questCondition.HasFlag(QuestCondition.Repeatable));
            [AllowNesting] [ShowIf("showRepeatable")]
            public LuaFieldBool questIsRepeatable;
            
            private bool showRepeatCount => (showQuestCondition && questCondition.HasFlag(QuestCondition.RepeatCount));
            [AllowNesting] [ShowIf("showRepeatCount")]
            public LuaFieldBool questHasPreviouslyCompleted;
            
            #endregion


            #region Location Conditions

            public enum LocationCondition
            {
                None = 0,
                ActorPresentInLocation = 1 << 0,
                SpecificLocation = 1 << 1,
            }
            
            private bool showLocationCondition => subcomponentDependencies.HasFlag(SubcomponentDependency.Location);
            [ShowIf("showLocationCondition")] [AllowNesting] [EnumFlags]
            public LocationCondition locationCondition;
            
            private bool showSpecificLocation => (showLocationCondition && locationCondition.HasFlag(LocationCondition.SpecificLocation));
            [AllowNesting] [ShowIf("showSpecificLocation")]
            [Dropdown("locationsDropdown")]
            public string location;

            private List<string> locationsDropdown =>
                Settings.Instance.dialogueDatabase.locations.Select(p => p.Name).ToList();
            
            
            private bool showActorPresentInLocation => (showLocationCondition && locationCondition.HasFlag(LocationCondition.ActorPresentInLocation));
            [AllowNesting] [ShowIf("showActorPresentInLocation")]
            public LuaFieldBool actorIsPresentInLocation;
            
            #endregion
            
            #region Points Conditions
            
            
            public enum PointsCondition
            {
                WillEarnAnyPoints,
                WillEarnSpecificPointTypes
            }
            
            private bool showPointsCondition => subcomponentDependencies.HasFlag(SubcomponentDependency.Points);
            [ShowIf("showPointsCondition")] [AllowNesting]
            public PointsCondition pointsCondition;
            
            
            private bool willEarnSpecificPointTypes => (showPointsCondition && pointsCondition.HasFlag(PointsCondition.WillEarnSpecificPointTypes));
            [AllowNesting] [ShowIf("willEarnSpecificPointTypes")][EnumFlags]
            public Points.Type pointTypes;
            
            
            
            #endregion
          
        }
        
        [Tooltip("Persistent subcomponents are always visible on the app screen")]
        public List<PersistentSubcomponent> persistentSubcomponents;
        [Tooltip("Conditional subcomponents are only visible under certain conditions related to the Dialogue System or other game systems")]
        public List<ConditionalSubcomponent> conditionalSubcomponents;
        
        
        
    }
}

