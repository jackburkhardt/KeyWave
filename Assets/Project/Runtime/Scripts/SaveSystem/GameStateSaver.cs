using PixelCrushers;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Manager;

namespace Project.Runtime.Scripts.SaveSystem
{
    public class GameStateSaver : Saver 
    {
        public override string RecordData()
        {
            return PixelCrushers.SaveSystem.Serialize(GameStateManager.instance.gameState);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            GameState data = PixelCrushers.SaveSystem.Deserialize<GameState>(s);
            if (data == null) return; // Serialized string isn't valid.
            GameStateManager.instance.gameState = data;
            
            GameManager.instance.SetLocation(data.current_scene);
        }


        public override void ApplyDataImmediate()
        {
            var s = PixelCrushers.SaveSystem.currentSavedGameData.GetData("gameState");
            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            GameState data = PixelCrushers.SaveSystem.Deserialize<GameState>(s);
            if (data == null) return; // Serialized string isn't valid.
            GameStateManager.instance.gameState = data;

            App.App.Instance.ChangeScene(data.current_scene, App.App.Instance.currentScene, LoadingScreen.Transition.Default);
        }

    }

}