using Newtonsoft.Json;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class DailyReportSaver : Saver 
    {
        public override string RecordData()
        {
            return PixelCrushers.SaveSystem.Serialize(GameManager.instance.dailyReport);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            DailyReport data = PixelCrushers.SaveSystem.Deserialize<DailyReport>(s);
            if (data == null) return; // Serialized string isn't valid.
            GameManager.instance.dailyReport = data;
        }

        public override void ApplyDataImmediate()
        {
            
            var s = PixelCrushers.SaveSystem.currentSavedGameData.GetData("dayReport");
            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            DailyReport data = PixelCrushers.SaveSystem.Deserialize<DailyReport>(s);
            if (data == null) return; // Serialized string isn't valid.
            GameManager.instance.dailyReport = data;
        }

    }

}