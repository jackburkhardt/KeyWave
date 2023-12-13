/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static List<string> availableGrassType;

        private static void GrassUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateGrass = EditorGUILayout.Toggle("Generate grass", prefs.generateGrass);
            if (!prefs.generateGrass)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            SelectGrassEngine();

            if (prefs.grassEngine == "Standard") StandardGrassEngineFields();
            else if (prefs.grassEngine == "Volume Grass") VolumeGrassFields();
            else if (prefs.grassEngine == "Vegetation Studio") VegetationStudioGrassFields();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private static void VegetationStudioGrassFields()
        {
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
#if !VEGETATION_STUDIO_PRO
            prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationPackage), false) as AwesomeTechnologies.VegetationPackage;
#else
            prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationSystem.VegetationPackagePro), false) as AwesomeTechnologies.VegetationSystem.VegetationPackagePro;
#endif
            if (prefs.vegetationStudioGrassTypes == null) prefs.vegetationStudioGrassTypes = new List<int> {1};

            int removeIndex = -1;
            for (int i = 0; i < prefs.vegetationStudioGrassTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefs.vegetationStudioGrassTypes[i] = EditorGUILayout.IntSlider("Vegetation Type " + (i + 1) + ": ", prefs.vegetationStudioGrassTypes[i], 1, 32);
                if (prefs.vegetationStudioGrassTypes.Count > 1 && GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex != -1) prefs.vegetationStudioGrassTypes.RemoveAt(removeIndex);
            if (GUILayout.Button("Add item")) prefs.vegetationStudioGrassTypes.Add(1);
#endif

        }

        private static void VolumeGrassFields()
        {
#if VOLUMEGRASS
            EditorGUILayout.HelpBox("Important: points outside terrains can crash Unity Editor.", MessageType.Info);
            prefs.volumeGrassOutsidePoints = (RealWorldTerrainVolumeGrassOutsidePoints) EditorGUILayout.EnumPopup("Outside Points", prefs.volumeGrassOutsidePoints);
#endif
        }

        private static void StandardGrassEngineFields()
        {
            prefs.grassDensity = EditorGUILayout.IntField("Density (%)", prefs.grassDensity);
            if (prefs.grassDensity < 1) prefs.grassDensity = 1;
            if (prefs.grassPrefabs == null) prefs.grassPrefabs = new List<Texture2D>();

            EditorGUILayout.LabelField("Grass Prefabs");
            for (int i = 0; i < prefs.grassPrefabs.Count; i++)
            {
                prefs.grassPrefabs[i] =
                    (Texture2D)
                    EditorGUILayout.ObjectField(i + 1 + ":", prefs.grassPrefabs[i], typeof(Texture2D), false);
            }

            Texture2D newGrass =
                (Texture2D)
                EditorGUILayout.ObjectField(prefs.grassPrefabs.Count + 1 + ":", null, typeof(Texture2D), false);
            if (newGrass != null) prefs.grassPrefabs.Add(newGrass);
            prefs.grassPrefabs.RemoveAll(go => go == null);
        }

        private static void SelectGrassEngine()
        {
            if (availableGrassType.Count > 1)
            {
                int grassEngineIndex = availableGrassType.IndexOf(prefs.grassEngine);
                if (grassEngineIndex == -1) grassEngineIndex = 0;
                grassEngineIndex = EditorGUILayout.Popup("Grass engine ", grassEngineIndex, availableGrassType.ToArray());
                prefs.grassEngine = availableGrassType[grassEngineIndex];
            }
            else prefs.grassEngine = availableGrassType[0];
        }

        private static void InitGrassEngines()
        {
            availableGrassType = new List<string>();
            availableGrassType.Add("Standard");
#if VOLUMEGRASS
            availableGrassType.Add("Volume Grass");
#endif
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            availableGrassType.Add("Vegetation Studio");
#endif

        }
    }
}