/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public partial class RealWorldTerrainPrefsBase
    {
        /// <summary>
        /// Density of trees.
        /// </summary>
        public int treeDensity = 100;

        public string treeEngine;

        /// <summary>
        /// List of tree prefabs.
        /// </summary>
        public List<GameObject> treePrefabs;

        public List<int> vegetationStudioTreeTypes;

    }
}