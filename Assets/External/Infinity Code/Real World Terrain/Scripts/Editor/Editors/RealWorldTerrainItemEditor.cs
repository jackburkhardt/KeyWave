/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using InfinityCode.RealWorldTerrain.Tools;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof(RealWorldTerrainItem))]
    public class RealWorldTerrainItemEditor : Editor
    {
        private RealWorldTerrainItem item;
        private Texture2D texture;

        private void DrawBaseDist()
        {
            if (item == null) return;

            float dist = item.terrain.basemapDistance;

            EditorGUI.BeginChangeCheck();

            dist = EditorGUILayout.Slider("Base Map Distance", dist, 0, 20000);

            if (EditorGUI.EndChangeCheck()) item.terrain.basemapDistance = dist;
        }

        private void DrawItemScale(float sizeX, float sizeY)
        {
            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            Bounds bounds = item.bounds;
            Vector3 p = bounds.min;
            Vector3 p2 = bounds.max;
            if (p.x < minX) minX = p.x;
            if (p.z < minZ) minZ = p.z;
            if (p2.x > maxX) maxX = p2.x;
            if (p2.z > maxZ) maxZ = p2.z;

            GUILayout.Label("Scale X: " + sizeX * 1000 / (maxX - minX) + " meter/unit");
            GUILayout.Label("Scale Z: " + sizeY * 1000 / (maxZ - minZ) + " meter/unit");

            EditorGUILayout.Space();
        }

        private void DrawItemSize(out float sizeX, out float sizeY)
        {
            sizeX = sizeY = 0;
            if (item == null || item.prefs == null) return;

            double tx = item.leftLongitude;
            double ty = item.topLatitude;
            double bx = item.rightLongitude;
            double by = item.bottomLatitude;
            double rx = bx - tx;

            if (item.prefs.sizeType == 0 || item.prefs.sizeType == 2)
            {
                double scfY = Math.Sin(ty * Mathf.Deg2Rad);
                double sctY = Math.Sin(by * Mathf.Deg2Rad);
                double ccfY = Math.Cos(ty * Mathf.Deg2Rad);
                double cctY = Math.Cos(by * Mathf.Deg2Rad);
                double cX = Math.Cos(rx * Mathf.Deg2Rad);
                double sizeX1 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
                double sizeX2 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(sctY * sctY + cctY * cctY * cX));
                sizeX = (float)((sizeX1 + sizeX2) / 2.0);
                sizeY = (float)(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * sctY + ccfY * cctY));
            }
            else if (item.prefs.sizeType == 1)
            {
                sizeX = (float)Math.Abs(rx / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
                sizeY = (float)Math.Abs((by - ty) / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
            }

            GUILayout.Label("Size X: " + sizeX + " km");
            GUILayout.Label("Size Z: " + sizeY + " km");
            EditorGUILayout.Space();
        }

        public static void DrawLocationInfo(RealWorldTerrainMonoBase item)
        {
            GUILayout.Label("Top-Left: ");
            GUILayout.Label("  Latitude: " + item.topLatitude);
            GUILayout.Label("  Longitude: " + item.leftLongitude);
            EditorGUILayout.Space();
            GUILayout.Label("Bottom-Right: ");
            GUILayout.Label("  Latitude: " + item.bottomLatitude);
            GUILayout.Label("  Longitude: " + item.rightLongitude);
            EditorGUILayout.Space();
        }

        private void DrawToolbar()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = 40,
                padding = new RectOffset(5, 5, 4, 4)
            };
            EditorGUILayout.BeginHorizontal(style);

            GUIStyle buttonStyle = new GUIStyle { margin = new RectOffset(5, 5, 0, 0) };

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.refreshIcon, "Real World Terrain"), buttonStyle, GUILayout.ExpandWidth(false))) ShowRegenerateMenu();
            if (item.generateTextures && item.prefs.textureResultType == RealWorldTerrainTextureResultType.regularTexture && GUILayout.Button(new GUIContent(RealWorldTerrainResources.wizardIcon, "Postprocess"), buttonStyle, GUILayout.ExpandWidth(false))) ShowPostprocessMenu();
            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.rawIcon, "Export/Import RAW"), buttonStyle, GUILayout.ExpandWidth(false))) ShowRawMenu();

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTreeAndGrassProps()
        {
            GUILayout.Label("Tree and Grass Distances:");

            item.terrain.detailObjectDistance = EditorGUILayout.Slider("Detail Distance", item.terrain.detailObjectDistance, 0, 250);
            item.terrain.detailObjectDensity = EditorGUILayout.Slider("Detail Density", item.terrain.detailObjectDensity, 0, 1);
            item.terrain.treeDistance = EditorGUILayout.Slider("Tree Distance", item.terrain.treeDistance, 0, 2000);
            item.terrain.treeBillboardDistance = EditorGUILayout.Slider("Billboard Start", item.terrain.treeBillboardDistance, 5, 2000);

            EditorGUILayout.Space();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        private void OnEnable()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            item = (RealWorldTerrainItem)target;
        }

        public override void OnInspectorGUI()
        {
            DrawToolbar();
            DrawLocationInfo(item);

            float sizeX, sizeY;
            DrawItemSize(out sizeX, out sizeY);
            DrawItemScale(sizeX, sizeY);

            EditorGUI.BeginChangeCheck();

            RealWorldTerrainPrefsBase prefs = item.prefs;
            RealWorldTerrainResultType resultType = prefs.resultType;
            RealWorldTerrainTextureResultType textureResultType = prefs.textureResultType;

            if (resultType == RealWorldTerrainResultType.terrain)
            {
                DrawTreeAndGrassProps();
                DrawBaseDist();
            }

            Texture currentTexture = null;

            if (textureResultType == RealWorldTerrainTextureResultType.regularTexture)
            {
                currentTexture = item.texture;
                currentTexture = (Texture2D)EditorGUILayout.ObjectField("Texture: ", currentTexture, typeof(Texture2D), true);
            }
            else if (textureResultType == RealWorldTerrainTextureResultType.hugeTexture && resultType == RealWorldTerrainResultType.terrain)
            {
                currentTexture = item.terrain.materialTemplate.mainTexture;
                currentTexture = (Texture2DArray)EditorGUILayout.ObjectField("Texture: ", currentTexture, typeof(Texture2DArray), true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (textureResultType == RealWorldTerrainTextureResultType.regularTexture) item.texture = currentTexture as Texture2D;
                else if (textureResultType == RealWorldTerrainTextureResultType.hugeTexture && resultType == RealWorldTerrainResultType.terrain)
                {
                    item.terrain.materialTemplate.mainTexture = currentTexture as Texture2DArray;
                }
            }
        }

        private void OnUpdate()
        {
            if (item != null && item.needUpdate)
            {
                item.needUpdate = false;
                Repaint();
            }
        }

        private void ShowPostprocessMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Brightness, Contrast and HUE"), false, () => RealWorldTerrainHUEWindow.OpenWindow(item));
            menu.AddItem(new GUIContent("Color Balance"), false, () => RealWorldTerrainColorBalance.OpenWindow(item));
            menu.AddItem(new GUIContent("Color Levels"), false, () => RealWorldTerrainColorLevels.OpenWindow(item));

            if (item.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                menu.AddItem(new GUIContent("Erosion"), false, () => RealWorldTerrainErosionFilter.OpenWindow(item));
                menu.AddItem(new GUIContent("Generate Grass from Texture"), false, () => RealWorldTerrainGrassGeneratorWindow.OpenWindow(item));
                menu.AddItem(new GUIContent("Generate SplatPrototypes from Texture"), false, () => RealWorldTerrainSplatPrototypeGenerator.OpenWindow(item));
            }

            menu.ShowAsContext();
        }

        private void ShowRawMenu()
        {
            GenericMenu menu = new GenericMenu();

            if (item.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                menu.AddItem(new GUIContent("Export Heightmap"), false, () => RealWorldTerraiHeightmapExporter.OpenWindow(item));
                menu.AddItem(new GUIContent("Import Heightmap"), false, () => RealWorldTerrainHeightmapImporter.OpenWindow(item));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Detailmap"), false, () => RealWorldTerrainDetailmapExporter.OpenWindow(item));
                menu.AddItem(new GUIContent("Import Detailmap"), false, () => RealWorldTerrainDetailmapImporter.OpenWindow(item));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Alphamap"), false, () => RealWorldTerrainAlphamapExporter.OpenWindow(item));
                menu.AddItem(new GUIContent("Import Alphamap"), false, () => RealWorldTerrainAlphamapImporter.OpenWindow(item));
                
            }
            else if (item.prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                menu.AddItem(new GUIContent("Export OBJ"), false, () => RealWorldTerrainMeshOBJExporter.Export(item));
            }

            if (item.generateTextures)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Textures"), false, () => RealWorldTerrainContainerEditor.ExportRawTextures(item));
                menu.AddItem(new GUIContent("Import Textures"), false, () => RealWorldTerrainContainerEditor.ImportRawTextures(item));
            }
            menu.ShowAsContext();
        }

        private void ShowRegenerateMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Regenerate Terrains"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.terrain, item));
            menu.AddItem(new GUIContent("Regenerate Textures"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.texture, item));
            menu.AddItem(new GUIContent("Regenerate Additional"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.additional, item));
            menu.ShowAsContext();
        }
    }
}