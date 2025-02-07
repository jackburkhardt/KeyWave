#if UNITY_2021_1_OR_NEWER
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;

[InitializeOnLoad]
[Overlay(typeof(SceneView), "Custom Toolbar")]
public class CustomToolbarOverlay : ToolbarOverlay
{
    public CustomToolbarOverlay() : base("CustomPlayButton") { }
}

[EditorToolbarElement("CustomPlayButton")]
public class CustomPlayButton : EditorToolbarButton
{
    private Color defaultColor;
    private Color playModeColor = new Color(0.25f, 0.5f, 1.0f, 1.0f); // Blue shade similar to Unity's Play button
    
   // private static string _lastScene;

    private static string Tooltip => 
        EditorApplication.isPlaying ?
            PlayerPrefs.HasKey("editor_lastScene") ?
                $"Stop and Return to {PlayerPrefs.GetString("editor_lastScene").Split("/")[^1].Split(".")[0]}"
                :"Stop and Return to Base (default)"
            : "Play Start Menu";
    public CustomPlayButton()
    {
        text = "Custom Button";
        tooltip = Tooltip;
        
        icon = EditorGUIUtility.IconContent("d_PlayButton").image as Texture2D;
        defaultColor = GUI.backgroundColor;

        // Assign the action to perform when clicked
        clicked += OnButtonClicked;
        
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    private void OnButtonClicked()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
        else
        {
            //EditorApplication.playModeStateChanged += LoadDefaultScene;
            PlayerPrefs.SetInt("editor_customToolbarButton", 1);
            EditorApplication.isPlaying = true;
            
        }
        
        
        
    }
    
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        UpdateButtonAppearance();
    }
    
    static void LoadDefaultScene(PlayModeStateChange state)
    {
        if (!PlayerPrefs.HasKey("editor_customToolbarButton") ||
            PlayerPrefs.GetInt("editor_customToolbarButton") != 1) return;
        
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
                PlayerPrefs.SetInt("editor_customToolbarButton", 0);
                        
                if (PlayerPrefs.HasKey("editor_lastScene"))
                {
                    var path = PlayerPrefs.GetString("editor_lastScene");
                    EditorSceneManager.OpenScene(path);
                }
                break;
            }
        }


    }
    
    private void UpdateButtonAppearance()
    {
        tooltip = Tooltip;
        // Update the button text, icon, and color based on Play Mode
        if (EditorApplication.isPlaying)
        {
            text = "Pause";
            icon = EditorGUIUtility.IconContent("d_PreMatQuad").image as Texture2D;
            GUI.backgroundColor = playModeColor; // Set to blue shade
        }
        else
        {
            text = "Play";
            icon = EditorGUIUtility.IconContent("d_PlayButton").image as Texture2D;
            GUI.backgroundColor = defaultColor; // Reset to default color
        }
    }
        
}
    

#endif