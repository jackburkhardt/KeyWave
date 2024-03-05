/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private const float TerrainLayerLineHeight = RealWorldTerrainVectorTerrainLayerFeature.TerrainLayerLineHeight;
        private static ReorderableList terrainLayersList;

        private static void AddVectorTerrainLayerFeature(ReorderableList list)
        {
            prefs.vectorTerrainLayers.Add(new RealWorldTerrainVectorTerrainLayerFeature());
        }

        private static int DrawVectorLayerData(RealWorldTerrainVectorTerrainLayerFeature.Rule rule, int index, ref Rect r)
        {
            int ret = 0; // 0 - do nothing, -1 - remove, 1 - update height

            GUI.Label(new Rect(r.x, r.y, r.width - 20, r.height), "Rule " + (index + 1) + ": ");
            if (GUI.Button(new Rect(r.xMax - 20, r.y, 20, r.height), "X")) ret = -1;
            r.y += TerrainLayerLineHeight;

            EditorGUI.BeginChangeCheck();
            rule.layer = (RealWorldTerrainMapboxLayer)EditorGUI.EnumPopup(r, "Layer", rule.layer);
            r.y += TerrainLayerLineHeight;
            if (EditorGUI.EndChangeCheck())
            {
                rule.extra = ~0;
                ret = 1;
            }

            if (rule.hasExtra)
            {
                if (rule.layer == RealWorldTerrainMapboxLayer.landuse)
                {
                    rule.extra = (int)(RealWorldTerrainMapboxLanduse)EditorGUI.EnumFlagsField(r, "Classes", (RealWorldTerrainMapboxLanduse)rule.extra);
                    r.y += TerrainLayerLineHeight;
                }
                else if (rule.layer == RealWorldTerrainMapboxLayer.landuse_overlay)
                {
                    rule.extra = (int)(RealWorldTerrainMapboxLanduseOverlay)EditorGUI.EnumFlagsField(r, "Classes", (RealWorldTerrainMapboxLanduseOverlay)rule.extra);
                    r.y += TerrainLayerLineHeight;
                }
                else if (rule.layer == RealWorldTerrainMapboxLayer.waterway)
                {
                    rule.extra = (int)(RealWorldTerrainMapboxWaterway)EditorGUI.EnumFlagsField(r, "Classes", (RealWorldTerrainMapboxWaterway)rule.extra);
                    r.y += TerrainLayerLineHeight;
                }
                else if (rule.layer == RealWorldTerrainMapboxLayer.structure)
                {
                    rule.extra = (int)(RealWorldTerrainMapboxStructure)EditorGUI.EnumFlagsField(r, "Classes", (RealWorldTerrainMapboxStructure)rule.extra);
                    r.y += TerrainLayerLineHeight;
                }
            }

            return ret;
        }

        private static void DrawVectorTerrainBaseLayers()
        {
            EditorGUILayout.LabelField("Base Layers");
            if (prefs.vectorTerrainBaseLayers == null) prefs.vectorTerrainBaseLayers = new List<TerrainLayer>();
            if (prefs.vectorTerrainBaseLayers.Count == 0) prefs.vectorTerrainBaseLayers.Add(null);

            int removeIndex = -1;
            for (int i = 0; i < prefs.vectorTerrainBaseLayers.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefs.vectorTerrainBaseLayers[i] = EditorGUILayout.ObjectField(new GUIContent((i + 1).ToString()), prefs.vectorTerrainBaseLayers[i], typeof(TerrainLayer), false) as TerrainLayer;
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }

            if (prefs.vectorTerrainBaseLayers.Count > 1)
            {
                prefs.vectorTerrainBaseLayersNoiseOffset = EditorGUILayout.Vector2Field("Noise Offset", prefs.vectorTerrainBaseLayersNoiseOffset);
                prefs.vectorTerrainBaseLayersNoiseScale = EditorGUILayout.FloatField("Noise Scale", prefs.vectorTerrainBaseLayersNoiseScale);
            }

            if (removeIndex != -1) prefs.vectorTerrainBaseLayers.RemoveAt(removeIndex);
            if (GUILayout.Button("Add Base Layer")) prefs.vectorTerrainBaseLayers.Add(null);
        }

        private static void DrawVectorTerrainLayerFeature(Rect rect, int index, bool isactive, bool isfocused)
        {
            Rect r = new Rect(rect)
            {
                height = TerrainLayerLineHeight - 2
            };

            RealWorldTerrainVectorTerrainLayerFeature layer = prefs.vectorTerrainLayers[index]; 
            GUI.Label(r, "Layer " + (index + 1) + ": ");
            r.y += TerrainLayerLineHeight;

            if (layer.terrainLayers == null) layer.terrainLayers = new List<TerrainLayer>();
            if (layer.terrainLayers.Count == 0) layer.terrainLayers.Add(null);

            int removeIndex = -1;

            for (int i = 0; i < layer.terrainLayers.Count; i++)
            {
                TerrainLayer terrainLayer = layer.terrainLayers[i];
                layer.terrainLayers[i] = EditorGUI.ObjectField(new Rect(r.x, r.y, r.width - 20, r.height), new GUIContent("Terrain Layer"), terrainLayer, typeof(TerrainLayer), false) as TerrainLayer;
                if (GUI.Button(new Rect(r.xMax - 20, r.y, 20, r.height),  "X")) removeIndex = i;
                r.y += TerrainLayerLineHeight;
            }

            if (layer.terrainLayers.Count > 1)
            {
                EditorGUI.LabelField(r, "Noise Offset");
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 20;
                float halfWidth = (r.width - labelWidth) / 2;
                layer.noiseOffset.x = EditorGUI.FloatField(new Rect(r.x + labelWidth, r.y, halfWidth, r.height), "X", layer.noiseOffset.x);
                layer.noiseOffset.y = EditorGUI.FloatField(new Rect(r.x + labelWidth + halfWidth, r.y, halfWidth, r.height), "Y", layer.noiseOffset.y);
                EditorGUIUtility.labelWidth = labelWidth;
                r.y += TerrainLayerLineHeight;
                layer.noiseScale = EditorGUI.FloatField(r, "Noise Scale", layer.noiseScale);
                r.y += TerrainLayerLineHeight;
            }

            if (GUI.Button(r, "Add TerrainLayer"))
            {
                layer.terrainLayers.Add(null);
                layer.UpdateHeight();
            }
            r.y += TerrainLayerLineHeight;

            if (removeIndex != -1)
            {
                layer.terrainLayers.RemoveAt(removeIndex);
                layer.UpdateHeight();
            }

            if (layer.rules == null) layer.rules = new List<RealWorldTerrainVectorTerrainLayerFeature.Rule>();

            removeIndex = -1;
            for (int i = 0; i < layer.rules.Count; i++)
            {
                int ret = DrawVectorLayerData(layer.rules[i], i, ref r);
                if (ret == -1) removeIndex = i;
                else if (ret == 1) layer.UpdateHeight();
            }

            if (removeIndex != -1)
            {
                layer.rules.RemoveAt(removeIndex);
                layer.UpdateHeight();
            }

            if (GUI.Button(r, "Add Rule"))
            {
                layer.rules.Add(new RealWorldTerrainVectorTerrainLayerFeature.Rule());
                layer.UpdateHeight();
            }
        }

        private static float HeightVectorTerrainLayerFeature(int index)
        {
            if (index < 0 || index >= prefs.vectorTerrainLayers.Count) return 0;
            RealWorldTerrainVectorTerrainLayerFeature layer = prefs.vectorTerrainLayers[index];
            return layer.height;
        }

        private static void RemoveVectorTerrainLayerFeature(ReorderableList list)
        {
            prefs.vectorTerrainLayers.RemoveAt(list.index);
        }

        private static void TerrainLayersUI()
        {
            EditorGUILayout.HelpBox("The ability to generate Terrain Layers is in beta. If you have any problems or have suggestions for improving this feature, please contact us (support@infinity-code.com).", MessageType.Info);
            EditorGUILayout.HelpBox("Important: you can specify materials only for new terrains. During regeneration, modified Terrain Layers will not be assigned. This is some kind of bug in Unity Editor. We are working on finding a way around this.", MessageType.Warning);

            MapboxAccessToken();
            TextureMaxLevelUI();
            DrawVectorTerrainBaseLayers();

            if (prefs.vectorTerrainLayers == null) prefs.vectorTerrainLayers = new List<RealWorldTerrainVectorTerrainLayerFeature>();

            if (terrainLayersList == null)
            {
                terrainLayersList = new ReorderableList(prefs.vectorTerrainLayers, typeof(RealWorldTerrainVectorTerrainLayerFeature), true, true, true, true);
                terrainLayersList.drawElementCallback += DrawVectorTerrainLayerFeature;
                terrainLayersList.drawHeaderCallback += r => GUI.Label(r, "Layers");
                terrainLayersList.onAddCallback += AddVectorTerrainLayerFeature;
                terrainLayersList.onRemoveCallback += RemoveVectorTerrainLayerFeature;
                terrainLayersList.elementHeightCallback += HeightVectorTerrainLayerFeature;
            }

            terrainLayersList.DoLayoutList();

            GUILayout.Space(10);
        }
    }
}