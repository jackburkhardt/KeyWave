using Project.Runtime.Scripts.UI;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Scripts
{
    [CustomEditor(typeof(CustomVerticalLayoutGroup))]
    public class GameObjectStateSetterEditor : UnityEditor.Editor {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var list = serializedObject.FindProperty("gameObjects");
            
            var listSize = list.arraySize;
            listSize = EditorGUILayout.IntField("Number of Game Objects", listSize);
            
            EditorGUILayout.LabelField("Add a new item with a button");
            
            var t = (GameObjectStateSetter)target;
   
            if(GUILayout.Button("Add New")){
                t.gameObjects.Add(new GameObjectStateSetter.GameObjectState());
            }
            
            for (int i = 0; i < list.arraySize; i++)
            {
                var listRef = list.GetArrayElementAtIndex(i);
                var gameObject = listRef.FindPropertyRelative("gameObject");
                var trigger = listRef.FindPropertyRelative("trigger");
                var state = listRef.FindPropertyRelative("state");
                
                EditorGUILayout.PropertyField(gameObject);
                EditorGUILayout.PropertyField(trigger);
                EditorGUILayout.PropertyField(state);
                
                if (GUILayout.Button("Remove"))
                {
                    t.gameObjects.RemoveAt(i);
                }
                
            }
        
            serializedObject.ApplyModifiedProperties();
        }
    }
}