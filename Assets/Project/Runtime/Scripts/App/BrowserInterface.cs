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
    }
}