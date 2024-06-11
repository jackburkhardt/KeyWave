using Project.Runtime.Scripts.UI;
using UnityEditor;

namespace Project.Editor.Scripts
{
    [CustomEditor(typeof(CustomVerticalLayoutGroup))]
    public class CustomVerticalLayoutGroupEditor : UnityEditor.Editor {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            for (int i = 0; i < CustomVerticalLayoutGroup.CustomFields.Count; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CustomVerticalLayoutGroup.CustomFields[i]), true);
            }
        
            serializedObject.ApplyModifiedProperties();
        }
    }
}