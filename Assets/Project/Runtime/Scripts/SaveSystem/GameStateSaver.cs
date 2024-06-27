using Newtonsoft.Json;
using PixelCrushers;
using Project.Runtime.Scripts.Manager;
using UnityEngine;

namespace Project.Runtime.Scripts.SaveSystem
{

    /// This is a starter template for Save System savers. To use it,
    /// make a copy, rename it, and remove the line marked above.
    /// Then fill in your code where indicated below.
    public class GameStateSaver : Saver // Rename this class.
    {
        public override string RecordData()
        {
            //Debug.Log("Recorded: " +JsonConvert.SerializeObject(GameStateManager.instance.gameState));
            return JsonConvert.SerializeObject(GameStateManager.instance.gameState);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            GameState data = JsonConvert.DeserializeObject<GameState>(s);
            //Debug.Log(s);
            if (data == null) return; // Serialized string isn't valid.
            GameStateManager.instance.gameState = data;
            
            //Debug.Log("Loading scene (normal): " + data.current_scene);
            App.App.Instance.LoadScene(data.current_scene);
        }


        public override void ApplyDataImmediate()
        {
            // If your Saver needs to pull data from the Save System immediately after
            // loading a scene, instead of waiting for ApplyData to be called at its
            // normal time, which may be some number of frames after the scene has started,
            // it can implement this method. For efficiency, the Save System will not look up 
            // the Saver's data; your method must look it up manually by calling 
            // SaveSystem.savedGameData.GetData(key).
            
            var s = PixelCrushers.SaveSystem.currentSavedGameData.GetData("gameState");
            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            GameState data = JsonConvert.DeserializeObject<GameState>(s);
            if (data == null) return; // Serialized string isn't valid.
            GameStateManager.instance.gameState = data;

            //Debug.Log("Loading scene (immediate): " + data.current_scene);
            App.App.Instance.LoadScene(data.current_scene);
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