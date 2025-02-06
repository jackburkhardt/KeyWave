using Project.Runtime.Scripts.App;
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
#if UNITY_WEBGL
        BrowserInterface.sendPlayerEvent(_report.SerializeForWeb());
        BrowserInterface.sendPlayerEvent(GameLog.SerializeForWeb());
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
        
            foreach (var task in _report.FailedTasks)
            {
                var taskObject = Instantiate(_taskDisplayPrefab, _failedPanel) as GameObject;
                taskObject.GetComponent<ObjectivePanelItem>().SetVisibleElements("failure", task);
            }
        }

        public void StartNextDay()
        {
            #if UNITY_WEBGL
            Application.ExternalCall("location.reload()");
            #endif
            // uncomment/remove above if we do add multiple days.
            // GameManager.instance.StartNewDay();
        }
    }
}