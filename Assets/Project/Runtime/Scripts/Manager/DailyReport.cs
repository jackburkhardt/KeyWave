using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Events;

namespace Project.Runtime.Scripts.Manager
{
    public class DailyReport
    {
        public readonly int Day;
        private List<string> _activeTasks = new();
        private List<string> _completedTasks = new();
        private List<string> _abandonedTasks = new();

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

        public List<string> CompletedTasks => _completedTasks;
        public List<string> ActiveTasks => _activeTasks;
        public List<string> AbandonedTasks => _abandonedTasks;

        private void OnPlayerEvent(PlayerEvent playerEvent)
        {
            string questName;
            if (playerEvent.EventType == "quest_state_change")
            {
                questName = QuestLog.GetQuestTitle(playerEvent.Data["questName"].ToString());
            } else if (playerEvent.EventType == "action_state_change")
            {
                questName = QuestLog.GetQuestTitle(playerEvent.Data["actionName"].ToString());
            }
            else
            {
                return;
            }
            
            if (string.IsNullOrEmpty(questName)) return;
            
            if (playerEvent.EventType is "quest_state_change" or "action_state_change")
            {
                switch (playerEvent.Data["state"].ToString())
                {
                    case "Success":
                        if (!_completedTasks.Contains(questName))
                        {
                            _completedTasks.Add(questName);
                        }
                        JArray pointsArray = JArray.FromObject(playerEvent.Data["points"] ?? new JArray() );
                        foreach (var field in pointsArray)
                        {
                            Points.PointsField pointsInfo = Points.PointsField.FromJObject(field);
                            EarnedPoints[pointsInfo.Type] += pointsInfo.Points;   
                        }
            
                        for( int i = 0; i < EarnedPoints.Count; i++)
                        {
                            DialogueLua.SetItemField( EarnedPoints.ElementAt(i).Key, "Score", EarnedPoints.ElementAt(i).Value);
                        }

                        break;
                    case "Active":
                        if (!_activeTasks.Contains(questName))
                        {
                            _activeTasks.Add(questName);
                        }

                        break;
                    case "Abandoned":
                        if (!_abandonedTasks.Contains(questName))
                        {
                            _abandonedTasks.Add(questName);
                        }

                        break;
                }
            }
        }

        public string SerializeForWeb()
        {
            var settings = new JsonSerializer()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = { new StringEnumConverter() }
            };

            JObject report = new()
            {
                ["EventType"] = "daily_report",
                ["Day"] = Day,
                ["CompletedTasks"] = JArray.FromObject(_completedTasks),
                ["ActiveTasks"] = JArray.FromObject(_activeTasks),
                ["AbandonedTasks"] = JArray.FromObject(_abandonedTasks),
                ["Points"] = JObject.FromObject(EarnedPoints, settings)
            };

            return JsonConvert.SerializeObject(report, Formatting.None);
        }

        ~DailyReport()
        {
            GameEvent.OnPlayerEvent -= OnPlayerEvent;
        }
    }
}