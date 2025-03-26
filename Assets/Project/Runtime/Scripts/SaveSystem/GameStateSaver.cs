using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Manager;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class GameStateSaver : Saver 
    {
        public override string RecordData()
        {
            return LocationManager.instance.PlayerLocation.Name;
        }

        public override void ApplyData(string s)
        {
            App.App.Instance.ChangeScene(s, App.App.Instance.currentScene, LoadingScreen.Transition.Default);
        }

    }

}