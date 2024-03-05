/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if BUILDR2
using BuildR2;
#endif

#if BUILDR3
using BuildRCities;
#endif

#if PROCEDURAL_TOOLKIT
using ProceduralToolkit.Buildings;
#endif

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static bool showBuildRCustomPresets;
        private static List<string> availableBuildingEngine;
        private static List<int> availableBuildingsIDs;

        private static void BuildR2Fields()
        {
#if BUILDR2
            prefs.buildRCollider = (RealWorldTerrainBuildR2Collider)EditorGUILayout.EnumPopup("Collider", prefs.buildRCollider);
            prefs.buildRRenderMode = (RealWorldTerrainBuildR2RenderMode)EditorGUILayout.EnumPopup("Render Mode", prefs.buildRRenderMode);

            if (prefs.buildR2Materials == null) prefs.buildR2Materials = new List<RealWorldTerrainBuildR2Material>{new RealWorldTerrainBuildR2Material()};
            else if (prefs.buildR2Materials.Count == 0)
            {
                prefs.buildR2Materials.Add(new RealWorldTerrainBuildR2Material());
            }

            EditorGUILayout.LabelField("Surfaces & Facades");

            int removeIndex = -1;

            for (int i = 0; i < prefs.buildR2Materials.Count; i++)
            {
                RealWorldTerrainBuildR2Material material = prefs.buildR2Materials[i];
                EditorGUILayout.BeginHorizontal(i % 2 == 0 ? evenStyle : oddStyle);

                EditorGUILayout.LabelField(i + 1 + ": ", GUILayout.Width(30));
                EditorGUILayout.BeginVertical();

                material.roofSurface = EditorGUILayout.ObjectField(new GUIContent("Roof Surface:"), material.roofSurface, typeof(Surface), false) as Surface;
                material.roofType = (Roof.Types)EditorGUILayout.EnumPopup("Roof Type:", material.roofType);

                if (material.facades == null) material.facades = new List<Facade>();

                EditorGUILayout.LabelField("Facades:");
                int removeFacadeIndex = -1;

                for (int j = 0; j < material.facades.Count; j++)
                {
                    EditorGUI.BeginChangeCheck();
                    material.facades[j] = EditorGUILayout.ObjectField(material.facades[j], typeof(Facade), false) as Facade;
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (material.facades[j] == null) removeFacadeIndex = j;
                    }
                }

                if (removeFacadeIndex != -1) material.facades.RemoveAt(removeFacadeIndex);

                Facade newFacade = EditorGUILayout.ObjectField(null, typeof(Facade), false) as Facade;
                if (newFacade != null) material.facades.Add(newFacade);

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex != -1) prefs.buildR2Materials.RemoveAt(removeIndex);
            if (GUILayout.Button("Add new item"))
            {
                prefs.buildR2Materials.Add(new RealWorldTerrainBuildR2Material());
            }
#endif
        }

        private static void BuildR3Fields()
        {
#if BUILDR3
            if (prefs.buildR3Materials == null) prefs.buildR3Materials = new List<RealWorldTerrainBuildR3Material> {new RealWorldTerrainBuildR3Material()};
            else if (prefs.buildR3Materials.Count == 0)
            {
                prefs.buildR3Materials.Add(new RealWorldTerrainBuildR3Material());
            }

            prefs.buildR3Collider = EditorGUILayout.Toggle("Generate Colliders", prefs.buildR3Collider);

            EditorGUILayout.LabelField("Surfaces & Facades");

            int removeIndex = -1;

            for (int i = 0; i < prefs.buildR3Materials.Count; i++)
            {
                RealWorldTerrainBuildR3Material material = prefs.buildR3Materials[i];
                EditorGUILayout.BeginHorizontal(i % 2 == 0 ? evenStyle : oddStyle);

                EditorGUILayout.LabelField(i + 1 + ": ", GUILayout.Width(30));
                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField("Facades:");

                material.wallFacade = EditorGUILayout.ObjectField("Wall:", material.wallFacade, typeof(FacadeAsset), false) as FacadeAsset;
                material.roofTexture = EditorGUILayout.ObjectField("Roof:", material.roofTexture, typeof(DynamicTextureAsset), false) as DynamicTextureAsset;
                material.roofType = (Roof.Types) EditorGUILayout.EnumPopup("Roof Type:", material.roofType);

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex != -1) prefs.buildR3Materials.RemoveAt(removeIndex);
            if (GUILayout.Button("Add new item"))
            {
                prefs.buildR3Materials.Add(new RealWorldTerrainBuildR3Material());
            }
#endif
        }

        private static void BuildingsUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateBuildings = EditorGUILayout.Toggle("Generate buildings", prefs.generateBuildings);

            if (!prefs.generateBuildings)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            prefs.buildingGenerator = EditorGUILayout.IntPopup("Building generator", prefs.buildingGenerator, availableBuildingEngine.ToArray(), availableBuildingsIDs.ToArray());

            prefs.buildingSingleRequest = Toggle("Download using a single request", prefs.buildingSingleRequest, false, "If enabled, one request will be sent for the entire area.\nIf disabled, the request will be split into small parts, which will make downloading data for megacities more stable.");

            if (prefs.buildingGenerator == 2) BuildR2Fields();
            else if (prefs.buildingGenerator == 5) BuildR3Fields();
            else if (prefs.buildingGenerator == 3) InstantiatePrefabsFields();
            else if (prefs.buildingGenerator == 4) ProceduralToolkitBuildingsFields();

            RealWorldTerrainRangeI range = prefs.buildingFloorLimits;
            float minLevelLimit = range.min;
            float maxLevelLimit = range.max;
            MinMaxSlider(string.Format("Floors if unknown ({0}-{1})", range.min, range.max), 
                ref minLevelLimit, ref maxLevelLimit, 1, 50,
                "If no building height or number of floors is specified in the OSM data, a random value for floors in this range will be used.");
            range.Set(minLevelLimit, maxLevelLimit);

            if (prefs.buildingGenerator == 0) BuiltInBuildingEngineFields();

            EditorGUILayout.EndVertical();
        }

        private static void BuiltInBuildingEngineFields()
        {
            prefs.buildingFloorHeight = EditorGUILayout.FloatField("Floor Height", prefs.buildingFloorHeight);
            prefs.buildingBottomMode = (RealWorldTerrainBuildingBottomMode)EditorGUILayout.EnumPopup("Bottom Mode", prefs.buildingBottomMode);
            prefs.buildingBasementDepth = FloatField("Basement Depth", prefs.buildingBasementDepth, "Building depth below ground level in meters (Zero or positive number).");
            prefs.buildingUseColorTags = Toggle("Use Color Tags", prefs.buildingUseColorTags, false, "Whether to use color tag from OSM data? If so, a new material will be created for each building with a color.");
            prefs.dynamicBuildings = Toggle("Dynamic Buildings", prefs.dynamicBuildings, false, "Dynamic buildings are generated at runtime and are not saved as meshes, thus bypassing the meshes import phase.");
            prefs.buildingSaveInResult = Toggle("Save In Result", prefs.buildingSaveInResult, false, "Should meshes and materials be saved in the project? If disabled, you may lose non-dynamic meshes and materials when resetting the scene state (saving / loading, compiling scripts, etc.).");

            GUILayout.Label("Building Materials");
            if (prefs.buildingMaterials == null) prefs.buildingMaterials = new List<RealWorldTerrainBuildingMaterial>();
            for (int i = 0; i < prefs.buildingMaterials.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label((i + 1).ToString(), GUILayout.ExpandWidth(false));

                EditorGUILayout.BeginVertical();

                RealWorldTerrainBuildingMaterial material = prefs.buildingMaterials[i];
                material.wall = EditorGUILayout.ObjectField("Wall material", material.wall, typeof(Material), false) as Material;
                material.roof = EditorGUILayout.ObjectField("Roof material", material.roof, typeof(Material), false) as Material;
                material.tileSize = EditorGUILayout.Vector2Field("Tile Size (meters)", material.tileSize);

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                {
                    prefs.buildingMaterials[i] = null;
                }

                EditorGUILayout.EndHorizontal();
            }

            prefs.buildingMaterials.RemoveAll(m => m == null);

            if (GUILayout.Button("Add material")) prefs.buildingMaterials.Add(new RealWorldTerrainBuildingMaterial());
        }

        private static void InitBuildingEngines()
        {
            availableBuildingEngine = new List<string> { "Built-in", "Instantiate Prefabs" };
            availableBuildingsIDs = new List<int> { 0, 3 };
#if BUILDR2
            availableBuildingEngine.Add("BuildR2");
            availableBuildingsIDs.Add(2);
#endif
#if BUILDR3
            availableBuildingEngine.Add("BuildR3");
            availableBuildingsIDs.Add(5);
#endif
#if PROCEDURAL_TOOLKIT
            availableBuildingEngine.Add("Procedural Toolkit");
            availableBuildingsIDs.Add(4);
#endif
        }

        private static void InstantiatePrefabsFields()
        {
            EditorGUILayout.HelpBox("If you use highly detailed buildings, when generating areas that contain a lot of buildings (for example, cities), Unity Editor can crashes.", MessageType.Info);
            EditorGUILayout.HelpBox("Prefab must contain a BoxCollider so that RWT can determine the boundaries of the building.", MessageType.Info);

            if (prefs.buildingPrefabs == null) prefs.buildingPrefabs = new List<RealWorldTerrainBuildingPrefab>();

            int removeIndex = -1;

            for (int i = 0; i < prefs.buildingPrefabs.Count; i++)
            {
                RealWorldTerrainBuildingPrefab b = prefs.buildingPrefabs[i];
                EditorGUILayout.BeginHorizontal(i % 2 == 0 ? evenStyle : oddStyle);

                EditorGUILayout.LabelField(i + 1 + ": ", GUILayout.Width(30));
                EditorGUILayout.BeginVertical();

                b.prefab = EditorGUILayout.ObjectField("Prefab", b.prefab, typeof(GameObject), false) as GameObject;
                b.sizeMode = (RealWorldTerrainBuildingPrefab.SizeMode) EditorGUILayout.EnumPopup("Size Mode", b.sizeMode);
                b.heightMode = (RealWorldTerrainBuildingPrefab.HeightMode) EditorGUILayout.EnumPopup("Height Mode", b.heightMode);

                if (b.heightMode == RealWorldTerrainBuildingPrefab.HeightMode.fixedHeight)
                {
                    b.fixedHeight = EditorGUILayout.FloatField("Height", b.fixedHeight);
                }

                b.placementMode = (RealWorldTerrainBuildingPrefab.PlacementMode)EditorGUILayout.EnumPopup("Placement Mode", b.placementMode);

                if (b.tags == null) b.tags = new List<RealWorldTerrainBuildingPrefab.OSMTag>();

                EditorGUILayout.LabelField("Tags (if no tags, prefab can be used for all buildings)");

                int tagRemoveIndex = -1;

                for (int j = 0; j < b.tags.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal(j % 2 == 0 ? evenStyle : oddStyle);

                    EditorGUILayout.LabelField(j + 1 + ": ", GUILayout.Width(30));
                    EditorGUILayout.BeginVertical();

                    RealWorldTerrainBuildingPrefab.OSMTag t = b.tags[j];
                    t.key = EditorGUILayout.TextField("Key", t.key);
                    t.value = EditorGUILayout.TextField("Value", t.value);

                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) tagRemoveIndex = j;

                    EditorGUILayout.EndHorizontal();
                }

                if (tagRemoveIndex != -1) b.tags.RemoveAt(tagRemoveIndex);
                if (GUILayout.Button("Add tag"))
                {
                    b.tags.Add(new RealWorldTerrainBuildingPrefab.OSMTag());
                }

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex != -1) prefs.buildingPrefabs.RemoveAt(removeIndex);
            if (GUILayout.Button("Add new item"))
            {
                RealWorldTerrainBuildingPrefab newPrefab = new RealWorldTerrainBuildingPrefab();
                newPrefab.tags = new List<RealWorldTerrainBuildingPrefab.OSMTag>();
                prefs.buildingPrefabs.Add(newPrefab);
            }
        }

        private static void ProceduralToolkitBuildingsFields()
        {
#if PROCEDURAL_TOOLKIT
            prefs.ptFacadePlanningStrategy = EditorGUILayout.ObjectField("Facade Planning Strategy", prefs.ptFacadePlanningStrategy, typeof(FacadePlanningStrategy), false) as FacadePlanningStrategy;
            prefs.ptFacadeConstructionStrategy = EditorGUILayout.ObjectField("Facade Construction Strategy", prefs.ptFacadeConstructionStrategy, typeof(FacadeConstructionStrategy), false) as FacadeConstructionStrategy;
            prefs.ptRoofPlanningStrategy = EditorGUILayout.ObjectField("Roof Planning Strategy", prefs.ptRoofPlanningStrategy, typeof(RoofPlanningStrategy), false) as RoofPlanningStrategy;
            prefs.ptRoofConstructionStrategy = EditorGUILayout.ObjectField("Roof Construction Strategy", prefs.ptRoofConstructionStrategy, typeof(RoofConstructionStrategy), false) as RoofConstructionStrategy;
#endif
        }
    }
}