using System;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.Manager;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class EndOfDayScreen : MonoBehaviour
{
    private DailyReport _report;
    [SerializeField] private Transform _completedPanel;
    [SerializeField] private Transform _activePanel;
    [SerializeField] private Transform _failedPanel;
    [SerializeField] private Object _taskDisplayPrefab;
    
    private void Start()
    {
        _report = GameManager.instance.dailyReport;
        #if !UNITY_EDITOR
        BrowserInterface.sendPlayerEvent(_report.Serialize());
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
        GameManager.instance.StartNewDay();
    }
}
