/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static List<string> availableTreeType;

        private static void StandardTreeEngineFields()
        {
            prefs.treeDensity = EditorGUILayout.IntField("Density (%)", prefs.treeDensity);
            if (prefs.treeDensity < 1) prefs.treeDensity = 1;
            if (prefs.treePrefabs == null) prefs.treePrefabs = new List<GameObject>();
            EditorGUILayout.LabelField("Tree Prefabs");
            for (int i = 0; i < prefs.treePrefabs.Count; i++)
            {
                prefs.treePrefabs[i] =
                    (GameObject)
                    EditorGUILayout.ObjectField(i + 1 + ":", prefs.treePrefabs[i], typeof(GameObject), false);
            }

            GameObject newTree =
                (GameObject)
                EditorGUILayout.ObjectField(prefs.treePrefabs.Count + 1 + ":", null, typeof(GameObject), false);
            if (newTree != null) prefs.treePrefabs.Add(newTree);
            prefs.treePrefabs.RemoveAll(go => go == null);
        }

        private static void VegetationStudioTreeEngineFields()
        {
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
#if !VEGETATION_STUDIO_PRO
            prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationPackage), false) as AwesomeTechnologies.VegetationPackage;
#else
            prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationSystem.VegetationPackagePro), false) as AwesomeTechnologies.VegetationSystem.VegetationPackagePro;
#endif
            if (prefs.vegetationStudioTreeTypes == null) prefs.vegetationStudioTreeTypes = new List<int>{1};

            int removeIndex = -1;
            for (int i = 0; i < prefs.vegetationStudioTreeTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefs.vegetationStudioTreeTypes[i] = EditorGUILayout.IntSlider("Vegetation Type " + (i + 1) + ": ", prefs.vegetationStudioTreeTypes[i], 1, 32);
                if (prefs.vegetationStudioTreeTypes.Count > 1 && GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex != -1) prefs.vegetationStudioTreeTypes.RemoveAt(removeIndex);
            if (GUILayout.Button("Add item")) prefs.vegetationStudioTreeTypes.Add(1);
#endif
        }

        private static void InitTreeEngines()
        {
            availableTreeType = new List<string>();
            availableTreeType.Add("Standard");
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            availableTreeType.Add("Vegetation Studio");
#endif
        }

        private static void SelectTreeEngine()
        {
            if (availableTreeType.Count > 1)
            {
                int treeEngineIndex = availableTreeType.IndexOf(prefs.treeEngine);
                if (treeEngineIndex == -1) treeEngineIndex = 0;
                treeEngineIndex = EditorGUILayout.Popup("Tree engine ", treeEngineIndex, availableTreeType.ToArray());
                prefs.treeEngine = availableTreeType[treeEngineIndex];
            }
            else prefs.treeEngine = availableTreeType[0];
        }

        private static void TreesUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateTrees = EditorGUILayout.Toggle("Generate trees", prefs.generateTrees);

            if (!prefs.generateTrees)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            SelectTreeEngine();

            if (prefs.treeEngine == "Standard") StandardTreeEngineFields();
            else if (prefs.treeEngine == "Vegetation Studio") VegetationStudioTreeEngineFields();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
    }
}