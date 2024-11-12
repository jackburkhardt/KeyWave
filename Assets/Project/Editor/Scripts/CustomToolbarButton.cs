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
    
    private static string _lastScene;
    
    public CustomPlayButton()
    {
        text = "Custom Button";
        tooltip = "This is a custom button added to the toolbar";
        
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
            EditorApplication.playModeStateChanged += LoadDefaultScene;
            EditorApplication.isPlaying = true;
        }
        
    }
    
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        UpdateButtonAppearance();
    }
    
    static void LoadDefaultScene(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                var lastScene = _lastScene = EditorSceneManager.GetActiveScene().path;
                PlayerPrefs.SetString("editor_lastScene", lastScene);
                
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene("Assets/Scenes/Menus/StartMenu.unity");
                break;
            case PlayModeStateChange.EnteredEditMode:
            {
                
                if (!string.IsNullOrEmpty(_lastScene))
                {
                    EditorSceneManager.OpenScene(_lastScene);
                    _lastScene = null;
                }
                else if (PlayerPrefs.HasKey("editor_lastScene"))
                {
                    var path = PlayerPrefs.GetString("editor_lastScene");
                    EditorSceneManager.OpenScene(path);
                }
               
                
               else EditorSceneManager.OpenScene("Assets/Scenes/Base.unity");
                
                EditorApplication.playModeStateChanged -= LoadDefaultScene;
                break;
            }
        }
            
        
    }
    
    private void UpdateButtonAppearance()
    {
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