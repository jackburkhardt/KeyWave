using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SortingLayerManager))]
public class SortingLayerManagerEditor : Editor
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