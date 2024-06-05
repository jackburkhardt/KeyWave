using System.Runtime.InteropServices;

namespace Project.Runtime.Scripts.App
{
    public class BrowserInterface
    {
        [DllImport("__Internal")]
        public static extern void canYouHearMe();

        [DllImport("__Internal")]
        public static extern void getSocketLibrarySource();
        
        [DllImport("__Internal")]
        public static extern void sendPlayerEvent(string playerEvent);
        
        [DllImport("__Internal")]
        public static extern void sendSaveGame(int slot, string saveData);
        
        [DllImport("__Internal")]
        public static extern string getSaveGame(int slot);
        
        [DllImport("__Internal")]
        public static extern bool saveGameExists(int slot);
    }
}