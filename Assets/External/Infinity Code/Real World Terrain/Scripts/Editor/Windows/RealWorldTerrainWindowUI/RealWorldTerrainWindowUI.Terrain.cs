/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        
        private static readonly string[] labelsBaseMapRes = { "16", "32", "64", "128", "256", "512", "1024", "2048" };
        private static readonly int[] valuesBaseMapRes = { 16, 32, 64, 128, 256, 512, 1024, 2048 };
        private static bool showTerrains = true;

        private static void CountTerrainsUI()
        {
            if (prefs.resultType != RealWorldTerrainResultType.terrain && prefs.resultType != RealWorldTerrainResultType.mesh) return;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Count terrains.    X");
            prefs.terrainCount.x = Mathf.Max(EditorGUILayout.IntField(prefs.terrainCount.x), 1);
            GUILayout.Label("Z");
            prefs.terrainCount.y = Mathf.Max(EditorGUILayout.IntField(prefs.terrainCount.y), 1);
            DrawHelpButton("The number of terrains horizontally and vertically (XZ axis).");
            GUILayout.EndHorizontal();
        }

        private static void ElevationRangeUI()
        {
            if (prefs.resultType == RealWorldTerrainResultType.rawFile) return;

            prefs.elevationRange = (RealWorldTerrainElevationRange) EditorGUILayout.EnumPopup("Elevation range", prefs.elevationRange);
            if (prefs.elevationRange == RealWorldTerrainElevationRange.autoDetect)
            {
                prefs.autoDetectElevationOffset.x = EditorGUILayout.FloatField("Min elevation offset", prefs.autoDetectElevationOffset.x);
                prefs.autoDetectElevationOffset.y = EditorGUILayout.FloatField("Max elevation offset", prefs.autoDetectElevationOffset.y);
            }
            else if (prefs.elevationRange == RealWorldTerrainElevationRange.fixedValue)
            {
                prefs.fixedMinElevation = EditorGUILayout.FloatField("Min elevation", prefs.fixedMinElevation);
                prefs.fixedMaxElevation = EditorGUILayout.FloatField("Max elevation", prefs.fixedMaxElevation);
            }
            else if (prefs.elevationRange == RealWorldTerrainElevationRange.realWorldValue)
            {
                EditorGUILayout.HelpBox("When using Elevation range - Real World Value, you can have steps in the hills.\nThis is due to the way the Unity Terrain Engine stores elevation data.\nUse Elevation range - Real World Value only for areas that have a very wide range of heights, and when you are sure that you really need it!", MessageType.Warning);
            }
        }

        private static void GaiaStampFields()
        {
#if !GAIA_PRESENT && !GAIA_PRO_PRESENT && !GAIA_2_PRESENT
            EditorGUILayout.HelpBox("Gaia not found. Import Gaia into the project.", MessageType.Error);
#endif
            prefs.gaiaStampResolution = EditorGUILayout.IntField("Stamp Resolution", prefs.gaiaStampResolution);
        }

        private static void GenerateUnderwaterUI()
        {
            prefs.generateUnderWater = EditorGUILayout.Toggle("Generate Underwater", prefs.generateUnderWater);

            if (!prefs.generateUnderWater) return;

            EditorGUI.indentLevel++;
            int nodata = IntField(
                "Max Underwater Depth",
                prefs.nodataValue,
                "SRTM v4.1 does not contain data on the underwater depths. Real World Terrain generates it by closest known areas of land. \nSpecify a value relative to sea level. \nFor example, if you want to get a depth of 200 meters, set the value \"-200\"."
            );
            if (nodata < short.MinValue) nodata = short.MinValue;
            if (nodata > short.MaxValue) nodata = short.MaxValue;
            prefs.depthSharpness = FloatField(
                "Depth Sharpness",
                prefs.depthSharpness,
                "Escarpment of the seabed. \nGreater value - steeper slope.\nUnknown Value = Average Neighbor Known Values - Depth Sharpness."
            );
            if (prefs.depthSharpness < 0) prefs.depthSharpness = 0;
            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps) prefs.bingMapsUseZeroAsUnknown = EditorGUILayout.Toggle("Zero Elevation is Unknown", prefs.bingMapsUseZeroAsUnknown);
            prefs.nodataValue = (short) nodata;
            prefs.waterDetectionTexture = ObjectField("Water Detection Texture", prefs.waterDetectionTexture, typeof(Texture2D), "The texture of the current area where the water is white and the rest is non-white.\nThe coordinates of the borders of the texture must match the coordinates of the borders of the generated area.\nMust have Read / Write Enabled - ON.") as Texture2D;
            EditorGUI.indentLevel--;
        }

        private static void RawFileFields()
        {
            EditorGUILayout.BeginHorizontal();
            prefs.rawFilename = EditorGUILayout.TextField("Filename", prefs.rawFilename);
            if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
            {
                GUI.FocusControl(null);
                string ext = prefs.rawType == RealWorldTerrainRawType.RAW ? "raw" : "png";
                string rawFilename = EditorUtility.SaveFilePanel("Output RAW filename", Application.dataPath, "terrain." + ext, ext);
                if (!string.IsNullOrEmpty(rawFilename)) prefs.rawFilename = rawFilename;
            }

            EditorGUILayout.EndHorizontal();
            prefs.rawType = (RealWorldTerrainRawType) EditorGUILayout.EnumPopup("Type", prefs.rawType);
            if (prefs.rawType == RealWorldTerrainRawType.RAW) prefs.rawByteOrder = (RealWorldTerrainByteOrder) EditorGUILayout.EnumPopup("Byte order", prefs.rawByteOrder);
            prefs.rawWidth = EditorGUILayout.IntField("Width", prefs.rawWidth);
            prefs.rawHeight = EditorGUILayout.IntField("Height", prefs.rawHeight);
        }

        private static void TerrainDataSettings()
        {
            if (values2n1.All(v => v != prefs.heightmapResolution))
            {
                prefs.heightmapResolution = Mathf.ClosestPowerOfTwo(prefs.heightmapResolution) + 1;
                if (values2n1.All(v => v != prefs.heightmapResolution)) prefs.heightmapResolution = 129;
            }

            prefs.heightmapResolution =
                IntPopup("Heightmap Resolution", prefs.heightmapResolution, labels2n1, values2n1,
                    "The Heightmap resolution for each Terrain.", "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            prefs.detailResolution = IntField("Detail Resolution", prefs.detailResolution,
                "The resolution of the map that controls grass and detail meshes. For performance reasons (to save on draw calls) the lower you set this number the better.",
                "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            prefs.detailResolution = Mathf.Clamp(prefs.detailResolution, 32, 4096);

            prefs.resolutionPerPatch = IntField("Resolution Per Patch", prefs.resolutionPerPatch, 
                "Specifies the size in pixels of each individually rendered detail patch. A larger number reduces draw calls, but might increase triangle count since detail patches are culled on a per batch basis. A recommended value is 16. If you use a very large detail object distance and your grass is very sparse, it makes sense to increase the value.",
                "http://docs.unity3d.com/Documentation/ScriptReference/TerrainData.SetDetailResolution.html");
            prefs.resolutionPerPatch = Mathf.Clamp(prefs.resolutionPerPatch, 8, 1000);

            if (prefs.detailResolution % prefs.resolutionPerPatch != 0)
            {
                prefs.detailResolution = prefs.detailResolution / prefs.resolutionPerPatch * prefs.resolutionPerPatch;
                if (prefs.detailResolution < 32) prefs.detailResolution = 32;
            }

            prefs.controlTextureResolution = IntPopup("Control Texture Resolution", prefs.controlTextureResolution, labels2n, values2n,
                "Resolution of the splatmap that controls the blending of the different terrain textures.",
                "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            prefs.baseMapResolution = IntPopup("Base Map Resolution", prefs.baseMapResolution, labelsBaseMapRes, valuesBaseMapRes,
                "The resolution of the composite texture that is used in place of the splat map at certain distances.",
                "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            if (!valuesBaseMapRes.Contains(prefs.baseMapResolution)) prefs.baseMapResolution = 1024;
        }

        private static void TerrainFullFields()
        {
            prefs.resultType = (RealWorldTerrainResultType)EnumPopup("Result", prefs.resultType, "The type of the generated object.");

            CountTerrainsUI();

            prefs.alignWaterLine = Toggle("Align Water Line", prefs.alignWaterLine, false,
                "If disabled terrains will have zero Y at minimum elevation.\nIf enabled, terrains will have zero Y on the water line(zero elevation).");

            if (prefs.resultType == RealWorldTerrainResultType.gaiaStamp) GaiaStampFields();
            else if (prefs.resultType == RealWorldTerrainResultType.rawFile) RawFileFields();
            else TerrainSizeTypeUI();

            ElevationRangeUI();
        }

        private static void TerrainSizeTypeUI()
        {
            prefs.sizeType = IntPopup(
                "Size type",
                prefs.sizeType, new[] {"Real world sizes", "Mercator sizes", "Fixed size"},
                new[] {0, 1, 2},
                "Specifies whether the projection will be determined by the size of the area.",
                new[] {"http://en.wikipedia.org/wiki/Cylindrical_equal-area_projection", "http://en.wikipedia.org/wiki/Mercator_projection"}
            );

            if (prefs.sizeType == 2)
            {
                prefs.fixedTerrainSize.x = FloatField("Terrain Width", prefs.fixedTerrainSize.x, "Width of each terrain (units)");
                prefs.fixedTerrainSize.z = FloatField("Terrain Length", prefs.fixedTerrainSize.z, "Length of each terrain (units)");
                prefs.terrainScale.y = FloatField("Scale Y", prefs.terrainScale.y, "The scale of the elevations.");
            }
            else prefs.terrainScale = Vector3Field("Scale", prefs.terrainScale, "The scale of the resulting objects.");
        }

        private static void TerrainUI()
        {
            if (generateType == RealWorldTerrainGenerateType.full) TerrainFullFields();

            if (prefs.resultType == RealWorldTerrainResultType.terrain) GenerateUnderwaterUI();
            else prefs.generateUnderWater = false;


            if (prefs.resultType == RealWorldTerrainResultType.terrain) TerrainDataSettings();
            else if (prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                prefs.heightmapResolution =
                    RealWorldTerrainUtils.Limit(IntField("Height Map Resolution", prefs.heightmapResolution,
                        "Heightmap resolution for each mesh."), 8, 250);
            }

            GUILayout.Space(10);
        }
    }
}