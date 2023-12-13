/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainSeamsFixer : EditorWindow
    {
        private bool createUndo = true;
        private Terrain terrain1;
        private Terrain terrain2;
        private Vector2 scrollPosition;

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Seams Fixer")]
        public static void CreateWizard()
        {
            GetWindow<RealWorldTerrainSeamsFixer>("Seams Fixer");
        }

        private void FixHeights(float[,] h1, float[,] h2)
        {
            for (int i = 0; i < h1.GetLength(0); i++)
            {
                for (int j = 0; j < h1.GetLength(1); j++)
                {
                    h1[i, j] = (h1[i, j] + h2[i, j]) / 2;
                }
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.HelpBox(@"A tool for repairing seams between adjacent Terrains.
Requirements:
1.Terrains must be located in the correct position to be neighbors.
2.Terrains must have the same heightmap resolution.", MessageType.Info);

            terrain1 = EditorGUILayout.ObjectField("Terrain 1", terrain1, typeof(Terrain), true) as Terrain;
            terrain2 = EditorGUILayout.ObjectField("Terrain 2", terrain2, typeof(Terrain), true) as Terrain;
            createUndo = EditorGUILayout.ToggleLeft("Create Undo (May take a long time)", createUndo);
            EditorGUILayout.EndScrollView();
            EditorGUI.BeginDisabledGroup(terrain1 == null || terrain2 == null);
            if (GUILayout.Button("Fix"))
            {
                OnFix();
            }
            EditorGUI.EndDisabledGroup();
        }

        public void OnFix()
        {
            if (terrain1 == terrain2)
            {
                EditorUtility.DisplayDialog("Error", "You have selected the same terrain.", "OK");
                return;
            }

            TerrainData td1 = terrain1.terrainData;
            TerrainData td2 = terrain2.terrainData;

            if (td1.heightmapResolution != td2.heightmapResolution)
            {
                EditorUtility.DisplayDialog("Error", "Terrains cannot have different heightmap resolution.", "OK");
                return;
            }

            if (createUndo) Undo.RecordObjects(new []{td1, td2}, "Fix seams");

            int r = td1.heightmapResolution;

            if (terrain1.transform.position + new Vector3(td1.size.x, 0, 0) == terrain2.transform.position) // Right
            {
                float[,] h1 = td1.GetHeights(r - 1, 0, 1, r);
                float[,] h2 = td2.GetHeights(0, 0, 1, r);
                FixHeights(h1, h2);
                td1.SetHeights(r - 1, 0, h1);
                td2.SetHeights(0, 0, h1);
            }
            else if (terrain2.transform.position + new Vector3(td2.size.x, 0, 0) == terrain1.transform.position) // Left
            {
                float[,] h1 = td2.GetHeights(r - 1, 0, 1, r);
                float[,] h2 = td1.GetHeights(0, 0, 1, r);
                FixHeights(h1, h2);
                td2.SetHeights(r - 1, 0, h1);
                td1.SetHeights(0, 0, h1);
            }
            else if (terrain1.transform.position + new Vector3(0, 0, td1.size.z) == terrain2.transform.position) // Down
            {
                float[,] h1 = td1.GetHeights(0, r - 1, r, 1);
                float[,] h2 = td2.GetHeights(0, 0, r, 1);
                FixHeights(h1, h2);
                td1.SetHeights(0, r - 1, h1);
                td2.SetHeights(0, 0, h1);
            }
            else if (terrain2.transform.position + new Vector3(0, 0, td2.size.z) == terrain1.transform.position) // Up
            {
                float[,] h1 = td2.GetHeights(0, r - 1, r, 1);
                float[,] h2 = td1.GetHeights(0, 0, r, 1);
                FixHeights(h1, h2);
                td2.SetHeights(0, r - 1, h1);
                td1.SetHeights(0, 0, h1);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "This is not neighboring terrains.", "OK");
                return;
            }
        }
    }
}