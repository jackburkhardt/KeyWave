



using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    
    
    // STEP 1: Name your type below, replacing "My Type":
    [CustomFieldTypeService.Name("Node")]

    // STEP 2: Rename the class by changing TemplateType to your type name:
    public class CustomFieldType_NodeType : CustomFieldType
    {
        
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Node;
            }
        }
        
        // STEP 3: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUILayout version.
        public override string Draw(string currentValue, DialogueDatabase database)
        {
           
            Rect r = EditorGUILayout.BeginVertical("TextField");
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);

            if (currentValue == string.Empty) return "0,0";
            
            var conversationID = Int32.Parse(currentValue.Split(',')[0]) == null ? 0 : Int32.Parse(currentValue.Split(',')[0]);
            var entryID = Int32.Parse(currentValue.Split(',')[1]) == null ? 0 : Int32.Parse(currentValue.Split(',')[1]);
            var conversation = EditorGUILayout.IntField("Conversation ID", conversationID);
            var entry = EditorGUILayout.IntField("Entry ID", entryID);
            EditorGUILayout.EndVertical();


            return $"{conversation},{entry}";


            //return EditorGUILayout.TextField(currentValue);
        }

        // STEP 4: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUI version, which
        // uses an absolute Rect position instead of auto-layout.
        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            
            Rect r = EditorGUILayout.BeginVertical("TextField");
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);
            
            if (currentValue == string.Empty) return "0,0";
            
            var conversationID = Int32.Parse(currentValue.Split(',')[0]) == null ? 0 : Int32.Parse(currentValue.Split(',')[0]);
            var entryID = Int32.Parse(currentValue.Split(',')[1]) == null ? 0 : Int32.Parse(currentValue.Split(',')[1]);
            var conversation = EditorGUILayout.IntField("Conversation ID", conversationID);
            var entry = EditorGUILayout.IntField("Entry ID", entryID);
            EditorGUILayout.EndVertical();


            return $"{conversation},{entry}";

        }
    }
}



/**/
