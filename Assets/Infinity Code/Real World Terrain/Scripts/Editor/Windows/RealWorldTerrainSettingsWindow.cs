/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainSettingsWindow : EditorWindow
    {
        private static RealWorldTerrainSettingsWindow wnd;

        private string customCacheFolder;
        private bool defaultCacheFolder = true;
        private bool hasThirdPartyAssets;
        private string resultName = "RealWorld Terrain";
        private Vector2 scrollPosition = Vector2.zero;
        private bool showGeneral = true;
        private bool showCacheFolder = true;
        private bool showResultName = true;
        private bool showTerrainNames = true;
        private bool showTerrainTokens;
        private bool showThirdPartyAssets = false;
        private bool showResultTokens;
        private static Assembly assembly;
        private bool appendResultNameIndex = true;
        private string terrainName = "Terrain {x}x{y}";
        private bool generateInThread = true;
        private RealWorldTerrainOSMOverpassServer osmServer = RealWorldTerrainOSMOverpassServer.main;
        private bool hasBuildR2;
        private bool hasBuildR3;
        private bool hasEasyRoads;
        private bool hasProceduralToolkit;
        private bool hasRoadArchitect;
        private bool hasRTP;
        private bool hasVolumeGrass;
        private bool hasWorldStreamer;
        private bool hasRAM2019;

        private static void AddCompilerDirective(string key)
        {
            string currentDefinitions =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup);

            string[] defs = currentDefinitions.Split(';').Select(d => d.Trim(' ')).ToArray();

            if (defs.All(d => d != key))
            {
                ArrayUtility.Add(ref defs, key);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", defs));
            }
        }

        public static void ClearSettings()
        {
            RealWorldTerrainPrefs.DeletePref("CacheFolder");
            RealWorldTerrainPrefs.DeletePref("ResultName");
            RealWorldTerrainPrefs.DeletePref("TerrainName");
            RealWorldTerrainPrefs.DeletePref("AppendIndex");
            RealWorldTerrainPrefs.DeletePref("GenerateInTread");
            RealWorldTerrainPrefs.DeletePref("OSMServer");

            if (wnd != null) wnd.Repaint();
        }

        private static void DeleteCompilerDirective(string key)
        {
            string currentDefinitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string[] defs = currentDefinitions.Split(';').Select(d => d.Trim(' ')).ToArray();

            if (defs.Any(d => d == key))
            {
                ArrayUtility.Remove(ref defs, key);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", defs));
            }
        }

        private void DrawResultTokens()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showResultTokens = EditorGUILayout.Foldout(showResultTokens, "Available tokens");
            if (showResultTokens)
            {
                GUILayout.Label("{title} - Title");
                GUILayout.Label("{tllat} - Top-Left latitude");
                GUILayout.Label("{tllng} - Top-Left longitude");
                GUILayout.Label("{brlat} - Bottom-Right latitude");
                GUILayout.Label("{brlng} - Bottom-Right longitude");
                GUILayout.Label("{cx} - Count X");
                GUILayout.Label("{cy} - Count Y");
                GUILayout.Label("{st} - Size type");
                GUILayout.Label("{me} - Max elevation");
                GUILayout.Label("{mu} - Max underwater depth");
                GUILayout.Label("{ds} - Depth shrapness");
                GUILayout.Label("{dr} - Detail resolution");
                GUILayout.Label("{rpp} - Resolution per patch");
                GUILayout.Label("{bmr} - Base map resolution");
                GUILayout.Label("{hmr} - Height map resolution");
                GUILayout.Label("{tp} - Texture provider");
                GUILayout.Label("{tw} - Texture width");
                GUILayout.Label("{th} - Texture height");
                GUILayout.Label("{tml} - Texture max level");
                GUILayout.Label("{ticks} - Current time ticks");
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTerrainTokens()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showTerrainTokens = EditorGUILayout.Foldout(showTerrainTokens, "Available tokens");
            if (showTerrainTokens)
            {
                GUILayout.Label("{tllat} - Top-Left latitude");
                GUILayout.Label("{tllng} - Top-Left longitude");
                GUILayout.Label("{brlat} - Bottom-Right latitude");
                GUILayout.Label("{brlng} - Bottom-Right longitude");
                GUILayout.Label("{x} - X Index");
                GUILayout.Label("{y} - Y Index");
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
        }

        private static Type FindType(string className)
        {
            if (assembly == null) assembly = typeof(RealWorldTerrainSettingsWindow).Assembly;
            return assembly.GetType(className);
        }

        private void OnCacheFolderGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showCacheFolder = EditorGUILayout.Foldout(showCacheFolder, "Cache folder:");
            if (showCacheFolder)
            {
                defaultCacheFolder = GUILayout.Toggle(defaultCacheFolder, "{PROJECT FOLDER}/RWT_Cache");
                defaultCacheFolder = !GUILayout.Toggle(!defaultCacheFolder, "Custom cache folder");

                if (!defaultCacheFolder)
                {
                    GUILayout.BeginHorizontal();
                    customCacheFolder = EditorGUILayout.TextField("", customCacheFolder);
                    GUI.SetNextControlName("BrowseButton");
                    if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                    {
                        GUI.FocusControl("BrowseButton");
                        string newCustomFolder = EditorUtility.OpenFolderPanel("Select the folder for the cache.", EditorApplication.applicationPath, "");
                        if (!string.IsNullOrEmpty(newCustomFolder)) customCacheFolder = newCustomFolder;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void OnBuildR2(ref bool dirty)
        {
            if (!hasBuildR2) return;

#if !BUILDR2
            if (GUILayout.Button("Enable BuildR2"))
            {
                AddCompilerDirective("BUILDR2");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable BuildR2"))
            {
                DeleteCompilerDirective("BUILDR2");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnBuildR3(ref bool dirty)
        {
            if (!hasBuildR3) return;

#if !BUILDR3
            if (GUILayout.Button("Enable BuildR3"))
            {
                AddCompilerDirective("BUILDR3");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable BuildR3"))
            {
                DeleteCompilerDirective("BUILDR3");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnEasyRoads(ref bool dirty)
        {
            if (!hasEasyRoads) return;

#if !EASYROADS3D
            if (GUILayout.Button("Enable EasyRoads3D v3"))
            {
                AddCompilerDirective("EASYROADS3D");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable EasyRoads3D v3"))
            {
                DeleteCompilerDirective("EASYROADS3D");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnDestroy()
        {
            wnd = null;
        }

        private void OnEnable()
        {
            wnd = this;
            customCacheFolder = RealWorldTerrainPrefs.LoadPref("CacheFolder", "");
            defaultCacheFolder = customCacheFolder == "";
            resultName = RealWorldTerrainPrefs.LoadPref("ResultName", "RealWorld Terrain");
            terrainName = RealWorldTerrainPrefs.LoadPref("TerrainName", "Terrain {x}x{y}");
            appendResultNameIndex = RealWorldTerrainPrefs.LoadPref("AppendIndex", true);
            generateInThread = RealWorldTerrainPrefs.LoadPref("GenerateInThread", true);
            osmServer = (RealWorldTerrainOSMOverpassServer)RealWorldTerrainPrefs.LoadPref("OSMServer", 0);

            hasBuildR2 = FindType("BuildR2.BuildingEditor") != null;
            hasBuildR3 = FindType("BuildRCities.EditorScripts.BuildREditor") != null;

            string[] easyRoads3DResult = Directory.GetFiles("Assets", "EasyRoads3Dv3.dll", SearchOption.AllDirectories);
            hasEasyRoads = easyRoads3DResult.Length > 0;

            string[] proceduralToolkitResults = Directory.GetFiles("Assets", "ProceduralToolkit.Editor.asmdef", SearchOption.AllDirectories);
            hasProceduralToolkit = proceduralToolkitResults.Length > 0;

            hasRoadArchitect = FindType("GSDRoadEditor") != null;
            hasRTP = FindType("RTP_LODmanagerEditor") != null;
            hasVolumeGrass = FindType("VolumeGrassEditor") != null;
            hasWorldStreamer = FindType("SceneSplitterEditor") != null;
            hasRAM2019 = FindType("RamSplineEditor") != null;
        }

        private void OnGeneralGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showGeneral = EditorGUILayout.Foldout(showGeneral, "General:");
            if (!showGeneral) EditorGUILayout.EndVertical();

            generateInThread = EditorGUILayout.Toggle("Generate in Thread", generateInThread);
            osmServer = (RealWorldTerrainOSMOverpassServer)EditorGUILayout.EnumPopup("OSM Overpass Server", osmServer);

            EditorGUILayout.EndVertical();
        }

        private void OnGUI()
        {
            bool dirty = false;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            OnGeneralGUI();
            OnCacheFolderGUI();
            OnResultNameGUI();
            OnTerrainNameGUI();

            OnThirdPartyGUI(ref dirty);

            GUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                if (defaultCacheFolder) RealWorldTerrainPrefs.DeletePref("CacheFolder");
                else RealWorldTerrainPrefs.SetPref("CacheFolder", customCacheFolder);

                if (resultName == "") RealWorldTerrainPrefs.DeletePref("ResultName");
                else RealWorldTerrainPrefs.SetPref("ResultName", resultName);

                if (terrainName == "") RealWorldTerrainPrefs.DeletePref("TerrainName");
                else RealWorldTerrainPrefs.SetPref("TerrainName", terrainName);

                RealWorldTerrainPrefs.SetPref("AppendIndex", appendResultNameIndex);
                RealWorldTerrainPrefs.SetPref("GenerateInThread", generateInThread);
                RealWorldTerrainPrefs.SetPref("OSMServer", (int)osmServer);

                RealWorldTerrainEditorUtils.ClearFoldersCache();

                Close();
            }

            if (GUILayout.Button("Revert to default settings", GUILayout.ExpandWidth(false)))
            {
                int result = EditorUtility.DisplayDialogComplex("Revert to default settings", "Reset generation settings?", "Reset", "Ignore", "Cancel");
                Debug.Log(result);
                if (result < 2)
                {
                    if (result == 0 && File.Exists(RealWorldTerrainPrefs.prefsFilename)) File.Delete(RealWorldTerrainPrefs.prefsFilename);
                    ClearSettings();
                    RealWorldTerrainEditorUtils.ClearFoldersCache();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (dirty) Repaint();
        }

        private void OnProceduralToolkit(ref bool dirty)
        {
            if (!hasProceduralToolkit) return;

#if !PROCEDURAL_TOOLKIT
            if (GUILayout.Button("Enable Procedural Toolkit"))
            {
                AddCompilerDirective("PROCEDURAL_TOOLKIT");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable Procedural Toolkit"))
            {
                DeleteCompilerDirective("PROCEDURAL_TOOLKIT");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnRAM2019(ref bool dirty)
        {
            if (!hasRAM2019) return;
#if !RAM2019
            if (GUILayout.Button("Enable R.A.M 2019"))
            {
                AddCompilerDirective("RAM2019");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable R.A.M 2019"))
            {
                DeleteCompilerDirective("RAM2019");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnRTP(ref bool dirty)
        {
            if (!hasRTP) return;
#if !RTP
            if (GUILayout.Button("Enable Relief Terrain Pack"))
            {
                AddCompilerDirective("RTP");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable Relief Terrain Pack"))
            {
                DeleteCompilerDirective("RTP");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnRoadArchitect(ref bool dirty)
        {
            if (!hasRoadArchitect) return;

#if !ROADARCHITECT
            if (GUILayout.Button("Enable Road Architect"))
            {
                AddCompilerDirective("ROADARCHITECT");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable Road Architect"))
            {
                DeleteCompilerDirective("ROADARCHITECT");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnResultNameGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showResultName = EditorGUILayout.Foldout(showResultName, "Result GameObject name: ");
            if (showResultName)
            {
                resultName = EditorGUILayout.TextField("", resultName);
                GUILayout.Label("Example:\nRWT_{cx}x{cy} = RWT_4x4");

                DrawResultTokens();

                appendResultNameIndex = GUILayout.Toggle(appendResultNameIndex, "Append index if GameObject already exists?");
            }
            EditorGUILayout.EndVertical();
        }

        private void OnTerrainNameGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showTerrainNames = EditorGUILayout.Foldout(showTerrainNames, "Terrain GameObjects name: ");
            if (showTerrainNames)
            {
                terrainName = EditorGUILayout.TextField("", terrainName);
                GUILayout.Label("Example:\nTerrain_{x}x{y} = Terrain_1x3");

                DrawTerrainTokens();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnThirdPartyGUI(ref bool dirty)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showThirdPartyAssets = EditorGUILayout.Foldout(showThirdPartyAssets, "Third Party Assets:");
            if (showThirdPartyAssets)
            {
                hasThirdPartyAssets = false;

                OnBuildR2(ref dirty);
                OnBuildR3(ref dirty);
                OnEasyRoads(ref dirty);
                //OnProceduralToolkit(ref dirty);
                OnRAM2019(ref dirty);
                OnRTP(ref dirty);
                OnRoadArchitect(ref dirty);
                OnVolumeGrass(ref dirty);
                OnWorldStreamer(ref dirty);

                if (!hasThirdPartyAssets) GUILayout.Label("Third Party Assets not found.");
            }

            EditorGUILayout.EndVertical();
        }

        private void OnVolumeGrass(ref bool dirty)
        {
            if (!hasVolumeGrass) return;

#if !VOLUMEGRASS
            if (GUILayout.Button("Enable Volume Grass"))
            {
                AddCompilerDirective("VOLUMEGRASS");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable Volume Grass"))
            {
                DeleteCompilerDirective("VOLUMEGRASS");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        private void OnWorldStreamer(ref bool dirty)
        {
            if (!hasWorldStreamer) return;

#if !WORLDSTREAMER
            if (GUILayout.Button("Enable WorldStreamer"))
            {
                AddCompilerDirective("WORLDSTREAMER");
                dirty = true;
            }
#else
            if (GUILayout.Button("Disable WorldStreamer"))
            {
                DeleteCompilerDirective("WORLDSTREAMER");
                dirty = true;
            }
#endif
            hasThirdPartyAssets = true;
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Settings")]
        public static void OpenWindow()
        {
            wnd = GetWindow<RealWorldTerrainSettingsWindow>(false, "Settings");
        }
    }
}
