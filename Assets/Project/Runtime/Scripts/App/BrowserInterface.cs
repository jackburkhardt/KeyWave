using System.Runtime.InteropServices;


namespace Project.Runtime.Scripts.App
{
    public class BrowserInterface 
    {

        [DllImport("__Internal")]
        public static extern void canYouHearMe();

        [DllImport("__Internal")]
        public static extern void sendPlayerEvent(string playerEvent);

        [DllImport("__Internal")]
        public static extern void sendSaveGame(string saveData);

        [DllImport("__Internal")]
        public static extern void unityReadyForData();
        
    }
}