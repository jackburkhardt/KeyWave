/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using InfinityCode.RealWorldTerrain.Generators;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static List<string> availableRiverEngines;
        private static string[] availableRiverEnginesArr;

        private static void InitRiverEngines()
        {
            availableRiverEngines = new List<string> {
                RealWorldTerrainRiverGenerator.BUILTIN_RIVER_ENGINE,
#if RAM2019
                RealWorldTerrainRiverGenerator.RAM2019_RIVER_ENGINE
#endif
            };

            availableRiverEnginesArr = availableRiverEngines.ToArray();
        }

        private static void RiversUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateRivers = EditorGUILayout.Toggle("Generate rivers", prefs.generateRivers);

            if (!prefs.generateRivers)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            int riverEngineIndex = availableRiverEngines.IndexOf(prefs.riverEngine);
            if (riverEngineIndex == -1)
            {
                riverEngineIndex = 0;
                prefs.riverEngine = availableRiverEnginesArr[0];
            }
            int ri = EditorGUILayout.Popup("River Engine", riverEngineIndex, availableRiverEnginesArr);
            if (ri != riverEngineIndex) prefs.riverEngine = availableRiverEnginesArr[ri];

            if (prefs.riverEngine == RealWorldTerrainRiverGenerator.BUILTIN_RIVER_ENGINE)
            {
                prefs.riverMaterial = EditorGUILayout.ObjectField("Material", prefs.riverMaterial, typeof(Material), false) as Material;
            }
            else if (prefs.riverEngine == RealWorldTerrainRiverGenerator.RAM2019_RIVER_ENGINE)
            {
#if RAM2019
                prefs.ramAreaProfile = EditorGUILayout.ObjectField("Area Profile", prefs.ramAreaProfile, typeof(LakePolygonProfile), false) as LakePolygonProfile;
                prefs.ramSplineProfile = EditorGUILayout.ObjectField("Spline Profile", prefs.ramSplineProfile, typeof(SplineProfile), false) as SplineProfile;
#endif
            }

            EditorGUILayout.EndVertical();
        }
    }
}