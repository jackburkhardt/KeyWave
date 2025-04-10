using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Runtime.Scripts.Manager
{
    public static class Points
    {
        /// <summary>
        /// Type of points given to player.
        /// </summary>
      

        private static bool isAnimating;

        private static Vector2 spawnPosition;
        
        public enum PointsChangeExpression
        {
            Explicit,
            Passive
        }
        
        public static Action<string, int, PointsChangeExpression> OnPointsChange;
        
        public static int TotalScore {
            get
            {
                var score = 0;
                foreach (var type in GetAllPointsTypes())
                {
                    score += type.LookupInt("Score");
                }

                return score;
            }
        }
        
        public static int TotalMaxScore(DialogueDatabase database = null) {
            database ??= GameManager.settings.dialogueDatabase;
            var score = 0;
            foreach (var type in GetAllPointsTypes(database))
            {
                score += type.LookupInt("Max Score");
            }

            return score;
        }
        
        public static bool IsAnimating => isAnimating;

        public static List<Item> GetAllPointsTypes(DialogueDatabase database = null)
        { 
            database ??= GameManager.dialogueDatabase;
            if (database == null) return new List<Item>();
            return database.items.FindAll(item => item.IsPointCategory);
        } 

        public static List<PointsField> GetPointsFieldsFromItem(Item item, DialogueDatabase database = null)
        {
            
            
            if (database == null) database = GameManager.dialogueDatabase;
            
            List<PointsField> itemPointsFields = new List<PointsField>();
            
            HashSet<string> pointTypeNames = new HashSet<string>(GetAllPointsTypes(database).Select(item => item.Name));

         
            foreach (var field in item.fields)
            {
                string[] words = field.title.Split(' ');
                
                if (words.Length >= 2)
                {
                    string possiblePointType = words[words.Length - 2]; // Second-to-last word
                    string lastWord = words[words.Length - 1]; // Last word

                    if (lastWord == "Points" && pointTypeNames.Contains(possiblePointType))
                    {
                        itemPointsFields.Add(PointsField.FromLuaField(field));
                    }
                }
            }
            return itemPointsFields;
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

        /// <summary>
        /// Gets the position currently used for spawning orbs in animations.
        /// </summary>
        public static Vector2 SpawnPosition => spawnPosition;

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

           
            if (animate) OnPointsChange?.Invoke(type, newScore, PointsChangeExpression.Explicit);
            else OnPointsChange?.Invoke(type, newScore, PointsChangeExpression.Passive);
            
            DialogueLua.SetItemField(pointsType.Name, "Score", Mathf.Max(newScore, 0));
            
        }

        /// <summary>
        /// Color associated with a given point type.
        /// </summary>
        /// <param name="type">Type of point.</param>
        /// <returns></returns>
        public static Color Color(string type)
        {
            var pointsType = GetDatabaseItem(type);
            if (pointsType == null) return default;

            return pointsType.LookupColor("Color");
        }
        public static bool IsPointsField(this Field field)
        {
            string pattern = @"^(.*?) Points$";
            return Regex.IsMatch(field.title, pattern);
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