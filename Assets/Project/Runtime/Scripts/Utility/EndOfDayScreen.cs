using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Runtime.Scripts.Utility
{
    public class EndOfDayScreen : MonoBehaviour
    {
        private DailyReport _report;
        [SerializeField] private string[] _danielTypes;
        [SerializeField] private TextMeshProUGUI _danieltypeDesc;
        [SerializeField] private TextMeshProUGUI _danieltypeTitle;

        private void Start()
        {
            _report = GameManager.instance.dailyReport;
            string eodEvent = ConstructEndOfDayEvent();
            Debug.Log("Transmitting EOD event...\n" + eodEvent);
            DetermineDanieltype();
            #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserInterface.sendPlayerEvent(eodEvent);
            #endif
        }

        private void DetermineDanieltype()
        {
            string trueDaniel = "uncontained_daniel";
            int trueDanielThreshold = int.MaxValue;
            for (int i = 0; i < _danielTypes.Length; i++)
            {
                string type = _danielTypes[i];
                int repVar = DialogueLua.GetVariable("reputation." + type, 0);
                int thresholdVar = DialogueLua.GetVariable("reputation." + type + ".threshold", 0);

                if (repVar >= thresholdVar && thresholdVar <= trueDanielThreshold)
                {
                    trueDaniel = type;
                    trueDanielThreshold = thresholdVar;
                }
            }

            _danieltypeTitle.text = trueDaniel.Replace('_', ' ').ToUpper();
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