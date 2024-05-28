using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Email State")]
    public class CustomFieldType_EmailState : CustomFieldType
    {
        private static readonly string[] emailStateStrings = {"(None)", "unread", "read"};

        public override string Draw(string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(GetCurrentIndex(currentValue), emailStateStrings);
            return emailStateStrings[index];
        }

        public override string Draw(GUIContent label, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(label.text, GetCurrentIndex(currentValue), emailStateStrings);
            return emailStateStrings[index];
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUI.Popup(rect, GetCurrentIndex(currentValue), emailStateStrings);
            return emailStateStrings[index];
        }

        private int GetCurrentIndex(string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue)) currentValue = emailStateStrings[0];
            for (int i = 0; i < emailStateStrings.Length; i++)
            {
                var item = emailStateStrings[i];
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
                return FieldType.EmailState;
            }
        }
    }
}