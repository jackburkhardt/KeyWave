/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// Building material class.
    /// </summary>
    [Serializable]
    public class RealWorldTerrainBuildingMaterial
    {
        /// <summary>
        /// Roof material.
        /// </summary>
        public Material roof;

        /// <summary>
        /// Wall material.
        /// </summary>
        public Material wall;

        //Size of a tile texture in meters.
        public Vector2 tileSize = new Vector2(30, 30);

        public RealWorldTerrainBuildingMaterial()
        {

        }
    }
}