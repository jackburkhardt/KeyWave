/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InfinityCode.RealWorldTerrain.Tools;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof(RealWorldTerrainContainer))]
    public class RealWorldTerrainContainerEditor : Editor
    {
        private RealWorldTerrainContainer container;

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Combine All Buildings")]
        public static void CombineAllBuildings()
        {
            RealWorldTerrainContainer[] containers = FindObjectsOfType<RealWorldTerrainContainer>();
            foreach (RealWorldTerrainContainer container in containers)
            {
                RealWorldTerrainBuilding[] buildings = container.GetComponentsInChildren<RealWorldTerrainBuilding>();
                if (buildings.Length > 0) CombineBuildings(container, buildings);
            }
        }

        private static void CombineBuildings(RealWorldTerrainContainer container, RealWorldTerrainBuilding[] buildings)
        {
            if (container == null)
            {
                EditorUtility.DisplayDialog("Error", "Container cannot be null!", "OK");
                return;
            }

            if (buildings == null || buildings.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No buildings to combine!", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Warning", "You will no longer be able to modify buildings and work with meta data. Continue?", "Combine", "Cancel")) return;

            Transform parentTransform = container.transform.Find("Combined Buildings");
            GameObject parent;

            if (parentTransform == null)
            {
                parent = new GameObject("Combined Buildings");
                parent.transform.SetParent(container.transform, true);
            }
            else parent = parentTransform.gameObject;

            if (buildings.Length == 0) return;

            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUV = new List<Vector2>();
            List<Vector3> newNormals = new List<Vector3>();
            List<int> newIndices = new List<int>();
            List<SubMeshDescriptor> subMeshes = new List<SubMeshDescriptor>();
            List<Material> materials = new List<Material>();

            Material prevMaterial = null;
            SubMeshDescriptor subMesh = new SubMeshDescriptor(0, 0);

            int buildingIndex = 1;

            foreach (RealWorldTerrainBuilding building in buildings.OrderBy(b => b.roofMaterial != null? b.roofMaterial.GetInstanceID(): int.MinValue))
            {
                Vector3 pos = building.transform.position;
                Mesh mesh = building.meshFilter.sharedMesh;

                int countWallVertices = building.generateWall? building.baseVertices.Length * 4: 0;
                int countVertices = mesh.vertexCount - countWallVertices;
                Vector3[] vertices = mesh.vertices.Take(countVertices).ToArray();

                if (newVertices.Count + countVertices > 65000)
                {
                    subMeshes.Add(subMesh);
                    FinalizeCombinedMesh(container, parent, ref buildingIndex, newVertices, newUV, newNormals, newIndices, subMeshes, materials);
                    subMesh = new SubMeshDescriptor(0, 0);
                }

                if (building.roofMaterial != prevMaterial)
                {
                    if (materials.Count > 63)
                    {
                        subMeshes.Add(subMesh);
                        FinalizeCombinedMesh(container, parent, ref buildingIndex, newVertices, newUV, newNormals, newIndices, subMeshes, materials);
                        materials.Clear();
                    }

                    if (newVertices.Count > 0) subMeshes.Add(subMesh);
                    subMesh = new SubMeshDescriptor(newIndices.Count, 0);

                    materials.Add(building.roofMaterial);
                    prevMaterial = building.roofMaterial;
                }

                newUV.AddRange(mesh.uv.Take(countVertices));
                newNormals.AddRange(mesh.normals.Take(countVertices));
                int[] indices = mesh.GetIndices(0);
                for (int i = 0; i < indices.Length; i++) indices[i] += newVertices.Count;
                newIndices.AddRange(indices);
                for (int i = 0; i < vertices.Length; i++) vertices[i] += pos;
                newVertices.AddRange(vertices);
                subMesh.indexCount += indices.Length;
            }

            foreach (RealWorldTerrainBuilding building in buildings.Where(b => b.generateWall).OrderBy(b => b.wallMaterial != null ? b.wallMaterial.GetInstanceID() : int.MinValue))
            {
                Mesh mesh = building.meshFilter.sharedMesh;
                if (mesh.subMeshCount == 1) continue;

                Vector3 pos = building.transform.position;

                int countWallVertices = building.baseVertices.Length * 4;
                int countRoofVertices = mesh.vertexCount - countWallVertices;
                Vector3[] vertices = mesh.vertices.Skip(countRoofVertices).ToArray();

                if (newVertices.Count + countWallVertices > 65000)
                {
                    subMeshes.Add(subMesh);
                    FinalizeCombinedMesh(container, parent, ref buildingIndex, newVertices, newUV, newNormals, newIndices, subMeshes, materials);
                    subMesh = new SubMeshDescriptor(0, 0);
                }

                if (building.wallMaterial != prevMaterial)
                {
                    if (materials.Count > 63)
                    {
                        subMeshes.Add(subMesh);
                        FinalizeCombinedMesh(container, parent, ref buildingIndex, newVertices, newUV, newNormals, newIndices, subMeshes, materials);
                        materials.Clear();
                    }

                    if (newVertices.Count > 0) subMeshes.Add(subMesh);
                    subMesh = new SubMeshDescriptor(newIndices.Count, 0);

                    materials.Add(building.wallMaterial);
                    prevMaterial = building.wallMaterial;
                }

                newUV.AddRange(mesh.uv.Skip(countRoofVertices));
                newNormals.AddRange(mesh.normals.Skip(countRoofVertices));
                int[] indices = mesh.GetIndices(1);
                for (int i = 0; i < indices.Length; i++) indices[i] += newVertices.Count - countRoofVertices;
                newIndices.AddRange(indices);
                for (int i = 0; i < vertices.Length; i++) vertices[i] += pos;
                newVertices.AddRange(vertices);
                subMesh.indexCount += indices.Length;
            }

            subMeshes.Add(subMesh);
            FinalizeCombinedMesh(container, parent, ref buildingIndex, newVertices, newUV, newNormals, newIndices, subMeshes, materials);

            if (EditorUtility.DisplayDialog("Remove the existing buildings?", "Remove existing buildings from the scene? Meshes are still present in the project.", "Remove", "Do not remove"))
            {
                for (int i = 0; i < buildings.Length; i++) DestroyImmediate(buildings[i].gameObject);
            }
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Combine Selected Buildings")]
        public static void CombineSelectedBuildings()
        {
            GameObject[] gameObjects = Selection.gameObjects;

            Dictionary<RealWorldTerrainContainer, List<RealWorldTerrainBuilding>> dict = new Dictionary<RealWorldTerrainContainer, List<RealWorldTerrainBuilding>>();

            for (int i = 0; i < gameObjects.Length; i++)
            {
                RealWorldTerrainBuilding building = gameObjects[i].GetComponent<RealWorldTerrainBuilding>();
                if (building == null) continue;

                List<RealWorldTerrainBuilding> buildings;

                if (!dict.TryGetValue(building.container, out buildings)) dict.Add(building.container, new List<RealWorldTerrainBuilding>{building});
                else buildings.Add(building);
            }

            foreach (KeyValuePair<RealWorldTerrainContainer, List<RealWorldTerrainBuilding>> pair in dict)
            {
                CombineBuildings(pair.Key, pair.Value.ToArray());
            }
        }

        private void CreatePrefab()
        {
            try
            {
                Transform housesTransform = container.transform.Find("Buildings/Houses");
                if (housesTransform != null)
                {
                    string buildingsFolder = container.folder + "/Buildings";

                    if (!Directory.Exists(buildingsFolder)) Directory.CreateDirectory(buildingsFolder);

                    int houseCount = housesTransform.childCount;
                    for (int i = 0; i < houseCount; i++)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Create Prefab", "Save house " + (i + 1) + " from " + houseCount,
                            i / (float)houseCount))
                        {
                            EditorUtility.ClearProgressBar();
                            return;
                        }

                        Transform houseTransform = housesTransform.GetChild(i);
                        Transform roofTransform = houseTransform.Find("Roof");
                        Transform wallTransform = houseTransform.Find("Wall");

                        string houseID = houseTransform.name;
                        string housePath = buildingsFolder + "/" + houseID;

                        if (!Directory.Exists(housePath)) Directory.CreateDirectory(housePath);

                        if (roofTransform != null) SaveBuildingPart(roofTransform, housePath, "Roof");
                        if (wallTransform != null) SaveBuildingPart(wallTransform, housePath, "Wall");
                    }
                }

                EditorUtility.DisplayCancelableProgressBar("Create Prefab", "Save Prefab", 1);

                string containerName = container.name;
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(container.gameObject, container.folder + "/" + containerName + ".prefab");
                DestroyImmediate(container.gameObject);
                container = ((GameObject)PrefabUtility.InstantiatePrefab(prefab)).GetComponent<RealWorldTerrainContainer>();
                container.name = containerName;

                EditorGUIUtility.PingObject(prefab);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message + "\n" + exception.StackTrace);
            }

            EditorUtility.ClearProgressBar();
        }

        private void DrawBaseDist()
        {
            if (container == null || container.terrains == null || container.terrains.Length == 0) return;
            Terrain terrain = container.terrains[0].terrain;
            if (terrain == null) return;

            float dist = terrain.basemapDistance;

            EditorGUI.BeginChangeCheck();

            dist = EditorGUILayout.Slider("Base Map Distance", dist, 0, 20000);

            if (EditorGUI.EndChangeCheck())
            {
                foreach (RealWorldTerrainItem item in container.terrains)
                {
                    item.terrain.basemapDistance = dist;
                }
            }
        }

        private void DrawItemScale(float sizeX, float sizeY)
        {
            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            if (container.terrains == null) return;

            foreach (RealWorldTerrainItem item in container.terrains)
            {
                if (item == null) continue;
                Bounds bounds = item.bounds;
                Vector3 p = bounds.min;
                Vector3 p2 = bounds.max;
                if (p.x < minX) minX = p.x;
                if (p.z < minZ) minZ = p.z;
                if (p2.x > maxX) maxX = p2.x;
                if (p2.z > maxZ) maxZ = p2.z;
            }

            GUILayout.Label("Scale X: " + sizeX * 1000 / (maxX - minX) + " meter/unit");
            GUILayout.Label("Scale Z: " + sizeY * 1000 / (maxZ - minZ) + " meter/unit");

            EditorGUILayout.Space();
        }

        private void DrawItemSize(out float sizeX, out float sizeY)
        {
            sizeX = sizeY = 0;
            if (container == null || container.prefs == null) return;

            double tx = container.leftLongitude;
            double ty = container.topLatitude;
            double bx = container.rightLongitude;
            double by = container.bottomLatitude;
            double rx = bx - tx;

            if (container.prefs.sizeType == 0 || container.prefs.sizeType == 2)
            {
                double scfY = Math.Sin(ty * Mathf.Deg2Rad);
                double sctY = Math.Sin(by * Mathf.Deg2Rad);
                double ccfY = Math.Cos(ty * Mathf.Deg2Rad);
                double cctY = Math.Cos(by * Mathf.Deg2Rad);
                double cX = Math.Cos(rx * Mathf.Deg2Rad);
                double sizeX1 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
                double sizeX2 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(sctY * sctY + cctY * cctY * cX));
                sizeX = (float)((sizeX1 + sizeX2) / 2.0);
                sizeY = (float)(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * sctY + ccfY * cctY));
            }
            else if (container.prefs.sizeType == 1)
            {
                sizeX = (float)Math.Abs(rx / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
                sizeY = (float)Math.Abs((by - ty) / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
            }

            GUILayout.Label("Size X: " + sizeX + " km");
            GUILayout.Label("Size Z: " + sizeY + " km");
            EditorGUILayout.Space();
        }

        private void DrawToolbar()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = 40,
                padding = new RectOffset(5, 5, 4, 4)
            };
            EditorGUILayout.BeginHorizontal(style);

            GUIStyle buttonStyle = new GUIStyle {margin = new RectOffset(5, 5, 0, 0)};

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.refreshIcon, "Real World Terrain"), buttonStyle, GUILayout.ExpandWidth(false))) ShowRegenerateMenu();
            if (container.generateTextures && container.prefs.textureResultType == RealWorldTerrainTextureResultType.regularTexture)
            {
                if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.wizardIcon, "Postprocess"), buttonStyle, GUILayout.ExpandWidth(false))) ShowPostprocessMenu();
            }

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.rawIcon, "Export/Import RAW"), buttonStyle, GUILayout.ExpandWidth(false))) ShowRawMenu();
            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.utilsIcon, "Utils"), buttonStyle, GUILayout.ExpandWidth(false))) ShowUtilsMenu();
            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.finishIcon, "Finish"), buttonStyle, GUILayout.ExpandWidth(false))) ShowFinishMenu();

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTreeAndGrassProps()
        {
            if (container == null || container.terrains == null) return;

            bool needRedrawScene = false;

            GUILayout.Label("Tree and Grass Distances:");

            EditorGUI.BeginChangeCheck();
            container.detailDistance = EditorGUILayout.Slider("Detail Distance", container.detailDistance, 0, 250);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RealWorldTerrainItem item in container.terrains)
                {
                    if (item == null) continue;
                    Terrain terrain = item.terrain;
                    if (terrain != null) terrain.detailObjectDistance = container.detailDistance;
                }
                needRedrawScene = true;
            }

            EditorGUI.BeginChangeCheck();
            container.detailDensity = EditorGUILayout.Slider("Detail Density", container.detailDensity, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RealWorldTerrainItem item in container.terrains)
                {
                    if (item == null) continue;
                    Terrain terrain = item.terrain;
                    if (terrain != null) terrain.detailObjectDensity = container.detailDensity;
                }
                needRedrawScene = true;
            }

            EditorGUI.BeginChangeCheck();
            container.treeDistance = EditorGUILayout.Slider("Tree Distance", container.treeDistance, 0, 2000);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RealWorldTerrainItem item in container.terrains)
                {
                    if (item == null) continue;
                    Terrain terrain = item.terrain;
                    if (terrain != null) terrain.treeDistance = container.treeDistance;
                }
                needRedrawScene = true;
            }

            EditorGUI.BeginChangeCheck();
            container.billboardStart = EditorGUILayout.Slider("Billboard Start", container.billboardStart, 5, 2000);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RealWorldTerrainItem item in container.terrains)
                {
                    if (item == null) continue;
                    Terrain terrain = item.terrain;
                    if (terrain != null) terrain.treeBillboardDistance = container.billboardStart;
                }
                needRedrawScene = true;
            }

            if (needRedrawScene) SceneView.RepaintAll();

            EditorGUILayout.Space();
        }

        public static void ExportRawTextures(RealWorldTerrainMonoBase target)
        {
            string filename = EditorUtility.SaveFilePanel("Export RAW Textures", Application.dataPath, "textures.raw", "raw");
            if (string.IsNullOrEmpty(filename)) return;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            int textureWidth = 0;
            int textureHeight = 0;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int tIndex = y * cx + x;

                    Texture2D texture = terrains[tIndex].texture;
                    if (texture == null)
                    {
                        Debug.Log(terrains[tIndex].name + " not have texture. Abort!!!");
                        continue;
                    }
                    if (x == 0 && y == 0)
                    {
                        textureWidth = texture.width;
                        textureHeight = texture.height;
                    }
                    else if (texture.width != textureWidth || texture.height != textureHeight)
                    {
                        Debug.LogWarning("Textures have different sizes. Abort!!!");
                        return;
                    }
                }
            }

            FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            int countRow = cy * textureHeight;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int tIndex = y * cx + x;
                    Texture2D texture = terrains[tIndex].texture;
                    Color[] colors = texture.GetPixels();

                    for (int ty = 0; ty < textureHeight; ty++)
                    {
                        float progress = ((y * cx + x) * textureHeight + ty) / (float)(cx * cy * textureHeight);
                        EditorUtility.DisplayProgressBar("Export RAW Textures", Mathf.RoundToInt(progress * 100) + "%", progress);

                        int ry = countRow - y * textureHeight - ty - 1;
                        stream.Seek((ry * textureWidth * cx + x * textureWidth) * 3, SeekOrigin.Begin);
                        for (int tx = 0; tx < textureWidth; tx++)
                        {
                            Color32 color32 = colors[ty * textureHeight + tx];
                            stream.WriteByte(color32.r);
                            stream.WriteByte(color32.g);
                            stream.WriteByte(color32.b);
                        }
                    }
                }
            }

            stream.Close();

            EditorUtility.ClearProgressBar();
        }

        private static void FinalizeCombinedMesh(RealWorldTerrainContainer container, GameObject parent, ref int buildingIndex, List<Vector3> vertices, List<Vector2> uv, List<Vector3> normals, List<int> indices, List<SubMeshDescriptor> subMeshes, List<Material> materials)
        {
            string name;
            string prefix = "Combined Buildings ";
            while (parent.transform.Find(name = prefix + buildingIndex) != null) buildingIndex++;

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, true);
            buildingIndex++;
            go.AddComponent<MeshRenderer>().materials = materials.ToArray();
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.name = go.name;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetNormals(normals);

            mesh.subMeshCount = subMeshes.Count;
            for (int i = 0; i < subMeshes.Count; i++)
            {
                SubMeshDescriptor sm = subMeshes[i];
                mesh.SetIndices(indices.Skip(sm.indexStart).Take(sm.indexCount).ToArray(), MeshTopology.Triangles, i);
            }

            string path = container.folder + "/Combined Buildings/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string assetPath;
            while (File.Exists(assetPath = path + prefix + buildingIndex + ".asset")) buildingIndex++;
            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.Refresh();

            meshFilter.mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

            vertices.Clear();
            uv.Clear();
            normals.Clear();
            indices.Clear();
            subMeshes.Clear();

            Material last = materials.Last();
            materials.Clear();
            materials.Add(last);
        }

        public static void ImportRawTextures(RealWorldTerrainMonoBase target)
        {
            string filename = EditorUtility.OpenFilePanel("Import RAW Textures", Application.dataPath, "raw");
            if (string.IsNullOrEmpty(filename)) return;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            Texture2D texture = terrains[0].texture;
            if (texture == null)
            {
                EditorUtility.DisplayDialog("Error", "Can not find a texture.", "OK");
                return;
            }

            int tw = texture.width;
            int th = texture.height;

            int textureWidth = cx * tw;
            int textureHeight = cy * th;

            FileInfo info = new FileInfo(filename);

            if (info.Length != textureWidth * textureHeight * 3)
            {
                EditorUtility.DisplayDialog("Error", String.Format("Invalid file size. {0} bytes expected.", textureWidth * textureHeight * 3), "OK");
                return;
            }

            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            texture = new Texture2D(tw, th, TextureFormat.ARGB32, false);

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    float progress = (y * cx + x) / (float) (cx * cy);
                    EditorUtility.DisplayProgressBar("Import RAW Texture", Mathf.RoundToInt(progress * 100) + "%", progress);

                    Color[] colors = new Color[tw * th];

                    for (int ty = 0; ty < th; ty++)
                    {
                        int ry = textureHeight - y * th - ty - 1;
                        int seek = ry * textureWidth + x * tw;
                        stream.Seek(seek * 3, SeekOrigin.Begin);

                        for (int tx = 0; tx < tw; tx++)
                        {
                            byte r = (byte)stream.ReadByte();
                            byte g = (byte)stream.ReadByte();
                            byte b = (byte)stream.ReadByte();
                            colors[ty * tw + tx] = new Color32(r, g, b, 255);
                        }
                    }

                    int tIndex = y * cx + x;

                    texture.SetPixels(colors);
                    texture.Apply();

                    string assetPath = AssetDatabase.GetAssetPath(terrains[tIndex].texture);
                    File.WriteAllBytes(assetPath, texture.EncodeToPNG());
                    terrains[tIndex].texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof (Texture2D)) as Texture2D;
                }
            }

            stream.Close();

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            EditorUtility.ClearProgressBar();
        }

        private void OnEnable()
        {
            container = (RealWorldTerrainContainer)target;
        }

        public override void OnInspectorGUI()
        {
            DrawToolbar();

            EditorGUILayout.LabelField("Title: " + container.title);

            RealWorldTerrainItemEditor.DrawLocationInfo(container);

            float sizeX, sizeY;
            DrawItemSize(out sizeX, out sizeY);
            DrawItemScale(sizeX, sizeY);

            if (container.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                DrawTreeAndGrassProps();
                DrawBaseDist();
            }
        }

#if WORLDSTREAMER
        private void OpenWorldStreamer()
        {
            if (!EditorUtility.DisplayDialog("Important", "Make a backup copy of the scene.\nAfter using WorldStreamer in this scene, you will not be able to use Real World Terrain or utilities.", "Continue", "Cancel")) return;

            SceneSplitterSettings settings = FindObjectOfType<SceneSplitterSettings>();
            if (settings == null)
            {
                GameObject settingsGO = new GameObject("_SceneSplitterSettings");
                settings = settingsGO.gameObject.AddComponent<SceneSplitterSettings>();
                settings.scenesPath = "Assets/SplitScenes/";
                if (!Directory.Exists(settings.scenesPath)) Directory.CreateDirectory(settings.scenesPath);
            }

            SceneCollection[] sceneCollections = FindObjectsOfType<SceneCollection>();
            if (sceneCollections.Length == 0)
            {
                GameObject collectionGO = new GameObject("SC_0");
                collectionGO.transform.parent = settings.transform;
                SceneCollection collection = collectionGO.AddComponent<SceneCollection>();
                collection.prefixName = "Terrain";
                collection.ySplitIs = true;
                Vector3 terrainSize = container.terrains[0].terrainData.size;
                collection.xSize = (int)terrainSize.x;
                collection.ySize = (int)terrainSize.y;
                collection.zSize = (int)terrainSize.z;
            }

            foreach (RealWorldTerrainItem terrain in container.terrains)
            {
                if (terrain == null) continue;
                terrain.transform.parent = null;
            }

            Type splitter = typeof(SceneSplitterEditor);
            MethodInfo method = splitter.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, null);
        }
#endif

        private static void SaveBuildingPart(Transform roofTransform, string housePath, string partName)
        {
            MeshFilter meshFilter = roofTransform.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                if (!AssetDatabase.Contains(meshFilter.sharedMesh))
                {
                    string path = housePath + "/" + partName + ".asset";
                    AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath(path, typeof(Mesh)) as Mesh;
                }
            }

            Renderer renderer = roofTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (!AssetDatabase.Contains(renderer.sharedMaterial))
                {
                    string path = housePath + "/" + partName + ".mat";
                    AssetDatabase.CreateAsset(renderer.sharedMaterial, path);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                }
            }
        }

        private void SetProductionTextureSettings()
        {
            bool needRefresh = false;

            foreach (RealWorldTerrainItem item in container.terrains)
            {
                Texture texture = item.texture;
                if (texture == null) continue;

                TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                if (importer == null) continue;

                importer.isReadable = false;
                importer.mipmapEnabled = true;

                needRefresh = true;
            }

            if (needRefresh) AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private void ShowFinishMenu()
        {
            GenericMenu menu = new GenericMenu();

            if (container.generateTextures)
            {
                menu.AddItem(new GUIContent("Set settings of textures for production"), false, SetProductionTextureSettings);
            }

            if (container.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
#if WORLDSTREAMER
                menu.AddItem(new GUIContent("Open WorldStreamer"), false, OpenWorldStreamer);
#endif
            }

            menu.AddItem(new GUIContent("Combine built-in buildings"), false, () => CombineBuildings(container, container.GetComponentsInChildren<RealWorldTerrainBuilding>()));
            menu.ShowAsContext();
        }

        private void ShowPostprocessMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Brightness, Contrast and HUE"), false, () => RealWorldTerrainHUEWindow.OpenWindow(container));
            menu.AddItem(new GUIContent("Color Balance"), false, () => RealWorldTerrainColorBalance.OpenWindow(container));
            menu.AddItem(new GUIContent("Color Levels"), false, () => RealWorldTerrainColorLevels.OpenWindow(container));

            if (container.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                menu.AddItem(new GUIContent("Erosion"), false, () => RealWorldTerrainErosionFilter.OpenWindow(container));
                menu.AddItem(new GUIContent("Generate Grass from Texture"), false, RealWorldTerrainGrassGeneratorWindow.OpenWindow);
                menu.AddItem(new GUIContent("Generate SplatPrototypes from Texture"), false, RealWorldTerrainSplatPrototypeGenerator.OpenWindow);
            }

            menu.ShowAsContext();
        }

        private void ShowRawMenu()
        {
            GenericMenu menu = new GenericMenu();

            if (container.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                menu.AddItem(new GUIContent("Export Heightmap"), false, () => RealWorldTerraiHeightmapExporter.OpenWindow(container));
                menu.AddItem(new GUIContent("Import Heightmap"), false, () => RealWorldTerrainHeightmapImporter.OpenWindow(container));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Detailmap"), false, () => RealWorldTerrainDetailmapExporter.OpenWindow(container));
                menu.AddItem(new GUIContent("Import Detailmap"), false, () => RealWorldTerrainDetailmapImporter.OpenWindow(container));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Alphamap"), false, () => RealWorldTerrainAlphamapExporter.OpenWindow(container));
                menu.AddItem(new GUIContent("Import Alphamap"), false, () => RealWorldTerrainAlphamapImporter.OpenWindow(container));
            }
            else if (container.prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                menu.AddItem(new GUIContent("Export OBJ"), false, () => RealWorldTerrainMeshOBJExporter.Export(container));
            }

            if (container.generateTextures)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Textures"), false, () => ExportRawTextures(container));
                menu.AddItem(new GUIContent("Import Textures"), false, () => ImportRawTextures(container));
            }
            menu.ShowAsContext();
        }

        private void ShowRegenerateMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open with current settings"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.full, container));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Regenerate Terrains"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.terrain, container));
            menu.AddItem(new GUIContent("Regenerate Textures"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.texture, container));
            menu.AddItem(new GUIContent("Regenerate Additional"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.additional, container));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Create Prefab"), false, CreatePrefab);
            if (container.terrainCount.count > 1) menu.AddItem(new GUIContent("Split into GameObjects"), false, SplitTerrains);
            menu.ShowAsContext();
        }

        private void ShowUtilsMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Building Manager"), false, RealWorldTerrainBuildingManager.OpenWindow);
            menu.AddItem(new GUIContent("Geocoder"), false, () => RealWorldTerrainGeocodingWindow.OpenWindow(container));
            menu.AddItem(new GUIContent("Object Placer"), false, () => RealWorldTerrainObjectPlacerWindow.OpenWindow(container));
            menu.AddItem(new GUIContent("POI Manager"), false, () => RealWorldTerrainPOIManager.OpenWindow(container));
            menu.AddItem(new GUIContent("Reverse Geocoder"), false, () => RealWorldTerrainReverseGeocodingWindow.OpenWindow(container));
            menu.AddItem(new GUIContent("Scaler"), false, () => RealWorldTerrainScalerWindow.OpenWindow(container));
            menu.AddItem(new GUIContent("Show Location"), false, () => RealWorldTerrainCurrentLatLon.OpenWindow(container));
            if (container.terrainCount > 1) menu.AddItem(new GUIContent("Update Neighbors"), false, () => RealWorldTerrainUpdateNeighbors.Update(container));
            menu.ShowAsContext();
        }

        private void SplitTerrains()
        {
            foreach (RealWorldTerrainItem terrainItem in container.terrains)
            {
                GameObject parent = new GameObject(terrainItem.gameObject.name);
                parent.transform.position = terrainItem.transform.position;
                terrainItem.transform.SetParent(parent.transform, true);
                RealWorldTerrainContainer c = parent.AddComponent<RealWorldTerrainContainer>();
                c.prefs = RealWorldTerrainPrefs.GetPrefs(terrainItem);
                c.leftLongitude = terrainItem.leftLongitude;
                c.topLatitude = terrainItem.topLatitude;
                c.rightLongitude = terrainItem.rightLongitude;
                c.bottomLatitude = terrainItem.bottomLatitude;
                c.leftMercator = terrainItem.leftMercator;
                c.topMercator = terrainItem.topMercator;
                c.rightMercator = terrainItem.rightMercator;
                c.bottomMercator = terrainItem.bottomMercator;
                c.terrainCount = RealWorldTerrainVector2i.one;
                c.terrains = new[] {terrainItem};
                c.bounds = terrainItem.bounds;
                c.folder = container.folder;
                terrainItem.container = c;
            }

            DestroyImmediate(container.gameObject);
        }

#if !UNITY_2019_3_OR_NEWER
        public struct SubMeshDescriptor
        {
            /// <summary>
            ///   <para>Starting point inside the whole Mesh index buffer where the face index data is found.</para>
            /// </summary>
            public int indexStart;

            /// <summary>
            ///   <para>Index count for this sub-mesh face data.</para>
            /// </summary>
            public int indexCount;

            public SubMeshDescriptor(int indexStart, int indexCount)
            {
                this.indexStart = indexStart;
                this.indexCount = indexCount;
            }
        }
#endif
    }
}