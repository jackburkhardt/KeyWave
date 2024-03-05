/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public partial class RealWorldTerrainPrefsBase
    {
        public int hugeTexturePageSize = 2048;
        public int hugeTextureRows = 13;
        public int hugeTextureCols = 13;

        /// <summary>
        /// Texture type ID.
        /// </summary>
        public string mapTypeID;

        /// <summary>
        /// Texture type extra fields.
        /// </summary>
        public string mapTypeExtraFields;

        /// <summary>
        /// The maximum level of zoom, to be used for texture generation.\n
        /// 0 - Autodetect.\n
        /// 1+ - Level of zoom.
        /// </summary>
        public int maxTextureLevel;

        /// <summary>
        /// Reducing the size of the texture, reduces the time texture generation and memory usage.
        /// </summary>
        public bool reduceTextures = true;

        /// <summary>
        /// Count of textures.
        /// </summary>
        public RealWorldTerrainVector2i textureCount = RealWorldTerrainVector2i.one;

        /// <summary>
        /// Type of texture file output
        /// </summary>
        public RealWorldTerrainTextureFileType textureFileType = RealWorldTerrainTextureFileType.jpg;

        /// <summary>
        /// Quality of file output
        /// </summary>
        public int textureFileQuality = 100;

        /// <summary>
        /// Provider of textures.
        /// </summary>
        public RealWorldTerrainTextureProvider textureProvider = RealWorldTerrainTextureProvider.virtualEarth;

        /// <summary>
        /// URL pattern of custom texture provider.
        /// </summary>
        public string textureProviderURL = "http://localhost/tiles/{zoom}/{x}/{y}";

        /// <summary>
        /// Size of texture.
        /// </summary>
        public RealWorldTerrainVector2i textureSize = new RealWorldTerrainVector2i(1024, 1024);

        /// <summary>
        /// Use mip-mapping for textures (not recommended)
        /// </summary>
        public bool textureMipMaps = false;

        /// <summary>
        /// Type of result texture
        /// </summary>
        public RealWorldTerrainTextureResultType textureResultType = RealWorldTerrainTextureResultType.regularTexture;

        /// <summary>
        /// Type of tile texture.
        /// </summary>
        public RealWorldTerrainTextureType textureType = RealWorldTerrainTextureType.satellite;

        public List<TerrainLayer> vectorTerrainBaseLayers;
        public Vector2 vectorTerrainBaseLayersNoiseOffset = Vector2.zero;
        public float vectorTerrainBaseLayersNoiseScale = 16;
        public List<RealWorldTerrainVectorTerrainLayerFeature> vectorTerrainLayers;
    }
}