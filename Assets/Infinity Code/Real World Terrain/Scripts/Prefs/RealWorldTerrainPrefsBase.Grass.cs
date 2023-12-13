/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public partial class RealWorldTerrainPrefsBase
    {
        /// <summary>
        /// Density of grass.
        /// </summary>
        public int grassDensity = 100;

        /// <summary>
        /// Grass engine ID.
        /// </summary>
        public string grassEngine;

        /// <summary>
        /// List of grass textures.
        /// </summary>
        public List<Texture2D> grassPrefabs;

        public List<int> vegetationStudioGrassTypes;

        /// <summary>
        /// What to do with outside points for VolumeGrass?
        /// </summary>
        public RealWorldTerrainVolumeGrassOutsidePoints volumeGrassOutsidePoints;
    }
}