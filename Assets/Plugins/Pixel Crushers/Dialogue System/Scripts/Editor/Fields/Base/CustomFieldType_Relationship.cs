using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Relationship")]
    public class CustomFieldType_Relationship : CustomFieldType
    {
        private static readonly string[] relationshipStrings = {"(None)",  "Very Positive", "Positive", "Neutral",  "Negative", "Very Negative"};

        public override string Draw(string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(GetCurrentIndex(currentValue), relationshipStrings);
            return relationshipStrings[index];
        }

        public override string Draw(GUIContent label, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(label.text, GetCurrentIndex(currentValue), relationshipStrings);
            return relationshipStrings[index];
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUI.Popup(rect, GetCurrentIndex(currentValue), relationshipStrings);
            return relationshipStrings[index];
        }

        private int GetCurrentIndex(string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue)) currentValue = relationshipStrings[0];
            for (int i = 0; i < relationshipStrings.Length; i++)
            {
                var item = relationshipStrings[i];
                if (item == currentValue)
                {
                    return i;
                }
            }
            return 0;
        }
        
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Relationship;
            }
        }
    }
}