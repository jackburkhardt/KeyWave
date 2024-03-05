/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.RealWorldTerrain.OSM;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainBuildingManager : EditorWindow
    {
        private RealWorldTerrainOSMMeta[] buildings;
        private Vector2 scrollPosition;
        private string filter;
        private FilterType filterType = FilterType.KeyAndValue;
        private RealWorldTerrainOSMMeta[] filteredBuildings;
        private int page = 0;

        private void FilterBuildings()
        {
            if (string.IsNullOrEmpty(filter)) filteredBuildings = buildings;
            else
            {
                string f = filter.ToUpperInvariant();
                bool searchInKey = filterType != FilterType.Value;
                bool searchInValue = filterType != FilterType.Key;
                filteredBuildings = buildings.Where(b => b.ContainKeyOrValue(f, searchInKey, searchInValue)).ToArray();
            }
        }

        private void OnEnable()
        {
            UpdateBuildings();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Filters buildings by tag (in whole or in part).", MessageType.Info);

            EditorGUI.BeginChangeCheck();
            filter = EditorGUILayout.TextField("Filter", filter);
            filterType = (FilterType)EditorGUILayout.EnumPopup("Search in", filterType);

            if (EditorGUI.EndChangeCheck()) FilterBuildings();

            int pageSize = 500;

            bool showPaginator = filteredBuildings.Length > pageSize;
            if (!showPaginator) page = 0;
            else if (page * pageSize >= filteredBuildings.Length) page = filteredBuildings.Length / pageSize;

            int start = page * pageSize;
            int end = Mathf.Min(start + pageSize, filteredBuildings.Length);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            bool hasNullItems = false;

            for (int i = start; i < end; i++)
            {
                RealWorldTerrainOSMMeta building = filteredBuildings[i];
                GUILayout.BeginHorizontal();

                string buildingName = "";
                if (building == null)
                {
                    buildingName = "Missed";
                    hasNullItems = true;
                }
                else buildingName = building.name;
                GUILayout.Label(buildingName);
                if (GUILayout.Button(new GUIContent(">", "Select"), GUILayout.ExpandWidth(false)))
                {
                    Selection.activeGameObject = building.gameObject;
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (showPaginator)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(page == 0);
                if (GUILayout.Button("<")) page--;
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.LabelField((start + 1) + "-" + end + " from " + filteredBuildings.Length);

                EditorGUI.BeginDisabledGroup(page == filteredBuildings.Length / pageSize);
                if (GUILayout.Button(">")) page++;
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Select All"))
            {
                Selection.objects = filteredBuildings.Select(b => b.gameObject).ToArray();
            }

            if (hasNullItems)
            {
                UpdateBuildings();
                Repaint();
            }
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Building Manager")]
        public static void OpenWindow()
        {
            GetWindow<RealWorldTerrainBuildingManager>(true, "Building Manager");
        }

        private void UpdateBuildings()
        {
            buildings = FindObjectsOfType<RealWorldTerrainBuilding>().Select(b => b.GetComponent<RealWorldTerrainOSMMeta>()).OrderBy(b => b.name).ToArray();
            FilterBuildings();
        }

        public enum FilterType
        {
            KeyAndValue,
            Key,
            Value
        }
    }
}