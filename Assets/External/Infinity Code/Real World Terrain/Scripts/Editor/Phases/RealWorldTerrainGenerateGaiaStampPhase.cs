/*         INFINITY CODE         */
/*   https://infinity-code.com   */

#if GAIA_PRESENT || GAIA_PRO_PRESENT || GAIA_2_PRESENT
using System;
using System.IO;
using System.Reflection;
using Gaia;
using InfinityCode.RealWorldTerrain.Generators;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateGaiaStampPhase : RealWorldTerrainPhase
    {
        private Scanner scanner;
        private Stamper stamper;

        public override string title
        {
            get { return "Generate Gaia Stamp..."; }
        }

        public override void Enter()
        {
            try
            {
                RealWorldTerrainGaiaStampGenerator.Generate();
            }
            catch
            {
                phaseComplete = true;
            }
            if (!phaseComplete) return;

#if !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
            string basesDir = "Assets/Gaia/Stamps/Bases/";

            if (!Directory.Exists(basesDir)) Directory.CreateDirectory(basesDir);

            string basesDataDir = basesDir + "Data/";
            if (!Directory.Exists(basesDataDir)) Directory.CreateDirectory(basesDataDir);
#endif
            
            GaiaConstants.RawBitDepth bd = GaiaConstants.RawBitDepth.Sixteen;
            int resolution = 0;
            scanner.LoadRawFile(RealWorldTerrainGaiaStampGenerator.fullFilename, GaiaConstants.RawByteOrder.IBM, ref bd, ref resolution);

#if !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
            scanner.m_featureType = GaiaConstants.FeatureType.Bases;
#else
            scanner.m_exportFolder = GaiaDirectories.GetScannerExportDirectory();
            scanner.m_exportFileName = RealWorldTerrainGaiaStampGenerator.shortFilename;
            scanner.m_baseLevel = 0;
#endif
            scanner.SaveScan();

            AssetDatabase.Refresh();

            Selection.activeGameObject = stamper.gameObject;
#if !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
            stamper.LoadStamp(basesDir + RealWorldTerrainGaiaStampGenerator.shortFilename + ".jpg");
#else
            string maskFilename = GaiaDirectories.GetScannerExportDirectory() + "/" + RealWorldTerrainGaiaStampGenerator.shortFilename + ".exr";
            Texture2D maskTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(maskFilename);
            stamper.m_settings.m_imageMasks = new[]
            {
                new ImageMask
                {
                    ImageMaskTexture = maskTexture
                }
            };
            stamper.m_seaLevel = 0;

            float maxElevation = RealWorldTerrainUtils.MAX_ELEVATION;
            float minElevation = -RealWorldTerrainUtils.MAX_ELEVATION;

            if (prefs.elevationRange == RealWorldTerrainElevationRange.autoDetect)
            {
                double maxEl, minEl;
                RealWorldTerrainElevationGenerator.GetElevationRange(out minEl, out maxEl);
                maxElevation = (float)maxEl + prefs.autoDetectElevationOffset.y;
                minElevation = (float)minEl - prefs.autoDetectElevationOffset.x;
            }
            else if (prefs.elevationRange == RealWorldTerrainElevationRange.fixedValue)
            {
                maxElevation = prefs.fixedMaxElevation;
                minElevation = prefs.fixedMinElevation;
            }

            float sY = (maxElevation - minElevation) / 1000;

            stamper.transform.localScale = new Vector3(100, sY, 100);
#endif

            Complete();
        }

        public override void Finish()
        {
            if (scanner != null && scanner.gameObject != null) Object.DestroyImmediate(scanner.gameObject);

            scanner = null;
            stamper = null;
        }

        private static string GetAssetPath(string name)
        {
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            return null;
        }

        public override void Start()
        {
            try
            {
                GaiaSessionManager.GetSessionManager();
#if !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
                GaiaSettings m_settings = (GaiaSettings)Gaia.Utils.GetAssetScriptableObject("GaiaSettings");
#else
                GaiaSettings m_settings = (GaiaSettings)GaiaUtils.GetAssetScriptableObject("GaiaSettings");
#endif
                if (m_settings == null) m_settings = GaiaManagerEditor.CreateSettingsAsset();

                GaiaDefaults m_defaults = m_settings.m_currentDefaults;

                if (TerrainHelper.GetActiveTerrainCount() == 0)
                {
#if !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
                    GaiaResource m_resources = m_settings.m_currentResources;
                    m_defaults.CreateTerrain(m_resources);
#else
                    WorldCreationSettings worldCreationSettings = ScriptableObject.CreateInstance<WorldCreationSettings>();
                    worldCreationSettings.m_xTiles = m_settings.m_tilesX;
                    worldCreationSettings.m_zTiles = m_settings.m_tilesZ;
                    worldCreationSettings.m_tileSize = m_settings.m_currentDefaults.m_terrainSize;
                    worldCreationSettings.m_tileHeight = m_settings.m_tilesX * worldCreationSettings.m_tileSize;
                    worldCreationSettings.m_createInScene = m_settings.m_createTerrainScenes;
                    worldCreationSettings.m_autoUnloadScenes = m_settings.m_unloadTerrainScenes;
                    worldCreationSettings.m_applyFloatingPointFix = m_settings.m_floatingPointFix;

                    Type gsm = typeof(GaiaSessionManager);

                    MethodInfo createWorldMethod = gsm.GetMethod("CreateWorld", BindingFlags.Static | BindingFlags.Public, null, new[]{typeof(WorldCreationSettings), typeof(bool) }, null);
                    if (createWorldMethod != null)
                    {
                        createWorldMethod.Invoke(null, new object[] { worldCreationSettings, true });
                    }
                    else
                    {
                        createWorldMethod = gsm.GetMethod("CreateOrUpdateWorld", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(WorldCreationSettings), typeof(bool), typeof(bool) }, null);
                        if (createWorldMethod != null)
                        {
                            createWorldMethod.Invoke(null, new object[] { worldCreationSettings, true, false });
                        }
                        else
                        {
                            phaseComplete = true;
                            NextPhase();
                            return;
                        }
                    }

#if GAIA_PRO_PRESENT
                    WorldOriginEditor.m_sessionManagerExits = true;
#endif
#endif
                }

                GameObject gaiaObj = GameObject.Find("Gaia");
                if (gaiaObj == null) gaiaObj = GameObject.Find("Gaia Tools");
                if (gaiaObj == null) gaiaObj = new GameObject("Gaia");

                GameObject stamperObj = GameObject.Find("Stamper");
                if (stamperObj == null)
                {
                    stamperObj = new GameObject("Stamper");
                    stamperObj.transform.parent = gaiaObj.transform;
                    stamper = stamperObj.AddComponent<Stamper>();
                    stamper.FitToTerrain();
#if !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
                    stamper.m_resources = m_resources;
                    stamperObj.transform.position = new Vector3(stamper.m_x, stamper.m_y, stamper.m_z);
#endif
                }
                else stamper = stamperObj.GetComponent<Stamper>();

                GameObject scannerObj = GameObject.Find("Scanner");
                if (scannerObj == null)
                {
                    scannerObj = new GameObject("Scanner");
                    scannerObj.transform.parent = gaiaObj.transform;
                    scannerObj.transform.position = TerrainHelper.GetActiveTerrainCenter(false);
                    scanner = scannerObj.AddComponent<Scanner>();

                    string matPath = GetAssetPath("GaiaScannerMaterial");
                    if (!string.IsNullOrEmpty(matPath)) scanner.m_previewMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }
                else scanner = scannerObj.GetComponent<Scanner>();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                phaseComplete = true;
                NextPhase();
            }
        }
    }
}

#endif