



using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEditor.Build.Content;

namespace PixelCrushers.DialogueSystem
{

 

   

    public enum Hour
    {
        _0 = 0,
        _1 = 1,
        _2 = 2,
        _3 = 3,
        _4 = 4,
        _5 = 5,
        _6 = 6,
        _7 = 7,
        _8 = 8,
        _9 = 9,
        _10 = 10,
        _11 = 11,
        _12 = 12,
        _13 = 13,
        _14 = 14,
        _15 = 15,
        _16 = 16,
        _17 = 17,
        _18 = 18,
        _19 = 19,
        _20 = 20,
        _21 = 21,
        _22 = 22,
        _23 = 23
    }

    public enum Minute
    {
        _00 = 0,
        _01 = 1,
        _02 = 2,
        _03 = 3,
        _04 = 4,
        _05 = 5,
        _06 = 6,
        _07 = 7,
        _08 = 8,
        _09 = 9,
        _10 = 10,
        _11 = 11,
        _12 = 12,
        _13 = 13,
        _14 = 14,
        _15 = 15,
        _16 = 16,
        _17 = 17,
        _18 = 18,
        _19 = 19,
        _20 = 20,
        _21 = 21,
        _22 = 22,
        _23 = 23,
        _24 = 24,
        _25 = 25,
        _26 = 26,
        _27 = 27,
        _28 = 28,
        _29 = 29,
        _30 = 30,
        _31 = 31,
        _32 = 32,
        _33 = 33,
        _34 = 34,
        _35 = 35,
        _36 = 36,
        _37 = 37,
        _38 = 38,
        _39 = 39,
        _40 = 40,
        _41 = 41,
        _42 = 42,
        _43 = 43,
        _44 = 44,
        _45 = 45,
        _46 = 46,
        _47 = 47,
        _48 = 48,
        _49 = 49,
        _50 = 50,
        _51 = 51,
        _52 = 52,
        _53 = 53,
        _54 = 54,
        _55 = 55,
        _56 = 56,
        _57 = 57,
        _58 = 58,
        _59 = 59
    }
    
    
    
    
    // STEP 1: Name your type below, replacing "My Type":
    [CustomFieldTypeService.Name("Time")]
    // STEP 2: Rename the class by changing TemplateType to your type name:
    public class CustomFieldType_TimeType : CustomFieldType
    {
        
        public override FieldType storeFieldAsType
        {
            get
            {
                return FieldType.Time;
            }
        }
        
        // STEP 3: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUILayout version.
        public override string Draw(string currentValue, DialogueDatabase database)
        {
           
            Rect r = EditorGUILayout.BeginHorizontal("IntField");
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);

            if (currentValue == string.Empty) return "00:00";
            
            var hour = Int32.Parse(currentValue.Split(':')[0]) == null ? 0 : Int32.Parse(currentValue.Split(':')[0]);
            var minute = Int32.Parse(currentValue.Split(':')[1]) == null ? 0 : Int32.Parse(currentValue.Split(':')[1]);

           
            var _hour = EditorGUILayout.EnumPopup((Hour)hour, GUILayout.MinWidth(0),GUILayout.ExpandWidth(false)).ToString().Substring(1);
            EditorGUILayout.LabelField(":", GUILayout.MinWidth(0),GUILayout.ExpandWidth(false),GUILayout.MaxWidth(10));
            var _minute = EditorGUILayout.EnumPopup((Minute)minute, GUILayout.MinWidth(0),GUILayout.ExpandWidth(false)).ToString().Substring(1);
            EditorGUILayout.EndHorizontal();

            return $"{_hour}:{_minute}";


            //return EditorGUILayout.TextField(currentValue);
        }

        // STEP 4: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUI version, which
        // uses an absolute Rect position instead of auto-layout.
        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            
            Rect r = EditorGUILayout.BeginHorizontal("TextField", GUILayout.Width(5));
            //EditorGUILayout.TextField("Conversation,Entry", currentValue);

            if (currentValue == string.Empty) return "00:00";
            
            var hour = Int32.Parse(currentValue.Split(':')[0]) == null ? 0 : Int32.Parse(currentValue.Split(':')[0]);
            var minute = Int32.Parse(currentValue.Split(':')[1]) == null ? 0 : Int32.Parse(currentValue.Split(':')[1]);

            var _hour = EditorGUILayout.EnumPopup("Hour", (Hour)hour).ToString().Substring(1);
            var _minute = EditorGUILayout.EnumPopup("Minute", (Minute)minute).ToString().Substring(1);
            EditorGUILayout.EndHorizontal();

            return $"{_hour}:{_minute}";
        }
    }
}



/**/
