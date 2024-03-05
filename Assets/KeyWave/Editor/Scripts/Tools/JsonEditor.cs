using UnityEditor;
using UnityEngine;
using System.IO;

public class JsonEditor : EditorWindow
{
    private static string _path = "/Resources/GameData/PerilsAndPitfalls/PlayerEventStack.json";
    private string jsonContent;

    private string[] options = new string[] { "Option 1", "Option 2", "Option 3" };
    private int selectedIndex = 0;

    /*
    [MenuItem("Tools/JSON Editor")]
    public static void ShowWindow()
    {
        GetWindow<JsonEditor>("JSON Editor");
    }
    */

    [MenuItem("Tools/Perils and Pitfalls/Clear Save")]

    public static void ClearSave()
    {
        File.WriteAllText(Application.dataPath + _path, "{ \"events\": [] }");
        Debug.Log("Save cleared.");
    }

    void OnGUI()
    {
        /*
        GUILayout.Label("JSON File Editor", EditorStyles.boldLabel);

        if (GUILayout.Button("Load JSON"))
        {
            jsonContent = File.ReadAllText(Application.dataPath + _path);
        }

        jsonContent = EditorGUILayout.TextArea(jsonContent, GUILayout.Height(250));

        if (GUILayout.Button("Save JSON"))
        {

            File.WriteAllText(Application.dataPath + _path, jsonContent);
            AssetDatabase.Refresh();
        }
        */

     //   GUILayout.Label("Select an Option:", EditorStyles.boldLabel);
    }
}
