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
            /// This method should return a string that represents the data you want to save.
            /// You can use SaveSystem.Serialize() to serialize a serializable object to a 
            /// string. This will use the serializer component on the Save System GameObject,
            /// which defaults to Unity's built-in JSON serialization. Remember that Unity
            /// cannot directly serialize lists or arrays, so you must put them inside a
            /// class.
            /// 
            /// If you use a class to hold the data, use SaveSystem.Serialize to return a 
            /// serialized version:

            var jason = JsonUtility.ToJson(GameManager.gameState);
            Debug.Log(jason);
            return jason;
        }

        public override void ApplyData(string s)
        {
            /// This method should process the string representation of saved data and apply
            /// it to the current state of the game. You can use SaveSystem.Deserialize()
            /// to deserialize the string to an object that specifies the state to apply to
            /// the game.

            if (string.IsNullOrEmpty(s))
            {
                return;
            }; // No data to apply.
            GameState data = JsonUtility.FromJson<GameState>(s);
            Debug.Log(s);
            if (data == null) return; // Serialized string isn't valid.
            GameStateManager.instance.gameState = data;
            
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