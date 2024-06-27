using Newtonsoft.Json;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;

namespace Project.Runtime.Scripts.SaveSystem
{

    /// This is a starter template for Save System savers. To use it,
    /// make a copy, rename it, and remove the line marked above.
    /// Then fill in your code where indicated below.
    public class DailyReportSaver : Saver // Rename this class.
    {
        public override string RecordData()
        {
            return JsonConvert.SerializeObject(GameManager.instance.dailyReport);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            DailyReport data = JsonConvert.DeserializeObject<DailyReport>(s);
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
            DailyReport data = JsonConvert.DeserializeObject<DailyReport>(s);
            if (data == null) return; // Serialized string isn't valid.
            GameManager.instance.dailyReport = data;
        }

        //public override void OnBeforeSceneChange()
        //{
        //    // The Save System will call this method before scene changes. If your saver listens for 
        //    // OnDisable or OnDestroy messages (see DestructibleSaver for example), it can use this 
        //    // method to ignore the next OnDisable or OnDestroy message since they will be called
        //    // because the entire scene is being unloaded.
        //}

        //public override void OnRestartGame()
        //{
        //    // The Save System will call this method when restarting the game from the beginning.
        //    // Your Saver can reset things to a fresh state if necessary.
        //}
    }

}