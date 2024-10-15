using Project.Runtime.Scripts.UI;
using UnityEditor;

namespace Project.Editor.Scripts
{
    [CustomEditor(typeof(CharacterMenuPanel))]
    public class CharacterMenuPanelEditor : CustomMenuPanelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        
            serializedObject.Update();
        
            // Your code here to show your custom fields. Example:
        
            for (int i = 0; i < CharacterMenuPanel.CustomFields.Count; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CharacterMenuPanel.CustomFields[i]), true);
            }
        
            serializedObject.ApplyModifiedProperties();
        }
    }
}