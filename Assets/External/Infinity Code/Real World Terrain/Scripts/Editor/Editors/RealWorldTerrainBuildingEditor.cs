/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof (RealWorldTerrainBuilding), true)]
    public class RealWorldTerrainBuildingEditor : Editor
    {
        private RealWorldTerrainBuilding building;

        private void InvertRoofNormals()
        {
            building.invertRoof = !building.invertRoof;
            building.Generate();
        }

        private void InvertWallNormals()
        {
            building.invertWall = !building.invertWall;
            building.Generate();
        }

        public void OnEnable()
        {
            building = (RealWorldTerrainBuilding)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Select Real World Terrain Container"))
            {
                Selection.activeGameObject = building.container.gameObject;
            }

            EditorGUI.BeginChangeCheck();

            building.baseHeight = EditorGUILayout.FloatField("Base Height (meters)", building.baseHeight);
            building.startHeight = EditorGUILayout.FloatField("Start Height (meters)", building.startHeight);

            building.wallMaterial = EditorGUILayout.ObjectField("Wall Material", building.wallMaterial, typeof(Material), false) as Material;
            building.roofMaterial = EditorGUILayout.ObjectField("Roof Material", building.roofMaterial, typeof(Material), false) as Material;

            building.tileSize = EditorGUILayout.Vector2Field("Tile Size (meters)", building.tileSize);
            building.uvOffset = EditorGUILayout.Vector2Field("UV Offset", building.uvOffset);

            building.roofType = (RealWorldTerrainRoofType)EditorGUILayout.EnumPopup("Roof type", building.roofType);
            if (building.roofType != RealWorldTerrainRoofType.flat) building.roofHeight = EditorGUILayout.FloatField("Roof Height (meters)", building.roofHeight);

            if (EditorGUI.EndChangeCheck()) UpdateBuilding();

            if (GUILayout.Button("Invert wall normals")) InvertWallNormals();
            if (GUILayout.Button("Invert roof normals")) InvertRoofNormals();
            if (GUILayout.Button("Update")) UpdateBuilding();

            if (GUILayout.Button("Export mesh to OBJ"))
            {
                string path = EditorUtility.SaveFilePanel("Save building to OBJ", "", building.name + ".obj", "obj");
                if (path.Length != 0) RealWorldTerrainUtils.ExportMesh(path, building.meshFilter);
            }
        }

        private void UpdateBuilding()
        {
            building.Generate();
        }
    }
}