using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using NaughtyAttributes.Editor;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using NaughtyAttributes;
using Unity.VisualScripting;

[CustomEditor(typeof(CustomVerticalLayoutGroup))]
public class CustomVerticalLayoutGroupEditor : Editor {
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
