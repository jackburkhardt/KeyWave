using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainInfoWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        private const float mb = 1048576;

        private static RealWorldTerrainInfoWindow instance;

        private MemoryUsage memoryUsage = new MemoryUsage();
        private DownloadInfo downloadInfo = new DownloadInfo();

        private int selectedTool = 0;

        private bool CalculateUsage()
        {
            RealWorldTerrainPrefs p = RealWorldTerrainWindow.prefs;
            if (p == null)
            {
                EditorUtility.DisplayDialog("Error", "Can not find the prefs. Open Real World Terrain window.", "OK");
                Close();
                return false;
            }

            if (selectedTool == 0) downloadInfo.Calculate(p);
            else if (selectedTool == 1) memoryUsage.Calculate(p);

            return true;
        }

        private static void DrawField(string prefix, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(prefix, GUILayout.MaxWidth(instance.position.size.x / 2 - 10));
            EditorGUILayout.LabelField(value, GUILayout.MaxWidth(instance.position.size.x / 2 - 10));
            EditorGUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            instance = this;
            CalculateUsage();
        }

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            //selectedTool = GUILayout.Toolbar(selectedTool, new[] {"Download", "Result"});
            if (EditorGUI.EndChangeCheck())
            {
                if (!CalculateUsage()) return;
                scrollPosition = Vector2.zero;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            /*if (selectedTool == 0) downloadInfo.Draw();
            else if (selectedTool == 1)*/ memoryUsage.Draw();
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh")) CalculateUsage();
        }

        public static void OpenWindow()
        {
            GetWindow<RealWorldTerrainInfoWindow>(true, "Area Info", true);
        }

        internal class DownloadInfo
        {
            private int heightmapRequests;
            private int texturesRequests;
            private int osmRequests;
            private int totalRequests;

            public void Calculate(RealWorldTerrainPrefs p)
            {
                
            }

            public void Draw()
            {
                
            }
        }

        internal class MemoryUsage
        {
            private long heightmap;
            private long controltexture;
            private long detailmap;
            private int countGrass;
            private long basemap;
            private long texture;
            private long totalPerTerrain;
            private RealWorldTerrainVector2i countTerrains;
            private long total;

            private string heightmapS;

            private string controltextureS;
            private string detailmapS;
            private string basemapS;
            private string textureS;
            private string totalPerTerrainS;
            private string totalS;
            private string countTerrainsS;


            public void Calculate(RealWorldTerrainPrefs p)
            {
                heightmap = p.heightmapResolution * p.heightmapResolution * 4;
                controltexture = p.controlTextureResolution * p.controlTextureResolution * 4;
                detailmap = p.detailResolution * p.detailResolution * 4;
                countGrass = p.generateGrass ? p.grassPrefabs.Count : 0;
                basemap = p.baseMapResolution * p.baseMapResolution * 4;
                texture = 0;
                if (p.generateTextures)
                {
                    if (p.textureResultType == RealWorldTerrainTextureResultType.regularTexture) texture = p.textureSize.x * p.textureSize.y * 4;
                    else if (p.textureResultType == RealWorldTerrainTextureResultType.hugeTexture) texture = p.hugeTexturePageSize * p.hugeTexturePageSize * p.hugeTextureCols * p.hugeTextureRows * 3;
                }
                totalPerTerrain = heightmap + countTerrains + detailmap * countGrass + basemap + texture;
                countTerrains = p.terrainCount;
                total = totalPerTerrain * countTerrains.count;

                string format = "{0:### ##0.00}";
                heightmapS = string.Format(format, heightmap / mb) + " mb";
                controltextureS = string.Format(format, controltexture / mb) + " mb";
                detailmapS = string.Format(format, detailmap * countGrass / mb) + " mb";
                basemapS = string.Format(format, basemap / mb) + " mb";
                textureS = string.Format(format, texture / mb) + " mb";
                totalPerTerrainS = string.Format(format, totalPerTerrain / mb) + " mb";
                countTerrainsS = countTerrains.count + " (" + countTerrains.x + "x" + countTerrains.y + ")";
                totalS = string.Format(format, total / mb) + " mb";
            }

            public void Draw()
            {
                EditorGUILayout.HelpBox("Uncompressed size of the result by the fields.\nHere only the main fields affecting the size are shown.\nNote that the memory that RWT will use for generation is not shown here.", MessageType.Info);

                DrawField("Height Map:", heightmapS);
                DrawField("Control Texture:", controltextureS);
                DrawField("Detail Map: ", detailmapS);
                DrawField("Base Map: ", basemapS);
                DrawField("Textures: ", textureS);
                EditorGUILayout.Space();
                DrawField("Total Per Terrain: ", totalPerTerrainS);
                EditorGUILayout.LabelField("---");
                DrawField("Count Terrains: ", countTerrainsS);
                DrawField("Total: ", totalS);
            }
        }
    }
}