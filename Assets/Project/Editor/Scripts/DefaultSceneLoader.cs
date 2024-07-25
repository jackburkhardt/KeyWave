using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Editor.Scripts
{
    [InitializeOnLoad]
    public static class DefaultSceneLoader
    {
        static DefaultSceneLoader(){
            EditorApplication.playModeStateChanged += LoadDefaultScene;
        }

        static void LoadDefaultScene(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    var lastScene = EditorSceneManager.GetActiveScene().path;
                    PlayerPrefs.SetString("editor_lastScene", lastScene);
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene("Assets/Scenes/Menus/StartMenu.unity");
                    break;
                case PlayModeStateChange.EnteredEditMode:
                {
                    if (PlayerPrefs.HasKey("editor_lastScene"))
                    {
                        var path = PlayerPrefs.GetString("editor_lastScene");
                        EditorSceneManager.OpenScene(path);
                    }
                    break;
                }
            }
        }
        
    }
}