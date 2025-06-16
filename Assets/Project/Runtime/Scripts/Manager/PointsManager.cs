using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Runtime.Scripts.Manager
{


    public class PointsManager : MonoBehaviour
    {
        protected void OnEnable()
        {
            Clock.onTimeChange += OnTimeChange;
        }
        
        protected void OnDisable()
        {
            Clock.onTimeChange -= OnTimeChange;
        }

        public void OnQuestSuccess(string questName)
        {
            
            var quest = DialogueManager.masterDatabase.GetItem(questName);
            if (!quest.IsAction && !quest.IsQuest) return;
            
            Points.PointsField[] points =  new []{ new Points.PointsField()};
            
            points = DialogueUtility.GetPointsFromField(quest!.fields);
            var repeatCount =  DialogueLua.GetQuestField(questName, "Repeat Count").asInt;
                
            foreach (var pointField in points)
            {
                var multiplier = 1 - quest.LookupFloat("Repeat Points Reduction");
                        
                for (int i = 0; i < repeatCount; i++)
                { 
                    if (pointField.Points > 0) pointField.Points = (int) (pointField.Points * multiplier);
                }
                    
                if (pointField.Points == 0) continue;
                
            }
                
            if (quest.IsAction)
            {
                    DialogueLua.SetQuestField(questName, "Repeat Count", repeatCount + 1);
            }
                
            foreach (var pointsField in points)  Points.AddPoints(pointsField.Type, pointsField.Points);
            
        }
        
        
        /// <summary>
        /// Used to reduce Wellness over time.
        /// </summary>
        /// <param name="timeChangeData"></param>
        
        public void OnTimeChange(Clock.TimeChangeData timeChangeData)
        {
            for (int i = timeChangeData.previousTime; i < timeChangeData.newTime; i++)
            {
                if (i % 600 == 0)
                {
                    Points.AddPoints( "Wellness", -1, false);
                }
            }
        }

        
        
        /// <summary>
        /// Returns the points given for completing a specific quest.
        /// </summary>
        /// <param name="questName"></param>
        /// <param name="useCurrentPointReductions">Get the amount of points rewarded when applying the current Points Reduction modifier</param>
        /// <returns></returns>
        
        public static Points.PointsField[] GetQuestPoints(string questName, bool useCurrentPointReductions)
        {
            var quest = DialogueManager.masterDatabase.GetItem(questName);
            if (!quest.IsAction && !quest.IsQuest) return null;
            
            var points = DialogueUtility.GetPointsFromField(quest!.fields);

            if (!useCurrentPointReductions) return points;
            
            var repeatCount =  DialogueLua.GetQuestField(questName, "Repeat Count").asInt;
                
            foreach (var pointField in points)
            {
                var multiplier = 1 - quest.LookupFloat("Repeat Points Reduction");
                        
                for (int i = 0; i < repeatCount; i++)
                {
                    if (pointField.Points > 0) pointField.Points = (int) (pointField.Points * multiplier);
                }
                    
                if (pointField.Points == 0) continue;
            }
            
            return points;
        }
    }
    
    
    public static class Points
    {
        /// <summary>
        /// Type of points given to player.
        /// </summary>
      
        public enum PointsChangeAnimation
        {
            Obvious, // with green or red glow
            Subtle, // no glow
        }
        
        public static Action<string,int, PointsChangeAnimation> OnPointsChange;
        
        public static int TotalMaxScore(DialogueDatabase database = null) {
            database ??= GameManager.settings.dialogueDatabase;
            var score = 0;
            foreach (var type in GetAllPointsTypes(database))
            {
                score += type.LookupInt("Max Score");
            }

            return score;
        }
        

        public static List<Item> GetAllPointsTypes(DialogueDatabase database = null)
        { 
            database ??= GameManager.dialogueDatabase;
            if (database == null) return new List<Item>();
            return database.items.FindAll(item => item.IsPointCategory);
        } 
        
        public static List<Item> GetAllItemsWithPointsType(Item pointType, DialogueDatabase database = null, bool includeZeroPoints = false)
        {
            if (database == null) database = GameManager.settings.dialogueDatabase;
            
            List<Item> items = new List<Item>();
            foreach (var item in database.items)
            {
                foreach (var itemWithPointType in item.fields.Where( p => p.title.EndsWith( $"{pointType.Name} Points")))
                {
                    if (int.Parse(itemWithPointType.value) == 0 && !includeZeroPoints) continue;
                    items.Add(item);
                }
            }

            return items;
        }

        public static string ToString(this PointsField field)
        {
            return $"{field.Type}:{field.Points}";
        }

        public static Item GetDatabaseItem(string type, DialogueDatabase database = null)
        {
            database ??= GameManager.dialogueDatabase;
            if (database == null) return null;
            var pointsType = database.items.Find(item => item.IsPointCategory && string.Equals(item.Name, type, StringComparison.CurrentCultureIgnoreCase));

            return pointsType;
            
        }

        public static int Score(string type, DialogueDatabase database = null)
        {
            database ??= GameManager.dialogueDatabase;
            if (database == null) return -1;

            var pointsType = GetDatabaseItem(type, database);
            if (pointsType == null) return -1;

            return DialogueLua.GetItemField( pointsType.Name, "Score").asInt;
        }

        public static int MaxScore(string type, DialogueDatabase database = null)
        {
            database ??= GameManager.settings != null ? GameManager.settings .dialogueDatabase : null;
            if (database == null) return 0;
            var pointsType = GetDatabaseItem( type, database);
            return DialogueLua.GetItemField( pointsType.Name, "Max Score").asInt;
        }
        
        public static void AddPoints(string type, int points, bool animate = true)
        {
            var pointsType = GetDatabaseItem(type);
            if (pointsType == null) return;
            if (points == 0) return;

            var currentScore = DialogueLua.GetItemField(pointsType.Name, "Score").asInt;
            var newScore = currentScore + points;

           
            if (animate) OnPointsChange?.Invoke(type, newScore, PointsChangeAnimation.Obvious);
            else OnPointsChange?.Invoke(type, newScore, PointsChangeAnimation.Subtle);
            
            DialogueLua.SetItemField(pointsType.Name, "Score", Mathf.Max(newScore, 0));
            
        }
        
        public class PointsField
        {
            [FormerlySerializedAs("type")] 
            public string Type;
        
            [FormerlySerializedAs("points")] 
            public int Points;
            
            public static PointsField FromJObject(JToken data)
            {
                if (data["Points"] == null || data["Type"] == null) return new PointsField {Type = string.Empty, Points = 0};
                
                var type = (string) data["Type"];
                var points = (int) data["Points"];
                return new PointsField {Type = type, Points = points};
            }

            public static PointsField FromLuaField(Field field)
            {
                string pattern = @"^(.*?) Points$";
                var pointType = Regex.Replace(field.title, pattern, "$1");
                
                if (pointType.Split(" ").Length > 1) pointType = pointType.Split(" ")[^1];
                var type = pointType;
                var points = int.Parse(field.value);
                return new PointsField { Type = type, Points = points };

            }

        }
    }
}