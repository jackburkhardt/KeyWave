using System.Collections.Generic;
using Newtonsoft.Json;
using PixelCrushers.DialogueSystem;

namespace Project.Runtime.Scripts.Manager
{
    public class DailyReport
    {
        public readonly int Day;
        private List<string> _completedTasks = new();
        private List<string> _activeTasks = new();
        private List<string> _failedTasks = new();
        
        public List<string> CompletedTasks => _completedTasks;
        public List<string> ActiveTasks => _activeTasks;
        public List<string> FailedTasks => _failedTasks;
        
        public Dictionary<Points.Type, int> EarnedPoints = new()
        {
            {Points.Type.Business, 0},
            {Points.Type.Savvy, 0},
            {Points.Type.Wellness, 0}
        };
        
        public DailyReport(int day)
        {
            Day = day;
            GameEvent.OnPlayerEvent += OnPlayerEvent;
        }

        private void OnPlayerEvent(PlayerEvent playerEvent)
        {
            switch (playerEvent.EventType)
            {
                case "points":
                {
                    Points.PointsField pointsInfo = Points.PointsField.FromString(playerEvent.Data);
                    EarnedPoints[pointsInfo.Type] += pointsInfo.Points;
                    break;
                }
                case "quest_state_change":
                {
                    string displayName = QuestLog.GetQuestTitle(playerEvent.Source);
                    if (string.IsNullOrEmpty(displayName)) return;
                
                    switch (playerEvent.Target)
                    {
                        case "Success":
                            _completedTasks.Add(displayName);
                            break;
                        case "Active":
                            _activeTasks.Add(displayName);
                            break;
                        case "Failure":
                            _failedTasks.Add(displayName);
                            break;
                    }
                    break;
                }
            }
        }
        
        public string Serialize()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            return JsonConvert.SerializeObject(this, Formatting.Indented, settings);
        }
        
        ~DailyReport()
        {
            GameEvent.OnPlayerEvent -= OnPlayerEvent;
        }
    }
}