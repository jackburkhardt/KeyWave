using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PointsPopupAttribute))]
public class PointsPopupDrawer : PropertyDrawer
{
    
    
    
    
    private DialogueDatabase titlesDatabase = null;
    private string[] titles = null;
    private bool showReferenceDatabase = false;
    private bool usePicker = true;
    private bool showFilter = false;
    private string filter = string.Empty;

     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) +
                (showReferenceDatabase ? EditorGUIUtility.singleLineHeight : 0) +
                (showFilter ? EditorGUIUtility.singleLineHeight : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Set up property drawer:
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            // Show database field if specified:
            showReferenceDatabase = (attribute as PointsPopupAttribute).showReferenceDatabase;
            showFilter = (attribute as PointsPopupAttribute).showFilter;
            if (EditorTools.selectedDatabase == null) EditorTools.SetInitialDatabaseIfNull();
            if (showReferenceDatabase)
            {
                var dbPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                position = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height - EditorGUIUtility.singleLineHeight);
                var newDatabase = EditorGUI.ObjectField(dbPosition, EditorTools.selectedDatabase, typeof(DialogueDatabase), true) as DialogueDatabase;
                if (newDatabase != EditorTools.selectedDatabase)
                {
                    EditorTools.selectedDatabase = newDatabase;
                    titlesDatabase = null;
                    titles = null;
                }
            }
            if (EditorTools.selectedDatabase == null) usePicker = false;

            // Filter:
            if (showFilter)
            {
                var filterLabelWidth = 48;
                var openButtonWidth = 48;
                EditorGUI.LabelField(new Rect(position.x, position.y, filterLabelWidth, EditorGUIUtility.singleLineHeight), "Filter:");
                EditorGUI.BeginChangeCheck();
                filter = EditorGUI.TextField(new Rect(position.x + filterLabelWidth, position.y, position.width - filterLabelWidth - openButtonWidth, EditorGUIUtility.singleLineHeight), filter);
                if (EditorGUI.EndChangeCheck())
                {
                    titles = null; // Need to update filtered title list.
                }
               
                position.y += EditorGUIUtility.singleLineHeight;
                position.height -= EditorGUIUtility.singleLineHeight;
            }

            // Set up titles array:
            if (titles == null || titlesDatabase != EditorTools.selectedDatabase) UpdateTitles(prop.stringValue, filter);

            // Update current index:
            var currentIndex = GetIndex(prop.stringValue);

            // Draw popup or plain text field:
            var rect = new Rect(position.x, position.y, position.width - 48, position.height);
            if (usePicker)
            {
                var newIndex = EditorGUI.Popup(rect, currentIndex, titles);
                if ((newIndex != currentIndex) && (0 <= newIndex && newIndex < titles.Length))
                {
                    currentIndex = newIndex;
                    prop.stringValue = titles[currentIndex];
                    GUI.changed = true;
                }
                if (GUI.Button(new Rect(position.x + position.width - 45, position.y, 18, 14), "x"))
                {
                    prop.stringValue = string.Empty;
                    currentIndex = -1;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                string value = EditorGUI.TextField(rect, prop.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.stringValue = value;
                }
            }

            // Radio button toggle between popup and plain text field:
            rect = new Rect(position.x + position.width - 22, position.y, 22, position.height);
            var newToggleValue = EditorGUI.Toggle(rect, usePicker, EditorStyles.radioButton);
            if (newToggleValue != usePicker)
            {
                usePicker = newToggleValue;
                if (usePicker && (EditorTools.selectedDatabase == null)) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
                titles = null;
            }

            EditorGUI.EndProperty();
        }

        public void UpdateTitles(string currentItem, string filter)
        {
            titlesDatabase = EditorTools.selectedDatabase;
            var foundCurrent = false;
            var list = new List<string>();
            var lowercaseFilter = string.IsNullOrEmpty(filter) ? string.Empty : filter.ToLower();
            if (titlesDatabase != null && titlesDatabase.items != null)
            {
                var points = titlesDatabase.items.Where(p => p.IsPointCategory).ToList();
                for (int i = 0; i < points.Count; i++)
                {
                    var name = points[i].Name;
                    if (!string.IsNullOrEmpty(filter) && !name.ToLower().Contains(filter)) continue;
                    list.Add(name);
                    if (string.Equals(currentItem, name))
                    {
                        foundCurrent = true;
                    }
                }
                if (!foundCurrent && !string.IsNullOrEmpty(currentItem))
                {
                    list.Add(currentItem);
                }
            }
            titles = list.ToArray();
        }

        public int GetIndex(string currentItem)
        {
            for (int i = 0; i < titles.Length; i++)
            {
                if (string.Equals(currentItem, titles[i])) return i;
            }
            return -1;
        }

    
}
