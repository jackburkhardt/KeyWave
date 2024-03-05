/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Diagnostics;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private const int LabelWidth = 170;

        public static int iProgress;
        public static string phasetitle = "";

        private static GUIStyle _oddStyle;
        private static GUIStyle _evenStyle;

        private static string mapboxAPI;
        private static Vector2 scrollPos = Vector2.zero;

        private static readonly string[] labels2n = { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        private static readonly int[] values2n = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        private static readonly string[] labels2n1 = { "33", "65", "129", "257", "513", "1025", "2049", "4097" };
        private static readonly int[] values2n1 = { 33, 65, 129, 257, 513, 1025, 2049, 4097 };

        public static GUIStyle evenStyle
        {
            get
            {
                if (_evenStyle == null) _evenStyle = new GUIStyle();
                return _evenStyle;
            }
        }

        public static GUIStyle oddStyle
        {
            get
            {
                if (_oddStyle == null)
                {
                    _oddStyle = new GUIStyle();
                    _oddStyle.normal.background = new Texture2D(1, 1);
                    _oddStyle.normal.background.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.2f));
                    _oddStyle.normal.background.Apply();
                }

                return _oddStyle;
            }
        }


        public static RealWorldTerrainGenerateType generateType
        {
            get { return RealWorldTerrainWindow.generateType; }
        }

        private static RealWorldTerrainPhase phase
        {
            get { return RealWorldTerrainPhase.activePhase; }
        }

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static RealWorldTerrainWindow wnd
        {
            get { return RealWorldTerrainWindow.wnd; }
        }

        public static double DoubleField(string label, double value, string tooltip, string href = "")
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.DoubleField(label, value);

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.helpIcon, tooltip),
                RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false)))
            {
                if (href != "") Application.OpenURL(href);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static void DrawHelpButton(string tooltip = null, string href = null)
        {
            if (string.IsNullOrEmpty(tooltip)) return;

            GUIContent content = new GUIContent(RealWorldTerrainResources.helpIcon, tooltip);
            if (GUILayout.Button(content, RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false))
                && !string.IsNullOrEmpty(href))
            {
                Application.OpenURL(href);
            }
        }

        public static System.Enum EnumPopup(string label, System.Enum selected, string tooltip, string href = "")
        {
            GUILayout.BeginHorizontal();
            Enum res = EditorGUILayout.EnumPopup(label, selected);
            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();

            return res;
        }

        public static float FloatField(string label, float value, string tooltip, string href = "")
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.FloatField(label, value);
            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();
            return value;
        }

        public static bool Foldout(bool value, string text)
        {
            return GUILayout.Toggle(value, text, EditorStyles.foldout);
        }

        public static int IntPopup(string label, int value, string[] displayedOptions, int[] optionValues, string tooltip, string href)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.IntPopup(label, value, displayedOptions, optionValues);

            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();
            return value;
        }

        public static int IntPopup(string label, int value, string[] displayedOptions, int[] optionValues, string tooltip, string[] hrefs)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.IntPopup(label, value, displayedOptions, optionValues);
            string href = null;
            if (hrefs != null && hrefs.Length > value) href = hrefs[value];
            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();
            return value;
        }

        public static int IntField(string label, int value, string tooltip, string href = null)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.IntField(label, value);
            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();
            return value;
        }

        private static void MapboxAccessToken()
        {
            if (mapboxAPI == null) mapboxAPI = RealWorldTerrainPrefs.LoadPref("MapboxAPI", "");
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            mapboxAPI = EditorGUILayout.TextField("Mapbox Access Token", mapboxAPI);
            if (EditorGUI.EndChangeCheck())
            {
                if (mapboxAPI == "") RealWorldTerrainPrefs.DeletePref("MapboxAPI");
                else RealWorldTerrainPrefs.SetPref("MapboxAPI", mapboxAPI);
            }

            if (string.IsNullOrEmpty(mapboxAPI))
            {
                GUILayout.Box(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Get Mapbox Access Token")) Process.Start("https://www.mapbox.com/studio/account/tokens/");
        }

        public static void OnEnable()
        {
            InitTextureProviders();
            InitBuildingEngines();
            InitRoadEngines();
            InitGrassEngines();
            InitTreeEngines();
            InitRiverEngines();
        }

        public static void OnGUI()
        {
            if (!RealWorldTerrainWindow.isCapturing) OnIdleGUI();
            else OnGenerate();
        }

        private static void OnGenerate()
        {
            if (phase != null && phase is RealWorldTerrainDownloadingPhase)
            {
                int completed = Mathf.FloorToInt(RealWorldTerrainDownloadManager.totalSizeMB * RealWorldTerrainWindow.progress);
                GUILayout.Label(phasetitle + " (" + completed + " of " + RealWorldTerrainDownloadManager.totalSizeMB + " mb)");
            }
            else GUILayout.Label(phasetitle);

            Rect r = EditorGUILayout.BeginVertical();
            iProgress = Mathf.FloorToInt(RealWorldTerrainWindow.progress * 100);
            EditorGUI.ProgressBar(r, RealWorldTerrainWindow.progress, iProgress + "%");
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Cancel")) RealWorldTerrainWindow.CancelCapture();

            GUILayout.Label("Warning: Keep this window open.");
        }

        private static void OnIdleButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start")) RealWorldTerrainWindow.StartCapture();
            if (RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.full)
            {
                if (GUILayout.Button("Memory Usage", GUILayout.ExpandWidth(false))) RealWorldTerrainMemoryUsageWindow.OpenWindow();
            }
            else
            {
                if (GUILayout.Button("Switch to Full", GUILayout.ExpandWidth(false)))
                {
                    RealWorldTerrainWindow.generateType = RealWorldTerrainGenerateType.full;
                    RealWorldTerrainWindow.generateTarget = null;
                }
            }

            if (GUILayout.Button("Clear Cache", GUILayout.ExpandWidth(false))) RealWorldTerrainWindow.ClearCache();
            GUILayout.EndHorizontal();
        }

        private static void OnIdleGUI()
        {
            ToolbarUI();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUIUtility.labelWidth = LabelWidth;

            bool isFull = RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.full;
            bool isTerrain = isFull || RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.terrain;
            bool isTexture = isFull || RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.texture;
            bool isAdditional = isFull || RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.additional;
            bool isRawOutput = prefs.resultType == RealWorldTerrainResultType.gaiaStamp || prefs.resultType == RealWorldTerrainResultType.rawFile;

            if (isFull) AreaUI();

            if (isTerrain)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                showTerrains = EditorGUILayout.Foldout(showTerrains, "Terrains");

                if (showTerrains) TerrainUI();

                EditorGUILayout.EndVertical();

                ElevationProviderUI();
            }

            if (isTexture && !isRawOutput)
            {
                if (isFull)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();
                    if (prefs.generateTextures) showTextures = GUILayout.Toggle(showTextures, "", EditorStyles.foldout, GUILayout.ExpandWidth(false));
                    prefs.generateTextures = GUILayout.Toggle(prefs.generateTextures, "Textures");
                    EditorGUILayout.EndHorizontal();
                    if (showTextures && prefs.generateTextures) TextureUI();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField("Textures");
                    prefs.generateTextures = true;

                    TextureUI();

                    EditorGUILayout.EndVertical();
                }
            }

            if (isFull && !isRawOutput)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                showPOI = EditorGUILayout.Foldout(showPOI, "POI");
                DrawHelpButton("Here you can specify a point of interest, which will be created on the terrains.");
                EditorGUILayout.EndHorizontal();
                if (showPOI) PoiUI();
                EditorGUILayout.EndVertical();
            }

            if (isAdditional && !isRawOutput) OsmUI();

            GUILayout.EndScrollView();
            OnIdleButtons();
        }

        public static Object ObjectField(string label, Object obj, Type type, string tooltip = null, string href = null)
        {
            EditorGUILayout.BeginHorizontal();
            obj = EditorGUILayout.ObjectField(label, obj, type, false);
            DrawHelpButton(tooltip, href);
            EditorGUILayout.EndHorizontal();
            return obj;
        }

        private static int Popup(string label, int selectedIndex, string[] displayedOptions, string tooltip = null, string href = null)
        {
            EditorGUILayout.BeginHorizontal();
            int res = EditorGUILayout.Popup(label, selectedIndex, displayedOptions);
            DrawHelpButton(tooltip, href);
            EditorGUILayout.EndHorizontal();
            return res;
        }

        private static bool Toggle(bool value, string text, string tooltip = null, string href = null)
        {
            return Toggle(value, text, true, tooltip, href);
        }

        private static bool Toggle(string text, bool value, bool left, string tooltip = null, string href = null)
        {
            return Toggle(value, text, left, tooltip, href);
        }

        private static bool Toggle(bool value, string text, bool left, string tooltip = null, string href = null)
        {
            EditorGUILayout.BeginHorizontal();

            if (left) value = GUILayout.Toggle(value, text);
            else value = EditorGUILayout.Toggle(text, value);
            DrawHelpButton(tooltip, href);
            EditorGUILayout.EndHorizontal();

            return value;
        }

        private static Vector3 Vector3Field(string label, Vector3 value, string tooltip = null, string href = null)
        {
            EditorGUILayout.BeginHorizontal();
            Vector3 res = EditorGUILayout.Vector3Field(label, value);
            DrawHelpButton(tooltip, href);
            EditorGUILayout.EndHorizontal();
            return res;
        }

        private static void MinMaxSlider(string label, ref float minValue, ref float maxValue, int minLimit, int maxLimit, string tooltip = null, string href = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider(label, ref minValue, ref maxValue, minLimit, maxLimit);
            DrawHelpButton(tooltip, href);
            EditorGUILayout.EndHorizontal();
        }
    }
}