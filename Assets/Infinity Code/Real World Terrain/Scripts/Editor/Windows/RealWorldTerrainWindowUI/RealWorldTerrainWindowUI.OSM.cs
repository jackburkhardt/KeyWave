/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static void OsmUI()
        {
            EditorGUIUtility.labelWidth = LabelWidth + 20;

            if (RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.additional)
            {
                prefs.elevationType = (RealWorldTerrainElevationType)EditorGUILayout.EnumPopup("Elevation", prefs.elevationType);
            }
            else prefs.elevationType = RealWorldTerrainElevationType.realWorld;


            BuildingsUI();
            RoadsUI();
            RiversUI();

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                TreesUI();
                GrassUI();
            }
            else
            {
                prefs.generateGrass = false;
                prefs.generateTrees = false;
            }

            EditorGUIUtility.labelWidth = LabelWidth;
        }
    }
}