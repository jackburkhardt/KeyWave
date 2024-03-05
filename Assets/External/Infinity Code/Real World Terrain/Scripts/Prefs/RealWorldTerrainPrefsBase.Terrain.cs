/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public partial class RealWorldTerrainPrefsBase
    {
        /// <summary>
        /// If disabled terrains will have zero Y at minimum elevation. <br/>
        /// If enabled, terrains will have zero Y on the water line(zero elevation).
        /// </summary>
        public bool alignWaterLine;

        public Vector2 autoDetectElevationOffset = new Vector2(100, 100);

        /// <summary>
        /// Resolution of the base map used for rendering far patches on the terrain.
        /// </summary>
        public int baseMapResolution = 1024;

        /// <summary>
        /// Flag indicating that the zero is used as unknown value.
        /// </summary>
        public bool bingMapsUseZeroAsUnknown;

        /// <summary>
        /// Resolution of control texture.
        /// </summary>
        public int controlTextureResolution = 512;

        /// <summary>
        /// Escarpment of the seabed. Greater value - steeper slope.
        /// </summary>
        public float depthSharpness = 0;

        /// <summary>
        /// The resolution of the map that controls grass and detail meshes.<br/>
        /// For performance reasons (to save on draw calls) the lower you set this number the better.
        /// </summary>
        public int detailResolution = 2048;

        /// <summary>
        /// Elevation provider
        /// </summary>
        public RealWorldTerrainElevationProvider elevationProvider = RealWorldTerrainElevationProvider.SRTM;

        public RealWorldTerrainElevationRange elevationRange = RealWorldTerrainElevationRange.autoDetect;

        public RealWorldTerrainElevationType elevationType = RealWorldTerrainElevationType.realWorld;

        /// <summary>
        /// The fixed size of terrain.<br/>
        /// X - Terrain Width<br/>
        /// Y - Terrain Height<br/>
        /// Z - Terrain Length
        /// </summary>
        public Vector3 fixedTerrainSize = new Vector3(500, 600, 500);

        /// <summary>
        /// The resolution of GAIA stamp
        /// </summary>
        public int gaiaStampResolution = 1024;

        /// <summary>
        /// Generate unknown underwater areas based on known data
        /// </summary>
        public bool generateUnderWater;

        /// <summary>
        /// The HeightMap resolution for each Terrain.
        /// </summary>
        public int heightmapResolution = 129;

        /// <summary>
        /// Errors of SRTM should be ignored?
        /// </summary>
        public bool ignoreSRTMErrors;

        public float fixedMaxElevation = 1000;
        public float fixedMinElevation = 0;


        /// <summary>
        /// Type of max elevation value.
        /// </summary>
        public RealWorldTerrainMaxElevation maxElevationType = RealWorldTerrainMaxElevation.autoDetect;

        /// <summary>
        /// Elevation value when there is no data.
        /// </summary>
        public short nodataValue;

        /// <summary>
        /// The order of bytes in a RAW file.
        /// </summary>
        public RealWorldTerrainByteOrder rawByteOrder = RealWorldTerrainByteOrder.Windows;

        /// <summary>
        /// Filename of RAW result
        /// </summary>
        public string rawFilename = "terrain";

        /// <summary>
        /// Height of RAW result
        /// </summary>
        public int rawHeight = 1024;

        /// <summary>
        /// Width of RAW result
        /// </summary>
        public int rawWidth = 1024;

        /// <summary>
        /// Type of RAW result
        /// </summary>
        public RealWorldTerrainRawType rawType = RealWorldTerrainRawType.RAW;

        /// <summary>
        /// Specifies the size in pixels of each individually rendered detail patch. <br/>
        /// A larger number reduces draw calls, but might increase triangle count since detail patches are culled on a per batch basis. <br/>
        /// A recommended value is 16. <br/>
        /// If you use a very large detail object distance and your grass is very sparse, it makes sense to increase the value.
        /// </summary>
        public int resolutionPerPatch = 16;

        /// <summary>
        /// Type of result (terrain, mesh).
        /// </summary>
        public RealWorldTerrainResultType resultType = RealWorldTerrainResultType.terrain;

        /// <summary>
        /// Specifies whether the projection will be determined by the size of the area.<br/>
        /// 0 - Real world sizes.<br/>
        /// 1 - Mercator sizes.<br/>
        /// 2 - Fixed size.
        /// </summary>
        public int sizeType = 0;

        /// <summary>
        /// Scale of terrains.
        /// </summary>
        public Vector3 terrainScale = Vector3.one;

        public Texture2D waterDetectionTexture;
    }
}