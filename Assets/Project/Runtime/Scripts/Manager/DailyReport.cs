using System.Collections.Generic;
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
        private List<string> _failedTasks = new();

        public Dictionary<Points.Type, int> EarnedPoints = new()
        {
            {Points.Type.Skills, 0},
            {Points.Type.Teamwork, 0},
            {Points.Type.Wellness, 0},
            {Points.Type.Context, 0}
        };

        public DailyReport(int day)
        {
            Day = day;
            GameEvent.OnPlayerEvent += OnPlayerEvent;
        }

        public List<string> CompletedTasks => _completedTasks;
        public List<string> ActiveTasks => _activeTasks;
        public List<string> FailedTasks => _failedTasks;

        private void OnPlayerEvent(PlayerEvent playerEvent)
        {
            switch (playerEvent.EventType)
            {
                case "points":
                {
                    Points.PointsField pointsInfo = Points.PointsField.FromJObject(playerEvent.Data);
                    EarnedPoints[pointsInfo.Type] += pointsInfo.Points;
                    
                    //poopoo code
                    DialogueLua.SetVariable("points.skills", EarnedPoints[Points.Type.Skills]);
                    DialogueLua.SetVariable("points.teamwork", EarnedPoints[Points.Type.Teamwork]);
                    DialogueLua.SetVariable("points.wellness", EarnedPoints[Points.Type.Wellness]);
                    DialogueLua.SetVariable("points.context", EarnedPoints[Points.Type.Context]);
                    
                    break;
                }
                case "quest_state_change":
                {
                    string questName = playerEvent.Data["questName"].ToString();
                    if (string.IsNullOrEmpty(questName)) return;
                
                    switch (playerEvent.Data["state"].ToString())
                    {
                        case "Success":
                            if (!_completedTasks.Contains(questName))
                            {
                                _completedTasks.Add(questName);
                            }
                            break;
                        case "Active":
                            if (!_activeTasks.Contains(questName))
                            {
                                _activeTasks.Add(questName);
                            }
                            break;
                        case "Failure":
                            if (!_failedTasks.Contains(questName))
                            {
                                _failedTasks.Add(questName);
                            }
                            break;
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