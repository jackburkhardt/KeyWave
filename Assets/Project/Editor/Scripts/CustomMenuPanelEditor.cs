using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.UI;
using UnityEditor;

namespace Project.Editor.Scripts
{
    [CustomEditor(typeof(CustomUIMenuPanel))]
    public class CustomMenuPanelEditor : StandardUIMenuPanelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        
            serializedObject.Update();
        
            // Your code here to show your custom fields. Example:
        
            for (int i = 0; i < CustomUIMenuPanel.CustomFields.Count; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CustomUIMenuPanel.CustomFields[i]), true);
            }
        
            serializedObject.ApplyModifiedProperties();
        }
    }
}