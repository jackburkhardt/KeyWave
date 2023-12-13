
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicEnum))]
public class DynamicEnumEditor : Editor
{


    SerializedProperty section;
    SerializedProperty sectionA;
    SerializedProperty sectionB;

    void OnEnable()
    {
        section = serializedObject.FindProperty("section");
        sectionA = serializedObject.FindProperty("sectionA");
        sectionB = serializedObject.FindProperty("sectionB");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(section);
        switch ((DynamicEnum.Section)section.enumValueIndex)
        {
            case DynamicEnum.Section.A:
                EditorGUILayout.PropertyField(sectionA);
                break;
            case DynamicEnum.Section.B:
                EditorGUILayout.PropertyField(sectionB);
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}