/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;

#if PROCEDURAL_TOOLKIT
using ProceduralToolkit.Buildings;
#endif

namespace InfinityCode.RealWorldTerrain
{
    public partial class RealWorldTerrainPrefsBase
    {
        /// <summary>
        /// Building depth below ground level
        /// </summary>
        public float buildingBasementDepth = 0;

        /// <summary>
        /// Where to take height of the bottom points
        /// </summary>
        public RealWorldTerrainBuildingBottomMode buildingBottomMode = RealWorldTerrainBuildingBottomMode.followRealWorldData;

        /// <summary>
        /// Height of the floor.
        /// </summary>
        public float buildingFloorHeight = 3.5f;

        /// <summary>
        /// Range the number of floors buildings.
        /// </summary>
        public RealWorldTerrainRangeI buildingFloorLimits = new RealWorldTerrainRangeI(5, 7, 1, 50);

        /// <summary>
        /// Index of building generator.
        /// 0 - Built-in
        /// 1 - BuildR
        /// 2 - BuildR2
        /// 3 - Instantiate prefabs
        /// </summary>
        public int buildingGenerator = 0;

        /// <summary>
        /// List of buildings materials.
        /// </summary>
        public List<RealWorldTerrainBuildingMaterial> buildingMaterials;

        /// <summary>
        /// List of prefabs to instantiate
        /// </summary>
        public List<RealWorldTerrainBuildingPrefab> buildingPrefabs;

        /// <summary>
        /// Whether to save meshes and materials of buildings in the assets folder.
        /// When false, meshes and materials may be lost when the scene is saved.
        /// </summary>
        public bool buildingSaveInResult = true;

        /// <summary>
        /// TRUE - All buildings will be loaded in one request to OSM Overpass, FALSE - buildings will be loaded in several requests (useful for megacities).
        /// </summary>
        public bool buildingSingleRequest = true;

        /// <summary>
        /// Use colors from OSM?
        /// </summary>
        public bool buildingUseColorTags = false;

        /// <summary>
        /// Type of collider for BuildR buildings.
        /// </summary>
        public RealWorldTerrainBuildR2Collider buildRCollider = RealWorldTerrainBuildR2Collider.none;

        /// <summary>
        /// Generate colliders for BuildR3.
        /// </summary>
        public bool buildR3Collider = false;

        /// <summary>
        /// Render mode for BuildR buildings.
        /// </summary>
        public RealWorldTerrainBuildR2RenderMode buildRRenderMode = RealWorldTerrainBuildR2RenderMode.full;

        public List<RealWorldTerrainBuildR2Material> buildR2Materials;
        public List<RealWorldTerrainBuildR3Material> buildR3Materials;

        /// <summary>
        /// Instance ID of BuildR generator style.
        /// </summary>
        public int customBuildRGeneratorStyle = 0;

        /// <summary>
        /// Instance ID of BuildR generator texture pack.
        /// </summary>
        public int customBuildRGeneratorTexturePack = 0;

        /// <summary>
        /// Array of BuildR presets.
        /// </summary>
        public RealWorldTerrainBuildRPresetsItem[] customBuildRPresets;

        public bool dynamicBuildings = true;

#if PROCEDURAL_TOOLKIT
        public FacadePlanningStrategy ptFacadePlanningStrategy;
        public FacadeConstructionStrategy ptFacadeConstructionStrategy;
        public RoofPlanningStrategy ptRoofPlanningStrategy;
        public RoofConstructionStrategy ptRoofConstructionStrategy;
#endif
    }
}