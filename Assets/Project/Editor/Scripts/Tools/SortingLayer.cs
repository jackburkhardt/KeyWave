using Project.Runtime.Scripts.Utility;
using UnityEditor;

namespace Project.Editor.Scripts.Tools
{
    [CustomEditor(typeof(SortingLayerManager))]
    public class SortingLayerManagerEditor : UnityEditor.Editor
    {
        SerializedProperty sortingLayerProperty;

        void OnEnable()
        {
            sortingLayerProperty = serializedObject.FindProperty("sortingLayer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sortingLayerProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}