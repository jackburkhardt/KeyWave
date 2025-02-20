



using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Linq;
using UnityEditor.Build.Content;

namespace PixelCrushers.DialogueSystem
{
    
    // STEP 1: Name your type below, replacing "My Type":
    [CustomFieldTypeService.Name("Color")]
    // STEP 2: Rename the class by changing TemplateType to your type name:
    public class CustomFieldType_Color : CustomFieldType
    {
        
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Color;
            }
        }
        
        // STEP 3: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUILayout version.
        public override string Draw(string currentValue, DialogueDatabase database)
        {
            if (currentValue == string.Empty) return "#000000";
            
            Rect r = EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);
            var value = EditorTools.NodeColorStringToColor(currentValue);
            var nodeColor = EditorGUILayout.ColorField(GUIContent.none, value, true, true, false);
            
            EditorGUILayout.EndHorizontal();
            return Tools.ToWebColor(nodeColor);
            //return EditorGUILayout.TextField(currentValue);
        }

        // STEP 4: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUI version, which
        // uses an absolute Rect position instead of auto-layout.
        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
           
            //Rect r = 
          //  Rect r = EditorGUILayout.BeginHorizontal("IntField");
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);

            if (currentValue == string.Empty) return "#000000";


            var color = EditorGUI.ColorField(rect, GUIContent.none, EditorTools.NodeColorStringToColor(currentValue), true, true, false);
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);
            
            
           return Tools.ToWebColor(color);
        }
    }
}



/**/
