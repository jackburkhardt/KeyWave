using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Vector2")]
    public class CustomFieldType_Vector2 : CustomFieldType
    {
        
        public override string Draw(string currentValue, DialogueDatabase dataBase)
        {
            
            if (string.IsNullOrEmpty(currentValue)) currentValue = "Vector2(0,0)";

           EditorGUILayout.BeginHorizontal();
           
           var x = currentValue.Split(',')[0].Replace("Vector2(", "");
           var y = currentValue.Split(',')[1].Replace(")", "");


         

           var defaultWidth = EditorGUIUtility.labelWidth;
           EditorGUIUtility.labelWidth = 10;
           
           var vector = EditorGUILayout.Vector2Field( "", new Vector2(float.Parse(x.Replace("(", "")), float.Parse(y.Replace(")", ""))));
           
           EditorGUIUtility.labelWidth = defaultWidth;
           EditorGUILayout.EndHorizontal();
           
           return vector.ToString();


        }

        public override string Draw(GUIContent label, string currentValue, DialogueDatabase dataBase)
        {
            return Draw(currentValue, dataBase);
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase dataBase)
        {
            return Draw(currentValue, dataBase);
        }
        
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Vector2;
            }
        }
    }
}