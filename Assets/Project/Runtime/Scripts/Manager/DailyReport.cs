using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Events;
using UnityEngine;

namespace Project.Runtime.Scripts.Manager
{
    public class DailyReport
    {
        public readonly int Day;
        private HashSet<string> _activeTasks = new();
        private HashSet<string> _completedTasks = new();
        private HashSet<string> _abandonedTasks = new();
        private HashSet<string> _seenLocations = new();

        public Dictionary<string, int> EarnedPoints;

        public DailyReport(int day)
        {
            Day = day;
            GameEvent.OnPlayerEvent += OnPlayerEvent;
            EarnedPoints = new Dictionary<string, int>();

            foreach (var pointType in Points.GetAllPointsTypes())
            {
                EarnedPoints.Add(pointType.Name , 0);
            }
        }

        public HashSet<string> CompletedTasks => _completedTasks;
        public HashSet<string> ActiveTasks => _activeTasks;
        public HashSet<string> AbandonedTasks => _abandonedTasks;

        private void OnPlayerEvent(PlayerEvent playerEvent)
        {
            string questName;
            if (playerEvent.EventType == "quest_state_change")
            {
                questName = QuestLog.GetQuestTitle(playerEvent.Data["questName"]?.ToString());
            } else if (playerEvent.EventType == "action_state_change")
            {
                questName = QuestLog.GetQuestTitle(playerEvent.Data["actionName"]?.ToString());
            } else if (playerEvent.EventType == "move")
            {
                string locName = playerEvent.Data["newLocation"]?.ToString();
                if (_seenLocations.Add(locName))
                {
                    if (_seenLocations.Count % 2 == 0) DialogueLua.SetVariable("reputation.traveler_daniel", DialogueLua.GetVariable("reputation.traveler_daniel", 0) + 1);
                }
                return;
            }
            else
            {
                return;
            }
            
            if (string.IsNullOrEmpty(questName)) return;
            
            if (playerEvent.EventType is "quest_state_change" or "action_state_change")
            {
                switch (playerEvent.Data["state"]?.ToString())
                {
                    case "Success":
                        if (_completedTasks.Add(questName))
                        {
                            _activeTasks.Remove(questName);
                        }
                        JArray pointsArray = JArray.FromObject(playerEvent.Data["points"] ?? new JArray() );
                        foreach (var field in pointsArray)
                        {
                            Points.PointsField pointsInfo = Points.PointsField.FromJObject(field);
                            EarnedPoints[pointsInfo.Type] += pointsInfo.Points;   
                        }
                        break;
                    case "Active":
                        _activeTasks.Add(questName);
                        break;
                    case "Abandoned":
                        if (_abandonedTasks.Add(questName))
                        {
                            _activeTasks.Remove(questName);
                        }
                        break;
                }
            }
        }

        public JObject ToJson()
        {
            var settings = new JsonSerializer()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = { new StringEnumConverter() }
            };

            JObject report = new()
            {
                ["Day"] = Day,
                ["CompletedTasks"] = JArray.FromObject(_completedTasks),
                ["ActiveTasks"] = JArray.FromObject(_activeTasks),
                ["AbandonedTasks"] = JArray.FromObject(_abandonedTasks),
                ["Points"] = JObject.FromObject(EarnedPoints, settings)
            };

            return report;
        }

        ~DailyReport()
        {
            GameEvent.OnPlayerEvent -= OnPlayerEvent;
        }
    }
}