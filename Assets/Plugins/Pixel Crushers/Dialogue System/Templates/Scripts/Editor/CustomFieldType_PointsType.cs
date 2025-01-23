



using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.Rendering;

namespace PixelCrushers.DialogueSystem
{

    public enum PointsType
    {
        Wellness,
        Teamwork,
        Context,
        Skills
    }
    
    
    // STEP 1: Name your type below, replacing "My Type":
    [CustomFieldTypeService.Name("Points")]

    // STEP 2: Rename the class by changing TemplateType to your type name:
    public class CustomFieldType_PointsType : CustomFieldType
    {
        
        
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Points;
            }
        }
        
        // STEP 3: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUILayout version.
        public override string Draw(string currentValue, DialogueDatabase database)
        {
            
            
            if (currentValue == string.Empty || currentValue.Split(':').Length < 2) return "Wellness:0";
           
            Rect r = EditorGUILayout.BeginHorizontal();

            var pointType =
                Enum.TryParse<PointsType>(currentValue.Split(':')[0], out var validType) ? validType : PointsType.Wellness;
            
            var pointValue = currentValue.Split(':')[1] == null ? 0 : Int32.Parse(currentValue.Split(':')[1]);
            
            var type = EditorGUILayout.EnumPopup(pointType, GUILayout.MinWidth(0),GUILayout.ExpandWidth(false));
            
            if (type == null) type = PointsType.Wellness;
            
            var value = EditorGUILayout.IntField(pointValue, GUILayout.MinWidth(0),GUILayout.ExpandWidth(false));
            
            EditorGUILayout.EndHorizontal();
            
            
            return $"{type}:{value}";


            //return EditorGUILayout.TextField(currentValue);
        }

        // STEP 4: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUI version, which
        // uses an absolute Rect position instead of auto-layout.
        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            if (currentValue == string.Empty || currentValue.Split(':').Length < 2) return "Wellness:0";
            
            //Rect r = EditorGUILayout.BeginVertical("TextField");
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);
            
          //  if (currentValue == string.Empty) return "0,0";
            
          var pointsType =
              Enum.TryParse<PointsType>(currentValue.Split(':')[0], out var validType) ? validType : PointsType.Wellness;
          
            
            var pointsValue = Int32.Parse(currentValue.Split(':')[1]) == null ? 0 : Int32.Parse(currentValue.Split(':')[1]);

            var label1 = "";
            var label2 = " Amount";

            var firstRectHalfPercentage = 0.5f; //(float)label1.Length / ((float)label1.Length + (float)label2.Length);
            
            var firstRectHalf = new Rect(rect.position, new Vector2(rect.width * firstRectHalfPercentage, rect.height));
            
            var secondRectHalf = new Rect(new Vector2(rect.position.x + rect.width * firstRectHalfPercentage, rect.position.y), new Vector2(rect.width * (1 - firstRectHalfPercentage), rect.height));



            EditorGUIUtility.wideMode = false;
            EditorGUIUtility.labelWidth = 0;
            
            EditorGUI.indentLevel = 0;
           

          
          
          
            GUIContent label = new GUIContent(label1);
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(label).x;
           
            var type = EditorGUI.EnumPopup(firstRectHalf, label1, pointsType);
            
            
            label = new GUIContent(label2);
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(label).x;
          
            var value = EditorGUI.IntField(secondRectHalf, label2, pointsValue);

            return $"{type}:{value}";
        }
    }
}



/**/
