// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System;
using GUIStyle = UnityEngine.GUIStyle;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Items tab. Since the quest system
    /// also uses the Item table, this part handles quests as well as standard items.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [SerializeField]
        private AssetFoldouts itemFoldouts = new AssetFoldouts();

        [SerializeField]
        private string itemFilter = string.Empty;

        [SerializeField]
        private bool hideFilteredOutItems = false;

        private bool needToBuildLanguageListFromItems = true;

        private ReorderableList itemReorderableList = null;

        [SerializeField]
        private int itemListSelectedIndex = -1;

        [SerializeField]
        private int questEntrySelectedIdx = -1;
        
        [SerializeField]
        private int repeatEntrySelectedIdx = -1;

        [SerializeField]
        private bool showCompactQuestEntryList = true; // Thanks to Tasta for compact view idea.
        
        [SerializeField]
        private bool showCompactRepeatEntryList = true;

        private HashSet<int> syncedItemIDs = null;

        private int isAddingNewFieldToEntryNumber = -1;
        private Field newEntryField;

        private List<Item> filteredItems;

        private static GUIContent questDescriptionLabel = new GUIContent("Description", "The description when the quest is active.");
        private static GUIContent questSuccessDescriptionLabel = new GUIContent("Success Description", "The description when the quest has been completed successfully. If blank, the Description field is used.");
        private static GUIContent questFailureDescriptionLabel = new GUIContent("Failure Description", "The description when the quest has failed. If blank, the Description field is used.");
        private static GUIContent actionConditionsLabel = new GUIContent("Conditions", "The conditions that must be met to use this action.");
        private static GUIContent groupLabel = new GUIContent("Group", "Use to categorize quests into groups.");
        private static GUIContent descriptionLabel = new GUIContent("Description", "The description of the asset.");

        private void ResetItemSection()
        {
            itemFoldouts = new AssetFoldouts();
            itemAssetList = null;
            UpdateTreatItemsAsQuests(template.treatItemsAsQuests);
            UpdateTreatQuestsAsActions(template.treatQuestsAsActions);
            needToBuildLanguageListFromItems = true;
            itemReorderableList = null;
            itemListSelectedIndex = -1;
            syncedItemIDs = null;
        }

        private void UpdateTreatItemsAsQuests(bool newValue)
        {
            if (newValue != template.treatItemsAsQuests)
            {
                template.treatItemsAsQuests = newValue;
                toolbar.UpdateTabNames(newValue);
            }
        }
        
        private void UpdateTreatQuestsAsActions(bool newValue)
        {
            if (newValue != template.treatQuestsAsActions)
            {
                template.treatQuestsAsActions = newValue;
                toolbar.UpdateTabNames(newValue);
            }
        }

        private void BuildLanguageListFromItems()
        {
            if (database == null || database.items == null) return;
            database.items.ForEach(item => { if (item.fields != null) BuildLanguageListFromFields(item.fields); });
            needToBuildLanguageListFromItems = false;
        }

        private void DrawItemSection()
        {
            if (template.treatItemsAsQuests)
            {
                if (needToBuildLanguageListFromItems) BuildLanguageListFromItems();
                if (itemReorderableList == null) InitializeItemReorderableList();
                var filterChanged = DrawFilterMenuBar("Quests/Item", DrawItemMenu, ref itemFilter, ref hideFilteredOutItems);
                if (filterChanged) InitializeItemReorderableList();
                if (database.syncInfo.syncItems)
                {
                    DrawItemSyncDatabase();
                    if (syncedItemIDs == null) RecordSyncedItemIDs();
                }
                itemReorderableList.DoLayoutList();
            }
            else
            {
                if (itemReorderableList == null) InitializeItemReorderableList();
                var filterChanged = DrawFilterMenuBar("Item", DrawItemMenu, ref itemFilter, ref hideFilteredOutItems);
                if (filterChanged) InitializeItemReorderableList();
                if (database.syncInfo.syncItems) DrawItemSyncDatabase();
                itemReorderableList.DoLayoutList();
            }
        }

        private bool HideFilteredOutItems()
        {
            return hideFilteredOutItems && !string.IsNullOrEmpty(itemFilter);
        }

        private void InitializeItemReorderableList()
        {
            if (HideFilteredOutItems())
            {
                filteredItems = database.items.FindAll(item => EditorTools.IsAssetInFilter(item, itemFilter));
                itemReorderableList = new ReorderableList(filteredItems, typeof(Item), true, true, true, true);
            }
            else
            {
                filteredItems = database.items;
                itemReorderableList = new ReorderableList(database.items, typeof(Item), true, true, true, true);
            }
            
            itemReorderableList.drawHeaderCallback = DrawItemListHeader;
            itemReorderableList.drawElementCallback = DrawItemListElement;
            itemReorderableList.drawElementBackgroundCallback = DrawItemListElementBackground;
            if (template.treatItemsAsQuests)
            {
                itemReorderableList.onAddDropdownCallback = OnAddItemOrQuestDropdown;
            }
            else
            {
                itemReorderableList.onAddCallback = OnItemListAdd;
            }            
            itemReorderableList.onRemoveCallback = OnItemListRemove;
            itemReorderableList.onSelectCallback = OnItemListSelect;
            itemReorderableList.onReorderCallback = OnItemListReorder;
        }

        private const float ItemReorderableListTypeWidth = 40f;

        private void DrawItemListHeader(Rect rect)
        {
            if (template.treatItemsAsQuests)
            {
                var fieldWidth = (rect.width - 14 - ItemReorderableListTypeWidth) / 4;
                EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, ItemReorderableListTypeWidth, rect.height), "Type");
                EditorGUI.LabelField(new Rect(rect.x + 14 + ItemReorderableListTypeWidth, rect.y, fieldWidth, rect.height), "Name");
                EditorGUI.LabelField(new Rect(rect.x + 14 + ItemReorderableListTypeWidth + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
            }
            else
            {
                var fieldWidth = (rect.width - 14) / 4;
                EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, fieldWidth, rect.height), "Name");
                EditorGUI.LabelField(new Rect(rect.x + 14 + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
            }
        }

        private void DrawItemListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < filteredItems.Count)) return;
            var nameControl = "ItemName" + index;
            var descriptionControl = "ItemDescription" + index;
            var item = filteredItems[index];
            var itemName = item.Name;
            var description = item.Description;
            EditorGUI.BeginDisabledGroup(!EditorTools.IsAssetInFilter(item, itemFilter) || IsItemSyncedFromOtherDB(item));
            if (template.treatItemsAsQuests)
            {
                var fieldWidth = (rect.width - ItemReorderableListTypeWidth) / 4;
                EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, ItemReorderableListTypeWidth, EditorGUIUtility.singleLineHeight), item.IsItem ? "Item" : item.IsAction ? "Action" : "Quest");
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(nameControl);
                itemName = EditorGUI.TextField(new Rect(rect.x + ItemReorderableListTypeWidth, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, item.Name);
                if (EditorGUI.EndChangeCheck()) item.Name = itemName;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(descriptionControl);
                description = EditorGUI.TextField(new Rect(rect.x + ItemReorderableListTypeWidth + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
                if (EditorGUI.EndChangeCheck()) item.Description = description;
            }
            else
            {
                var fieldWidth = rect.width / 4;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(nameControl);
                itemName = EditorGUI.TextField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, item.Name);
                if (EditorGUI.EndChangeCheck()) item.Name = itemName;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(descriptionControl);
                description = EditorGUI.TextField(new Rect(rect.x + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
                if (EditorGUI.EndChangeCheck()) item.Description = description;
            }
            EditorGUI.EndDisabledGroup();
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.Equals(nameControl, focusedControl) || string.Equals(descriptionControl, focusedControl))
            {
                inspectorSelection = item;
            }
        }

        private void DrawItemListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < filteredItems.Count)) return;
            var item = filteredItems[index];
            if (EditorTools.IsAssetInFilter(item, itemFilter))
            {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, true);
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.225f, 0.225f, 0.225f, 1));
            }
        }

        private void OnAddItemOrQuestDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Quest"), false, OnAddNewQuest, null);
            menu.AddItem(new GUIContent("Item"), false, OnAddNewItem, null);
            menu.AddItem(new GUIContent("Action"), false, OnAddNewAction, null);
            menu.ShowAsContext();
        }

        private void OnItemListAdd(ReorderableList list)
        {
            AddNewItem();
        }

        private void OnItemListRemove(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.items.Count)) return;
            var item = database.items[list.index];
            if (item == null) return;
            if (IsItemSyncedFromOtherDB(item)) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(item)), "Are you sure you want to delete this?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (deletedLastOne) inspectorSelection = null;
                else inspectorSelection = (list.index < list.count) ? database.items[list.index] : (list.count > 0) ? database.items[list.count - 1] : null;
                SetDatabaseDirty("Remove Item");
            }
        }

        private void OnItemListReorder(ReorderableList list)
        {
            SetDatabaseDirty("Reorder Items");
        }

        private void OnItemListSelect(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.items.Count)) return;
            inspectorSelection = database.items[list.index];
            itemListSelectedIndex = list.index;
        }

        public void DrawSelectedItemSecondPart()
        {
            var item = inspectorSelection as Item;
            if (item == null) return;
            DrawFieldsFoldout<Item>(item, itemListSelectedIndex, itemFoldouts);
            DrawAssetSpecificPropertiesSecondPart(item, itemListSelectedIndex, itemFoldouts);
        }

        private void DrawItemMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New Item"), false, AddNewItem);
                if (template.treatItemsAsQuests)
                {
                    menu.AddItem(new GUIContent("New Quest"), false, AddNewQuest);
                    
                    if (template.treatQuestsAsActions)
                    {
                        menu.AddItem(new GUIContent("New Action"), false, AddNewAction);
                    }
                    
                    else 
                    {
                        menu.AddDisabledItem(new GUIContent("New Action"));
                    }
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("New Quest"));
                }
                menu.AddItem(new GUIContent("Use Quest System"), template.treatItemsAsQuests, ToggleUseQuestSystem);
                menu.AddItem(new GUIContent("Use Action System"), template.treatQuestsAsActions, ToggleUseActionSystem);
                menu.AddItem(new GUIContent("Sort/By Name"), false, SortItemsByName);
                menu.AddItem(new GUIContent("Sort/By Group"), false, SortItemsByGroup);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortItemsByID);
                menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncItems, ToggleSyncItemsFromDB);
                menu.ShowAsContext();
            }
        }

        private void OnAddNewItem(object data)
        {
            AddNewItem();
        }

        private void OnAddNewQuest(object data)
        {
            AddNewQuest();
        }
        
        private void OnAddNewAction(object data)
        {
            AddNewAction();
        }

        private void AddNewItem()
        {
            AddNewAssetFromTemplate<Item>(database.items, (template != null) ? template.itemFields : null, "Item");
            SetDatabaseDirty("Add New Item");
        }

        private void AddNewQuest()
        {
            AddNewAssetFromTemplate<Item>(database.items, (template != null) ? template.questFields : null, "Quest");
            BuildLanguageListFromItems();
            SetDatabaseDirty("Add New Quest");
        }
        
        private void AddNewAction()
        {
            AddNewAssetFromTemplate<Item>(database.items, (template != null) ? template.actionFields : null, "Action");
            BuildLanguageListFromItems();
            SetDatabaseDirty("Add New Action");
        }

        private void SortItemsByName()
        {
            database.items.Sort((x, y) => x.Name.CompareTo(y.Name));
            SetDatabaseDirty("Sort by Name");
        }

        private void SortItemsByGroup()
        {
            database.items.Sort((x, y) => (x.Group == null) ? -1 : x.Group.CompareTo(y.Group));
            SetDatabaseDirty("Sort by Group");
        }

        private void SortItemsByID()
        {
            database.items.Sort((x, y) => x.id.CompareTo(y.id));
            SetDatabaseDirty("Sort by ID");
        }

        private void ToggleUseQuestSystem()
        {
            UpdateTreatItemsAsQuests(!template.treatItemsAsQuests);
            showStateFieldAsQuest = template.treatItemsAsQuests;
        }
        
        private void ToggleUseActionSystem()
        {
            UpdateTreatQuestsAsActions(!template.treatQuestsAsActions);
            showStateFieldAsQuest = template.treatItemsAsQuests;
        }

        private void ToggleSyncItemsFromDB()
        {
            database.syncInfo.syncItems = !database.syncInfo.syncItems;
            if (!database.syncInfo.syncItems && database.syncInfo.syncItemsDatabase != null)
            {
                if (EditorUtility.DisplayDialog("Disconnect Synced DB",
                    "Also delete synced items/quests from this database?", "Yes", "No"))
                {
                    database.items.RemoveAll(x => syncedItemIDs.Contains(x.id));
                }
            }
            InitializeItemReorderableList();
            SetDatabaseDirty("Toggle Sync Items");
        }

        private void DrawItemSyncDatabase()
        {
            EditorGUILayout.BeginHorizontal();
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync items/quests from."),
                                                                       database.syncInfo.syncItemsDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (newDatabase != database.syncInfo.syncItemsDatabase)
            {
                database.syncInfo.syncItemsDatabase = newDatabase;
                database.SyncItems();
                InitializeItemReorderableList();
                syncedItemIDs = null;
                SetDatabaseDirty("Change Sync Items Database");
            }
            if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72)))
            {
                database.SyncItems();
                InitializeItemReorderableList();
                syncedItemIDs = null;
                SetDatabaseDirty("Manual Sync Items");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RecordSyncedItemIDs()
        {
            syncedItemIDs = new HashSet<int>();
            if (database.syncInfo.syncItems && database.syncInfo.syncItemsDatabase != null)
            {
                database.syncInfo.syncItemsDatabase.items.ForEach(x => syncedItemIDs.Add(x.id));
            }
        }

        public bool IsItemSyncedFromOtherDB(Item item)
        {
            return item != null && syncedItemIDs != null && syncedItemIDs.Contains(item.id);
        }

        private void DrawItemPropertiesFirstPart(Item item)
        {
            if (item.IsItem)
            {
                DrawItemProperties(item);
            }
            else if (item.IsAction)
            {
                DrawActionProperties(item);
            }
            else
            
            {
                DrawQuestProperties(item);
            }
        }

        private void DrawItemProperties(Item item)
        {
            if (item == null || item.fields == null) return;
            DrawOtherItemPrimaryFields(item);
        }

        private void DrawOtherItemPrimaryFields(Item item)
        {
            if (item == null || item.fields == null || template.itemPrimaryFieldTitles== null) return;
            foreach (var field in item.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.itemPrimaryFieldTitles.Contains(field.title)) continue;
                DrawMainSectionField(field);
            }
        }

        private void DrawQuestProperties(Item item)
        {
            if (item == null || item.fields == null) return;

            // Display Name:
            var displayNameField = Field.Lookup(item.fields, "Display Name");
            var hasDisplayNameField = (displayNameField != null);
            var useDisplayNameField = EditorGUILayout.Toggle(new GUIContent("Use Display Name", "Tick to use a Display Name in UIs that's different from the Name."), hasDisplayNameField);
            if (hasDisplayNameField && !useDisplayNameField)
            {
                item.fields.Remove(displayNameField);
                SetDatabaseDirty("Don't Use Display Name");
            }
            else if (useDisplayNameField)
            {
                DrawRevisableTextField(displayNameLabel, item, null, item.fields, "Display Name");
                DrawLocalizedVersions(item, item.fields, "Display Name {0}", false, FieldType.Text);
            }

            // Group:
            var groupField = Field.Lookup(item.fields, "Group");
            var hasGroupField = (groupField != null);
            var useGroupField = EditorGUILayout.Toggle(new GUIContent("Use Groups", "Tick to organize this quest under a quest group."), hasGroupField);
            if (hasGroupField && !useGroupField)
            {
                item.fields.Remove(groupField);
                SetDatabaseDirty("Don't Use Groups");
            }
            else if (useGroupField)
            {
                if (groupField == null)
                {
                    groupField = new Field("Group", string.Empty, FieldType.Text);
                    item.fields.Add(groupField);
                    SetDatabaseDirty("Create Group Field");
                }
                if (groupField.typeString == "CustomFieldType_Text")
                {
                    DrawRevisableTextField(groupLabel, item, null, groupField);
                    DrawLocalizedVersions(item, item.fields, "Group {0}", false, FieldType.Text);
                }
                else
                {
                    DrawField(new GUIContent("Group", "The group this quest belongs to."), groupField, false);
                }
            }

            // State:
            //EditorGUILayout.BeginHorizontal();
            Field stateField = Field.Lookup(item.fields, "State");
            if (stateField == null)
            {
                stateField = new Field("State", "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Create State Field");
            }
            //EditorGUILayout.LabelField(new GUIContent("State", "The starting state of the quest."), GUILayout.Width(140));
            stateField.value = DrawQuestStateField(new GUIContent("State", "The starting state of the quest."), stateField.value);
            //EditorGUILayout.EndHorizontal();

            // Trackable:
            bool trackable = item.LookupBool("Trackable");
            bool newTrackable = EditorGUILayout.Toggle(new GUIContent("Trackable", "Tick to mark this quest trackable in a gameplay HUD."), trackable);
            if (newTrackable != trackable)
            {
                Field.SetValue(item.fields, "Trackable", newTrackable);
                SetDatabaseDirty("Create Trackable Field");
            }

            // Track on Start (only if Trackable):
            if (trackable)
            {
                bool track = item.LookupBool("Track");
                bool newTrack = EditorGUILayout.Toggle(new GUIContent("Track on Start", "Tick to show in HUD when the quest becomes active without the player having to toggle tracking on in the quest log window first."), track);
                if (newTrack != track)
                {
                    Field.SetValue(item.fields, "Track", newTrack);
                    SetDatabaseDirty("Create Track Field");
                }
            }

            // Abandonable:
            bool abandonable = item.LookupBool("Abandonable");
            bool newAbandonable = EditorGUILayout.Toggle(new GUIContent("Abandonable", "Tick to mark this quest abandonable in the quest window."), abandonable);
            if (newAbandonable != abandonable)
            {
                Field.SetValue(item.fields, "Abandonable", newAbandonable);
                SetDatabaseDirty("Create Abandonable Field");
            }

            // Has Entries:
            bool hasQuestEntries = item.FieldExists("Entry Count");
            bool newHasQuestEntries = EditorGUILayout.Toggle(new GUIContent("Has Entries (Subtasks)", "Tick to add quest entries to this quest."), hasQuestEntries);
            if (newHasQuestEntries != hasQuestEntries) ToggleHasQuestEntries(item, newHasQuestEntries);

            // Other main fields specified in template:
            DrawOtherQuestPrimaryFields(item);

            // Descriptions:
            DrawRevisableTextAreaField(questDescriptionLabel, item, null, item.fields, "Description");
            DrawLocalizedVersions(item, item.fields, "Description {0}", false, FieldType.Text);
            DrawRevisableTextAreaField(questSuccessDescriptionLabel, item, null, item.fields, "Success Description");
            DrawLocalizedVersions(item, item.fields, "Success Description {0}", false, FieldType.Text);
            DrawRevisableTextAreaField(questFailureDescriptionLabel, item, null, item.fields, "Failure Description");
            DrawLocalizedVersions(item, item.fields, "Failure Description {0}", false, FieldType.Text);

            // Entries:
            if (newHasQuestEntries) DrawQuestEntries(item);
        }
        
    

        private enum StartConversationMethod
        {
            Generate,
            Automatic,
            Manual
        }
        
        private enum TimeFlow
        {
            Explicit,
            Natural
        }

        private int SecondsPerCharacter => int.Parse(database.GetVariable("game.clock.secondsPerCharacter").InitialValue);
        private int SecondsPerLine => int.Parse(database.GetVariable("game.clock.secondsBetweenLines").InitialValue);
        
        private Texture2D RepeatableIcon => EditorGUIUtility.Load($"Icons/Restart.png") as Texture2D;
        private Texture2D ShuffleIcon => EditorGUIUtility.Load($"Icons/Shuffle.png") as Texture2D;
        
        private void DrawActionProperties(Item item)
        {
            
            var itemsWithMatchingName = database.items.FindAll(x => x.Name == item.Name);
            if (itemsWithMatchingName.Count > 1)
            {
                EditorGUILayout.HelpBox("There are multiple items with the same name. This action must have a unique name or it will not work properly.", MessageType.Warning);
            }
            
            
            if (item == null || item.fields == null) return;
            
            var defaultContentColor = GUI.contentColor;
            var defaultLabelWidth = EditorGUIUtility.labelWidth;
            var defaultFieldWidth = EditorGUIUtility.fieldWidth;

            // Display Name:
            var displayNameField = Field.Lookup(item.fields, "Display Name");
            var hasDisplayNameField = (displayNameField != null);
            var useDisplayNameField = EditorGUILayout.Toggle(new GUIContent("Use Display Name", "Tick to use a Display Name in UIs that's different from the Name."), hasDisplayNameField);
            if (hasDisplayNameField && !useDisplayNameField)
            {
                item.fields.Remove(displayNameField);
                SetDatabaseDirty("Don't Use Display Name");
            }
            else if (useDisplayNameField)
            {
                DrawRevisableTextField(displayNameLabel, item, null, item.fields, "Display Name");
                DrawLocalizedVersions(item, item.fields, "Display Name {0}", false, FieldType.Text);
            }

            // Group:
            var groupField = Field.Lookup(item.fields, "Group");
            var hasGroupField = (groupField != null);
            var useGroupField = EditorGUILayout.Toggle(new GUIContent("Use Groups", "Tick to organize this quest under a quest group."), hasGroupField);
            if (hasGroupField && !useGroupField)
            {
                item.fields.Remove(groupField);
                SetDatabaseDirty("Don't Use Groups");
            }
            else if (useGroupField)
            {
                if (groupField == null)
                {
                    groupField = new Field("Group", string.Empty, FieldType.Text);
                    item.fields.Add(groupField);
                    SetDatabaseDirty("Create Group Field");
                }
                if (groupField.typeString == "CustomFieldType_Text")
                {
                    DrawRevisableTextField(groupLabel, item, null, groupField);
                    DrawLocalizedVersions(item, item.fields, "Group {0}", false, FieldType.Text);
                }
                else
                {
                    DrawField(new GUIContent("Group", "The group this quest belongs to."), groupField, false);
                }
            }

            // State:
            
            Field stateField = Field.Lookup(item.fields, "State");
            if (stateField == null)
            {
                stateField = new Field("State", "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Create State Field");
            }
            
            stateField.value = DrawQuestStateField(new GUIContent("State", "The starting state of the quest."), stateField.value);

            
            //Repeatable


            var repeatable = item.IsRepeatable;
            
            var newRepeatable = false;
            
            DrawEditorItemWithRepeatableIcon( () =>  newRepeatable = EditorGUILayout.Toggle(
                new GUIContent("Repeatable", "Tick to set the action as repeatable."),
                repeatable));

            if (newRepeatable != repeatable)
            {
                ToggleHasRepeatableFields(item, newRepeatable);
                SetDatabaseDirty("Remove Starts Conversation Field");
            }
            EditorGUILayout.Space();
            
            //Location
            
            DrawActionLocation(item);
            
            //Start Conversation
            
            bool startsConversation = Field.Lookup(item.fields, "Conversation") != null;
            bool newStartsConversation = EditorGUILayout.Toggle(new GUIContent("Starts Conversation", "Tick to mark this quest abandonable in the quest window."), startsConversation);

            if (startsConversation != newStartsConversation)
            {
                Field conversation;
                
                if (!startsConversation)
                {
                    conversation = new Field("Conversation", string.Empty, FieldType.Text);
                    item.fields.Add(conversation);
                   
                }
                
                ToggleStartsConversation( item, newStartsConversation);
                SetDatabaseDirty(!startsConversation ? "Create Starts Conversation Field" : "Remove Starts Conversation Field");
            }

            if (newStartsConversation)
            {
                DrawStartConversationProperties(item);
                
                Field newSublocation = Field.Lookup(item.fields, "New Sublocation");

                if (newSublocation != null)
                {
                    Field sublocationSwitcherMethod = Field.Lookup(item.fields, "Sublocation Switcher Method");
                    if (sublocationSwitcherMethod == null)
                    {
                        sublocationSwitcherMethod = new Field("Sublocation Switcher Method", SublocationSwitcherMethod.MoveAfterConversation.ToString(), FieldType.Text);
                        item.fields.Add(sublocationSwitcherMethod);
                        SetDatabaseDirty("Create Sublocation Switcher Method Field");
                    }
                        
                    SublocationSwitcherMethod newSublocationSwitcherMethod = (SublocationSwitcherMethod)Enum.Parse(typeof(SublocationSwitcherMethod),
                        sublocationSwitcherMethod.value);
                        
                    DrawEditorItemWithShuffleIcon(() =>
                        newSublocationSwitcherMethod = (SublocationSwitcherMethod)EditorGUILayout.EnumPopup(
                            new GUIContent("Switcher Method",
                                "The method used to change the sublocation."),newSublocationSwitcherMethod));
                        
                        
                    sublocationSwitcherMethod.value = newSublocationSwitcherMethod.ToString();
                }

                else
                {
                    Field sublocationSwitcherMethod = Field.Lookup(item.fields, "Sublocation Switcher Method");
                    if (sublocationSwitcherMethod != null)
                    {
                        item.fields.Remove(sublocationSwitcherMethod);
                        SetDatabaseDirty("Remove Sublocation Switcher Method Field");
                    }
                }

                
            }
            
            
            

            
            
            EditorGUILayout.Space();
            
            //Conditions
            
            DrawActionConditions(item);
            
             
            Field showInvalid = Field.Lookup(item.fields, "Show Invalid");
            if (showInvalid == null)
            {
                showInvalid = new Field("Show Invalid", "False", FieldType.Boolean);
                item.fields.Add(showInvalid);
                SetDatabaseDirty("Create Show Invalid Field");
            }
            
            showInvalid.value = EditorGUILayout.Toggle(new GUIContent("Show If Invalid", "Tick to present the action as an option (in an invalid state) if the conditions aren't meant, or to hide/remove the option completely."), item.LookupBool("Show Invalid")).ToString();

            
            EditorGUILayout.Space();
            
              
            
            //Script
            DrawActionScript(item);
            
            EditorGUILayout.Space();
            
            //Points
            DrawActionPoints(item);
              
            // Other main fields specified in template:
            DrawOtherActionPrimaryFields(item);

            // Descriptions:
            DrawRevisableTextAreaField(questDescriptionLabel, item, null, item.fields, "Description");
            DrawLocalizedVersions(item, item.fields, "Description {0}", false, FieldType.Text);
            DrawRevisableTextAreaField(questSuccessDescriptionLabel, item, null, item.fields, "Success Description");
            DrawLocalizedVersions(item, item.fields, "Success Description {0}", false, FieldType.Text);
            DrawRevisableTextAreaField(questFailureDescriptionLabel, item, null, item.fields, "Failure Description");
            DrawLocalizedVersions(item, item.fields, "Failure Description {0}", false, FieldType.Text);
                
            EditorGUILayout.Space();

                
            
                
            EditorWindowTools.EditorGUILayoutBeginGroup();
                
                
            Field duration = Field.Lookup(item.fields, $"Explicit Duration");
               
            if (duration == null)
            {
                duration = new Field("Explicit Duration", "0", FieldType.Number);
                item.fields.Add(duration);
                SetDatabaseDirty("Create Explicit Duration Field");
            }

                
            EditorGUILayout.PrefixLabel("Time");

            var timeFlow = Field.Lookup(item.fields,"Time Flow");
            if (timeFlow == null)
            {
                timeFlow = new Field("Time Flow", "Explicit", FieldType.Text);
                item.fields.Add(timeFlow);
                SetDatabaseDirty("Create Time Flow Field");
            }
            
            timeFlow.value = EditorGUILayout.EnumPopup("Time Flow", (TimeFlow) Enum.Parse(typeof(TimeFlow), timeFlow.value)).ToString();
            
            if (timeFlow.value == TimeFlow.Explicit.ToString())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.BeginVertical();
                EditorGUIUtility.labelWidth = 45f;
                var hours = EditorGUILayout.IntField("Hours" , int.Parse(duration.value) / 3600, GUILayout.MaxWidth(125f), GUILayout.MinWidth(60f));
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
                EditorGUILayout.BeginVertical();
                EditorGUIUtility.labelWidth = 60f;
                var mins = EditorGUILayout.IntField("Minutes" , (int.Parse(duration.value) % 3600) / 60, GUILayout.MaxWidth(140f), GUILayout.MinWidth(85f));
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
                EditorGUILayout.BeginVertical();
                EditorGUIUtility.labelWidth = 60f;
                var secs = EditorGUILayout.IntField("Seconds" , (int.Parse(duration.value) % 3600) % 60, GUILayout.MaxWidth(140f) , GUILayout.MinWidth(85f));
                EditorGUILayout.EndVertical();
                
                duration.value = (hours * 3600 + mins * 60 + secs).ToString();
                EditorGUIUtility.labelWidth = defaultLabelWidth;
                EditorGUILayout.EndHorizontal();
                
                
                if (int.Parse(duration.value) < 0)
                {
                    duration.value = "0";
                }
                
                
                EditorGUILayout.Space();
                
                GUI.enabled = false;
                duration.value = EditorGUILayout.IntField("Duration (s)" , item.LookupInt("Duration")).ToString();
                GUI.enabled = true;
                
            }
            
            else
            {
                
                var naturalDurationFieldCount = item.fields.Count(p => p.title == "Natural Duration");
                
                if (naturalDurationFieldCount > 2)
                {
                   item.fields.RemoveAll(p => p.title == "Natural Duration");
                }
                
                else if (naturalDurationFieldCount == 0)
                {
                    item.fields.Add(new Field("Natural Duration", "0", FieldType.Number));
                    SetDatabaseDirty("Create Natural Duration Field");
                }

                else
                {
                    var conversation = Field.Lookup(item.fields, "Conversation");

                    if (conversation == null)
                    {
                        
                    }

                    else
                    {
                        int naturalDurationA = -1;
                    int naturalDurationB = -1;

                    if (conversation.value == string.Empty)
                    {
                        naturalDurationA = 0;

                        if (item.LookupInt("Entry Count") > 0)
                        {
                            for (int i = 1; i < item.LookupInt("Entry Count") + 1; i++)
                            {
                                var subtitleText = item.LookupValue($"Entry {i} Dialogue Text");
                                if (!string.IsNullOrEmpty(subtitleText))
                                    naturalDurationA += subtitleText.Length > 0
                                        ? subtitleText.Length * SecondsPerCharacter + SecondsPerLine
                                        : 0;
                            }
                        }
                        
                      
                        EditorGUILayout.LabelField("Duration", DurationLabel(naturalDurationA));

                        if (item.IsRepeatable && item.IsFieldAssigned("Repeat Entry Count"))
                        {
                            
                            naturalDurationB = 0;
                            
                            for (int i = 1; i < item.LookupInt("Repeat Entry Count") + 1; i++)
                            {
                                var subtitleText = item.LookupValue($"Repeat Entry {i} Dialogue Text");
                                naturalDurationB += subtitleText.Length > 0
                                    ? subtitleText.Length * SecondsPerCharacter + SecondsPerLine
                                    : 0;
                            }
                            
                            DrawEditorItemWithRepeatableIcon( () => EditorGUILayout.LabelField("Duration", DurationLabel(naturalDurationB)));
                            
                        }
                    }

                    else
                    {
                        
                        
                        var timeEstimate = TimeEstimate(database.GetConversation(conversation.value).GetFirstDialogueEntry());
                    
                        naturalDurationA = timeEstimate.Item1;
                        naturalDurationB = timeEstimate.Item2;
                    
                        var minDurationLabel = DurationLabel( naturalDurationA);
                        var maxDurationLabel = DurationLabel( naturalDurationB);
                    
                    
                    
                        if (naturalDurationA == naturalDurationB)
                        {
                            EditorGUILayout.LabelField("Duration", minDurationLabel);
                            naturalDurationB = -1;
                        }
                    
                        else
                        {
                            EditorGUILayout.LabelField("Min Duration", minDurationLabel);
                            EditorGUILayout.LabelField("Max Duration", maxDurationLabel);
                        }
                    
                    }
                    
                    item.fields.First(p => p.title == "Natural Duration").value = naturalDurationA.ToString();

                    if (naturalDurationB >= 0)
                    {
                        if (naturalDurationFieldCount < 2) item.fields.Add(new Field("Natural Duration", naturalDurationB.ToString(), FieldType.Number));
                        else item.fields.Last(p => p.title == "Natural Duration").value = naturalDurationB.ToString();
                    }

                    else
                    {
                        if (naturalDurationFieldCount > 1)
                            item.fields.Remove(item.fields.Last(p => p.title == "Natural Duration"));
                    }
                    }
                }
            }
                
            EditorWindowTools.EditorGUILayoutEndGroup();
            
        }
        
        
        private void DrawEditorItemWithRepeatableIcon( Action action, float? labelWidth = null)
        {
            if (action == null) return;
                
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(RepeatableIcon , GUILayout.Width(18), GUILayout.Height(18));
            var defaultContentColor = GUI.contentColor;
            var defaultLabelWidth = EditorGUIUtility.labelWidth;
            
            GUI.contentColor = defaultContentColor;
            
            if (labelWidth.HasValue)
            {
                EditorGUIUtility.labelWidth = labelWidth.Value;
            }
            
            else EditorGUIUtility.labelWidth = defaultLabelWidth - 22;
            
            action.Invoke();
            
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            EditorGUILayout.EndHorizontal();
            
        }
        
        private void DrawEditorItemWithShuffleIcon( Action action, float? labelWidth = null)
        {
            if (action == null) return;
            var defaultContentColor = GUI.contentColor;
            var defaultLabelWidth = EditorGUIUtility.labelWidth;
                
            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = Color.magenta;
            GUILayout.Label(ShuffleIcon , GUILayout.Width(15), GUILayout.Height(15));
            GUI.contentColor = defaultContentColor;
            
            GUI.contentColor = defaultContentColor;
            
            if (labelWidth.HasValue)
            {
                EditorGUIUtility.labelWidth = labelWidth.Value;
            }
            
            else  EditorGUIUtility.labelWidth = defaultLabelWidth - 20;
            
            action.Invoke();
            
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            EditorGUILayout.EndHorizontal();
            
        }
        
        
        private void DrawEditorItemWithAudioClipIcon( Action action, float? labelWidth = null, Color color = default)
        {
            if (action == null) return;
            var defaultContentColor = GUI.contentColor;
            var defaultLabelWidth = EditorGUIUtility.labelWidth;
                
            if (color != default) GUI.contentColor = color;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("d_AudioClip Icon").image , GUILayout.Width(22), GUILayout.Height(22));
            GUI.contentColor = defaultContentColor;
            
            if (labelWidth.HasValue)
            {
                EditorGUIUtility.labelWidth = labelWidth.Value;
            }
            
            else  EditorGUIUtility.labelWidth = defaultLabelWidth - 25;
            
            action.Invoke();
            
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            EditorGUILayout.EndHorizontal();
            
        }
        
       

        private static List<string> questBuiltInFieldTitles = new List<string>(new string[] { "Display Name", "Group", "State", "Trackable", "Track", "Abandonable" });

        private void DrawOtherQuestPrimaryFields(Item item)
        {
            if (item == null || item.fields == null || template.questPrimaryFieldTitles == null) return;
            foreach (var field in item.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.questPrimaryFieldTitles.Contains(field.title)) continue;
                if (questBuiltInFieldTitles.Contains(fieldTitle)) continue;
                if (fieldTitle.StartsWith("Description") || fieldTitle.StartsWith("Success Description") || fieldTitle.StartsWith("Failure Description")) continue;
                DrawMainSectionField(field);
            }
        }
        
        private static List<string> actionBuiltInFieldTitles = new List<string>(new string[] { "Display Name", "Group", "State", "Location", "Abandonable", "Repeatable" });
        
        private void DrawOtherActionPrimaryFields(Item item)
        {
            if (item == null || item.fields == null || template.actionPrimaryFieldTitles == null) return;
            foreach (var field in item.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.actionPrimaryFieldTitles.Contains(field.title)) continue;
                if (actionBuiltInFieldTitles.Contains(fieldTitle)) continue;
                if (fieldTitle.StartsWith("Description") || fieldTitle.StartsWith("Success Description") || fieldTitle.StartsWith("Failure Description") || field.title.StartsWith("Conditions")) continue;
                DrawMainSectionField(field);
            }
        }

        private void DrawActionConditions(Item item)
        {
             var conditions = Field.Lookup(item.fields, "Conditions");
             if (conditions == null)
             {
                 conditions = new Field("Conditions", "true", FieldType.Text);
                 item.fields.Add(conditions);
             }

             if (!conditions.value.Contains(" and "))
             {
                    conditions.value = $"true and (true)";
                    return;
             }
             
             EditorWindowTools.EditorGUILayoutBeginGroup();
             
             luaConditionWizard.database = database; 
             
             GUI.enabled = false; 
             var defaultCondition = $"CurrentQuestState(\"{item.Name}\") == \"active\""; 
             var newDefaultCondition = luaConditionWizard.Draw(new GUIContent("Default Condition", "Default lua statement for actions."), defaultCondition, false, true); 
             GUI.enabled = true; 
             
             var appendDefaultCondition = !string.IsNullOrWhiteSpace(newDefaultCondition);
             if (!appendDefaultCondition)
             {
                 newDefaultCondition = "true";
             } 
             var additionalConditionsText = conditions.value.Substring(conditions.value.Split(" and ")[0].Length + 5);
             additionalConditionsText = additionalConditionsText.Substring(1, additionalConditionsText.Length - 2); //removes parenthesis
             if (additionalConditionsText == "true") additionalConditionsText = string.Empty; 
             var newAdditionalConditions = luaConditionWizard.Draw(new GUIContent( "Additional Conditions", "Optional Lua statement that must be true to use this entry."), additionalConditionsText); 
             if (newAdditionalConditions.Length == 0) newAdditionalConditions = "true"; 
             conditions.value = $"{newDefaultCondition} and ({newAdditionalConditions})";
                
            EditorWindowTools.EditorGUILayoutEndGroup();
        }
        
        private void DrawActionScript(Item item)
        {
            var script = Field.Lookup(item.fields, "Script");
            if (script == null)
            {
                script = new Field("Script", string.Empty, FieldType.Text);
                item.fields.Add(script);
                SetDatabaseDirty("Create Script Field");
            }
            
            if (!script.value.Contains(";")) 
            {
                script.value = $"; {script.value}";
            }
            
            EditorWindowTools.EditorGUILayoutBeginGroup();
            luaScriptWizard.database = database;
            
            GUI.enabled = false;
            var defaultScript = $"SetQuestState(\"{item.Name}\", \"success\")";
            var newDefaultScript = luaScriptWizard.Draw(new GUIContent("Default Script", "Default lua script for actions that runs when the action is terminated."), defaultScript, false, true);
            GUI.enabled = true;
            
            var additionalScript = script.value.Substring(script.value.Split(script.value.Contains("; ") ? "; " : ";") [0].Length + 2);
            var newAdditionalScript = luaScriptWizard.Draw(new GUIContent( "Additional Script", "Optional Lua script that will be run after the default script."), additionalScript);
            
            
            script.value = $"{newDefaultScript}; {newAdditionalScript}";
            EditorWindowTools.EditorGUILayoutEndGroup();
        }

        private void DrawActionPoints(Item item)
        {
             var points = new string[]
                {
                    "Skills",
                    "Context",
                    "Teamwork",
                    "Wellness",
                };
           
                var pointColors = new Color[]
                {
                    new Color(  243f/255f, 223f/255f, 184f/255f),
                    new Color(  242f/255f, 184f/255f, 194f/255f),
                    new Color( 191f/255f, 184f/255f, 242f/255f),
                    new Color( 213f/255f, 242f/255f, 201f/255f),
                
                };

                var alignedTextStyle = new GUIStyle( EditorStyles.label );
                alignedTextStyle.alignment = TextAnchor.MiddleCenter;

                var repeatable = item.IsRepeatable;
                var defaultContentColor = GUI.contentColor;
                var defaultLabelWidth = EditorGUIUtility.labelWidth;
                
                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < points.Length; i++)
                {
                    Field pointValue = Field.Lookup(item.fields, $"{points[i]} Points");
               
                    if (pointValue == null)
                    {
                        pointValue = new Field($"{points[i]} Points", "0", FieldType.Number);
                        item.fields.Add(pointValue);
                        SetDatabaseDirty("Create Points Field");
                    }
                
                
               
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(points[i], alignedTextStyle,  GUILayout.MaxWidth(100));

                    var pointsAsInt = Field.LookupInt(item.fields, $"{points[i]} Points");



                    if (pointsAsInt > 0) GUI.contentColor = pointColors[i];
                    else if (pointsAsInt < 0) GUI.contentColor = Color.Lerp(new Color(0.25f, 0, 0), pointColors[i], 0.05f);
                    else GUI.contentColor = new Color(0.1f, 0.1f, 0.1f);
               
                    var icon = EditorGUIUtility.Load($"Icons/{points[i]}.png") as Texture2D;
               
                    GUILayout.Label(icon , GUILayout.MaxWidth(100), GUILayout.MaxHeight(100), GUILayout.MinWidth(50), GUILayout.MinHeight(50));
                    GUI.contentColor = defaultContentColor;
               
                    pointValue.value =
                        $"{EditorGUILayout.IntField(pointsAsInt, GUILayout.MaxWidth(100))}";
                    EditorGUILayout.EndVertical();
                }
            
                GUI.contentColor = defaultContentColor;
                EditorGUILayout.EndHorizontal();
            
                
                if (repeatable)
                {
                    
                    Field pointsReductionOnRepeat = Field.Lookup(item.fields, "Repeat Points Reduction");
                    
                    if (pointsReductionOnRepeat == null)
                    {
                        pointsReductionOnRepeat = new Field("Repeat Points Reduction", "0", FieldType.Number);
                        item.fields.Add(pointsReductionOnRepeat);
                        SetDatabaseDirty("Create Repeatable Field");
                    }
                    
                    DrawEditorItemWithRepeatableIcon( ()=>
                            pointsReductionOnRepeat.value = EditorGUILayout.FloatField(new GUIContent($"{Field.LookupFloat( item.fields, "Repeat Points Reduction") * 100}% Points Reduction", "The amount of points reduced from the action when repeated."), Field.LookupFloat(item.fields, "Repeat Points Reduction")).ToString(), 
                        150f);
                    
                 
                    if (Field.LookupFloat(item.fields, "Repeat Points Reduction") < 0)
                    {
                        Field.SetValue(item.fields, "Repeat Points Reduction", "0");
                    }
                    
                    if (Field.LookupFloat(item.fields, "Repeat Points Reduction") > 1)
                    {
                        Field.SetValue(item.fields, "Repeat Points Reduction", "1");
                    }
                }
                
                
        }

        private void DrawActionLocation(Item item)
        {
            var defaultLabelWidth = EditorGUIUtility.labelWidth;
            var defaultContentColor = GUI.contentColor;
            EditorWindowTools.EditorGUILayoutBeginGroup();
            Field location = Field.Lookup(item.fields, "Location");
            if (location == null)
            {
                location = new Field("Location", "", FieldType.Location);
                item.fields.Add(location);
                SetDatabaseDirty("Create Location Field");
            }
            
            var chosenLocation = DrawLocationField(new GUIContent("Location", "The location of the action. If the player is not present, the action will not be presented, regardless of any followup conditions."), location.value, false);
            if (chosenLocation != location.value)
            {
                var loc = database.GetLocation(int.Parse(location.value));
                if (loc == null || !loc.IsSublocation || loc.LookupValue("Parent Location") != chosenLocation)
                {
                    location.value = chosenLocation;
                }
                
                SetDatabaseDirty( "Change Location Field");
            }
            
            var chosenSublocation = string.Empty;

            var chosenLocationHasSublocation = database.locations.Any(p =>
                p.IsSublocation && p.LookupValue("Parent Location") == chosenLocation);
            
            
            if (!string.IsNullOrEmpty(chosenLocation) || chosenLocation != "-1")
            {
                if (chosenLocationHasSublocation)
                {
                    Field ignoreSublocations = Field.Lookup(item.fields, "Ignore Sublocations");
                    
                    if (ignoreSublocations == null)
                    {
                        ignoreSublocations = new Field("Ignore Sublocations", "False", FieldType.Boolean);
                        item.fields.Add(ignoreSublocations);
                        SetDatabaseDirty("Create Ignore Sublocations Field");
                    }


                    if (ignoreSublocations.value == "True")
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("Sublocation");
                        GUI.enabled = true;
                        chosenSublocation = "-1";
                    }

                    else
                    {
                        chosenSublocation = DrawSublocationField( new GUIContent("Sublocation", "The Sublocation of the location above."), database.GetLocation(int.Parse(chosenLocation)), location.value == chosenLocation ? "-1" : location.value);

                    }
                    
                    ignoreSublocations.value = Field.Lookup(item.fields, "New Sublocation") != null ? "True" : "False";
                }
            }

            location.value = string.IsNullOrEmpty(chosenSublocation) || chosenSublocation == "-1" ? chosenLocation : chosenSublocation;
            

            if (chosenLocationHasSublocation)
            {
                var newSublocationFieldExists = Field.Lookup(item.fields, "New Sublocation") != null;
                bool changeSublocation = false;
                bool newChangeSublocation = false;
                
                DrawEditorItemWithShuffleIcon ( ()=>
                        newChangeSublocation = EditorGUILayout.Toggle(
                            new GUIContent("Sublocation Switcher", "Tick to set whether the action changes the sublocation to the selected one."),
                            newSublocationFieldExists)
                    );



                if (changeSublocation != newChangeSublocation)
                {
                    SetDatabaseDirty("Change Sublocation Switcher Field");

                }
                
                if (newChangeSublocation) {
                    

                    Field newSublocationField;
                    
                    if (!newSublocationFieldExists)
                    {
                        newSublocationField = new Field("New Sublocation", "-1", FieldType.Location);
                        item.fields.Add(newSublocationField);
                        SetDatabaseDirty("Create New Sublocation Field");
                    }
                    
                    else newSublocationField = Field.Lookup(item.fields, "New Sublocation");
                    
                     
                    var label = newSublocationField.value == chosenLocation ? "(Return to Base Location)" : "New Sublocation";
                              
                    DrawEditorItemWithShuffleIcon ( ()=>
                            newSublocationField.value = DrawSublocationField( 
                            new GUIContent(label, "The Sublocation of the location above."),
                            database.GetLocation(int.Parse(chosenLocation)), newSublocationField.value));

                    if (newSublocationField.value == "-1") newSublocationField.value = chosenLocation;
                }
                else
                {
                    if (newSublocationFieldExists)
                    {
                        item.fields.Remove(Field.Lookup(item.fields, "New Sublocation"));
                        SetDatabaseDirty("Remove New Sublocation Field");
                    }
                }
            }
            
            EditorGUILayout.Space();
            EditorWindowTools.EditorGUILayoutEndGroup();

            
        }
        
        private enum SublocationSwitcherMethod
        {
            MoveAfterConversation,
            MoveBeforeConversation,
            MoveBeforeConversationAndReturnWhenDone
        }

        private void DrawStartConversationProperties(Asset asset)
        {
                Field conversation = Field.Lookup(asset.fields, "Conversation");
                StartConversationMethod startConversationMethod = asset.FieldExists("Entry Count")
                    ?  StartConversationMethod.Generate : asset.LookupBool( "Auto Conversation Title")
                        ?
                    StartConversationMethod.Automatic
                       : StartConversationMethod.Manual;
                
                var newStartConversationMethod = (StartConversationMethod)EditorGUILayout.EnumPopup(
                    new GUIContent("Start Conversation Method", "The method used to start the conversation."),
                    startConversationMethod);
                
                
                var startConversationTitle = asset.LookupBool( "Auto Conversation Title") ? ClosestMatch(asset.Name, database.conversations.Select(p => p.Title).ToList()) : conversation.value == string.Empty ? string.Empty : conversation.value;
                var newConversationTitle = string.Empty;
                var newConversationFieldValue = string.Empty;

                Actor actor = string.IsNullOrEmpty(startConversationTitle) || asset.FieldExists("Entry Actor")
                    ? database.GetActor(asset.LookupInt("Entry Actor"))
                    : database.GetActor(database.GetConversation(startConversationTitle).ActorID);
                Actor newActor = null;

                if (startConversationMethod != newStartConversationMethod)
                {
                    SetDatabaseDirty("Change Start Conversation Method");
                    
                    if (startConversationMethod == StartConversationMethod.Generate || newStartConversationMethod == StartConversationMethod.Generate)
                    {
                        ToggleHasGeneratedDialogueEntries(asset, newStartConversationMethod == StartConversationMethod.Generate);
                    }
                }

                if (newStartConversationMethod == StartConversationMethod.Generate)
                {
                    if (asset.FieldExists("Auto Conversation Title"))
                    {
                        asset.fields.Remove(Field.Lookup(asset.fields, "Auto Conversation Title"));
                        SetDatabaseDirty("Remove Automatic Conversation Title Field");
                    }
                    
                    
                    DrawGeneratedDialogueEntries(asset);

                    if (asset is Item item)
                    {
                        if (item.IsRepeatable) DrawGeneratedRepeatDialogueEntries(item);
                    }

                    else if (asset is Location)
                    {
                        Field conversationPlaysMoreThanOnce = Field.Lookup(asset.fields, "Loop Conversation");
                        
                        if (conversationPlaysMoreThanOnce == null)
                        {
                            conversationPlaysMoreThanOnce = new Field("Loop Conversation", "False", FieldType.Boolean);
                            asset.fields.Add(conversationPlaysMoreThanOnce);
                            SetDatabaseDirty("Create Repeat Start Conversation Field");
                        }

                        DrawEditorItemWithRepeatableIcon(() => conversationPlaysMoreThanOnce.value = EditorGUILayout
                            .Toggle(
                                new GUIContent("Start Conversation On Repeat Visits",
                                    "Tick to play this conversation more than once when entering the location."),
                                asset.LookupBool("Loop Conversation")).ToString(), 215f);
                        
                              
                        if (conversationPlaysMoreThanOnce.value == "True")
                        {
                            DrawGeneratedRepeatDialogueEntries(asset);
                        }
                    }

                    else
                    {
                        DrawGeneratedRepeatDialogueEntries(asset);
                    }
                    
                    Field entryActor = Field.Lookup(asset.fields, "Entry Actor");
                    if (entryActor == null)
                    {
                       
                        if (!string.IsNullOrEmpty(startConversationTitle))
                        {
                            var startConversationActor = database.GetConversation(startConversationTitle).ActorID;
                            if (startConversationActor >= 0)  entryActor = new Field("Entry Actor", startConversationActor.ToString(), FieldType.Actor);
                        }
                        entryActor ??= new Field("Entry Actor", database.GetActor("Game").id.ToString(), FieldType.Actor);
                        asset.fields.Add(entryActor);
                        SetDatabaseDirty("Create Entry Actor Field");
                    }

                    string newEntryActor;
                    
                    if (asset is Item)
                    {
                        newEntryActor = DrawActorField(
                            new GUIContent("Actor", "The actor that starts the conversation."), entryActor.value);
                    
                        var requiredActor = asset.fields.Find(p => p.title == "Required Nearby Actor" && p.value == entryActor.value);
                        if (requiredActor != null && database.GetActor(int.Parse(requiredActor.value)) != database.GetActor(int.Parse(newEntryActor)))
                        {
                            if (!database.GetActor(int.Parse(newEntryActor)).IsPlayer) requiredActor.value = newEntryActor;
                            else asset.fields.Remove(requiredActor);
                        }
                    }

                    else
                    {
                        newEntryActor = DrawPlayerActorField(
                            new GUIContent("Actor", "The player actor that starts the conversation."), entryActor.value);
                    }
                    
                    
                    entryActor.value = newEntryActor;
                    newActor = database.GetActor(int.Parse(entryActor.value));
                    
                    newConversationFieldValue = string.Empty;
                }

                else
                {
                    
                    Field automaticConversation = Field.Lookup(asset.fields, "Auto Conversation Title");
                    if (automaticConversation == null)
                    {
                        automaticConversation = new Field("Auto Conversation Title", "True", FieldType.Boolean);
                        asset.fields.Add(automaticConversation);
                        SetDatabaseDirty("Create Automatic Conversation Title Field");
                    }
                    
                    if (newStartConversationMethod == StartConversationMethod.Automatic)
                    {
                        automaticConversation.value = "True";

                        newConversationTitle =  ClosestMatch(asset.Name, database.conversations.Select(p => p.Title).ToList());
                        GUI.enabled = false;
                        EditorGUILayout.TextField(
                            new GUIContent("Conversation", "The conversation that the action starts."), newConversationTitle);
                        GUI.enabled = true;
                        newConversationFieldValue = newConversationTitle;
                    }

                    if (newStartConversationMethod == StartConversationMethod.Manual)
                    {
                        automaticConversation.value = "False";
                        
                        var conversationIndex = EditorGUILayout
                            .Popup(new GUIContent("Conversation", "The conversation that the action starts."),
                                database.conversations.FindIndex(p => p.Title == conversation.value),
                                database.conversations.Select(p => p.Title).ToArray());

                        if (conversationIndex >= 0) newConversationTitle = database.conversations.Select(p => p.Title).ToArray()[conversationIndex];
                        else newConversationTitle = ClosestMatch(asset.Name, database.conversations.Select(p => p.Title).ToList());
                        newConversationFieldValue = newConversationTitle;
                    }
                    
                    if (asset is Location)
                    {
                        Field conversationPlaysMoreThanOnce = Field.Lookup(asset.fields, "Loop Conversation");
                        
                        if (conversationPlaysMoreThanOnce == null)
                        {
                            conversationPlaysMoreThanOnce = new Field("Loop Conversation", "False", FieldType.Boolean);
                            asset.fields.Add(conversationPlaysMoreThanOnce);
                            SetDatabaseDirty("Create Repeat Start Conversation Field");
                        }
                        
                        DrawEditorItemWithRepeatableIcon(() => conversationPlaysMoreThanOnce.value = EditorGUILayout
                            .Toggle(
                                new GUIContent("Start Conversation On Repeat Visits",
                                    "Tick to play this conversation more than once when entering the location."),
                                asset.LookupBool("Loop Conversation")).ToString(), 215f);
                    }
                    
                    

                    newActor = database.GetConversation(newConversationTitle) != null
                        ? database.GetActor(database.GetConversation(newConversationTitle).ActorID)
                        : null;
                    
                    GUI.enabled = false;
                    DrawActorField(
                        new GUIContent("Actor", "The actor that starts the conversation, assigned by the Conversation's fields (not Dialogue Entry fields)."), newActor != null ? newActor.id.ToString() : "-1");
                    GUI.enabled = true;
                }


                if (asset is Item)
                {
                    if (actor != newActor)
                    {

                        if (newActor != null && !newActor.IsPlayer && (actor == null || actor.IsPlayer))
                        {
                            var requiredActor = new Field("Required Nearby Actor", newActor.id.ToString(),
                                FieldType.Actor);
                            asset.fields.Add(requiredActor);
                            SetDatabaseDirty("Create Required Nearby Actor Field");
                        }

                        else
                        {
                            var requiredNearbyActor = actor == null
                                ? null
                                : asset.fields.Find(p =>
                                    p.title == "Required Nearby Actor" && p.value == actor.id.ToString());
                            if (requiredNearbyActor != null)
                            {
                                if (newActor != null && !newActor.IsPlayer)
                                    requiredNearbyActor.value = newActor.id.ToString();
                                else asset.fields.Remove(requiredNearbyActor);
                            }

                            if (newConversationTitle != string.Empty)
                            {
                                if (asset.FieldExists("Entry Actor"))
                                {
                                    asset.fields.Remove(Field.Lookup(asset.fields, "Entry Actor"));
                                    SetDatabaseDirty("Remove Entry Actor Field");
                                }
                            }
                        }
                    }


                    if (newActor != null && !newActor.IsPlayer && newActor.FieldExists("Location"))
                    {
                        var requiredActor = asset.fields.Find(p =>
                            p.title == "Required Nearby Actor" && p.value == newActor.id.ToString());
                        var requiresConversationActor = requiredActor != null;
                        var newRequiresConversationActor = EditorGUILayout.Toggle(
                            new GUIContent("Require Actor Nearby Player",
                                "Tick to require the actor to be at the location to start the conversation."),
                            requiresConversationActor);

                        if (!newRequiresConversationActor)
                        {
                            if (requiresConversationActor) asset.fields.Remove(requiredActor);
                            SetDatabaseDirty("Remove Required Nearby Actor Field");
                        }
                        else
                        {
                            if (!requiresConversationActor)
                            {
                                requiredActor = new Field("Required Nearby Actor", newActor.id.ToString(),
                                    FieldType.Actor);
                                asset.fields.Add(requiredActor);
                                SetDatabaseDirty("Create Required Nearby Actor Field");
                            }
                        }
                    }
                    
                   
                    
                    
                }

                conversation.value = newConversationFieldValue;
        }

        private void ToggleHasQuestEntries(Item item, bool hasEntries)
        {
            SetDatabaseDirty("Toggle Has Quest Entries");
            if (hasEntries)
            {
                if (!item.FieldExists("Entry Count")) Field.SetValue(item.fields, "Entry Count", (int)0);
            }
            else
            {
                int entryCount = Field.LookupInt(item.fields, "Entry Count");
                if (entryCount > 0)
                {
                    if (!EditorUtility.DisplayDialog("Delete all entries?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                item.fields.RemoveAll(field => field.title.StartsWith("Entry "));
            }
        }
        
        private void ToggleHasGeneratedDialogueEntries(Asset asset, bool hasEntries)
        {
            SetDatabaseDirty("Toggle Has Generated Dialogue Entries");
            if (hasEntries)
            {
                if (!asset.FieldExists("Entry Count")) Field.SetValue(asset.fields, "Entry Count", (int)0);
            }
            else
            {
                int entryCount = Field.LookupInt(asset.fields, "Entry Count");
                if (entryCount > 0)
                {
                    if (!EditorUtility.DisplayDialog("Delete all entries?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                asset.fields.RemoveAll(field => field.title.StartsWith("Entry "));
                ToggleHasGeneratedRepeatDialogueEntries(asset, false, true);
            }
        }

        private void ToggleHasGeneratedRepeatDialogueEntries(Asset asset, bool hasEntries, bool overrideWarning = false)
        {
            SetDatabaseDirty("Toggle Has Generated Repeat Dialogue Entries");
            if (hasEntries)
            {
                if (!asset.FieldExists("Repeat Entry Count")) Field.SetValue(asset.fields, "Repeat Entry Count", (int)0);
            }
            else
            {
                int entryCount = Field.LookupInt(asset.fields, "Repeat Entry Count");
                if (entryCount > 0 && !overrideWarning)
                {
                    if (!EditorUtility.DisplayDialog("Delete all repeat entries?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                asset.fields.RemoveAll(field => field.title.StartsWith("Repeat Entry "));
            }
        }
        
        private void ToggleHasRepeatableFields(Item item, bool hasRepeatableFields)
        {
            SetDatabaseDirty("Toggle Has Repeatable Fields");
            if (hasRepeatableFields)
            {
                item.IsRepeatable = true;
            }
            else
            {
                int repeatableFieldCount = item.fields.FindAll( field => field.title.StartsWith("Repeat ") && field.title != "Repeat Points Reduction" && field.title != "Repeat Count").Count;
                if (repeatableFieldCount > 0)
                {
                    if (!EditorUtility.DisplayDialog("Delete all repeatable fields?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                item.fields.RemoveAll(field => field.title.StartsWith("Repeat"));
            }
        }

        private void ToggleStartsConversation(Asset asset, bool startsConversation)
        {
            SetDatabaseDirty("Toggle Starts Conversation");
            if (startsConversation)
            {
                if (!asset.FieldExists("Conversation")) Field.SetValue(asset.fields, "Conversation", string.Empty);
            }
            else
            {
                int entryCounts = asset.fields.FindAll( field => field.title.StartsWith( "Entry ")).Count + asset.fields.FindAll( field => field.title.StartsWith( "Repeat Entry ")).Count;
                if (entryCounts > 0)
                {
                    if (!EditorUtility.DisplayDialog("Delete all repeatable fields?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                
                asset.fields.Remove( Field.Lookup(asset.fields, "Conversation"));
                asset.fields.RemoveAll(field => field.title.StartsWith("Entry "));
                asset.fields.RemoveAll(field => field.title.StartsWith("Repeat Entry "));
            }
        }
        
        
        

        private void DrawQuestEntries(Item item)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quest Entries", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Compact View", GUILayout.Width(90));
            showCompactQuestEntryList = EditorGUILayout.Toggle(GUIContent.none, showCompactQuestEntryList, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            int entryCount = Field.LookupInt(item.fields, "Entry Count");

            string[] entryTabs = null;
            if (showCompactQuestEntryList)
            {
                entryTabs = new string[entryCount];
                for (int i = 1; i <= entryCount; i++)
                {
                    entryTabs[i - 1] = "Entry " + i;
                }
                questEntrySelectedIdx = GUILayout.Toolbar(questEntrySelectedIdx, entryTabs);
            }

            EditorWindowTools.StartIndentedSection();
            int entryToDelete = -1;
            int entryToMoveUp = -1;
            int entryToMoveDown = -1;
            for (int i = 1; i <= entryCount; i++)
            {
                if (showCompactQuestEntryList && (i != questEntrySelectedIdx + 1)) continue;
                DrawQuestEntry(item, i, entryCount, ref entryToDelete, ref entryToMoveUp, ref entryToMoveDown);
            }
            if (entryToDelete != -1)
            {
                DeleteQuestEntry(item, entryToDelete, entryCount);
                SetDatabaseDirty("Delete Quest Entry");
                GUIUtility.ExitGUI();
            }
            if (entryToMoveUp != -1)
            {
                MoveQuestEntryUp(item, entryToMoveUp, entryCount);
            }
            if (entryToMoveDown != -1)
            {
                MoveQuestEntryDown(item, entryToMoveDown, entryCount);
            }
            if (GUILayout.Button(new GUIContent("Add New Quest Entry", "Adds a new quest entry to this quest.")))
            {
                entryCount++;
                questEntrySelectedIdx = entryCount - 1;
                Field.SetValue(item.fields, "Entry Count", entryCount);
                Field.SetValue(item.fields, string.Format("Entry {0} State", entryCount), "unassigned");
                Field.SetValue(item.fields, string.Format("Entry {0}", entryCount), string.Empty);
                List<string> questLanguages = new List<string>();
                item.fields.ForEach(field =>
                {
                    if (field.title.StartsWith("Description "))
                    {
                        string language = field.title.Substring("Description ".Length);
                        questLanguages.Add(language);
                        languages.Add(language);
                    }
                });
                questLanguages.ForEach(language => item.fields.Add(new Field(string.Format("Entry {0} {1}", entryCount, language), string.Empty, FieldType.Localization)));

                if (entryCount > 1)
                {
                    // Copy any custom "Entry 1 ..." fields to "Entry # ..." fields:
                    var fieldCount = item.fields.Count;
                    for (int i = 0; i < fieldCount; i++)
                    {
                        var field = item.fields[i];
                        if (field.title.StartsWith("Entry 1 "))
                        {
                            // Skip Entry 1, Entry 1 State, or Entry 1 Language:
                            if (string.Equals(field.title, "Entry 1") || string.Equals(field.title, "Entry 1 State")) continue;
                            var afterEntryNumber = field.title.Substring("Entry 1 ".Length);
                            if (questLanguages.Contains(afterEntryNumber)) continue;

                            // Otherwise add:
                            var newFieldTitle = "Entry " + entryCount + " " + afterEntryNumber;
                            if (item.FieldExists(newFieldTitle)) continue;
                            var copiedField = new Field(field);
                            copiedField.title = newFieldTitle;
                            if (copiedField.type == FieldType.Text) copiedField.value = string.Empty;
                            item.fields.Add(copiedField);
                        }
                    }
                }

                SetDatabaseDirty("Add New Quest Entry");
            }
            EditorWindowTools.EndIndentedSection();
        }
        
         private void DrawGeneratedDialogueEntries(Asset asset, bool repeatEntries = false)
        {
            EditorGUILayout.BeginHorizontal();

            if (repeatEntries)
            {
                
                DrawEditorItemWithRepeatableIcon( () =>
                {
                    EditorGUILayout.LabelField("Generated Dialogue Entries", EditorStyles.boldLabel);
                });
            }
            
            else  EditorGUILayout.LabelField("Generated Dialogue Entries", EditorStyles.boldLabel);
            
           
            
            
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Compact View", GUILayout.Width(90));
            
            
            
            if (repeatEntries) showCompactRepeatEntryList = EditorGUILayout.Toggle(GUIContent.none, showCompactRepeatEntryList, GUILayout.Width(20));
            else showCompactQuestEntryList = EditorGUILayout.Toggle(GUIContent.none, showCompactQuestEntryList, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            
            var entryTitle = repeatEntries ? "Repeat Entry" : "Entry";

            int entryCount = Field.LookupInt(asset.fields, $"{entryTitle} Count");

            string[] entryTabs = null;
            if ((!repeatEntries && showCompactQuestEntryList) || (repeatEntries && showCompactRepeatEntryList))
            {
                entryTabs = new string[entryCount];
                for (int i = 1; i <= entryCount; i++)
                {
                    entryTabs[i - 1] = $"{entryTitle}" + i;
                }
                if (repeatEntries) repeatEntrySelectedIdx = GUILayout.Toolbar(repeatEntrySelectedIdx, entryTabs);
                else questEntrySelectedIdx = GUILayout.Toolbar(questEntrySelectedIdx, entryTabs);
            }

            EditorWindowTools.StartIndentedSection();
            int entryToDelete = -1;
            int entryToMoveUp = -1;
            int entryToMoveDown = -1;
            for (int i = 1; i <= entryCount; i++)
            {
                if (repeatEntries && showCompactRepeatEntryList  && (i != repeatEntrySelectedIdx + 1)) continue;
                if (!repeatEntries && showCompactQuestEntryList && (i != questEntrySelectedIdx + 1)) continue;
                DrawGeneratedDialogueEntry(asset, i, entryCount, ref entryToDelete, ref entryToMoveUp, ref entryToMoveDown, repeatEntries);
            }
            if (entryToDelete != -1)
            {
                DeleteQuestEntry(asset, entryToDelete, entryCount, repeatEntries);
                SetDatabaseDirty("Delete Entry");
                GUIUtility.ExitGUI();
            }
            if (entryToMoveUp != -1)
            {
                MoveQuestEntryUp(asset, entryToMoveUp, entryCount, repeatEntries);
            }
            if (entryToMoveDown != -1)
            {
                MoveQuestEntryDown(asset, entryToMoveDown, entryCount, repeatEntries);
            }
            if (GUILayout.Button(new GUIContent("Add New Dialogue Entry", "Adds a new quest entry to this quest.")))
            {
                entryCount++;
               
                if (repeatEntries) repeatEntrySelectedIdx = entryCount - 1;
                else questEntrySelectedIdx = entryCount - 1;
                
                Field.SetValue(asset.fields, $"{entryTitle} Count", entryCount);
                Field.SetValue(asset.fields, string.Format(entryTitle + " {0}", entryCount), string.Empty);
                List<string> questLanguages = new List<string>();
                asset.fields.ForEach(field =>
                {
                    if (field.title.StartsWith("Description "))
                    {
                        string language = field.title.Substring("Description ".Length);
                        questLanguages.Add(language);
                        languages.Add(language);
                    }
                });
                questLanguages.ForEach(language => asset.fields.Add(new Field(string.Format(entryTitle + " {0} {1}", entryCount, language), string.Empty, FieldType.Localization)));

                if (entryCount > 1)
                {
                    // Copy any custom "Entry 1 ..." fields to "Entry # ..." fields:
                    var fieldCount = asset.fields.Count;
                    for (int i = 0; i < fieldCount; i++)
                    {
                        var field = asset.fields[i];
                        if (field.title.StartsWith($"{entryTitle} 1 "))
                        {
                            // Skip Entry 1, Entry 1 State, or Entry 1 Language:
                            if (string.Equals(field.title, $"{entryTitle} 1") || string.Equals(field.title, $"{entryTitle} 1 State")) continue;
                            var afterEntryNumber = field.title.Substring($"{entryTitle} 1 ".Length);
                            if (questLanguages.Contains(afterEntryNumber)) continue;

                            // Otherwise add:
                            var newFieldTitle = $"{entryTitle} " + entryCount + " " + afterEntryNumber;
                            if (asset.FieldExists(newFieldTitle)) continue;
                            var copiedField = new Field(field);
                            copiedField.title = newFieldTitle;
                            if (copiedField.type == FieldType.Text) copiedField.value = string.Empty;
                            asset.fields.Add(copiedField);
                        }
                    }
                }

                SetDatabaseDirty("Add New Generated Dialogue Entry");
            }
            EditorWindowTools.EndIndentedSection();
        }

        private void DrawGeneratedRepeatDialogueEntries(Asset asset)
        {
            
            EditorGUILayout.Space();
            
            var defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

            EditorWindowTools.EditorGUILayoutBeginGroup( );
                  
            bool hasRepeatEntries = asset.FieldExists("Repeat Entry Count");


            bool newHasRepeatEntries = false;
            
            DrawEditorItemWithRepeatableIcon( () =>  newHasRepeatEntries = EditorGUILayout.Toggle(new GUIContent("Use Different Entries on Repeats", "Tick to add quest entries to this quest."), hasRepeatEntries)
                , 200f);
                        
            if (hasRepeatEntries != newHasRepeatEntries) ToggleHasGeneratedRepeatDialogueEntries(asset, newHasRepeatEntries);
                        
                        
            if (newHasRepeatEntries) DrawGeneratedDialogueEntries(asset, true);
                    
            EditorWindowTools.EditorGUILayoutEndGroup();
                    
            GUI.backgroundColor = defaultColor;
        }
         
        

        private void DrawQuestEntry(Item item, int entryNumber, int entryCount, ref int entryToDelete, ref int entryToMoveUp, ref int entryToMoveDown)
        {
            EditorGUILayout.BeginVertical("button");

            // Keep track of which fields we've already drawn:
            List<Field> alreadyDrawn = new List<Field>();

            // Heading:
            EditorGUILayout.BeginHorizontal();
            string entryTitle = string.Format("Entry {0}", entryNumber);
            EditorGUILayout.LabelField(entryTitle);
            GUILayout.FlexibleSpace();

            //--- Framework for future move up/down buttons:
            EditorGUI.BeginDisabledGroup(entryNumber <= 1);
            if (GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButtonLeft, GUILayout.Width(22)))
            {
                entryToMoveUp = entryNumber;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(entryNumber >= entryCount);
            if (GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButtonMid, GUILayout.Width(22)))
            {
                entryToMoveDown = entryNumber;
            }
            EditorGUI.EndDisabledGroup();
            // Also change Delete button below to miniButtonRight

            if (GUILayout.Button(new GUIContent("-", "Delete"), EditorStyles.miniButtonRight, GUILayout.Width(22)))
            //if (GUILayout.Button(new GUIContent(" ", "Delete quest entry"), "OL Minus", GUILayout.Width(16)))
            {
                entryToDelete = entryNumber;
            }
            EditorGUILayout.EndHorizontal();

            
            // State:
            EditorGUILayout.BeginHorizontal();
            string stateTitle = entryTitle + " State";
            Field stateField = Field.Lookup(item.fields, stateTitle);
            if (stateField == null)
            {
                stateField = new Field(stateTitle, "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Change Quest Entry State");
            }
            EditorGUILayout.LabelField(new GUIContent("State", "The starting state of this entry."), GUILayout.Width(140));
            stateField.value = DrawQuestStateField(stateField.value);
            EditorGUILayout.EndHorizontal();
            alreadyDrawn.Add(stateField);
            
            
            

            // Text:
            DrawRevisableTextField(new GUIContent(entryTitle), item, null, item.fields, entryTitle);
            DrawLocalizedVersions(item, null, item.fields, entryTitle + " {0}", false, FieldType.Text, alreadyDrawn);
            
            

            // Other "Entry # " fields:
            string entryTitleWithSpace = entryTitle + " ";
            string entryIDTitle = entryTitle + " ID";
            for (int i = 0; i < item.fields.Count; i++)
            {
                var field = item.fields[i];
                if (field.title == null) field.title = string.Empty;
                if (!alreadyDrawn.Contains(field) && field.title.StartsWith(entryTitleWithSpace) && !string.Equals(field.title, entryIDTitle))
                {
                    if (field.type == FieldType.Text && field.typeString == "CustomFieldType_Text")
                    {
                        EditTextField(item.fields, field.title, field.title, true, null);
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        DrawField(field);
                        EditorGUILayout.EndHorizontal();
                    }
                    alreadyDrawn.Add(field);
                }
            }

            // Add new entry field:
            if (isAddingNewFieldToEntryNumber == entryNumber)
            {
                EditorGUILayout.BeginHorizontal();
                if (newEntryField == null) newEntryField = new Field(string.Empty, string.Empty, FieldType.Text);
                newEntryField.title = EditorGUILayout.TextField(GUIContent.none, newEntryField.title);
                DrawFieldType(newEntryField);
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newEntryField.title));
                if (GUILayout.Button("Create", GUILayout.Width(80)))
                {
                    newEntryField.title = "Entry " + entryNumber + " " + newEntryField.title;
                    if (!item.FieldExists(newEntryField.title))
                    {
                        item.fields.Add(newEntryField);
                        isAddingNewFieldToEntryNumber = -1;
                    }
                }
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                {
                    isAddingNewFieldToEntryNumber = -1;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Add New Field To Entry"))
                {
                    isAddingNewFieldToEntryNumber = entryNumber;
                    newEntryField = null;
                }
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawGeneratedDialogueEntry(Asset asset, int entryNumber, int entryCount, ref int entryToDelete,
            ref int entryToMoveUp, ref int entryToMoveDown, bool isRepeatEntry = false)
        {
            EditorGUILayout.BeginVertical("button");

            // Keep track of which fields we've already drawn:
            List<Field> alreadyDrawn = new List<Field>();

            // Heading:
            EditorGUILayout.BeginHorizontal();
            string entryTitle = string.Format(isRepeatEntry ? "Repeat Entry {0}" : "Entry {0}", entryNumber);
            EditorGUILayout.LabelField(entryTitle);
            GUILayout.FlexibleSpace();


            if (asset is Location)
            {
                
                var music = Field.LookupValue(asset.fields, "Music");

                if (!string.IsNullOrEmpty(music))
                {
                    Field musicEntry = Field.Lookup(asset.fields, isRepeatEntry ? "Music Repeat Entry" : "Music Entry");
                    
                    bool isMusicEntry = musicEntry != null && musicEntry.value == entryNumber.ToString();
                    //var audioClipTexture = EditorGUIUtility.IconContent( "d_AudioClip Icon",  "Hi").image;

                    var image = EditorGUIUtility.IconContent("d_AudioClip Icon").image;
                    var newIsMusicEntry = GUILayout.Toggle(isMusicEntry, new GUIContent(image, "Start playing the location's Music when this entry plays. (If this toggle is not enabled on any entry, then the music plays when the conversation finishes.)"), 
                        GUILayout.Width(35), GUILayout.Height( 22));

                    if (isMusicEntry != newIsMusicEntry)
                    {
                        if (!newIsMusicEntry) 
                        {
                            asset.fields.Remove(musicEntry);
                            SetDatabaseDirty("Remove Music Entry Field");
                        }
                        else
                        {
                            if (musicEntry == null)
                            {
                                musicEntry = new Field(isRepeatEntry ? "Music Repeat Entry" : "Music Entry", entryNumber.ToString(), FieldType.Text);
                                asset.fields.Add(musicEntry);
                                SetDatabaseDirty("Create Music Entry Field");
                            }
                            musicEntry.value = entryNumber.ToString();
                        }
                    }
                    
                    
                }
                
                
            }

            //--- Framework for future move up/down buttons:
            EditorGUI.BeginDisabledGroup(entryNumber <= 1);
            if (GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButtonLeft, GUILayout.Width(22)))
            {
                entryToMoveUp = entryNumber;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(entryNumber >= entryCount);
            if (GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButtonMid, GUILayout.Width(22)))
            {
                entryToMoveDown = entryNumber;
            }

            EditorGUI.EndDisabledGroup();
            // Also change Delete button below to miniButtonRight

            if (GUILayout.Button(new GUIContent("-", "Delete"), EditorStyles.miniButtonRight, GUILayout.Width(22)))
                //if (GUILayout.Button(new GUIContent(" ", "Delete quest entry"), "OL Minus", GUILayout.Width(16)))
            {
                entryToDelete = entryNumber;
            }

            EditorGUILayout.EndHorizontal();

            EditorWindowTools.EditorGUILayoutBeginGroup();

            EditorGUI.BeginChangeCheck();

            var menuTextFieldTitle = entryTitle + " Menu Text";
            var menuTextField = Field.Lookup(asset.fields, menuTextFieldTitle);
            if (menuTextField == null)
            {
                menuTextField = new Field(menuTextFieldTitle, "", FieldType.Text);
                asset.fields.Add(menuTextField);
            }

            var menuText = menuTextField.value;
            var menuTextLabel = string.IsNullOrEmpty(menuText)
                ? "Menu Text"
                : ("Menu Text (" + menuText.Length + " chars)");
            DrawRevisableTextAreaField(
                new GUIContent(menuTextLabel,
                    "Response menu text (e.g., short paraphrase). If blank, uses Dialogue Text."), asset, null,
                asset.fields, menuTextFieldTitle);
            DrawLocalizedVersions(asset, null, asset.fields, entryTitle + " {0}", false, FieldType.Text, alreadyDrawn);
            alreadyDrawn.Add(menuTextField);


            var dialogueTextFieldTitle = entryTitle + " Dialogue Text";
            var dialogueTextField = Field.Lookup(asset.fields, dialogueTextFieldTitle);
            if (dialogueTextField == null)
            {
                dialogueTextField = new Field(dialogueTextFieldTitle, "", FieldType.Text);
                asset.fields.Add(dialogueTextField);
            }

            var dialogueText = dialogueTextField.value;
            var dialogueTextLabel = string.IsNullOrEmpty(dialogueText)
                ? "Dialogue Text"
                : ("Dialogue Text (" + dialogueText.Length + " chars)");
            DrawRevisableTextAreaField(
                new GUIContent(dialogueTextLabel, "Line spoken by actor. If blank, uses Menu Text."), asset, null,
                asset.fields, dialogueTextFieldTitle);
            DrawLocalizedVersions(asset, null, asset.fields, entryTitle + " {0}", false, FieldType.Text, alreadyDrawn);
            alreadyDrawn.Add(dialogueTextField);

            EditorWindowTools.EditorGUILayoutEndGroup();

            EditorGUILayout.EndVertical();


        }

        private void DeleteQuestEntry(Asset asset, int entryNumber, int entryCount, bool isRepeatEntry = false)
        {
            if (EditorUtility.DisplayDialog(string.Format("Delete entry {0}?", entryNumber), "You cannot undo this action.", "Delete", "Cancel"))
            {
                CutEntry(asset, entryNumber, entryCount, isRepeatEntry);
                SetDatabaseDirty("Delete Quest Entry");

                if (isRepeatEntry)
                {
                    if (entryNumber == repeatEntrySelectedIdx + 1)
                    {
                        repeatEntrySelectedIdx = Mathf.Max(repeatEntrySelectedIdx - 1, 0);
                    }
                }
                else
                {
                    if (entryNumber == questEntrySelectedIdx + 1)
                    {
                        questEntrySelectedIdx = Mathf.Max(questEntrySelectedIdx - 1, 0);
                    }
                }
                
            }
        }

        private void MoveQuestEntryUp(Asset asset, int entryNumber, int entryCount, bool isRepeatEntry = false)
        {
            if (entryNumber <= 1) return;
            var clipboard = CutEntry(asset, entryNumber, entryCount, isRepeatEntry);
            entryCount--;
            PasteEntry(asset, entryNumber - 1, entryCount, clipboard);
            if (isRepeatEntry) repeatEntrySelectedIdx--;
            else questEntrySelectedIdx--;
        }

        private void MoveQuestEntryDown(Asset asset, int entryNumber, int entryCount, bool isRepeatEntry = false)
        {
            if (entryNumber >= entryCount) return;
            var clipboard = CutEntry(asset, entryNumber, entryCount, isRepeatEntry);
            entryCount--;
            PasteEntry(asset, entryNumber + 1, entryCount, clipboard);
            if (isRepeatEntry) repeatEntrySelectedIdx++;
            else questEntrySelectedIdx++;
        }

        private List<Field> CutEntry(Asset asset, int entryNumber, int entryCount, bool isRepeatEntry = false)
        {
            var entryTitle = isRepeatEntry ? "Repeat Entry": "Entry";
            var clipboard = new List<Field>();

            // Remove the entry and put it on the clipboard:
            string entryFieldTitle = $"{entryTitle} {entryNumber}";
            var extractedField = asset.fields.Find(field => string.Equals(field.title, entryFieldTitle));
            clipboard.Add(extractedField);
            asset.fields.Remove(extractedField);

            // Remove the other fields associated with the entry and put them on the clipboard:
            string entryPrefix = $"{entryTitle} {entryNumber} ";
            var extractedFields = asset.fields.FindAll(field => field.title.StartsWith(entryPrefix));
            clipboard.AddRange(extractedFields);
            asset.fields.RemoveAll(field => field.title.StartsWith(entryPrefix));

            // Renumber any higher entries:
            for (int i = entryNumber + 1; i <= entryCount; i++)
            {
                Field entryField = Field.Lookup(asset.fields,  $"{entryTitle} {i}");
                if (entryField != null) entryField.title = $"{entryTitle} {i - 1}";
                string oldEntryPrefix = $"{entryTitle} {i} ";
                string newEntryPrefix = $"{entryTitle} {i - 1} ";
                for (int j = 0; j < asset.fields.Count; j++)
                {
                    var field = asset.fields[j];
                    if (field.title.StartsWith(oldEntryPrefix))
                    {
                        field.title = newEntryPrefix + field.title.Substring(oldEntryPrefix.Length);
                    }
                }
            }

            // Decrement the count:
            Field.SetValue(asset.fields, $"{entryTitle} Count", entryCount - 1);

            return clipboard;
        }

        private void PasteEntry(Asset asset, int entryNumber, int entryCount, List<Field> clipboard)
        {
           
            var entryTitle =  clipboard.Any(p => p.title.StartsWith("Repeat")) ? "Repeat Entry": "Entry";
            
            // Set the clipboard's new entry number:
            var stateField = clipboard.Find(field => field.title.StartsWith($"{entryTitle} ") && field.title.Split(" ").Length > 1);
            
            if (stateField != null)
            {

                var oldEntryWithoutTitle = stateField.title.Substring($"{entryTitle} ".Length);
                
                var oldEntryNumber = oldEntryWithoutTitle.Contains(" ")
                    ? oldEntryWithoutTitle.Split(" ")[0]
                    : oldEntryWithoutTitle;
                
                var oldEntryPrefix =
                    stateField.title.Substring(0, $"{entryTitle} ".Length + oldEntryNumber.Length);
                  var newEntryPrefix = $"{entryTitle} {entryNumber}";
                
                  
                foreach (var field in clipboard)
                {
                    field.title = newEntryPrefix + field.title.Substring(oldEntryPrefix.Length);
                }
            }

            // Renumber any higher entries:
            for (int i = entryCount; i >= entryNumber; i--)
            {
                Field entryField = Field.Lookup(asset.fields, $"{entryTitle} {i}");
                if (entryField != null) entryField.title = $"{entryTitle} {i + 1}";
                string oldEntryPrefix = $"{entryTitle} {i} ";
                string newEntryPrefix = $"{entryTitle} {i + 1} ";
                for (int j = 0; j < asset.fields.Count; j++)
                {
                    var field = asset.fields[j];
                    if (field.title.StartsWith(oldEntryPrefix))
                    {
                        field.title = newEntryPrefix + field.title.Substring(oldEntryPrefix.Length);
                    }
                }
            }

            // Add the clipboard entry:
            asset.fields.AddRange(clipboard);

            // Increment the count:
            Field.SetValue(asset.fields, $"{entryTitle} Count", entryCount + 1);
        }

        private void DrawItemSpecificPropertiesSecondPart(Item item, int index, AssetFoldouts foldouts)
        {
            // Doesn't do anything right now.
        }

        private void SortItemFields(List<Field> fields, bool isRepeatEntry = false)
        {
            List<Field> entryFields = fields.Where(field => field.title.StartsWith(isRepeatEntry ? "Repeat Entry " : "Entry")).OrderBy(field => field.title).ToList();
            fields.RemoveAll(field => entryFields.Contains(field));
            fields.AddRange(entryFields);
            SetDatabaseDirty("Sort Fields");
        }
        
        
        private string ClosestMatch(string value, List<string> options)
        {
            if (options == null || options.Count == 0) return string.Empty;
            if (options.Contains(value)) return value;
            int minDistance = int.MaxValue;
            string closestMatch = string.Empty;
            
            foreach (var option in options)
            {
                int distance = LevenshteinDistance(value, option);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestMatch = option;
                }
            }
            return closestMatch;




            int LevenshteinDistance(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                // Step 1
                if (n == 0)
                {
                    return m;
                }

                if (m == 0)
                {
                    return n;
                }

                // Step 2
                for (int i = 0; i <= n; d[i, 0] = i++)
                {
                }

                for (int j = 0; j <= m; d[0, j] = j++)
                {
                }

                // Step 3
                for (int i = 1; i <= n; i++)
                {
                    //Step 4
                    for (int j = 1; j <= m; j++)
                    {
                        // Step 5
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                        // Step 6
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost);
                    }
                }
                // Step 7
                return d[n, m];
            }
        }
        

        public (int, int) TimeEstimate(DialogueEntry node)
        {
            
            List<List<DialogueEntry>> FindAllPathsBetweenNodes(DialogueEntry node1, DialogueEntry node2)
            {

                var stack = new List<DialogueEntry>();
                var visited = new List<DialogueEntry>();
                var paths = new List<List<DialogueEntry>>();

                var currentNode = node1;

                stack.Add(currentNode);

                // get all paths from node1 to node2 using DFS algorithm


                void DFS(DialogueEntry node)
                {
                    if (node == node2)
                    {
                        paths.Add(new List<DialogueEntry>(stack));
                        return;
                    }

                    visited.Add(node);

                    foreach (var link in node.outgoingLinks)
                    {
                        var nextNode = database.GetDialogueEntry(link.destinationConversationID, link.destinationDialogueID);
                        if (visited.Contains(nextNode)) continue;

                        stack.Add(nextNode);
                        DFS(nextNode);
                        stack.Remove(nextNode);
                    }
                }

                DFS(currentNode);
        
                return paths;
        
            }
            
            int GetTimespan(DialogueEntry dialogueEntry)
            {
                if (!Field.FieldExists(dialogueEntry.fields, "Timespan")) return -1;
        
                var timespanField = Field.Lookup(dialogueEntry.fields, "Timespan");
        
                var value = timespanField.value.Split(':')[0] == null ? 0 : int.Parse(timespanField.value.Split(':')[0]);

                var unit = timespanField.value.Split(':')[1];

                switch (unit)
                {
                    case "seconds":
                        break;
                    case "minutes":
                        value *= 60;
                        break;
                    case "hours":
                        value *= 3600;
                        break;
                }

                return value;
            }
            
            int GetNodeDuration(DialogueEntry dialogueEntry)
        {
            var time = 0;
            var timespan = GetTimespan(dialogueEntry);
            
            if (timespan >= 0)
            {
                time += timespan;
            }

            if (dialogueEntry.Sequence.Contains("BlackOut"))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"BlackOut\(([^)]*)\)";
                
                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(dialogueEntry.Sequence, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim());
                    
                }

                var blackoutTime = 0;
                
                if (int.TryParse( extractedContents[0] , out var secondsFromInt)) blackoutTime = secondsFromInt;
                
                else if (int.TryParse(Lua.Run($"return {extractedContents[0]}))]").AsString, out var secondsFromLua)) blackoutTime = secondsFromLua;
                
                if (extractedContents.Count > 1)
                {
                    switch (extractedContents[1])
                    {
                        case "seconds":
                            time += blackoutTime;
                            break;
                        case "minutes":
                            time += blackoutTime * 60;
                            break;
                        case "hours":
                            time += blackoutTime * 3600;
                            break;
                    }
                    
                }
                
                else blackoutTime *= 60;
                
                time += blackoutTime;

            }
           
            
            if (dialogueEntry.Sequence.Contains("AddSeconds"))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"AddSeconds\(([^)]*)\)";
                
                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(dialogueEntry.Sequence, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim());
                }
                
                if (int.TryParse( extractedContents[0] , out var secondsFromInt)) time += secondsFromInt;
                
                else if (int.TryParse(Lua.Run($"return {extractedContents[0]}))]").AsString, out var secondsFromLua)) time += secondsFromLua;
            }
            
            if (dialogueEntry.Sequence.Contains("AddMinutes"))
            {
                List<string> extractedContents = new List<string>();
                string pattern = @"AddMinutes\(([^)]*)\)";
                
                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(dialogueEntry.Sequence, pattern))
                {
                    extractedContents.Add(match.Groups[1].Value.Trim());
                }
                
                if (int.TryParse( extractedContents[0] , out var secondsFromInt)) time += secondsFromInt;
                
                else if (int.TryParse(Lua.Run($"return {extractedContents[0]}))]").AsString, out var secondsFromLua)) time += secondsFromLua;
            }
            
        
            return time > 0 ? time : GetLineAutoDuration(dialogueEntry.currentDialogueText);
        }
            
            int GetLineAutoDuration(string line)
            {

                if (line == string.Empty)
                {
            
                    return 0;
                }
                
                return (int)(line.Length * SecondsPerCharacter + SecondsPerLine);
            }
            
            int FindShortestDurationBetweenPaths(List<List<DialogueEntry>> paths)
            {
                var shortestDistance = int.MaxValue;
        
                foreach (var path in paths)
                {
                    var distance = 0;
            
                    for (int i = 0; i < path.Count; i++)
                    {
                        distance += GetNodeDuration(database.GetDialogueEntry(path[i].conversationID, path[i].id));
                    }
            
                    if (distance < shortestDistance) shortestDistance = distance;
                }
                return shortestDistance;
            }

            int FindLargestDurationBetweenPaths(List<List<DialogueEntry>> paths)
            {
                var largestDistance = 0;
        
                foreach (var path in paths)
                {
                    var distance = 0;
            
                    for (int i = 0; i < path.Count; i++)
                    {
                        distance += GetNodeDuration(database.GetDialogueEntry(path[i].conversationID, path[i].id));
                    }
            
                    if (distance > largestDistance) largestDistance = distance;
                }
        
                return largestDistance;
            }
            
            
            (int, int) DurationRangeBetweenNodes(DialogueEntry node1, DialogueEntry node2)
            {
                var paths = FindAllPathsBetweenNodes(node1, node2);
                var shortest = FindShortestDurationBetweenPaths(paths);
                var largest = FindLargestDurationBetweenPaths(paths);
                return (shortest, largest);
            }
            
            
            var minTimeEstimate = int.MaxValue;
            var maxTimeEstimate = 0;
            
            var timespan = GetNodeDuration(node);
            
            if (timespan != 0) minTimeEstimate = maxTimeEstimate = timespan;
            
            
            var finalNodes = new List<DialogueEntry>();
            
            foreach (var nodes in database.GetConversation(node.conversationID).dialogueEntries)
            {
                if (nodes.outgoingLinks.Count == 0) finalNodes.Add(nodes);
            }
            
            foreach (var finalNode in finalNodes)
            {
                var timeEstimate = DurationRangeBetweenNodes(node, finalNode);
                
                if (timeEstimate.Item1 < minTimeEstimate) minTimeEstimate = timeEstimate.Item1;
                if (timeEstimate.Item2 > maxTimeEstimate) maxTimeEstimate = timeEstimate.Item2;
            }

            if (minTimeEstimate > maxTimeEstimate) minTimeEstimate = maxTimeEstimate;
            
            return (minTimeEstimate, maxTimeEstimate);
        }


        private string DurationLabel(int duration)
        {
            var entryHours = duration / 3600;
            var entryMins = (duration % 3600) / 60;
            var entrySecs = (duration % 3600) % 60;
                    
            var entryHoursLabel = entryHours == 1 ? "Hour" : "Hours";
            var entryMinsLabel = entryMins == 1 ? "Minute" : "Minutes";
            var entrySecsLabel = entrySecs == 1 ? "Second" : "Seconds";

            var entryLabel = entryHours > 0 ? $"{entryHours} {entryHoursLabel}" : string.Empty;
            if (!string.IsNullOrEmpty(entryLabel) && entryMins > 0) entryLabel += ", ";
            entryLabel += entryMins > 0 ? $"{entryMins} {entryMinsLabel}" : string.Empty;
            if (!string.IsNullOrEmpty(entryLabel) && entrySecs > 0) entryLabel += ", ";
            entryLabel += entrySecs > 0 ? $"{entrySecs} {entrySecsLabel}" : string.Empty;
            
            if (string.IsNullOrEmpty(entryLabel)) entryLabel = "0 Seconds";
            
            return entryLabel;
        }

    }

}