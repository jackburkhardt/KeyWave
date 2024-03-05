/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Phases;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainWindow : EditorWindow
    {
        /// <summary>
        /// Current version number
        /// </summary>
        public const string version = "4.8.0.1";

        /// <summary>
        /// The action that occurs when the user aborts the generation.
        /// </summary>
        public static Action OnCaptureCanceled;

        /// <summary>
        /// The action that occurs when the generation is completed.
        /// </summary>
        public static Action OnCaptureCompleted;

        /// <summary>
        /// The action that occurs when the generation is started.
        /// </summary>
        public static Action OnCaptureStarted;

        public static RealWorldTerrainContainer container;
        public static bool generateInThread;
        public static RealWorldTerrainMonoBase generateTarget;
        public static RealWorldTerrainGenerateType generateType = RealWorldTerrainGenerateType.full;
        public static bool isCapturing;
        public static float progress;
        public static RealWorldTerrainPrefs prefs;
        public static RealWorldTerrainItem[,] terrains;
        public static RealWorldTerrainWindow wnd;
        
        private static int textureLevel;
        private static Thread thread;

        public static void CancelCapture()
        {
            Dispose();

            if (OnCaptureCanceled != null) OnCaptureCanceled();
        }

        public static void CancelInMainThread()
        {
            EditorApplication.update += OnCancelInMainThread;
        }

        private static bool CheckFields()
        {
            if (prefs.resultType == RealWorldTerrainResultType.gaiaStamp)
            {
#if !GAIA_PRESENT && !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
                Debug.Log("Gaia not found. Import Gaia into the project.");
                return false;
#endif
            }
            else if (prefs.resultType == RealWorldTerrainResultType.rawFile)
            {
                string filename = prefs.rawFilename;
                string ext = prefs.rawType == RealWorldTerrainRawType.RAW ? ".raw" : ".png";
                if (!filename.ToLower().EndsWith(ext)) filename += ext;

                if (File.Exists(filename))
                {
                    if (!EditorUtility.DisplayDialog("Warning", "File already exists. Overwrite?", "Overwrite", "Cancel"))
                    {
                        return false;
                    }
                }
            }

            if (prefs.leftLongitude >= prefs.rightLongitude)
            {
                Debug.Log("Bottom-Right Longitude must be greater than Top-Left Longitude");
                return false;
            }
            if (prefs.topLatitude <= prefs.bottomLatitude)
            {
                Debug.Log("Top-Left Latitude must be greater than Bottom-Right Latitude");
                return false;
            }
            if (prefs.leftLongitude < -180 || prefs.rightLongitude < -180 || prefs.leftLongitude > 180 || prefs.rightLongitude > 180)
            {
                Debug.Log("Longitude must be between -180 and 180.");
                return false;
            }

            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps)
            {
                RealWorldTerrainBingElevationGenerator.key = RealWorldTerrainPrefs.LoadPref("BingAPI", string.Empty);
                if (string.IsNullOrEmpty(RealWorldTerrainBingElevationGenerator.key))
                {
                    Debug.LogError("Bing Maps API key is not specified.");
                    return false;
                }
            }
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.Mapbox)
            {
                if (string.IsNullOrEmpty(RealWorldTerrainPrefs.mapboxAccessToken))
                {
                    Debug.LogError("Mapbox Access Token is not specified.");
                    return false;
                }
            }
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30)
            {
                RealWorldTerrainSRTM30ElevationGenerator.login = RealWorldTerrainPrefs.LoadPref("EarthDataLogin", string.Empty);
                RealWorldTerrainSRTM30ElevationGenerator.pass = RealWorldTerrainPrefs.LoadPref("EarthDataPass", string.Empty);
                if (string.IsNullOrEmpty(RealWorldTerrainSRTM30ElevationGenerator.login))
                {
                    Debug.LogError("EarthData username is not specified.");
                    return false;
                }
                if (string.IsNullOrEmpty(RealWorldTerrainSRTM30ElevationGenerator.pass))
                {
                    Debug.LogError("EarthData password is not specified.");
                    return false;
                }
            }

            if (prefs.elevationRange == RealWorldTerrainElevationRange.fixedValue && prefs.fixedMaxElevation - prefs.fixedMinElevation < 0)
            {
                Debug.LogError("EarthData password is not specified.");
                return false;
            }

            if (prefs.resultType == RealWorldTerrainResultType.terrain && !CheckHeightmapMemory()) return false;
            return true;
        }

        private static bool CheckHeightmapMemory()
        {
            int count = prefs.terrainCount;
            long size = prefs.heightmapResolution * prefs.heightmapResolution * 4 * count;
            size += prefs.baseMapResolution * prefs.baseMapResolution * 4 * count;
            size += prefs.detailResolution * prefs.detailResolution * 4 * count;
            size += 513 * 513 * 4 * count; //Alphamaps

            if (size > int.MaxValue * 0.75f)
            {
                return EditorUtility.DisplayDialog("Warning", "Too high settings. Perhaps out of memory error.", "Continue", "Abort");
            }
            return true;
        }

        public static void ClearCache()
        {
            RealWorldTerrainClearCacheWindow.OpenWindow();
        }

        public static void Dispose()
        {
            isCapturing = false;

            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }

            RealWorldTerrainElevationGenerator.Dispose();

            RealWorldTerrainTextureGenerator.Dispose();
            RealWorldTerrainTerrainLayersGenerator.Dispose();
            RealWorldTerrainDownloadManager.Dispose();
            RealWorldTerrainRoadGenerator.Dispose();
            RealWorldTerrainGrassGenerator.Dispose();
            RealWorldTerrainTreesGenerator.Dispose();
            RealWorldTerrainBuildingGenerator.Dispose();

            EditorUtility.UnloadUnusedAssetsImmediate();

            RealWorldTerrainPhase.requiredPhases = null;
            if (RealWorldTerrainPhase.activePhase != null)
            {
                try
                {
                    RealWorldTerrainPhase.activePhase.Finish();
                }
                catch
                {

                }
            }
            RealWorldTerrainPhase.activePhase = null;
            RealWorldTerrainPhase.activePhaseIndex = -1;

            if (wnd != null) wnd.Repaint();

            GC.Collect();
        }

        private static void OnCancelInMainThread()
        {
            CancelCapture();
            EditorApplication.update -= OnCancelInMainThread;
        }

        private void OnDestroy()
        {
            wnd = null;
            prefs.Save();
        }

        private void OnEnable()
        {
            wnd = this;
            if (prefs == null) prefs = new RealWorldTerrainPrefs();
            prefs.Load();

            RealWorldTerrainWindowUI.OnEnable();

            RealWorldTerrainUpdaterWindow.CheckNewVersionAvailable();
        }

        private void OnGUI()
        {
            RealWorldTerrainWindowUI.OnGUI();
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Open Real World Terrain", false, 0)]
        private static void OpenWindow()
        {
            OpenWindow(RealWorldTerrainGenerateType.full);
        }

        public static void OpenWindow(RealWorldTerrainGenerateType type, RealWorldTerrainMonoBase target = null)
        {
            generateTarget = target;
            generateType = type;
            wnd = GetWindow<RealWorldTerrainWindow>(false, "Real World Terrain");
            if (target == null)
            {
                prefs = new RealWorldTerrainPrefs();
                prefs.Load();
            }
            else if (type == RealWorldTerrainGenerateType.full)
            {
                prefs = RealWorldTerrainPrefs.GetPrefs(target, true);
                generateTarget = null;
            }
            else prefs = RealWorldTerrainPrefs.GetPrefs(target);

            if (type == RealWorldTerrainGenerateType.additional)
            {
                prefs.generateBuildings = false;
                prefs.generateGrass = false;
                prefs.generateRoads = false;
                prefs.generateTrees = false;
            }

            RealWorldTerrainWindowUI.InitTextureProviders();
            RealWorldTerrainUpdaterWindow.CheckNewVersionAvailable();
        }

        private static void PrepareStart()
        {
            generateInThread = RealWorldTerrainPrefs.LoadPref("GenerateInThread", true);
            RealWorldTerrainOSMUtils.InitOSMServer();

            if (!CheckFields()) return;

            if (prefs.resultType == RealWorldTerrainResultType.mesh || 
                prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                prefs.textureCount = prefs.terrainCount;
            }

            RealWorldTerrainElevationGenerator.elevations = new List<RealWorldTerrainElevationGenerator>();

            if (generateType == RealWorldTerrainGenerateType.full || generateType == RealWorldTerrainGenerateType.terrain || generateType == RealWorldTerrainGenerateType.additional)
            {
                if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM) RealWorldTerrainSRTMElevationGenerator.Init();
                else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps) RealWorldTerrainBingElevationGenerator.Init();
                else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30) RealWorldTerrainSRTM30ElevationGenerator.Init();
                else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.Mapbox) RealWorldTerrainMapboxElevationGenerator.Init();
                //else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.ArcGIS) RealWorldTerrainArcGISElevationGenerator.Init();
            }

            if (generateType == RealWorldTerrainGenerateType.full || generateType == RealWorldTerrainGenerateType.texture)
            {
                if (prefs.generateTextures && prefs.resultType != RealWorldTerrainResultType.gaiaStamp)
                {
                    if (prefs.textureResultType == RealWorldTerrainTextureResultType.regularTexture || prefs.textureResultType == RealWorldTerrainTextureResultType.hugeTexture)
                    {
                        RealWorldTerrainTextureGenerator.Init();
                    }
                    else if (prefs.textureResultType == RealWorldTerrainTextureResultType.terrainLayers)
                    {
                        RealWorldTerrainTerrainLayersGenerator.Init();
                    }
                }
            }

            if (generateType == RealWorldTerrainGenerateType.full || generateType == RealWorldTerrainGenerateType.additional)
            {
                if (prefs.resultType != RealWorldTerrainResultType.gaiaStamp)
                {
                    RealWorldTerrainBuildingGenerator.Download();
                    RealWorldTerrainRoadGenerator.Download();
                    RealWorldTerrainTreesGenerator.Download();
                    RealWorldTerrainGrassGenerator.Download();
                    RealWorldTerrainRiverGenerator.Download();
                }
            }

            if (prefs.mapType == null) prefs.mapType = RealWorldTerrainTextureProviderManager.FindMapType(prefs.mapTypeID);
            prefs.mapTypeID = prefs.mapType.fullID;
            prefs.mapTypeExtraFields = prefs.mapType.GetSettings();

            if (generateTarget != null) prefs.Apply(generateTarget);
            if (generateType == RealWorldTerrainGenerateType.full) RealWorldTerrainHistoryWindow.Add(prefs);

            isCapturing = true;

            RealWorldTerrainPhase.Init();

            if (OnCaptureStarted != null) OnCaptureStarted();
        }

        public static void StartCapture()
        {
            prefs.Save();

            if (generateType == RealWorldTerrainGenerateType.additional)
            {
                RealWorldTerrainMonoBase target = generateTarget;

                if (target is RealWorldTerrainContainer)
                {
                    if (prefs.generateBuildings) RealWorldTerrainUtils.DeleteGameObject(target.transform, "Buildings");
                    if (prefs.generateRoads) RealWorldTerrainUtils.DeleteGameObject(target.transform, "Roads");
                    if (prefs.generateRivers) RealWorldTerrainUtils.DeleteGameObject(target.transform, "Rivers");
                    if (prefs.generateTrees)
                    {
                        foreach (RealWorldTerrainItem item in (target as RealWorldTerrainContainer).terrains)
                        {
                            item.terrainData.treeInstances = new TreeInstance[0];
                            item.terrainData.treePrototypes = new TreePrototype[0];
                        }
                    }
                    if (prefs.generateGrass)
                    {
                        foreach (RealWorldTerrainItem item in (target as RealWorldTerrainContainer).terrains)
                        {
                            item.terrainData.detailPrototypes = new DetailPrototype[0];
                        }
                    }
                }
                else
                {
                    RealWorldTerrainItem item = target as RealWorldTerrainItem;

                    string index = item.x + "x" + (item.container.terrainCount.y - item.y - 1);
                    if (prefs.generateBuildings) RealWorldTerrainUtils.DeleteGameObject(target.transform.parent, "Buildings " + index);
                    if (prefs.generateRoads) RealWorldTerrainUtils.DeleteGameObject(target.transform.parent, "Roads " + index);
                    if (prefs.generateTrees)
                    {
                        item.terrainData.treeInstances = new TreeInstance[0];
                        item.terrainData.treePrototypes = new TreePrototype[0];
                    }
                    if (prefs.generateGrass) item.terrainData.detailPrototypes = new DetailPrototype[0];
                }
            }

            PrepareStart();
        }

        private void Update()
        {
            if (RealWorldTerrainPhase.activePhase == null) return;

            try
            {
                RealWorldTerrainPhase.activePhase.Enter();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                CancelCapture();
            }
        }
    }
}