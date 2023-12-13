/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static bool showPOI;

        private static void PoiUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (prefs.POI == null) prefs.POI = new List<RealWorldTerrainPOI>();

            if (GUILayout.Button(new GUIContent("+", "Add POI"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                prefs.POI.Add(new RealWorldTerrainPOI("New POI " + (prefs.POI.Count + 1),
                    (prefs.leftLongitude + prefs.rightLongitude) / 2, (prefs.topLatitude + prefs.bottomLatitude) / 2));
            }

            GUILayout.Label("");

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) prefs.POI = new List<RealWorldTerrainPOI>();

            EditorGUILayout.EndHorizontal();

            if (prefs.POI.Count == 0)
            {
                GUILayout.Label("POI is not specified.");
            }

            int poiIndex = 1;
            int poiDeleteIndex = -1;

            foreach (RealWorldTerrainPOI poi in prefs.POI)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(poiIndex.ToString(), GUILayout.ExpandWidth(false));
                poi.title = EditorGUILayout.TextField("", poi.title);
                GUILayout.Label("Lat", GUILayout.ExpandWidth(false));
                poi.y = EditorGUILayout.DoubleField("", poi.y, GUILayout.Width(80));
                GUILayout.Label("Lng", GUILayout.ExpandWidth(false));
                poi.x = EditorGUILayout.DoubleField("", poi.x, GUILayout.Width(80));
                if (GUILayout.Button(new GUIContent("X", "Delete POI"), GUILayout.ExpandWidth(false))) poiDeleteIndex = poiIndex - 1;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.Label("Altitude", GUILayout.ExpandWidth(false));
                poi.altitude = EditorGUILayout.FloatField("", poi.altitude, GUILayout.Width(40));
                GUILayout.Label("Prefab", GUILayout.ExpandWidth(false));
                poi.prefab = EditorGUILayout.ObjectField("", poi.prefab, typeof(GameObject), false) as GameObject;
                EditorGUILayout.EndHorizontal();

                poiIndex++;
            }

            if (poiDeleteIndex != -1) prefs.POI.RemoveAt(poiDeleteIndex);

            EditorGUILayout.Space();
        }
    }
}