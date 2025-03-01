// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
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
        
        private ReorderableList actionReorderableList = null;

        [SerializeField]
        private int itemListSelectedIndex = -1;

        [SerializeField]
        private int questEntrySelectedIdx = -1;
        
        [SerializeField]
        private int repeatEntrySelectedIdx = -1;
        
        [SerializeField]
        private int conditionalDisplayEntrySelectedIdx = -1;

        [SerializeField]
        private bool showCompactQuestEntryList = true; // Thanks to Tasta for compact view idea.
        
        [SerializeField]
        private bool showCompactConditionalDisplayEntryList = true;
        
        [SerializeField]
        private bool showCompactRepeatEntryList = true;

        private HashSet<int> syncedItemIDs = null;

        private int isAddingNewFieldToEntryNumber = -1;
        
        private int itemToolbarIndex = 0;
        
        private Field newEntryField;

        private List<Item> filteredItems;
        
        private List<Item> items => database.items.FindAll(p => p.IsItem);
        private List<Item> quests => database.items.FindAll(p => p.IsQuest);
        private List<Item> actions => database.items.FindAll(p => p.IsAction);
        private List<Item> emails => database.items.FindAll(p => p.IsEmail);
        
        private List<Item> tutorials => database.items.FindAll(p => p.IsTutorial);
        
        private List<Item> contacts => database.items.FindAll(p => p.IsContact);
        
        private List<Item> points => database.items.FindAll(p => p.IsPointCategory);
        
        private List<Item> apps => database.items.FindAll(p => p.IsApp);
        
        
        private string[] itemToolbarNames => new[] {"Item", "Quest", "Action", "Email", "Contact", "Tutorial", "Point", "App", "Uncategorized"};
        private string[] itemToolbarNamesPlural => new[] {"Items", "Quests", "Actions", "Emails", "Contacts", "Tutorials", "Points", "Apps"};
        
        private List<Action<Item>> drawMethods => new() {DrawItemProperties,  DrawQuestProperties, DrawActionProperties, DrawEmailProperties, DrawContactProperties, DrawTutorialProperties, DrawPointsCategoryProperties, DrawAppProperties, DrawUncategorizedProperties};
        
        private List<List<Item>> itemMatrix => new() { items, quests, actions, emails, contacts, tutorials, points, apps, uncategorized};

        private List<Item> uncategorized;
        private List<Field> CurrentItemTemplateFields => new[] {template.itemFields, template.questFields, template.actionFields, template.emailFields, template.contactFields, template.tutorialFields, template.pointsCategoryFields, template.appFields}[itemToolbarIndex];
        
        private string CurrentItemLabel => itemToolbarNames[itemToolbarIndex];
        private List<Item> CurrentItemList => itemMatrix[itemToolbarIndex];
        
        private Action<Item> CurrentDrawMethod => drawMethods[itemToolbarIndex];
        
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
           // UpdateTreatItemsAsQuests(template.treatItemsAsQuests);
            //UpdateTreatQuestsAsActions(template.treatQuestsAsActions);
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
            uncategorized = database.items.FindAll(p => !p.IsItem && !p.IsQuest && !p.IsAction && !p.IsEmail && !p.IsContact && !p.IsTutorial && !p.IsPointCategory && !p.IsApp);

            var toolbarNames = itemToolbarNamesPlural.ToList();
            
            if (uncategorized.Count != 0)
            {
                toolbarNames.Add($"Uncategorized");
            }
            
            var newItemToolbarIndex = GUILayout.Toolbar(itemToolbarIndex, toolbarNames.ToArray(), GUILayout.Width(toolbarNames.Count * 90));

            if (newItemToolbarIndex != itemToolbarIndex)
            {
                itemToolbarIndex = newItemToolbarIndex;
                itemReorderableList = null;
                inspectorSelection = null;
            }
            
            DrawItems( CurrentItemLabel, CurrentItemList);
        }


        private void DrawItems(string label, List<Item> itemList)
        {
            if (itemReorderableList == null) InitializeItemReorderableList(itemList);
            var filterChanged = DrawFilterMenuBar(label, DrawItemMenu, ref itemFilter, ref hideFilteredOutItems);
            if (filterChanged)  InitializeItemReorderableList(itemList);
            if (database.syncInfo.syncItems) DrawItemSyncDatabase();

            if (itemReorderableList != null && itemReorderableList.list != null && itemReorderableList.list.Count > 0)
            {
                itemReorderableList.DoLayoutList();
            }

            else
            {
                EditorGUILayout.HelpBox("No items found.", MessageType.Info);
            }
        }

        private bool HideFilteredOutItems()
        {
            return hideFilteredOutItems && !string.IsNullOrEmpty(itemFilter);
        }

        private void InitializeItemReorderableList(List<Item> itemsList)
        {
            foreach (var item in database.items)
            {
                if (item.FieldExists("Is Item") && item.LookupBool("Is Item") && !item.LookupBool("Is Action"))
                {
                    item.IsItem = true;
                    item.fields.Remove( Field.Lookup(item.fields, "Is Item"));
                }

                else if (item.FieldExists("Is Item") && !item.LookupBool("Is Item") && !item.LookupBool("Is Action"))
                {
                    item.IsQuest = true;
                    item.fields.Remove( Field.Lookup(item.fields, "Is Item"));
                }
                else if (item.FieldExists("Is Static") && !item.IsAction)
                {
                    item.IsAction = true; item.fields.Remove( Field.Lookup(item.fields, "Is Action"));
                }
                
                if (!item.FieldExists("Item Type")) item.fields.Add(new Field("Item Type", "Quest", FieldType.Text));

                if (item.IsFieldAssigned("Item Type") && item.AssignedField("Item Type").value == "Static")
                    item.AssignedField("Item Type").value = "Action";
            }
            
            
            if (HideFilteredOutItems())
            {
                filteredItems = itemsList.FindAll(item => EditorTools.IsAssetInFilter(item, itemFilter));
               
            }
            else
            {
                filteredItems = itemsList;
            }
            
            itemReorderableList = new ReorderableList(filteredItems, typeof(Item), true, true, true, true);
            
            itemReorderableList.drawHeaderCallback = DrawItemListHeader;
            itemReorderableList.drawElementCallback = DrawItemListElement;
            itemReorderableList.drawElementBackgroundCallback = DrawItemListElementBackground;
            itemReorderableList.onAddCallback = OnItemListAdd;    
            itemReorderableList.onRemoveCallback = OnItemListRemove;
            itemReorderableList.onSelectCallback = OnItemListSelect;
            itemReorderableList.onReorderCallback = OnItemListReorder;


            
            
        }
        
        
        


        private const float ItemReorderableListTypeWidth = 40f;

        private void DrawItemListHeader(Rect rect)
        {
            var label = CurrentItemLabel == "Email" ? "Subject" : "Name";
            var description = CurrentItemLabel == "Email" ? "Body" : "Description";
            
            var fieldWidth = (rect.width - 14) / 4;
            EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, fieldWidth, rect.height), label);
            EditorGUI.LabelField(new Rect(rect.x + 14 + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), description);
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
           
                var fieldWidth = rect.width / 4;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(nameControl);
                
                
                itemName = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, item.Name);
                if (EditorGUI.EndChangeCheck()) item.Name = itemName;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(descriptionControl);
                description = EditorGUI.TextField(new Rect(rect.x + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
                if (EditorGUI.EndChangeCheck()) item.Description = description;
           
                
            EditorGUI.EndDisabledGroup();
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.Equals(nameControl, focusedControl) || string.Equals(descriptionControl, focusedControl))
            {
                inspectorSelection = item;
            }
        }

        private void DrawItemListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = filteredItems[index];

            if (itemToolbarIndex == 2)
            {
                if (!(0 <= index && index < filteredItems.Count)) return;
                var defaultColor = new Color(0.225f, 0.225f, 0.225f, 1);
                var location = item.LookupLocation("Location", database);
          
                if (location != null && template.useLocationColors)
                {
                    location = location.GetRootLocation(database);
                    var color = location.LookupColor("Color");
                    if (!isActive)
                    {
                        color = EditorTools.ColorBlend(color, defaultColor, EditorTools.ColorBlendMode.SoftLight);
                        // color *= 0.5f;
                        // color += defaultColor * 0.3f;
                        color = Color.Lerp(defaultColor, color, 0.25f);
                    }

                    else color = Color.Lerp(defaultColor, color, 0.5f);
                    EditorGUI.DrawRect(rect, color);
                    return;
                }
            }
            
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
          //  var menu = new GenericMenu();
            //menu.AddItem(new GUIContent("Quest"), false, OnAddNewQuest, null);
          //  menu.AddItem(new GUIContent("Item"), false, OnAddNewItem, null);
           // menu.ShowAsContext();
        }

        private void OnItemListAdd(ReorderableList list)
        {
            OnAddNewItem( null);
        }

        private void OnItemListRemove(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < CurrentItemList.Count)) return;
            var item = list.list [list.index] as Item;
            if (item == null) return;
            if (IsItemSyncedFromOtherDB(item)) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(item)), "Are you sure you want to delete this?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (deletedLastOne) inspectorSelection = null;
                else inspectorSelection = (list.index < list.count) ? CurrentItemList[list.index] : (list.count > 0) ? CurrentItemList[list.count - 1] : null;
                SetDatabaseDirty("Remove Item");
                
                database.items.Remove(item);
            }
        }
        
        private void OnActionListRemove(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < actions.Count)) return;
            var action = list.list [list.index] as Item;
            if (action == null) return;
            if (IsItemSyncedFromOtherDB(action)) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(action)), "Are you sure you want to delete this?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (deletedLastOne) inspectorSelection = null;
                else inspectorSelection = (list.index < list.count) ? actions[list.index] : (list.count > 0) ? actions[list.count - 1] : null;
                SetDatabaseDirty("Remove Action");
                
                database.items.Remove(action);
            }
        }

        private void OnItemListReorder(ReorderableList list)
        {
            
            var item = list.list [list.index] as Item;
            var currentIndexInDatabase = database.items.IndexOf(CurrentItemList[list.index]) - 1;
            currentIndexInDatabase = Mathf.Clamp(currentIndexInDatabase, 0, database.items.Count - 1);
            
            database.items.Remove(item);
            
            if (list.index < CurrentItemList.Count - 1)
            {
                database.items.Insert(currentIndexInDatabase, item );
            }
            else
            {
                database.items.Add(item);
            }
            SetDatabaseDirty("Reorder Items");
            
        }
        
        private void OnItemListSelect(ReorderableList list)
        {
          
            if (!(0 <= list.index && list.index < CurrentItemList.Count)) return;
            inspectorSelection = CurrentItemList[list.index];
            itemListSelectedIndex = list.index;
        }
        

        public void DrawSelectedItemSecondPart()
        {
            var item = inspectorSelection as Item;
            if (item == null) return;
            var index = itemListSelectedIndex;
            DrawFieldsFoldout<Item>(item, index, itemFoldouts);
            DrawAssetSpecificPropertiesSecondPart(item, index, itemFoldouts);
        }

        private void DrawItemMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent($"New {CurrentItemLabel}"), false, AddNewItem);
                
                if (CurrentItemLabel == "Action") 
                    menu.AddItem(new GUIContent("Use Location Colors"), template.useLocationColors, ToggleUseLocationColors);
                
                if (CurrentItemLabel == "Point") 
                    menu.AddItem(new GUIContent("Cleanup Excess Points"), false, CleanupExcessPoints);
                
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

        private void AddNewItem()
        {
            template = Template.FromDefault();
            AddNewAssetFromTemplate<Item>(database.items, CurrentItemTemplateFields, CurrentItemLabel);
            SetDatabaseDirty("Add New Item");
            InitializeItemReorderableList(CurrentItemList);
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

        private void ToggleUseLocationColors()
        {
            template.useLocationColors = !template.useLocationColors;
            SetDatabaseDirty("Toggle Use Location Colors");
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
            InitializeItemReorderableList(items);
            SetDatabaseDirty("Toggle Sync Items");
        }

        private void CleanupExcessPoints()
        {
            var pointLabels = points.Select(x => x.Name).ToList();
            
            bool cleanup = false;
            List<Field> fieldsToRemove = new List<Field>();
            List<string> excessPointLabels = new List<string>();

            foreach (var item in database.items)
            {
                var nonmatchingPoints = item.fields
                    .Where(p => p.title.EndsWith(" Points") && !pointLabels.Contains(p.title.Split(" ")[^2])).ToList();
                if (nonmatchingPoints.Count > 0)
                {
                    fieldsToRemove.AddRange(nonmatchingPoints);
                    excessPointLabels.AddRange(nonmatchingPoints.Select(x => x.title));
                }
            }

            excessPointLabels = excessPointLabels.Distinct().ToList();
            
            if (fieldsToRemove.Count > 0)
            {
                if (EditorUtility.DisplayDialog("Cleanup Excess Points",
                        $"Delete {fieldsToRemove.Count} fields that don't match any point category?", "Yes", "No"))
                {
                    foreach (var item in database.items)
                    {
                        item.fields.RemoveAll(x => fieldsToRemove.Contains(x));
                    }
                }
            }
            
            
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
                InitializeItemReorderableList(items);
                syncedItemIDs = null;
                SetDatabaseDirty("Change Sync Items Database");
            }
            if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72)))
            {
                database.SyncItems();
                InitializeItemReorderableList(items);
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
            if (item == null) return;

            foreach (var field in item.fields)
            {
                if (field.title == null) field.title = string.Empty;
                if (field.value == null) field.value = string.Empty;
            }
            
            CurrentDrawMethod.Invoke(item);
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
            
            
            // Descriptions:
            DrawRevisableTextAreaField(questDescriptionLabel, item, null, item.fields, "Description");
            DrawLocalizedVersions(item, item.fields, "Description {0}", false, FieldType.Text);
            DrawRevisableTextAreaField(questSuccessDescriptionLabel, item, null, item.fields, "Success Description");
            DrawLocalizedVersions(item, item.fields, "Success Description {0}", false, FieldType.Text);
            DrawRevisableTextAreaField(questFailureDescriptionLabel, item, null, item.fields, "Failure Description");
            DrawLocalizedVersions(item, item.fields, "Failure Description {0}", false, FieldType.Text);
            
            

            // Has Entries:
            bool hasQuestEntries = item.FieldExists("Entry Count");
            
            
            bool newHasQuestEntries = EditorGUILayout.Toggle(new GUIContent("Has Entries (Subtasks)", "Tick to add quest entries to this quest."), hasQuestEntries);
            if (newHasQuestEntries != hasQuestEntries) ToggleHasQuestEntries(item, newHasQuestEntries);

            
            //Points
            if (!newHasQuestEntries) DrawPoints(item, string.Empty, false);
            
            // Entries:
            if (newHasQuestEntries) DrawReorderableEntries(item, "Entry", ref questEntrySelectedIdx, ref showCompactQuestEntryList, DrawQuestEntry);
            
            // Other main fields specified in template:
            DrawOtherQuestPrimaryFields(item);
            
            if (newHasQuestEntries)
            {
                Field autoSetSuccess = Field.Lookup(item.fields, "Auto Set Success");
            
                if (autoSetSuccess == null)
                {
                    autoSetSuccess = new Field("Auto Set Success", "True", FieldType.Boolean);
                    item.fields.Add(autoSetSuccess);
                    SetDatabaseDirty("Create Auto Set Success Field");
                }
                
                
                autoSetSuccess.value = EditorGUILayout.Toggle(new GUIContent("Auto Set Success", "If true, when all quest entries are Success, then the quest itself is set to Success. Conversely, if the quest is Success, then all quest entries are set to Success."), item.LookupBool("Auto Set Success")).ToString();
            }

            else
            {
                Field autoSetSuccess = Field.Lookup(item.fields, "Auto Set Success");
            
                if (autoSetSuccess != null)
                {
                    item.fields.Remove(autoSetSuccess);
                    SetDatabaseDirty("Remove Auto Set Success Field");
                }
            }
        }


        private void DrawEmailProperties(Item item)
        {
            
            // Descriptions:
            DrawRevisableTextAreaField(new GUIContent("Body"), item, null, item.fields, "Description");
            
            // From:
            
            Field senderField = Field.Lookup(item.fields, "Sender");
            if (senderField == null)
            {
                senderField = new Field("Sender", string.Empty, FieldType.Actor);
                item.fields.Add(senderField);
                SetDatabaseDirty("Create Sender Field");
            }
            
            senderField.value = DrawActorField(new GUIContent("Sender"), senderField.value);
            
            
            Field stateField = Field.Lookup(item.fields, "State");
            if (stateField == null)
            {
                stateField = new Field("State", "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Create State Field");
            }
            //EditorGUILayout.LabelField(new GUIContent("State", "The starting state of the quest."), GUILayout.Width(140));
            stateField.value = DrawQuestStateFieldTruncated(new GUIContent("State", "The starting state of this email."), stateField.value);

            switch (stateField.value)
            {
                case "active":
                    EditorGUILayout.HelpBox("This email is delivered but unread. It will appear in the player's inbox", MessageType.Info);
                    break;
                case "success":
                    EditorGUILayout.HelpBox("This email has been read by the player.", MessageType.Info);
                    break;
                default:
                    EditorGUILayout.HelpBox("This email is unsent. It will not appear in the player's inbox.", MessageType.Info);
                    break;
            }
            
            
            EditorGUILayout.Space();
            DrawScript( item, "Script", label: new GUIContent( "Script", "The script that is sent when the player reads the email for the first time."));
         
        }

        private void DrawContactProperties(Item item)
        {
            // Descriptions:
            DrawRevisableTextAreaField(new GUIContent("Body"), item, null, item.fields, "Description");
            
            DrawStartConversationProperties( item);
            
            Field availableField = Field.Lookup(item.fields, "Available");
            
            if (availableField == null)
            {
                availableField = new Field("Available", "True", FieldType.Boolean);
                item.fields.Add(availableField);
                SetDatabaseDirty("Create Available Field");
            }
            
            availableField.value = EditorGUILayout.Toggle(new GUIContent("Available", "Tick to make this contact available to the player."), item.LookupBool("Available")).ToString();
           
        }
        
        private void DrawTutorialProperties(Item item)
        {
            // Descriptions:
            DrawRevisableTextAreaField(new GUIContent("Body"), item, null, item.fields, "Description");
            
            
            Field stateField = Field.Lookup(item.fields, "State");
            if (stateField == null)
            {
                stateField = new Field("State", "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Create State Field");
            }
            //EditorGUILayout.LabelField(new GUIContent("State", "The starting state of the quest."), GUILayout.Width(140));
            stateField.value = DrawQuestStateFieldTruncated(new GUIContent("State", "The starting state of this email."), stateField.value);

            switch (stateField.value)
            {
                case "active":
                    EditorGUILayout.HelpBox("This tutorial is available to the player but unread.", MessageType.Info);
                    break;
                case "success":
                    EditorGUILayout.HelpBox("This tutorial has been read by the player.", MessageType.Info);
                    break;
                default:
                    EditorGUILayout.HelpBox("This tutorial is not visible to the player.", MessageType.Info);
                    break;
            }
         
        }

        private void DrawPointsCategoryIconPreview(Item item, float size = 100f, Color multiplyColor = default, Color backgroundColor = default)
        {
            if (item.icon == null) return;
            Color defaultBGColor = backgroundColor == default ? EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.76f, 0.76f, 0.76f) : backgroundColor;

            
            
            void DrawImageWithRing(Texture2D image, float size, float thickness, Color imageColor, Color ringColor,  float imageScale = 0.3f)
            {
                thickness *= size;
                // Get position
                Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
                Vector2 center = rect.center;

                // Draw the outer ring
                Handles.color = multiplyColor == default ? ringColor : EditorTools.ColorBlend( ringColor, multiplyColor, EditorTools.ColorBlendMode.Multiply);
                
                
                Handles.DrawSolidArc(center, Vector3.forward, Vector2.up, 360, size * 0.5f);

                // Fill the center with the default Unity inspector background color
                 Handles.color = defaultBGColor;
                
                
                Handles.DrawSolidArc(center, Vector3.forward, Vector2.up, 360, size * 0.5f - thickness);

                // Draw the image inside the ring
                float innerSize = size - thickness * 2 * imageScale;  // Ensure it fits within the ring
                Rect imageRect = new Rect(center.x - innerSize * 0.5f, center.y - innerSize * 0.5f, innerSize, innerSize);
                var oldColor = GUI.color;
                GUI.color = multiplyColor == default ? imageColor : EditorTools.ColorBlend( imageColor, multiplyColor, EditorTools.ColorBlendMode.Multiply);
                GUI.DrawTexture(imageRect, image, ScaleMode.ScaleToFit);
                GUI.color = oldColor;
            }
            
            DrawImageWithRing(item.icon, size, 0.05f, item.LookupColor("Color"), item.LookupColor("Ring Color"), item.LookupFloat("Icon Scale"));
            
        }
        private void DrawPointsCategoryProperties(Item item)
        {
            
            Field name = Field.Lookup(item.fields, "Name");
            if (name == null)
            {
                name = new Field("Name", $"New Points {item.id}", FieldType.Text);
                item.fields.Add(name);
                SetDatabaseDirty("Create Name Field");
            }
            
            var newName = EditorGUILayout.TextField(new GUIContent("Name"), name.value);
            
            
            
            if (string.IsNullOrEmpty(newName)) newName = name.value;
            
            if (points.Any(p => p.Name == newName))
            {
                newName = name.value;
            }
            
            if (newName != name.value)
            {
                SetDatabaseDirty("Change Name Field");

                foreach (var i in database.items)
                {
                    if (i.IsFieldAssigned($"{name.value} Points"))
                    {
                        i.AssignedField($"{name.value} Points").title = $"{newName} Points";
                    }
                    
                    if (i.IsFieldAssigned($"{name.value} Affinity"))
                    {
                        i.AssignedField($"{name.value} Affinity").title = $"{newName} Affinity";
                    }
                }
            }
            
            name.value = newName;
            
            DrawRevisableTextAreaField(new GUIContent("Description"), item, null, item.fields, "Description");
            
            
            DrawColorField( new GUIContent("Color"), item, "Color");
            
            DrawColorField( new GUIContent("Ring Color"), item, "Ring Color");
            
            Field iconScale = Field.Lookup(item.fields, "Icon Scale");
            if (iconScale == null)
            {
                iconScale = new Field("Icon Scale", "0.3", FieldType.Text);
                item.fields.Add(iconScale);
                SetDatabaseDirty("Create Icon Scale Field");
            }
            
            iconScale.value = EditorGUILayout.TextField(new GUIContent("Icon Scale"), iconScale.value);

            DrawIconField(new GUIContent("Icon", "The icon used for this point category."), item);
            
            DrawPointsCategoryIconPreview(item);
            
            Field scoreField = Field.Lookup(item.fields, "Score");
            
            if (scoreField == null)
            {
                scoreField = new Field("Score", "0", FieldType.Number);
                item.fields.Add(scoreField);
                SetDatabaseDirty("Create Score Field");
            }
            
            var currentScore = item.LookupInt("Score");
            var newScore = EditorGUILayout.IntField(new GUIContent("Score"), currentScore);
            if (newScore != currentScore)
            {
                Field.SetValue(item.fields, "Score", newScore);
                SetDatabaseDirty("Change Score Field");
            }

            var maxScore = 0;
            
            Field overrideMaxScore = Field.Lookup(item.fields, "Override Max Score");
            if (overrideMaxScore == null)
            {
                overrideMaxScore = new Field("Override Max Score", "False", FieldType.Boolean);
                item.fields.Add(overrideMaxScore);
                SetDatabaseDirty("Create Override Max Score Field");
            }


            for (int i = 0; i < database.items.Count(); i++)
            {
                foreach (var pointsField in database.items[i].fields.Where(p => p.title.EndsWith(" Points")))
                {
                    var type = pointsField.title.Split(" ")[^2];
                    if (type == item.Name)
                    {
                        var value = database.items[i].LookupInt(pointsField.title);
                        if (value > 0) maxScore += database.items[i].LookupInt(pointsField.title);
                    }
                }
            }

            EditorGUI.BeginDisabledGroup((overrideMaxScore.value == "True"));
            var newMaxScore = overrideMaxScore.value == "True" ? item.LookupInt("Max Score") : maxScore;
            EditorGUILayout.IntField(new GUIContent("Max Score"), maxScore);
            EditorGUI.EndDisabledGroup();
            
            
            EditorWindowTools.EditorGUILayoutBeginIndent();
            overrideMaxScore.value = EditorGUILayout.Toggle(new GUIContent("Override Max Score", "Tick to override the max score with the value above."), item.LookupBool("Override Max Score")).ToString();
            
            if (overrideMaxScore.value == "True")
            {
                newMaxScore = EditorGUILayout.IntField(new GUIContent("Max Score"), newMaxScore);
            }
            
            Field.SetValue(item.fields, "Max Score", newMaxScore);
            
            EditorWindowTools.EditorGUILayoutEndIndent();
            
        }
        
        private void DrawAppProperties(Item item)
        {
            Field isDefaultField = Field.Lookup(item.fields, "Is Default");
            
            if (isDefaultField == null)
            {
                isDefaultField = new Field("Is Default", "False", FieldType.Boolean);
                item.fields.Add(isDefaultField);
                SetDatabaseDirty("Create Is Default Field");
            }
            
            var anyAppDefault = apps.Any(x => x.LookupBool("Is Default"));

            if (!anyAppDefault || isDefaultField.value == "True")
            {
                var newIsDefault = EditorGUILayout.Toggle(new GUIContent("Is Default", "Tick to make this the default (home screen) app."), item.LookupBool("Is Default")).ToString();
                if (newIsDefault != isDefaultField.value)
                {
                    foreach (var app in apps)
                    {
                        if (app != item)
                        {
                            app.fields.Find(x => x.title == "Is Default").value = "False";
                        }
                    }
                
                    isDefaultField.value = newIsDefault;
                }
            }

           

            if (item.LookupBool("Is Default")) return;
            
            DrawDoubleScripts( item, "Script", $"SetSmartWatchApp(\"{item.Name}\")");
            
            var forceResponseMenu = Field.Lookup(item.fields, "Force Response Menu");
            if (forceResponseMenu == null)
            {
                forceResponseMenu = new Field("Force Response Menu", "False", FieldType.Boolean);
                item.fields.Add(forceResponseMenu);
                SetDatabaseDirty("Create Force Response Menu Field");
            }
            
            forceResponseMenu.value = EditorGUILayout.Toggle(new GUIContent("Force Response Menu", "Tick to force the response menu to appear when this app is opened by generating a dialogue entry."), item.LookupBool("Force Response Menu")).ToString();
            
            EditorGUILayout.Space();
            
            DrawColorField( new GUIContent("Color"), item, "Color");
            
            DrawIconField(new GUIContent("Icon", "The icon used for this app in the home screen."), item);
            
        }
        
        private void DrawUncategorizedProperties(Item item)
        {
            Field itemTypeField = Field.Lookup(item.fields, "Item Type");
            if (itemTypeField == null)
            {
                itemTypeField = new Field("Item Type", "Uncategorized", FieldType.Text);
                item.fields.Add(itemTypeField);
                SetDatabaseDirty("Create Item Type Field");
            }
            
            itemTypeField.value = EditorGUILayout.TextField(new GUIContent("Item Type"), itemTypeField.value);
         
        }
        
        private void DrawIconField( GUIContent label, Item item)
        {
            var newIcon = EditorGUILayout.ObjectField(label,
                item.icon, typeof(Texture2D), false, GUILayout.Height(64)) as Texture2D;
            if (newIcon != item.icon)
            {
                item.icon = newIcon;
                ClearActorInfoCaches();
                SetDatabaseDirty("Item Icon");
            }

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
        
        private bool conversationPropertiesFoldout = false;
        private bool locationPropertiesFoldout = false;
        private bool pointsPropertiesFoldout = true;
        private bool staticPropertiesFoldout = true;

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

            // State:
            
            Field stateField = Field.Lookup(item.fields, "State");
            if (stateField == null)
            {
                stateField = new Field("State", "unassigned", FieldType.Text);
                item.fields.Add(stateField);
                SetDatabaseDirty("Create State Field");
            }
            
            stateField.value = DrawQuestStateField(new GUIContent("State", "The starting state of the quest."), stateField.value);
            
             
            if (item.IsStatic && stateField.value != "active")
            {
                EditorGUILayout.HelpBox( "Static actions must be \"active\" to become available. You can set this state during a conversation.", MessageType.Warning);
            }
            
            
            
            // Static

            var isStaticAction = item.IsStatic;
            
            var newIsStaticAction = false;
            
            DrawEditorItemWithStaticIcon( () =>  newIsStaticAction = EditorGUILayout.Toggle(
                new GUIContent("Static", "Static actions show when the state is \"active\" and are set to \"success\" when completed. Static actions that are set to \"success\" will immediately revert to \"active\"."),
                isStaticAction));
            
            
            if (newIsStaticAction != isStaticAction)
            {
                ToggleStatic(item, newIsStaticAction);
            }


            //static properties
            if (item.IsStatic)
            {
                staticPropertiesFoldout = EditorGUILayout.Foldout(staticPropertiesFoldout, "Static Properties");
                if (staticPropertiesFoldout)
                {
                    var hasConditionalDisplayText = Field.Lookup(item.fields, "Conditional Display Entry Count") != null;
                    
                    var newHasConditionalDisplayText = EditorGUILayout.Toggle(new GUIContent("Conditional Display", "Tick to show this action only if the specified number of entries are complete."), hasConditionalDisplayText);
                    
                    if (newHasConditionalDisplayText != hasConditionalDisplayText)
                    {
                        ToggleHasConditionalDisplayEntries( item, newHasConditionalDisplayText);
                    }
                    
                    if (newHasConditionalDisplayText)  DrawReorderableEntries(item, "Conditional Display Entry", ref conditionalDisplayEntrySelectedIdx,
                        ref showCompactConditionalDisplayEntryList, DrawConditionalDisplayEntry);
                    
                }
            }
            
            SetDatabaseDirty("Remove Starts Conversation Field");
            
            
            EditorGUILayout.Space();
            
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
                
                EditorWindowTools.StartIndentedSection();
                
                conversationPropertiesFoldout = EditorGUILayout.Foldout( conversationPropertiesFoldout, "Conversation Properties");
                if (conversationPropertiesFoldout)
                {
                    DrawStartConversationProperties(item);
                
                   
                }
                
                EditorWindowTools.EndIndentedSection();

                
            }
            
            EditorGUILayout.Space();
            
            //Conditions
            
            var defaultCondition = $"CurrentQuestState(\"{item.Name}\") == \"active\"";
            if (item.IsStatic)
            {
                DrawConditions(item, "Conditions", defaultCondition);
            }

            else
            {
                DrawDoubleConditions(item, "Conditions", defaultCondition);
            }
             
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
            
            if (item.IsStatic) DrawScript( item, "Script", $"SetQuestState(\"{item.Name}\", \"success\");");
            else DrawDoubleScripts( item, "Success", $"SetQuestState(\"{item.Name}\", \"success\");");
            
            EditorGUILayout.Space();
            
            
            
           
              
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
                timeFlow = new Field("Time Flow", "Natural", FieldType.Text);
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
                duration.value = EditorGUILayout.IntField("Duration (s)" , item.LookupInt("Explicit Duration")).ToString();
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

                        if (item.IsFieldAssigned("Repeat Entry Count"))
                        {
                            
                            naturalDurationB = 0;
                            
                            for (int i = 1; i < item.LookupInt("Repeat Entry Count") + 1; i++)
                            {
                                var subtitleText = item.LookupValue($"Repeat Entry {i} Dialogue Text");
                                
                                if (!string.IsNullOrEmpty(subtitleText)) naturalDurationB += subtitleText.Length > 0
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
            
            
            
            var hasPoints = item.fields.Any(p => p.title.EndsWith(" Points"));


         
            
           var newHasPoints = EditorGUILayout.Toggle(new GUIContent("Points", "Tick to add points to this action."), hasPoints);

           if (hasPoints) DrawPoints(item);
           
           if (newHasPoints != hasPoints)

           {
               TogglePoints(item, newHasPoints);
               SetDatabaseDirty(!hasPoints ? "Create Points Field" : "Remove Points Field");
           }



           Field repeatCount = Field.Lookup(item.fields, "Repeat Count");
            if (repeatCount == null)
            {
                repeatCount = new Field("Repeat Count", "0", FieldType.Number);
                item.fields.Add(repeatCount);
                SetDatabaseDirty("Create Repeat Count Field");
            }
            
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
        
        #region Icons 
        
        
        private void DrawEditorItemWithStaticIcon( Action action, float? labelWidth = null)
        {
            if (action == null) return;
            var defaultContentColor = GUI.contentColor;
            var defaultLabelWidth = EditorGUIUtility.labelWidth;
                
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("AnimatorController On Icon").image , GUILayout.Width(22), GUILayout.Height(22));
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
        
        #endregion
        
        
        
        #region DrawOtherProperties 
        
        
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
        
        private void DrawConditions(Asset asset, string fieldName, string prependedCondition = "")
        {
            var conditions = Field.Lookup(asset.fields, fieldName);
                
            var hasPrepended = !string.IsNullOrEmpty(prependedCondition);
            
            if (conditions == null)
            {
                conditions = new Field(fieldName,  hasPrepended ? $"{prependedCondition} and (true)" : "", FieldType.Text);
                asset.fields.Add(conditions);
            }
            
            
            EditorWindowTools.EditorGUILayoutBeginGroup();

            luaConditionWizard.database = database;
             
            var conditionsText = hasPrepended ? conditions.value.Substring(conditions.value.Split(" and ")[0].Length + 5) : conditions.value;
            if (conditionsText.StartsWith("(") && conditionsText.EndsWith(")")) conditionsText = conditionsText.Substring(1, conditionsText.Length - 2); //removes parenthesis
            if (conditionsText == "true") conditionsText = string.Empty;
             
             
            //var label = item.IsStatic ? "Conditions" : "Additional Conditions";
             
            var newConditions = luaConditionWizard.Draw(new GUIContent( "Conditions", "Optional Lua statement that must be true to use this entry."), conditionsText); 
            if (newConditions.Length == 0) newConditions = "true"; 
            conditions.value = hasPrepended ? $"{prependedCondition} and ({newConditions})" : newConditions;
                
            EditorWindowTools.EditorGUILayoutEndGroup();
        }

        private void DrawDoubleConditions(Asset asset, string fieldName, string firstConditionDefaultValue)
        {
             var conditions = Field.Lookup(asset.fields, fieldName);
             
             if (conditions == null)
             {
                 conditions = new Field(fieldName,  $"{firstConditionDefaultValue} and (true)", FieldType.Text);
                 asset.fields.Add(conditions);
             }

             if (!conditions.value.Contains(" and "))
             {
                    conditions.value = $"{firstConditionDefaultValue} and (true)";
                    return;
             }
             
             EditorWindowTools.EditorGUILayoutBeginGroup();

             luaConditionWizard.database = database;

             string newDefaultCondition;
             
             var additionalConditionsText = conditions.value.Substring(conditions.value.Split(" and ")[0].Length + 5);
             additionalConditionsText = additionalConditionsText.Substring(1, additionalConditionsText.Length - 2); //removes parenthesis
             if (additionalConditionsText == "true") additionalConditionsText = string.Empty;

             
             var hasDefault = conditions.value.StartsWith($"{firstConditionDefaultValue} and (");

           
             if (hasDefault)
             {
                 GUI.enabled = false; 
                 newDefaultCondition = luaConditionWizard.DrawWithToggle(new GUIContent("Default Condition", "Default lua statement for actions."), firstConditionDefaultValue, ref hasDefault, "Include Default ");
                 if (!hasDefault || string.IsNullOrEmpty(newDefaultCondition)) newDefaultCondition = "true";
                 
                 GUI.enabled = true;
                 
                 var newAdditionalConditions = luaConditionWizard.Draw(new GUIContent( "Additional Conditions", "Optional Lua statement that must be true to use this entry."), additionalConditionsText); 
                 if (newAdditionalConditions.Length == 0) newAdditionalConditions = "true"; 
                 conditions.value = $"{newDefaultCondition} and ({newAdditionalConditions})";
             }

             else
             {
                 newDefaultCondition = "true";
                 var newAdditionalConditions = luaConditionWizard.DrawWithToggle(new GUIContent( "Conditions", "Optional Lua statement that must be true to use this entry."), additionalConditionsText, ref hasDefault, "Include Default ");
                 if (newAdditionalConditions.Length == 0) newAdditionalConditions = "true"; 
                 if (hasDefault)
                 {
                     conditions.value = $"{firstConditionDefaultValue} and ({newAdditionalConditions})";
                 }
                 else
                 {
                     conditions.value = $"{newDefaultCondition} and ({newAdditionalConditions})";
                 }
             }
                
             EditorWindowTools.EditorGUILayoutEndGroup();
        }
        
        
        private void DrawScript(Asset asset, string fieldName, string prependedScript = "", GUIContent label = null)
        {
            
            var script = Field.Lookup(asset.fields, fieldName);
                
            var hasPrepended = !string.IsNullOrEmpty(prependedScript);
            
            if (hasPrepended && prependedScript.EndsWith(";")) prependedScript = prependedScript.Substring(0, prependedScript.Length - 1);
            
            if (script == null)
            {
                script = new Field(fieldName,  hasPrepended ? $"{prependedScript};" : "", FieldType.Text);
                asset.fields.Add(script);
            }
            
            
            EditorWindowTools.EditorGUILayoutBeginGroup();

            luaConditionWizard.database = database;
             
            var scriptText = hasPrepended ? script.value.Substring(script.value.Split(script.value.Contains("; ") ? "; " : ";") [0].Length + 2) : script.value;
             
            //var label = item.IsStatic ? "Conditions" : "Additional Conditions";

            label ??= new GUIContent("Script", "Script that runs when this action is completed.");
             
            var newScript = luaConditionWizard.Draw(label, scriptText); 
            script.value = hasPrepended ? $"{prependedScript}; {newScript}" : newScript;
                
            EditorWindowTools.EditorGUILayoutEndGroup();
        }

       
        private void DrawDoubleScripts(Item item, string fieldName, string firstScriptDefaultValue)
        {
            if (firstScriptDefaultValue.EndsWith(";")) firstScriptDefaultValue = firstScriptDefaultValue.Substring(0, firstScriptDefaultValue.Length - 1);
            
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
            string newDefaultScript = string.Empty;
            
            
            var additionalScript = script.value.Substring(script.value.Split(script.value.Contains("; ") ? "; " : ";") [0].Length + 2);
            var newAdditionalScript = string.Empty;
            
             
            var hasDefault = script.value.StartsWith($"{firstScriptDefaultValue}; ");
            if (hasDefault)
            {
                GUI.enabled = false; 
                newDefaultScript = luaScriptWizard.DrawWithToggle(new GUIContent("Default Script", "Default lua script for actions."), firstScriptDefaultValue, ref hasDefault, "Include Default ");
                if (!hasDefault || string.IsNullOrEmpty(newDefaultScript)) newDefaultScript = string.Empty;
                 
                GUI.enabled = true;
                 
                newAdditionalScript = luaConditionWizard.Draw(new GUIContent( "Additional Script", "Optional Lua script that runs after this action."), additionalScript); 
            }

            else
            {
                newAdditionalScript = luaConditionWizard.DrawWithToggle(new GUIContent( "Script", "Optional Lua statement that must be true to use this entry."), additionalScript, ref hasDefault, "Include Default ");
                if (hasDefault) newDefaultScript = firstScriptDefaultValue;
               
            }
            
            script.value =  $"{newDefaultScript}; {newAdditionalScript}";
            
            EditorWindowTools.EditorGUILayoutEndGroup();
        }

        private void DrawPoints(Asset asset, string prefix = "", bool includePointsReduction = true, Color backgroundColor = default)
        {
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith(" ")) prefix += " ";
             var pointsLabels = points.Select(p => p.Name).ToArray();
             var pointsColors = points.Select(p => p.LookupColor("Color")).ToArray();
           

                var alignedTextStyle = new GUIStyle( EditorStyles.label );
                alignedTextStyle.alignment = TextAnchor.MiddleCenter;

                var defaultContentColor = GUI.contentColor;
                var defaultLabelWidth = EditorGUIUtility.labelWidth;

                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < points.Count; i++)
                {
                    Field pointValue = Field.Lookup(asset.fields, $"{prefix}{points[i].Name} Points");
               
                    if (pointValue == null)
                    {
                        pointValue = new Field($"{prefix}{points[i].Name} Points", "0", FieldType.Number);
                        asset.fields.Add(pointValue);
                        SetDatabaseDirty("Create Points Field");
                    }
                
                
               
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(points[i].Name, alignedTextStyle,  GUILayout.MaxWidth(100));

                    var pointsAsInt = Field.LookupInt(asset.fields, $"{prefix}{points[i].Name} Points");

                    var colorMultiplier = pointsAsInt < 0 ? Color.red : pointsAsInt == 0 ? Color.grey : default;
                    
                    DrawPointsCategoryIconPreview(points[i], 100f, colorMultiplier, backgroundColor);
                    
                    pointValue.value =
                        $"{EditorGUILayout.IntField(pointsAsInt, GUILayout.MaxWidth(100))}";
                    EditorGUILayout.EndVertical();
                }
            
                GUI.contentColor = defaultContentColor;
                EditorGUILayout.EndHorizontal();
                
                if (includePointsReduction)
                {
                    Field pointsReductionOnRepeat = Field.Lookup(asset.fields, "Repeat Points Reduction");
                    
                    if (pointsReductionOnRepeat == null)
                    {
                        pointsReductionOnRepeat = new Field("Repeat Points Reduction", "0", FieldType.Number);
                        asset.fields.Add(pointsReductionOnRepeat);
                        SetDatabaseDirty("Create Repeatable Field");
                    }
                    
                    DrawEditorItemWithRepeatableIcon( ()=>
                            pointsReductionOnRepeat.value = EditorGUILayout.FloatField(new GUIContent($"{Field.LookupFloat( asset.fields, "Repeat Points Reduction") * 100}% Points Reduction", "The amount of points reduced from the action when repeated."), Field.LookupFloat(asset.fields, "Repeat Points Reduction")).ToString(), 
                        150f);
                    
                 
                    if (Field.LookupFloat(asset.fields, "Repeat Points Reduction") < 0)
                    {
                        Field.SetValue(asset.fields, "Repeat Points Reduction", "0");
                    }
                    
                    if (Field.LookupFloat(asset.fields, "Repeat Points Reduction") > 1)
                    {
                        Field.SetValue(asset.fields, "Repeat Points Reduction", "1");
                    }
                }
                
                else 
                {
                    Field pointsReductionOnRepeat = Field.Lookup(asset.fields, "Repeat Points Reduction");
                    
                    if (pointsReductionOnRepeat != null)
                    {
                        asset.fields.Remove(pointsReductionOnRepeat);
                        SetDatabaseDirty("Remove Repeatable Field");
                    }
                }
                
                EditorGUILayout.EndVertical();
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
            
            var chosenLocation = DrawLocationField(new GUIContent("Location", "The location of the action. If the player is not present, the action will not be presented, regardless of any followup conditions."), location.value, "(Not Required)", false);
            if (chosenLocation != location.value)
            {
                var id = int.TryParse( location.value, out var result) ? result : 0;
                var loc = database.GetLocation(id);
                if (loc == null || !loc.IsSublocation || loc.LookupValue("Parent Location") != chosenLocation)
                {
                    location.value = chosenLocation;
                }
                
                SetDatabaseDirty( "Change Location Field");
            }
            
            EditorWindowTools.EditorGUILayoutBeginIndent();
            
            

            var chosenSublocation = string.Empty;

            var chosenLocationHasSublocation = database.locations.Any(p =>
                p.IsSublocation && p.LookupValue("Parent Location") == chosenLocation);
            
            
                  
            Field ignoreSublocations = Field.Lookup(item.fields, "Ignore Sublocations");

            if (ignoreSublocations == null)
            {
                ignoreSublocations = new Field("Ignore Sublocations", "False", FieldType.Boolean);
                item.fields.Add(ignoreSublocations);
                SetDatabaseDirty("Create Ignore Sublocations Field");
            }

            var rect = EditorGUILayout.BeginHorizontal();
            
            
            if (chosenLocationHasSublocation) locationPropertiesFoldout = EditorGUILayout.Foldout(locationPropertiesFoldout, "Sublocation");
            
            EditorGUIUtility.labelWidth = 1f;
            
            if (ignoreSublocations.value == "True")
            {
                EditorGUILayout.LabelField(" ", "(All Sublocations)");
                chosenSublocation = "-1";
            }

            else
            {

                
                if (chosenLocationHasSublocation) chosenSublocation = DrawSublocationField(
                    new GUIContent(" ", "The Sublocation of the location above."),
                    database.GetLocation(int.Parse(chosenLocation)),
                    location.value == chosenLocation ? "-1" : location.value, "(Root)");
                
               
            }
            
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            
            location.value = string.IsNullOrEmpty(chosenSublocation) || chosenSublocation == "-1"
                ? chosenLocation
                : chosenSublocation;

            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            if (locationPropertiesFoldout)
            {

                

                if (chosenLocationHasSublocation)
                {
                    
                    
                    
                    
                    Field newSublocationField = Field.Lookup(item.fields, "New Sublocation");
                    
                    var sublocationHandler = item.IsFieldAssigned("New Sublocation") ? "Sublocation Switcher"
                        : item.LookupBool("Ignore Sublocations") ? "Ignore Sublocations" : "Default";
                    
                    
                    var handlers = new List<string> {"Default", "Ignore Sublocations", "Sublocation Switcher"};
                    
                    var newSublocationHandler = handlers[EditorGUILayout.Popup( "Sublocation Handler", handlers.IndexOf(sublocationHandler), handlers.ToArray())];


                    if (newSublocationHandler != sublocationHandler)
                    {
                        if (newSublocationHandler != "Sublocation Switcher")
                        {
                            if (item.IsFieldAssigned("New Sublocation"))
                            {
                                item.fields.Remove(Field.Lookup(item.fields, "New Sublocation"));
                                SetDatabaseDirty("Remove New Sublocation Field");
                            }
                        }

                        switch (newSublocationHandler)
                        {
                            case "Default":
                                item.AssignedField("Ignore Sublocations").value = "False";
                                break;
                            case "Ignore Sublocations":
                                item.AssignedField("Ignore Sublocations").value = "True";
                                break;
                            case "Sublocation Switcher":
                                if (!Field.FieldExists(item.fields, "New Sublocation"))
                                {
                                    newSublocationField = new Field("New Sublocation", "-1", FieldType.Location);
                                    item.fields.Add(newSublocationField);
                                    SetDatabaseDirty("Create New Sublocation Field");
                                }

                                item.AssignedField("Ignore Sublocations").value = "True";

                                break;
                        }
                        
                      
                    }
                    
                    if (Field.FieldExists(item.fields, "New Sublocation")) {
                            
                        DrawEditorItemWithShuffleIcon(() =>
                            newSublocationField.value = DrawSublocationField(
                                new GUIContent("New Sublocation", "The Sublocation of the location above."),
                                database.GetLocation(int.Parse(chosenLocation)), newSublocationField.value, "(Return to Root Location)"));

                        if (newSublocationField.value == "0") newSublocationField.value = chosenLocation;
                                
                        Field conversation = Field.Lookup(item.fields, "Conversation");

                        if (conversation != null)
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
                }

               
            }
            
            EditorWindowTools.EditorGUILayoutEndIndent();
            
            EditorWindowTools.EditorGUILayoutEndGroup();
            EditorGUILayout.Space();
            


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
                if (conversation == null)
                {
                    conversation = new Field("Conversation", string.Empty, FieldType.Text);
                    asset.fields.Add(conversation);
                    SetDatabaseDirty("Create Conversation Field");
                }
                
                
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
                
                
                

                Actor actor;
                    if (string.IsNullOrEmpty(startConversationTitle) || asset.FieldExists("Entry Actor")) actor = database.GetActor(asset.LookupInt("Entry Actor"));

                    else
                    {
                        var conv = database.GetConversation(startConversationTitle);
                        if (conv != null) actor = database.GetActor(conv.ActorID);
                        else actor = database.GetActor("Game");
                        
                        
                    }
                    
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
                    
                    
                    DrawReorderableEntries(asset, "Entry", ref questEntrySelectedIdx, ref showCompactQuestEntryList, DrawGeneratedDialogueEntry);

                  
                    

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
                        
                              
                        if (conversationPlaysMoreThanOnce.value == "True")
                        {
                            
                            DrawRepeatEntriesToggle(asset);
                            
                        }
                    }

                    else
                    {
                        DrawRepeatEntriesToggle(asset);
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
                        newEntryActor = DrawActorField(
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
        
        #endregion
        
        #region Toggles

        private void ToggleHasQuestEntries(Item item, bool hasEntries)
        {
            SetDatabaseDirty("Toggle Has Quest Entries");
            if (hasEntries)
            {
                if (!item.IsAction && item.fields.Any(p => p.title.EndsWith(" Points") && !p.title.StartsWith("Entry") && int.Parse(p.value) > 0))
                {
                    if (!EditorUtility.DisplayDialog("Delete all points?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                
                
                if (!item.FieldExists("Entry Count")) Field.SetValue(item.fields, "Entry Count", (int)0);
                item.fields.RemoveAll(field => field.title.EndsWith(" Points") && !field.title.StartsWith("Entry"));
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
        
        private void ToggleHasConditionalDisplayEntries(Item item, bool hasEntries)
        {
            SetDatabaseDirty("Toggle Has Conditional Display Entries");
            if (hasEntries)
            {
                if (!item.FieldExists("Conditional Display Entry Count")) Field.SetValue(item.fields, "Conditional Display Entry Count", (int)0);

               
            }
            else
            {
                int entryCount = Field.LookupInt(item.fields, "Conditional Display Entry Count");
                if (entryCount > 0)
                {
                    if (!EditorUtility.DisplayDialog("Delete all entries?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                item.fields.RemoveAll(field => field.title.StartsWith("Conditional Display Entry "));
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
               // item.IsRepeatable = true;
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

        private void ToggleStatic(Item item, bool isStatic)
        {
            SetDatabaseDirty( "Toggle Static");
            if (isStatic)
            {
                item.IsStatic = true;
            }
            else
            {
                var conditionalDisplayEntryCount = Field.LookupInt(item.fields, "Conditional Display Entry Count");
                if (conditionalDisplayEntryCount > 0)
                {
                    if (!EditorUtility.DisplayDialog("Delete all conditional display entries?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                item.IsStatic = false;
                item.fields.RemoveAll(field => field.title.StartsWith("Conditional Display Entry"));
            }
        }
        
        private void TogglePoints(Item item, bool hasPoints)
        {
            SetDatabaseDirty("Toggle Points");
            if (hasPoints)
            {
                if (!item.FieldExists("Skills Points")) Field.SetValue(item.fields, "Skills Points", (int)0);
                if (!item.FieldExists("Context Points")) Field.SetValue(item.fields, "Context Points", (int)0);
                if (!item.FieldExists("Teamwork Points")) Field.SetValue(item.fields, "Teamwork Points", (int)0);
                if (!item.FieldExists("Wellness Points")) Field.SetValue(item.fields, "Wellness Points", (int)0);
            }
            else
            {
                if (item.fields.Any(p => p.title.EndsWith(" Points") && p.value != "0"))
                {
                    if (!EditorUtility.DisplayDialog("Delete all points?", "You cannot undo this action.", "Delete", "Cancel"))
                    {
                        return;
                    }
                }
                item.fields.RemoveAll(field => field.title.EndsWith(" Points"));
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


        #endregion

        
        private void DrawRepeatEntriesToggle(Asset asset)
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
            
            if (newHasRepeatEntries)  DrawReorderableEntries(asset, "Repeat Entry", ref repeatEntrySelectedIdx, ref showCompactRepeatEntryList, DrawGeneratedDialogueEntry);
                    
            EditorWindowTools.EditorGUILayoutEndGroup();
            GUI.backgroundColor = defaultColor;
        }


        private void DrawReorderableEntries(Asset asset, string prefix, ref int selectedIndex, ref bool showCompactList,
            Action<Asset, string, int> drawEntryMethod, Action drawMainLabelMethod = null, GUIContent buttonContent = null)
        {
            EditorGUILayout.BeginHorizontal();

            var label = prefix;
            if (label.EndsWith("Entry")) label = label.Substring(0, label.Length - 5); label += " Entries";
            
            if (drawMainLabelMethod != null) drawMainLabelMethod.Invoke();
            else EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Compact View", GUILayout.Width(90));
            
            
            
            showCompactList = EditorGUILayout.Toggle(GUIContent.none, showCompactList, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            
            var entryTitle = prefix;

            int entryCount = Field.LookupInt(asset.fields, $"{entryTitle} Count");

            string[] entryTabs = null;
            if (showCompactList)
            {
                entryTabs = new string[entryCount];
                for (int i = 1; i <= entryCount; i++)
                {
                    entryTabs[i - 1] = $"{entryTitle}" + i;
                }
                selectedIndex = GUILayout.Toolbar(selectedIndex, entryTabs);
            }

            EditorWindowTools.StartIndentedSection();
            int entryToDelete = -1;
            int entryToMoveUp = -1;
            int entryToMoveDown = -1;
            for (int i = 1; i <= entryCount; i++)
            {
                if (showCompactList && (i != selectedIndex + 1)) continue;
                DrawReorderableEntryBase(asset, prefix, i, entryCount, ref entryToDelete, ref entryToMoveUp, ref entryToMoveDown);
                drawEntryMethod.Invoke( asset, prefix, i);
            }
            if (entryToDelete != -1)
            {
                DeleteReorderableEntry(asset, entryToDelete, entryCount, prefix, ref selectedIndex);
                SetDatabaseDirty("Delete Entry");
                GUIUtility.ExitGUI();
            }
            if (entryToMoveUp != -1)
            {
                MoveReorderableEntryUp(asset, entryToMoveUp, entryCount, prefix, ref selectedIndex);
            }
            if (entryToMoveDown != -1)
            {
                MoveReorderableEntryDown(asset, entryToMoveDown, entryCount, prefix, ref selectedIndex);
            }
            
            var guiButtonContent = buttonContent ?? new GUIContent($"Add New {prefix}", $"Adds a new {prefix.ToLower()} to this asset.");
            
            if (GUILayout.Button(guiButtonContent))
            {
                entryCount++;
                selectedIndex = entryCount - 1;
                
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

        private void DrawReorderableEntryBase(Asset asset, string prefix, int entryNumber, int entryCount, ref int entryToDelete, ref int entryToMoveUp, ref int entryToMoveDown)
        {
            EditorGUILayout.BeginVertical("button");
            // Heading:
            EditorGUILayout.BeginHorizontal();
            string entryTitle = $"{prefix} {entryNumber}";
            EditorGUILayout.LabelField(entryTitle);
            GUILayout.FlexibleSpace();
            
            
            if (asset is Location)
            {
                
                var music = Field.LookupValue(asset.fields, "Music");

                if (!string.IsNullOrEmpty(music))
                {
                    var musicEntryTitle = $"Music {prefix}";
                    Field musicEntry = Field.Lookup(asset.fields, musicEntryTitle);
                    
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
                                musicEntry = new Field(musicEntryTitle, entryNumber.ToString(), FieldType.Text);
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
        }
        
        

        private void DrawQuestEntry(Asset item, string prefix, int entryNumber)
        {
            
            // Keep track of which fields we've already drawn:
            List<Field> alreadyDrawn = new List<Field>();
            string entryTitle = $"{prefix} {entryNumber}";
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
            DrawRevisableTextAreaField(new GUIContent(entryTitle), item, null, item.fields, entryTitle);
            DrawLocalizedVersions(item, null, item.fields, entryTitle + " {0}", false, FieldType.Text, alreadyDrawn);

            // Other "Entry # " fields:
            string entryTitleWithSpace = entryTitle + " ";
            string entryIDTitle = entryTitle + " ID";
            for (int i = 0; i < item.fields.Count; i++)
            {
                var field = item.fields[i];
                if (field.title == null) field.title = string.Empty;

                if (field.title.EndsWith(" Points")) continue;
                
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


            var rect = EditorGUILayout.BeginHorizontal();
            
            //empty space
            
            EditorGUILayout.Space( rect.x / 4);
            
            DrawPoints(item, entryTitle, false, backgroundColor: new Color(0.3f, 0.3f, 0.3f));
            
            EditorGUILayout.Space( rect.x / 4);
            EditorGUILayout.EndHorizontal();

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


        private void DrawGeneratedDialogueEntry(Asset asset, string prefix, int entryNumber)
        {
            
            List<Field> alreadyDrawn = new List<Field>();
            
            EditorWindowTools.EditorGUILayoutBeginGroup();

            EditorGUI.BeginChangeCheck();

            var entryTitle = prefix + " " + entryNumber;
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
        
        private void DrawConditionalDisplayEntry(Asset asset, string prefix, int entryNumber)
        {
            
            List<Field> alreadyDrawn = new List<Field>();
            
            EditorWindowTools.EditorGUILayoutBeginGroup();

            EditorGUI.BeginChangeCheck();

            var entryTitle = $"{prefix} {entryNumber}";
            
            var displayNameFieldTitle = $"{entryTitle}";
            
            var displayNameLabel =  "Display Name";
            
            DrawRevisableTextField(new GUIContent(displayNameLabel), asset, null, asset.fields, entryTitle);
            DrawLocalizedVersions(asset, null, asset.fields, entryTitle + " {0}", false, FieldType.Text, alreadyDrawn);

            var descriptionFieldTitle = $"{entryTitle} Description";
            var descriptionField = Field.Lookup(asset.fields, descriptionFieldTitle);
            if (descriptionField == null)
            {
                descriptionField = new Field(descriptionFieldTitle, "", FieldType.Text);
                asset.fields.Add(descriptionField);
            }

            var descriptionText = descriptionField.value;
            
           
            var descriptionTextLabel = "Description";
            if (asset is Item item && item.IsAction) descriptionTextLabel += " (Tooltip)";
            DrawRevisableTextAreaField(
                new GUIContent(descriptionTextLabel, "The description text shown as a tooltip when this condition is true."), asset, null,
                asset.fields, descriptionFieldTitle);
            DrawLocalizedVersions(asset, null, asset.fields, entryTitle + " {0}", false, FieldType.Text, alreadyDrawn);
            alreadyDrawn.Add(descriptionField);
            
            

            EditorWindowTools.EditorGUILayoutEndGroup();
            
            
            var conditionsFieldTitle = $"{entryTitle} Conditions";
            
            DrawConditions(asset, conditionsFieldTitle);
            
            if (asset.LookupValue(conditionsFieldTitle) == string.Empty || asset.LookupValue(conditionsFieldTitle) == "true")
            {
                EditorGUILayout.HelpBox("You must add conditions to this entry for it to display.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DeleteReorderableEntry(Asset asset, int entryNumber, int entryCount, string prefix, ref int index)
        {
            
            if (EditorUtility.DisplayDialog(string.Format("Delete entry {0}?", entryNumber), "You cannot undo this action.", "Delete", "Cancel"))
            {
                CutReorderableEntry(asset, entryNumber, entryCount, prefix);
                SetDatabaseDirty("Delete Quest Entry");
                
                if (entryNumber == index + 1)
                {
                    index = Mathf.Max(index - 1, 0);
                }
            }
        }

        private void MoveReorderableEntryUp(Asset asset, int entryNumber, int entryCount, string prefix, ref int index)
        {
            if (entryNumber <= 1) return;
            var clipboard = CutReorderableEntry(asset, entryNumber, entryCount, prefix);
            entryCount--;
            PasteReorderableEntry(asset, entryNumber - 1, entryCount, clipboard, prefix);
            index--;
        }

        private void MoveReorderableEntryDown(Asset asset, int entryNumber, int entryCount, string prefix, ref int index)
        {
            if (entryNumber >= entryCount) return;
            var clipboard = CutReorderableEntry(asset, entryNumber, entryCount, prefix);
            entryCount--;
            PasteReorderableEntry(asset, entryNumber + 1, entryCount, clipboard, prefix);
            index++;
        }

        private List<Field> CutReorderableEntry(Asset asset, int entryNumber, int entryCount, string prefix)
        {
            var entryTitle = prefix;
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

        private void PasteReorderableEntry(Asset asset, int entryNumber, int entryCount, List<Field> clipboard, string prefix)
        {
           
            var entryTitle =  prefix;
            
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

        private void SortItemFields(List<Field> fields, string prefix)
        {
            List<Field> entryFields = fields.Where(field => field.title.StartsWith(prefix)).OrderBy(field => field.title).ToList();
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
                
               // else if (int.TryParse(Lua.Run($"return {extractedContents[0]}))]").AsString, out var secondsFromLua)) blackoutTime = secondsFromLua;
                
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