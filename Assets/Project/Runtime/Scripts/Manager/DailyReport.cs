using System.Collections.Generic;
using Newtonsoft.Json;
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
            {Points.Type.Credibility, 0},
            {Points.Type.Engagement, 0},
            {Points.Type.Wellness, 0},
            {Points.Type.Commitment, 0}
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
                    DialogueLua.SetVariable("points.credibility", EarnedPoints[Points.Type.Credibility]);
                    DialogueLua.SetVariable("points.engagement", EarnedPoints[Points.Type.Engagement]);
                    DialogueLua.SetVariable("points.wellness", EarnedPoints[Points.Type.Wellness]);
                    DialogueLua.SetVariable("points.commitment", EarnedPoints[Points.Type.Commitment]);
                    
                    break;
                }
                case "quest_state_change":
                {
                    string displayName = QuestLog.GetQuestTitle(playerEvent.Data["questName"].ToString());
                    if (string.IsNullOrEmpty(displayName)) return;
                
                    switch (playerEvent.Data["state"].ToString())
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