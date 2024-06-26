using Project.Runtime.Scripts.UI;
using UnityEditor;

namespace Project.Editor.Scripts
{
    [CustomEditor(typeof(CircularUIMenuPanel))]
    public class CircularMenuPanelEditor : CustomMenuPanelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        
            serializedObject.Update();
        
            // Your code here to show your custom fields. Example:
        
            for (int i = 0; i < CircularUIMenuPanel.CustomFields.Count; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CircularUIMenuPanel.CustomFields[i]), true);
            }
        
            serializedObject.ApplyModifiedProperties();
        }
    }
}