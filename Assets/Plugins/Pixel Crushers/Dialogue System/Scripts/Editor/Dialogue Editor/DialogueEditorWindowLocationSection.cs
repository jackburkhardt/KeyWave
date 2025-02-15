// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Locations tab. Locations are
    /// just treated as basic assets, so it uses the generic asset methods.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [SerializeField]
        private AssetFoldouts locationFoldouts = new AssetFoldouts();

        [SerializeField]
        private string locationFilter = string.Empty;

        [SerializeField]
        private bool hideFilteredOutLocations = false;

        private ReorderableList locationReorderableList = null;

        [SerializeField]
        private int locationListSelectedIndex = -1;

        private HashSet<int> syncedLocationIDs = null;

        private void ResetLocationSection()
        {
            UpdateTreatLocationsAsSublocations(template.treatLocationsAsSublocations);
            
            
            locationFoldouts = new AssetFoldouts();
            locationAssetList = null;
            locationReorderableList = null;
            locationListSelectedIndex = -1;
            syncedLocationIDs = null;
        }
        
        private void UpdateTreatLocationsAsSublocations(bool newValue)
        {
            template.treatLocationsAsSublocations = newValue;
        }

        private void DrawLocationSection()
        {
            
            if (locationReorderableList == null) InitializeLocationReorderableList();
            DrawFilterMenuBar("Location", DrawLocationMenu, ref locationFilter, ref hideFilteredOutLocations);
            if (database.syncInfo.syncLocations)
            {
                DrawLocationSyncDatabase();
                if (syncedLocationIDs == null) RecordSyncedLocationIDs();
            }
            locationReorderableList.DoLayoutList();
        }

        private void InitializeLocationReorderableList()
        {
            locationReorderableList = new ReorderableList(database.locations, typeof(Location), true, true, true, true);
            locationReorderableList.drawHeaderCallback = DrawLocationListHeader;
            locationReorderableList.drawElementCallback = DrawLocationListElement;
            locationReorderableList.drawElementBackgroundCallback = DrawLocationListElementBackground;

            if (template.treatLocationsAsSublocations)
            {
                locationReorderableList.onAddDropdownCallback = OnAddLocationOrSublocationDropdown;
            }

            else
            {
                locationReorderableList.onAddCallback = OnLocationListAdd;
            }
            
           
            locationReorderableList.onRemoveCallback = OnLocationListRemove;
            locationReorderableList.onSelectCallback = OnLocationListSelect;
            locationReorderableList.onReorderCallback = OnLocationListReorder;
        }
        
        private const float LocationReorderableListTypeWidth = 65f;
        private float SublocationReorderableListTypeWidth => LocationReorderableListTypeWidth + 25f;

        private void DrawLocationListHeader(Rect rect)
        {
            if (template.treatLocationsAsSublocations)
            {
                var fieldWidth = (rect.width - 14 - LocationReorderableListTypeWidth) / 4;
                EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, LocationReorderableListTypeWidth, rect.height), "Type");
                EditorGUI.LabelField(new Rect(rect.x + 14 + LocationReorderableListTypeWidth, rect.y, fieldWidth, rect.height), "Name");
                EditorGUI.LabelField(new Rect(rect.x + 14 + LocationReorderableListTypeWidth + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
            }
            else
            {
                var fieldWidth = (rect.width - 14) / 4;
                EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, fieldWidth, rect.height), "Name");
                EditorGUI.LabelField(new Rect(rect.x + 14 + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Description");
            }
        }

        private void DrawLocationListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < database.locations.Count)) return;
            var nameControl = "LocationName" + index;
            var descriptionControl = "LocationDescription" + index;
            var location = database.locations[index];
            var locationName = location.Name;
            var description = location.Description;
            EditorGUI.BeginDisabledGroup(!EditorTools.IsAssetInFilter(location, locationFilter) || IsLocationSyncedFromOtherDB(location));
            
            if (template.treatLocationsAsSublocations)
            {
                var fieldWidth = (rect.width - LocationReorderableListTypeWidth) / 4;
                
                if (location.IsSublocation)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, SublocationReorderableListTypeWidth, EditorGUIUtility.singleLineHeight), "Sublocation");
                }
                else
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, LocationReorderableListTypeWidth, EditorGUIUtility.singleLineHeight), "Location");
                }
                
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(nameControl);
                
                if (location.IsSublocation)
                {
                    locationName = EditorGUI.TextField(new Rect(rect.x + SublocationReorderableListTypeWidth - 2, rect.y + 2,  rect.width - (3 * fieldWidth - 2) - SublocationReorderableListTypeWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, location.Name);
                 }
                else
                {
                    locationName = EditorGUI.TextField(new Rect(rect.x + LocationReorderableListTypeWidth, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, location.Name);
                }
                
                if (EditorGUI.EndChangeCheck()) location.Name = locationName;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(descriptionControl);
                description = EditorGUI.TextField(new Rect(rect.x + LocationReorderableListTypeWidth + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
                if (EditorGUI.EndChangeCheck()) location.Description = description;
            }
            else
            {
                var fieldWidth = rect.width / 4;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(nameControl);
                locationName = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, locationName); 
                if (EditorGUI.EndChangeCheck()) location.Name = locationName;
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(descriptionControl);
                description = EditorGUI.TextField(new Rect(rect.x + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
                if (EditorGUI.EndChangeCheck()) location.Description = description;
            }
            
            EditorGUI.EndDisabledGroup();
            var focusedControl = GUI.GetNameOfFocusedControl();
            if (string.Equals(nameControl, focusedControl) || string.Equals(descriptionControl, focusedControl))
            {
                inspectorSelection = location;
            }
        }

        private void DrawLocationListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < database.locations.Count)) return;
            var location = database.locations[index];
            if (EditorTools.IsAssetInFilter(location, locationFilter))
            {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, true);
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.225f, 0.225f, 0.225f, 1));
            }
        }
        
        private void OnAddLocationOrSublocationDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Location"), false, AddNewLocation);
            menu.AddItem(new GUIContent("Add Sublocation"), false, AddNewSublocation);
            menu.ShowAsContext();
        }

        private void OnLocationListAdd(ReorderableList list)
        {
            AddNewLocation();
        }

        private void OnLocationListRemove(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.locations.Count)) return;
            var location = database.locations[list.index];
            if (location == null) return;
            if (IsLocationSyncedFromOtherDB(location)) return;
            var deletedLastOne = list.count == 1;
            if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", EditorTools.GetAssetName(location)), "Are you sure you want to delete this location?", "Delete", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                if (deletedLastOne) inspectorSelection = null;
                else inspectorSelection = (list.index < list.count) ? database.locations[list.index] : (list.count > 0) ? database.locations[list.count - 1] : null;
                SetDatabaseDirty("Remove Location");
            }
        }

        private void OnLocationListReorder(ReorderableList list)
        {
            SetDatabaseDirty("Reorder Locations");
        }

        private void OnLocationListSelect(ReorderableList list)
        {
            if (!(0 <= list.index && list.index < database.locations.Count)) return;
            inspectorSelection = database.locations[list.index];
            locationListSelectedIndex = list.index;
        }

        public void DrawSelectedLocationSecondPart()
        {
            var location = inspectorSelection as Location;
            if (location == null) return;
            DrawOtherLocationPrimaryFields(location);
            DrawFieldsFoldout<Location>(location, locationListSelectedIndex, locationFoldouts);
            DrawAssetSpecificPropertiesSecondPart(location, locationListSelectedIndex, locationFoldouts);
        }

        private void DrawOtherLocationPrimaryFields(Location location)
        {
            if (location == null || location.fields == null || template.locationPrimaryFieldTitles == null) return;
            foreach (var field in location.fields)
            {
                var fieldTitle = field.title;
                if (string.IsNullOrEmpty(fieldTitle)) continue;
                if (!template.locationPrimaryFieldTitles.Contains(field.title)) continue;
                DrawMainSectionField(field);
            }
        }

        private void DrawLocationMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Use Sublocations"), template.treatLocationsAsSublocations, ToggleUseSublocations);
                menu.AddItem(new GUIContent("New Location"), false, AddNewLocation);
                menu.AddItem(new GUIContent("Sort/By Name"), false, SortLocationsByName);
                menu.AddItem(new GUIContent("Sort/By ID"), false, SortLocationsByID);
                menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncLocations, ToggleSyncLocationsFromDB);
                menu.ShowAsContext();
            }
        }

        private void AddNewLocation()
        {
            var location =   AddNewAsset( database.locations);
            SetDatabaseDirty("Add New Location");
        }

        private void AddNewSublocation()
        {
            var location =  AddNewAsset( database.locations);
            location.IsSublocation = true;
            SetDatabaseDirty("Add New Sublocation");
        }

        private void SortLocationsByName()
        {
            database.locations.Sort((x, y) => x.Name.CompareTo(y.Name));
            SetDatabaseDirty("Sort Locations by Name");
        }

        private void SortLocationsByID()
        {
            database.locations.Sort((x, y) => x.id.CompareTo(y.id));
            SetDatabaseDirty("Sort Locations by ID");
        }
        
        private void ToggleUseSublocations()
        {
            UpdateTreatLocationsAsSublocations(!template.treatLocationsAsSublocations);
        }


        private void ToggleSyncLocationsFromDB()
        {
            database.syncInfo.syncLocations = !database.syncInfo.syncLocations;
            if (!database.syncInfo.syncLocations && database.syncInfo.syncLocationsDatabase != null)
            {
                if (EditorUtility.DisplayDialog("Disconnect Synced DB",
                    "Also delete synced locations from this database?", "Yes", "No"))
                {
                    database.locations.RemoveAll(x => syncedLocationIDs.Contains(x.id));
                }
            }
            InitializeLocationReorderableList();
            SetDatabaseDirty("Toggle Sync Locations");
        }

        private void DrawLocationSyncDatabase()
        {
            EditorGUILayout.BeginHorizontal();
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync locations from."),
                                                                       database.syncInfo.syncLocationsDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
            if (newDatabase != database.syncInfo.syncLocationsDatabase)
            {
                database.syncInfo.syncLocationsDatabase = newDatabase;
                database.SyncLocations();
                InitializeLocationReorderableList();
                syncedLocationIDs = null;
                SetDatabaseDirty("Change Location Sync Database");
            }
            if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72)))
            {
                InitializeLocationReorderableList();
                syncedLocationIDs = null;
                database.SyncLocations();
                SetDatabaseDirty("Manual Sync Locations");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RecordSyncedLocationIDs()
        {
            syncedLocationIDs = new HashSet<int>();
            if (database.syncInfo.syncLocations && database.syncInfo.syncLocationsDatabase != null)
            {
                database.syncInfo.syncLocationsDatabase.locations.ForEach(x => syncedLocationIDs.Add(x.id));
            }
        }

        public bool IsLocationSyncedFromOtherDB(Location location)
        {
            return location != null && syncedLocationIDs != null && syncedLocationIDs.Contains(location.id);
        }
        
        private void DrawLocationPropertiesFirstPart(Location location)
        {
            if (location.IsSublocation)
            {
                DrawSublocationProperties(location);
            }
            else
            {
                DrawLocationProperties(location);
            }



            if (!location.IsFieldAssigned("Visit Count"))
            {
                var visitCountField = new Field("Visit Count", "0", FieldType.Number);
                location.fields.Add(visitCountField);
                SetDatabaseDirty("Add Visit Count Field");
            }
            
            
        }
        
        
        
        private void DrawLocationProperties(Location location)
        {
            if (location == null || location.fields == null) return;
            
            var displayNameField = Field.Lookup(location.fields, "Display Name");
            var hasDisplayNameField = (displayNameField != null);
            var useDisplayNameField = EditorGUILayout.Toggle(new GUIContent("Use Display Name", "Tick to use a Display Name in UIs that's different from the Name."), hasDisplayNameField);
            if (hasDisplayNameField && !useDisplayNameField)
            {
                location.fields.Remove(displayNameField);
                SetDatabaseDirty("Don't Use Display Name");
            }
            else if (useDisplayNameField)
            {
                DrawRevisableTextAreaField(displayNameLabel, location, null, location.fields, "Display Name");
                DrawLocalizedVersions(location, location.fields, "Display Name {0}", false, FieldType.Text);
            }
            
            
            var descriptionField = Field.Lookup(location.fields, "Description");
            
            if (descriptionField == null)
            {
                descriptionField = new Field("Description", string.Empty, FieldType.Text);
                location.fields.Add(descriptionField);
                SetDatabaseDirty("Add Description Field");
            }
            
            
            DrawRevisableTextAreaField(descriptionLabel, location, null, location.fields, "Description");
            
            
            DrawLocationConversationProperties(location);
            
            
            // Coordinates
            
            var coordinatesField = Field.Lookup(location.fields, "Coordinates");
            
            if (coordinatesField == null)
            {
                coordinatesField = new Field("Coordinates", string.Empty, FieldType.Vector2);
                location.fields.Add(coordinatesField);
                SetDatabaseDirty("Add Coordinates Field");
            }
            
            coordinatesField.value = EditorGUILayout.Vector2Field(new GUIContent("Coordinates", "The location's coordinates on the map."), location.LookupVector2("Coordinates")).ToString();
            
            // Audio
            
            EditorGUILayout.PrefixLabel("Audio");
            
            var musicField = Field.Lookup(location.fields, "Music");
            
            if (musicField == null)
            {
                musicField = new Field("Music", string.Empty, FieldType.Text);
                location.fields.Add(musicField);
                SetDatabaseDirty("Add Music Field");
            }
            
            DrawEditorItemWithAudioClipIcon(
                () => musicField.value = EditorGUILayout.TextField(new GUIContent("Music", "The location's music."), musicField.value));
            
            
            var environmentAudioField = Field.Lookup(location.fields, "Environment");
            
            if (environmentAudioField == null)
            {
                environmentAudioField = new Field("Environment", string.Empty, FieldType.Text);
                location.fields.Add(environmentAudioField);
                SetDatabaseDirty("Add Environment Field");
            }
            
            DrawEditorItemWithAudioClipIcon(
                () => environmentAudioField.value = EditorGUILayout.TextField(new GUIContent("Environment", "The location's environment sounds."), environmentAudioField.value), color: Color.cyan);
            
           
            
            var sublocations = database.locations.Where(p => p.IsSublocation && database.GetLocation(p.RootID) == location).ToList();

            if (sublocations.Count > 0)
            {
                EditorGUILayout.PrefixLabel("Sublocations");
                EditorWindowTools.StartIndentedSection();
                GUI.enabled = false;
                foreach (var sublocation in sublocations)
                {
                    DrawLocationField(new GUIContent("Sublocation"), sublocation.id.ToString(), true);
                }
                EditorWindowTools.EndIndentedSection();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.Space();

            var presentActors =
                database.actors.Where(p => p.IsFieldAssigned("Location") && p.LookupInt("Location") == location.id).ToList();

            if (presentActors.Count > 0)
            {
                EditorGUILayout.PrefixLabel("Present Actors");
                
                EditorWindowTools.StartIndentedSection();

                GUI.enabled = false;
                foreach (var actor in database.actors.Where(p => p.IsFieldAssigned("Location") && p.LookupInt("Location") == location.id))
                {
                    DrawActorField(new GUIContent("Actor"), actor.id.ToString());
                }
                
                EditorWindowTools.EndIndentedSection();
                
                GUI.enabled = true;
            }
           
            
           
        }
        
        private bool autoSetParentLocation = true;
        
        private void DrawSublocationProperties(Location location)
        {
            if (location == null || location.fields == null) return;
            
            var displayNameField = Field.Lookup(location.fields, "Display Name");
            var hasDisplayNameField = (displayNameField != null);
            
            
            var useDisplayNameField = EditorGUILayout.Toggle(new GUIContent("Use Display Name", "Tick to use a Display Name in UIs that's different from the Name."), hasDisplayNameField);
            if (hasDisplayNameField && !useDisplayNameField)
            {
                location.fields.Remove(displayNameField);
                SetDatabaseDirty("Don't Use Display Name");
            }
            else if (useDisplayNameField)
            {
                DrawRevisableTextField(displayNameLabel, location, null, location.fields, "Display Name");
                DrawLocalizedVersions(location, location.fields, "Display Name {0}", false, FieldType.Text);
            }
            
            var parentLocationField = Field.Lookup(location.fields, "Parent Location");

            if (parentLocationField == null)
            {
                parentLocationField = new Field("Parent Location", string.Empty, FieldType.Location);
                location.fields.Add(parentLocationField);
                SetDatabaseDirty("Add Parent Location Field");
            }
            
            

            if (autoSetParentLocation)
            {
                
                GUI.enabled = false;
                
                var indexInReorderableList = ((List<Location>)locationReorderableList.list).IndexOf( location);

                parentLocationField.value = string.Empty;
                
                for (int i = indexInReorderableList - 1; i >= 0; i--)
                {
                    if (!((List<Location>)locationReorderableList.list)[i].IsSublocation)
                    {
                        parentLocationField.value = ((List<Location>)locationReorderableList.list)[i].id.ToString();
                        break;
                    }
                }
                
                if (parentLocationField.value == string.Empty)
                {
                    parentLocationField.value =((List<Location>)locationReorderableList.list).First(p => !p.IsSublocation).id.ToString();
                }
                
                DrawLocationField(new GUIContent("Parent", "The location of the action."), parentLocationField.value, false);
              
                GUI.enabled = true;
            }

            else
            {
            
                parentLocationField.value = DrawLocationField(new GUIContent("Location", "The location of the action."), parentLocationField.value, false);
            }
            
            autoSetParentLocation = EditorGUILayout.Toggle(new GUIContent("Auto Set Parent Location", "Tick to automatically set the parent location to the first location above this one that isn't a sublocation."), autoSetParentLocation);


            DrawLocationConversationProperties(location);


        }


        private void DrawLocationConversationProperties(Location location)
        {
            bool startsConversation = Field.Lookup(location.fields, "Conversation") != null;
            bool newStartsConversation = EditorGUILayout.Toggle(new GUIContent("Starts Conversation", "Tick to mark this quest abandonable in the quest window."), startsConversation);

            if (startsConversation != newStartsConversation)
            {
                Field conversation;
                
                if (!startsConversation)
                {
                    conversation = new Field("Conversation", string.Empty, FieldType.Text);
                    location.fields.Add(conversation);
                   
                }
                
                ToggleStartsConversation( location, newStartsConversation);
                SetDatabaseDirty(!startsConversation ? "Create Starts Conversation Field" : "Remove Starts Conversation Field");
            }

            if (newStartsConversation)
            {
                DrawStartConversationProperties(location);
            }
            
            
            EditorGUILayout.Space();
        }
        
        
    }

}