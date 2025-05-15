using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Runtime.Scripts.Utility
{
    public class EndOfDayScreen : MonoBehaviour
    {
        [SerializeField] private Transform _completedPanel;
        [SerializeField] private Transform _activePanel;
        [SerializeField] private Transform _failedPanel;
        [SerializeField] private Object _taskDisplayPrefab;
        private DailyReport _report;

        private void Start()
        {
            _report = GameManager.instance.dailyReport;
            string eodEvent = ConstructEndOfDayEvent();
            Debug.Log("Transmitting EOD event...\n" + eodEvent);
            #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.sendPlayerEvent(eodEvent);
            #endif
            
            foreach (var task in _report.CompletedTasks)
            {
                var taskObject = Instantiate(_taskDisplayPrefab, _completedPanel) as GameObject;
                taskObject.GetComponent<ObjectivePanelItem>().SetVisibleElements("success", task);
            }
        
            foreach (var task in _report.ActiveTasks)
            {
                var taskObject = Instantiate(_taskDisplayPrefab, _activePanel) as GameObject;
                taskObject.GetComponent<ObjectivePanelItem>().SetVisibleElements("active", task);
            }
        
            foreach (var task in _report.AbandonedTasks)
            {
                var taskObject = Instantiate(_taskDisplayPrefab, _failedPanel) as GameObject;
                taskObject.GetComponent<ObjectivePanelItem>().SetVisibleElements("failure", task);
            }
        }

        public void StartNextDay()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("location.reload()");
            #endif
            // uncomment/remove above if we do add multiple days.
            // GameManager.instance.StartNewDay();
        }

        private string ConstructEndOfDayEvent()
        {
            var dayReport = _report.ToJson();
            var gameLog = GameLog.Log2Json();
            dayReport.Merge(gameLog);
            var playerEvent = new PlayerEvent("end_of_day", dayReport);
            return playerEvent.ToString();
        }
    }
}