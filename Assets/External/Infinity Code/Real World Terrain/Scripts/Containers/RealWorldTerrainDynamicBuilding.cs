/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    [ExecuteInEditMode]
    public class RealWorldTerrainDynamicBuilding : RealWorldTerrainBuilding
    {
        private void Awake()
        {
            if (baseVertices != null) Generate();
        }
    }
}