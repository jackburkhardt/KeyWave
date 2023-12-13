/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public partial class RealWorldTerrainPrefsBase
    {
        public string[] erRoadTypes;

        public bool erGenerateConnection = true;

        /// <summary>
        /// EasyRoads3D SnapToTerrain
        /// </summary>
        public bool erSnapToTerrain = true;

        public float erWidthMultiplier = 1;

        public bool normalizeRoadDistances = true;

        /// <summary>
        /// Name of road engine.
        /// </summary>
        public string roadEngine;

        /// <summary>
        /// Types of roads that will be created.
        /// </summary>
        public RealWorldTerrainRoadType roadTypes = (RealWorldTerrainRoadType)(~0);

        /// <summary>
        /// The mode of generation of road types
        /// </summary>
        public RealWorldTerrainRoadTypeMode roadTypeMode = RealWorldTerrainRoadTypeMode.simple;

        /// <summary>
        /// The material that will be used to SplineBend roads.
        /// </summary>
        public Material splineBendMaterial;

        /// <summary>
        /// The mesh that will be used to SplineBend roads.
        /// </summary>
        public Mesh splineBendMesh;
    }
}