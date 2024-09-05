using System;
using DG.DemiEditor;
using Project.Runtime.Scripts.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace Project.Editor.Scripts
{
    
    [CustomPropertyDrawer(typeof(AudioClipDatabase.AudioData))]
    public class AudioClipDatabaseEditor : PropertyDrawer
    {
        private const int rows = 6;
        
       // private bool[] foldouts;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /*
            if (foldouts == null || foldouts.Length != property.arraySize)
            {
                foldouts = new bool[property.arraySize];
                Array.Fill(foldouts, true);
            }
            */
            
           var currentYposition = position.y;
           
           var expandedRect = new Rect(position.x , position.y , position.width, base.GetPropertyHeight(property, label));
           
            property.isExpanded = EditorGUI.Foldout(expandedRect, property.isExpanded, label);

            EditorGUI.BeginProperty(position, label, property);

            
            
            if (property.isExpanded)
            {
                var clipAddressRect = GetRect(position, 1, out currentYposition, property, label);
            
                EditorGUI.PropertyField(clipAddressRect, property.FindPropertyRelative("clipAddress"), new GUIContent("Clip Address"));
            
               
            
                //   EditorGUI.PropertyField(position, property.FindPropertyRelative("pauseAddress"), label);
         
                var volumeRect = GetRect(position, 2, out currentYposition, property, label);
         
                EditorGUI.Slider(volumeRect, property.FindPropertyRelative("volume"), 0, 1, new GUIContent("Volume"));
            
                var channelRect = GetRect(position, 3, out currentYposition, property, label);
            
                EditorGUI.PropertyField(channelRect, property.FindPropertyRelative("channel"), new GUIContent("Channel"));
                
              
                
                var variantsRect = GetRect(position, 4, out currentYposition, property, label);

                var channel = property.FindPropertyRelative("channel").objectReferenceValue as AudioMixerGroup;
                
                if (channel.name.Contains("Music"))
                {
                    EditorGUI.PropertyField(variantsRect, property.FindPropertyRelative("includeVariants"), new GUIContent("Include Variants"));
                    
                    if (property.FindPropertyRelative("includeVariants").boolValue)
                    {
                        var variantArray = property.FindPropertyRelative("Variants");
                        
                        var variantArrayRect = GetRect(position, 5, out currentYposition, property, label);
                        
                        EditorGUI.PropertyField(variantArrayRect, variantArray, true);


                    }
                }
            }
            
            EditorGUI.EndProperty();
        }
        
        private Rect GetRect(Rect position, int row, out float yPosition, SerializedProperty property,  GUIContent label)
        {
            var rect = new Rect(position.x, position.y + base.GetPropertyHeight(property, label) * row, position.width, base.GetPropertyHeight(property, label));
            yPosition = rect.y;
            return rect;
        }
        
        private float VariantArrayHeight(SerializedProperty property, GUIContent label)
        {
            var variantArray = property.FindPropertyRelative("Variants");
            var arraySize = variantArray.arraySize;
            
            var baseHeight = base.GetPropertyHeight(variantArray, label);

            var elementHeight = baseHeight * 4;

            var totalHeight = 0f;
            
            for (int i = 0; i < arraySize; i++)
            {
                var element = variantArray.GetArrayElementAtIndex(i);
                totalHeight += element.isExpanded ? elementHeight : baseHeight;
            }
            
            return totalHeight + baseHeight * 3;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var variantArray = property.FindPropertyRelative("Variants");
            var includeVariants = property.FindPropertyRelative("includeVariants").boolValue;
           
            return base.GetPropertyHeight(property, label) * (property.isExpanded ? rows : 1) + (variantArray.isExpanded ? VariantArrayHeight(property, label) : 0) * (property.isExpanded && includeVariants ? 1 : 0);  // assuming original is one row
        }
        
        
    }
}