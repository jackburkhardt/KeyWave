/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;


namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// Basic settings generation of terrain.
    /// </summary>
    [Serializable]
    public partial class RealWorldTerrainPrefsBase
    {
        /// <summary>
        /// List of points of interest.
        /// </summary>
        public List<RealWorldTerrainPOI> POI;

        public string riverEngine = "Built-In";

        public Material riverMaterial;

#if RAM2019
        public LakePolygonProfile ramAreaProfile;
        public SplineProfile ramSplineProfile;
#endif

        /// <summary>
        /// Title
        /// </summary>
        public string title;

#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
#if !VEGETATION_STUDIO_PRO
        public AwesomeTechnologies.VegetationPackage vegetationStudioPackage;
#else
        public AwesomeTechnologies.VegetationSystem.VegetationPackagePro vegetationStudioPackage;
#endif
#endif
    }
}