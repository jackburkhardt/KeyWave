using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Actor State")]
    public class CustomFieldType_ActorState : CustomFieldType
    {
        private static readonly string[] actorStateString = { "(None)", "unidentified", "mentioned", "amicable", "botched", "unapproachable", "confronted" };

        public override string Draw(string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(GetCurrentIndex(currentValue), actorStateString);
            return actorStateString[index];
        }

        public override string Draw(GUIContent label, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(label.text, GetCurrentIndex(currentValue), actorStateString);
            return actorStateString[index];
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUI.Popup(rect, GetCurrentIndex(currentValue), actorStateString);
            return actorStateString[index];
        }

        private int GetCurrentIndex(string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue)) currentValue = actorStateString[0];
            for (int i = 0; i < actorStateString.Length; i++)
            {
                var item = actorStateString[i];
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
                return FieldType.ActorState;
            }
        }
    }
}