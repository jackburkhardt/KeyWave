﻿// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    public enum BooleanType { True, False }

    /// <summary>
    /// This part of the Dialogue Editor window handles drawing a single field 
    /// and a list of fields.
    /// Drawing fields is complicated because a field can be one of several types.
    /// Actor fields need to provide a popup menu of the actors in the database,
    /// quest state fields need to provide a popup menu of the quest states, etc.
    /// </summary>
    public partial class DialogueEditorWindow
    {
        private List<string> textAreaFields => new List<string>() { "Description", "Success Description", "Failure Description", "Conditions", "Script"};
        private static readonly string[] questStateStrings = { "(None)", "unassigned", "active", "success", "failure", "done", "abandoned", "grantable", "returnToNPC" };
        private static readonly string[] actorStateStrings = { "(None)", "unidentified", "mentioned", "amicable", "botched", "unapproachable", "approachable", "confronted" };

        private bool showStateFieldAsQuest = true;

        private static GUIContent displayNameLabel = new GUIContent("Display Name", "The name to show in UIs.");

        private void DrawFieldsSection(List<Field> fields, List<string> primaryFieldTitles = null)
        {
            EditorWindowTools.StartIndentedSection();
            DrawFieldsHeading(primaryFieldTitles != null);
            DrawFieldsContent(fields, primaryFieldTitles);
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawFieldsHeading(bool isTemplate = false)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.TextField("Title");
            EditorGUILayout.TextField("Value");
            EditorGUILayout.TextField("Type");
            if (isTemplate) EditorGUILayout.LabelField("Main", GUILayout.Width(30));
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButton, GUILayout.Width(22));
            GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButton, GUILayout.Width(22));
            EditorGUI.EndDisabledGroup();
            GUILayout.Button(" ", "OL Minus", GUILayout.Width(16));
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFieldsContent(List<Field> fields, List<string> primaryFieldTitles = null)
        {
            int fieldToRemove = -1;
            int fieldToMoveUp = -1;
            int fieldToMoveDown = -1;
            bool isTemplate = (primaryFieldTitles != null);
            for (int i = 0; i < fields.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (IsTextAreaField(fields[i]))
                {
                    DrawTextAreaFirstPart(fields[i]);
                    if (isTemplate) DrawPrimaryFieldToggle(fields[i].title, primaryFieldTitles);
                    DrawFieldManipulationButtons(i, fields.Count, fields[i].title, ref fieldToRemove, ref fieldToMoveUp, ref fieldToMoveDown);
                    DrawTextAreaSecondPart(fields[i]);
                }
                else {
                    DrawField(fields[i]);
                    if (isTemplate) DrawPrimaryFieldToggle(fields[i].title, primaryFieldTitles);
                    DrawFieldManipulationButtons(i, fields.Count, fields[i].title, ref fieldToRemove, ref fieldToMoveUp, ref fieldToMoveDown);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (fieldToRemove >= 0)
            {
                // Sorry, a bit hacky, but we need to clean up localization when removing a template field:
                var confirmRemove = true;
                var scrubFromDatabase = false;
                var isTemplateTab = (toolbar.Current == Toolbar.Tab.Templates);
                var isLocalizationTemplate = (toolbar.Current == Toolbar.Tab.Templates) && (fields[fieldToRemove].type == FieldType.Localization);
                var fieldTitle = fields[fieldToRemove].title;
                if (isLocalizationTemplate)
                {
                    confirmRemove = EditorUtility.DisplayDialog("Delete Localization '" + fieldTitle + "'?", "This will also remove instances of this field from all assets in the database. Are you sure?", "OK", "Cancel");
                }
                else if (isTemplateTab)
                {
                    var result = EditorUtility.DisplayDialogComplex("Delete '" + fieldTitle + "'", "Delete this field from all assets in the database, or only from the template?", "Remove All", "Template Only", "Cancel");
                    confirmRemove = result != 2;
                    scrubFromDatabase = result == 0;
                }
                else if (string.Equals(fieldTitle, "Is Item") && toolbar.Current == Toolbar.Tab.Items)
                {
                    confirmRemove = EditorUtility.DisplayDialog("Delete 'Is Item' field?", "'Is Item' is a special built-in field that specifies whether this entry is an item or a quest. Unless you're really sure what you're doing, you shouldn't delete it. Are you sure you want to delete it?", "OK", "Cancel");
                }
                if (confirmRemove)
                {
                    fields.RemoveAt(fieldToRemove);
                    if (isLocalizationTemplate)
                    {
                        ScrubFieldFromDatabase(fieldTitle);
                        languages.Remove(fieldTitle);
                        SetDatabaseDirty("Remove Localization Template Field and all instances in database");
                    }
                    else if (isTemplateTab && scrubFromDatabase)
                    {
                        ScrubFieldFromCurrentDatabaseCategory(fieldTitle);
                        SetDatabaseDirty("Remove Field and all instances in database");
                    }
                    else
                    {
                        SetDatabaseDirty("Remove Field");
                    }
                }
            }
            if (fieldToMoveUp >= 0)
            {
                var field = fields[fieldToMoveUp];
                fields.RemoveAt(fieldToMoveUp);
                fields.Insert(fieldToMoveUp - 1, field);
                SetDatabaseDirty("Move Field Up");
            }
            if (fieldToMoveDown >= 0)
            {
                var field = fields[fieldToMoveDown];
                fields.RemoveAt(fieldToMoveDown);
                fields.Insert(fieldToMoveDown + 1, field);
                SetDatabaseDirty("Move Field Down");
            }
        }

        private void DrawPrimaryFieldToggle(string fieldTitle, List<String> primaryFieldTitles)
        {
            // Checkbox used only for template. Specifies which fields should be shown in the main editor section, not just All Fields foldout:
            var primary = primaryFieldTitles.Contains(fieldTitle);
            var newPrimary = EditorGUILayout.Toggle(new GUIContent(string.Empty, "Show in the main editor section, not just in the All Fields foldout. Some built-in fields are always shown in the main section regardless of this checkbox."), primary, GUILayout.Width(30));
            if (newPrimary != primary)
            {
                if (newPrimary)
                {
                    if (!primaryFieldTitles.Contains(fieldTitle)) primaryFieldTitles.Add(fieldTitle);
                }
                else
                {
                    primaryFieldTitles.Remove(fieldTitle);
                }
            }
        }

        private void DrawFieldManipulationButtons(int i, int fieldCount, string fieldTitle, ref int fieldToRemove, ref int fieldToMoveUp, ref int fieldToMoveDown)
        {
            // Up/down buttons:
            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButton, GUILayout.Width(22))) fieldToMoveUp = i;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(i == fieldCount - 1);
            if (GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButton, GUILayout.Width(22))) fieldToMoveDown = i;
            EditorGUI.EndDisabledGroup();

            // Delete button:
            if (GUILayout.Button(new GUIContent(" ", string.Format("Delete field {0}.", fieldTitle)), "OL Minus", GUILayout.Width(16))) fieldToRemove = i;
        }

        private void DrawMainSectionField(Field field)
        {
            EditorGUILayout.BeginHorizontal();
            if (field.typeString == "CustomFieldType_Text")
            {
                DrawTextArea(field);
            }
            else
            {
                DrawField(field, false, false);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRevisableTextField(GUIContent label, Asset asset, DialogueEntry entry, List<Field> fields, string fieldTitle)
        {
            Field field = Field.Lookup(fields, fieldTitle);
            if (field == null)
            {
                field = new Field(fieldTitle, string.Empty, FieldType.Text);
                fields.Add(field);
                SetDatabaseDirty("Create Field " + fieldTitle);
            }
            DrawRevisableTextField(label, asset, entry, field);
        }

        private void DrawRevisableTextField(GUIContent label, Asset asset, DialogueEntry entry, Field field)
        {
            if (field == null) return;
            EditorGUILayout.BeginHorizontal();
            field.value = EditorGUILayout.TextField(label, field.value);
            DrawAIReviseTextButton(asset, entry, field);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRevisableTextAreaField(GUIContent label, Asset asset, DialogueEntry entry, List<Field> fields, string fieldTitle)
        {
            Field field = Field.Lookup(fields, fieldTitle);
            if (field == null)
            {
                field = new Field(fieldTitle, string.Empty, FieldType.Text);
                fields.Add(field);
                SetDatabaseDirty("Create Field " + fieldTitle);
            }
            DrawRevisableTextAreaField(label, asset, entry, field);
        }

        private void DrawRevisableTextAreaField(GUIContent label, Asset asset, DialogueEntry entry, Field field)
        {
            EditorGUILayout.BeginHorizontal();
            if (label != null) EditorGUILayout.LabelField(label);
            DrawAIReviseTextButton(asset, entry, field);
            EditorGUILayout.EndHorizontal();
            field.value = EditorGUILayout.TextArea(field.value);
        }

        private void DrawLocalizableTextAreaField(GUIContent label, Asset asset, DialogueEntry entry, Field field)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            DrawAILocalizeTextButton(asset, entry, field);
            EditorGUILayout.EndHorizontal();
            field.value = EditorGUILayout.TextArea(field.value);
        }
        
        
        private void DrawColorField(GUIContent label, Asset asset, string fieldTitle)
        {
            Field colorField = Field.Lookup(asset.fields, fieldTitle);
            
            if (colorField == null)
            {
                colorField = new Field(fieldTitle, "FFFFFF", FieldType.Color);
                asset.fields.Add(colorField);
                SetDatabaseDirty("Create Color Field");
            }
            
            var color = EditorGUILayout.ColorField(label,
                EditorTools.NodeColorStringToColor(colorField.value), true, true, false);
            colorField.value = Tools.ToWebColor(color);
        }
        
        private void DrawTextArea(Field field)
        {
            EditorGUI.BeginChangeCheck();
            DrawTextAreaFirstPart(field, false, false);
            DrawTextAreaSecondPart(field);
            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty((field != null) ? field.title : string.Empty);
        }

        private void DrawTextAreaFirstPart(Field field, bool isTitleEditable = true, bool showMiddleField = true)
        {
            if (!isTitleEditable && !showMiddleField)
            {
                EditorGUILayout.LabelField(field.title);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!isTitleEditable);
                field.title = EditorGUILayout.TextField(field.title);
                EditorGUI.EndDisabledGroup();
                if (showMiddleField)
                {
                    GUI.enabled = false;
                    EditorGUILayout.TextField(" ");
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }
            }
            DrawFieldType(field);
        }

        private void DrawTextAreaSecondPart(Field field)
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            field.value = EditorGUILayout.TextArea(field.value);
        }

        private void DrawField(Field field, bool isTitleEditable = true, bool showType = true)
        {
            EditorGUI.BeginChangeCheck();
            if (isTitleEditable)
            {
                field.title = EditorGUILayout.TextField(field.title);
            }
            else
            {
                EditorGUILayout.LabelField(field.title);
            }

            // Custom field types:
            field.value = CustomFieldTypeService.DrawField(field, database);

            if (showType) DrawFieldType(field);
            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty((field != null) ? field.title : string.Empty);
        }

        private void DrawField(GUIContent label, Field field, bool showType = true)
        {
            EditorGUI.BeginChangeCheck();
            // Custom field types:
            field.value = CustomFieldTypeService.DrawField(label, field, database);

            if (showType) DrawFieldType(field);
            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty((field != null) ? field.title : string.Empty);
        }

        private void DrawFieldType(Field field)
        {
            // Custom field types:
            CustomFieldTypeService.DrawFieldType(field);
        }

        private bool IsTextAreaField(Field field)
        {
            if (field == null || field.title == null) return false;
            return textAreaFields.Contains(field.title) || IsQuestEntryDescription(field);
        }

        private bool IsQuestEntryDescription(Field field)
        {
            if (field == null || field.title == null) return false;
            return field.title.StartsWith("Entry ") && !field.title.EndsWith(" State") &&
                !field.title.EndsWith(" Count") && !field.title.EndsWith(" End") &&
                    !field.title.EndsWith(" ID");
        }

        private bool IsQuestStateField(Field field)
        {
            return showStateFieldAsQuest && (field != null) &&
                (string.Equals(field.title, "State") ||
                 (!string.IsNullOrEmpty(field.title) && field.title.EndsWith(" State")));
        }

        private string DrawQuestStateField(string value)
        {
            int index = 0;
            for (int i = 0; i < questStateStrings.Length; i++)
            {
                if (string.Equals(value, questStateStrings[i])) index = i;
            }
            int newIndex = EditorGUILayout.Popup(index, questStateStrings);
            return (newIndex == index)
                ? value
                : ((newIndex == 0) ? string.Empty : questStateStrings[newIndex]);
        }

        private string DrawActorStateField(string value)
        {
            int index = 0;
            for (int i = 0; i < actorStateStrings.Length; i++)
            {
                if (string.Equals(value, actorStateStrings[i])) index = i;
            }
            int newIndex = EditorGUILayout.Popup(index, actorStateStrings);
            return (newIndex == index)
                ? value
                : ((newIndex == 0) ? string.Empty : actorStateStrings[newIndex]);
        }

        private string DrawLocationField(GUIContent label, string value, string nullText, bool includeSublocations = true)
        {
            if (!includeSublocations)
            {
                var id = int.TryParse(value, out var result) ? result : 0;
                var location = database.GetLocation(id);
                if (location != null && location.IsSublocation && location.IsFieldAssigned("Parent Location"))
                {
                    value = location.LookupValue("Parent Location");
//                    Debug.Log("changed value: " + value);
                }
                
                var assetList = database != null ? GetAssetList<Location>(database.locations.Where(p => !p.IsSublocation).ToList()) : null;
                
                if (assetList == null) return value;

                for (int i = 0; i < assetList.IDs.Length; i++)
                {
                   if (assetList.names[i].text == "(None)")
                   {
                       assetList.names[i].text = nullText;
                   }
                }
                
                return DrawAssetPopup<Location>(value, assetList, label);
                
            }
            
            return DrawAssetPopup<Location>(value, (database != null) ? database.locations: null, label);
        }
        
        private string DrawSublocationField(GUIContent label, Location parentLocation, string value, string nullText)
        {
            if (parentLocation == null)
            {
                Debug.LogWarning("Dialogue System: Can't draw sublocation field because parent location is null.");
                return null;
            }


            var assetList = database != null
                ? GetAssetList<Location>(database.locations.Where(p =>
                        p.IsSublocation && p.AssignedField("Parent Location").value == parentLocation.id.ToString())
                    .ToList())
                : null;


            if (assetList == null) return value;
            
            for (int i = 0; i < assetList.IDs.Length; i++)
            {
                if (assetList.names[i].text == "(None)")
                {
                    assetList.names[i].text = nullText;
                }
                
                if (assetList.names[i].text.StartsWith($"{parentLocation.Name}/"))
                {
                    assetList.names[i].text = assetList.names[i].text.Substring(parentLocation.Name.Length + 1);
                }
            }
            
            return DrawAssetPopup<Location>(value, assetList, label);
        }
        
        private string DrawSublocationField(GUIContent label, Location parentLocation, string value, string nullText, out string newValue)
        {
            newValue = DrawSublocationField(label, parentLocation, value, nullText);
            return newValue;
        }

        private string DrawActorField(GUIContent label, string value)
        {
            var result =  DrawAssetPopup<Actor>(value, (database != null) ? database.actors : null, label);
            return result;
        }
        
        private string DrawActorField(GUIContent label, string value, out Actor actor)
        {
         
            var result = DrawActorField( label, value);
            actor = (string.IsNullOrEmpty(result)) ? null : database.GetActor(int.Parse(result));
            return result;
        }
        
        private string DrawQuestfield(GUIContent label, string value)
        {
            var databaseItems = database != null ? database.items.Where(p => p.IsQuest).ToList() : null;
            var result = DrawAssetPopup<Item>(value, databaseItems, label);
            return result;
        }

        private string DrawQuestStateField(GUIContent label, string value)
        {
            int index = 0;
            for (int i = 0; i < questStateStrings.Length; i++)
            {
                if (string.Equals(value, questStateStrings[i])) index = i;
            }
            int newIndex = EditorGUILayout.Popup(label.text, index, questStateStrings);
            return (newIndex == index)
                ? value
                : ((newIndex == 0) ? string.Empty : questStateStrings[newIndex]);
        }

        private string DrawQuestStateFieldTruncated(GUIContent label, string value)
        {
            int index = 0;
            var truncatedQuestStateStrings = new string[] { "unassigned", "active", "success" };
            for (int i = 0; i < truncatedQuestStateStrings.Length; i++)
            {
                if (string.Equals(value, truncatedQuestStateStrings[i])) index = i;
            }
            int newIndex = EditorGUILayout.Popup(label.text, index, truncatedQuestStateStrings);
            return (newIndex == index)
                ? value
                : ((newIndex == 0) ? string.Empty : truncatedQuestStateStrings[newIndex]);
        }

        private string DrawActorStateField(GUIContent label, string value)
        {
            int index = 0;
            for (int i = 0; i < actorStateStrings.Length; i++)
            {
                if (string.Equals(value, actorStateStrings[i])) index = i;
            }
            int newIndex = EditorGUILayout.Popup(label.text, index, actorStateStrings);
            return (newIndex == index)
                ? value
                : ((newIndex == 0) ? string.Empty : actorStateStrings[newIndex]);
        }

        public string DrawAssetPopup<T>(string value, List<T> assets, GUIContent assetLabel) where T : Asset
        {
            if (assets != null)
            {
                AssetList assetList = GetAssetList<T>(assets);
                return DrawAssetPopup<T>(value, assetList, assetLabel);
            }
            else {
                EditorGUILayout.LabelField("(no database)");
                return value;
            }
        }
        
        private string DrawAssetPopup<T>(string value, AssetList assetList, GUIContent assetLabel) where T : Asset
        {
            int id = -1;
            int.TryParse(value, out id);
            int index = assetList.GetIndex(id);
            int newIndex;
            if ((assetLabel == null) || string.IsNullOrEmpty(assetLabel.text))
            {
                newIndex = EditorGUILayout.Popup(index, assetList.names);
            }
            else {
                newIndex = EditorGUILayout.Popup(assetLabel, index, assetList.names);
            }
            return (newIndex != index) ? assetList.GetID(newIndex) : value;
        }


        public string DrawAssetPopup<T>(Rect rect, string value, List<T> assets, GUIContent assetLabel) where T : Asset
        {
            if (assets != null)
            {
                AssetList assetList = GetAssetList<T>(assets);
                int id = -1;
                int.TryParse(value, out id);
                int index = assetList.GetIndex(id);
                int newIndex;
                if ((assetLabel == null) || string.IsNullOrEmpty(assetLabel.text))
                {
                    newIndex = EditorGUI.Popup(rect, index, assetList.names);
                }
                else
                {
                    newIndex = EditorGUI.Popup(rect, assetLabel, index, assetList.names);
                }
                return (newIndex != index) ? assetList.GetID(newIndex) : value;
            }
            else
            {
                EditorGUILayout.LabelField("(no database)");
                return value;
            }
        }
        
        
        private string DrawLabeledAssetPopup<T>(string label, string value, List<T> assets) where T : Asset
        {
            AssetList assetList = GetAssetList<T>(assets);
            int index = -1;
            int.TryParse(value, out index);
            int newIndex = EditorGUILayout.Popup(new GUIContent(label, string.Empty), index, assetList.names);
            return (newIndex != index) ? assetList.GetID(newIndex) : value;
        }

        private static BooleanType StringToBooleanType(string s)
        {
            return (string.Compare(s, "true", System.StringComparison.OrdinalIgnoreCase) == 0) ? BooleanType.True : BooleanType.False;
        }

        private int StringToInt(string s, int defaultValue)
        {
            int result;
            return int.TryParse(s, out result) ? result : defaultValue;
        }

        private float StringToFloat(string s, int defaultValue)
        {
            float result;
            return float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result) ? result : defaultValue;
        }

        private void EditTextField(List<Field> fields, string fieldTitle, string tooltip, bool isTextArea)
        {
            EditTextField(fields, fieldTitle, fieldTitle, tooltip, isTextArea, null);
        }

        private void EditTextField(List<Field> fields, string fieldTitle, string tooltip, bool isTextArea, List<Field> alreadyDrawn)
        {
            EditTextField(fields, fieldTitle, fieldTitle, tooltip, isTextArea, alreadyDrawn);
        }

        private void EditTextField(List<Field> fields, string fieldTitle, string label, string tooltip, bool isTextArea)
        {
            EditTextField(fields, fieldTitle, label, tooltip, isTextArea, null);
        }

        private void EditTextField(List<Field> fields, string fieldTitle, string label, string tooltip, bool isTextArea, List<Field> alreadyDrawn)
        {
            Field field = Field.Lookup(fields, fieldTitle);
            if (field == null)
            {
                field = new Field(fieldTitle, string.Empty, FieldType.Text);
                fields.Add(field);
                SetDatabaseDirty("Create Field " + fieldTitle);
            }
            EditorGUI.BeginChangeCheck();
            if (isTextArea)
            {
                EditorGUILayout.LabelField(new GUIContent(label, tooltip));
                field.value = EditorGUILayout.TextArea(field.value);
            }
            else {
                field.value = EditorGUILayout.TextField(new GUIContent(label, tooltip), field.value);
            }
            if (EditorGUI.EndChangeCheck()) SetDatabaseDirty(fieldTitle);
            if (alreadyDrawn != null) alreadyDrawn.Add(field);
        }

    }

}