/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainRiverGenerator
    {
        public const string BUILTIN_RIVER_ENGINE = "Built-In";
        public const string RAM2019_RIVER_ENGINE = "R.A.M 2019";

        private static Dictionary<string, RealWorldTerrainOSMNode> nodes;
        private static Dictionary<string, RealWorldTerrainOSMWay> ways;
        private static List<RealWorldTerrainOSMRelation> relations;
        private static bool loaded;
        private static string riverFolder;
        private static List<MeshFilter> riversToSave;
        private static Material emptyMaterial;

        public static string url
        {
            get
            {
                string request = string.Format(
                    RealWorldTerrainCultureInfo.numberFormat,
                    "node({0},{1},{2},{3});way(bn);rel(bw)['natural'~'water'];(._;>;);out;node({0},{1},{2},{3});way(bn)['natural'~'water'];(._;>;);out;node({0},{1},{2},{3});way(bn)['waterway'~'stream|river'];(._;>;);out;", 
                    prefs.bottomLatitude, 
                    prefs.leftLongitude, 
                    prefs.topLatitude, 
                    prefs.rightLongitude);
                return RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(request); ;
            }
        }

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public static string filename
        {
            get
            {
                return Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, 
                    String.Format(
                        RealWorldTerrainCultureInfo.numberFormat,
                        "rivers_{0}_{1}_{2}_{3}.osm", 
                        prefs.bottomLatitude, 
                        prefs.leftLongitude, 
                        prefs.topLatitude, 
                        prefs.rightLongitude));
            }
        }

        public static string compressedFilename
        {
            get
            {
                return filename + "c";
            }
        }

        public static void CreateRAMRiverArea(RealWorldTerrainOSMRelation rel, GameObject container, RealWorldTerrainContainer globalContainer)
        {
#if RAM2019
            List<Vector3> points = new List<Vector3>();

            List<RealWorldTerrainOSMWay> usedWays = new List<RealWorldTerrainOSMWay>();

            foreach (RealWorldTerrainOSMRelationMember member in rel.members)
            {
                if (member.type != "way") continue;

                RealWorldTerrainOSMWay way;
                if (!ways.TryGetValue(member.reference, out way)) continue;

                usedWays.Add(way);
            }

            points.AddRange(RealWorldTerrainOSMUtils.GetWorldPointsFromWay(usedWays[0], nodes, globalContainer));
            string lastID = usedWays[0].nodeRefs.Last();
            usedWays.RemoveAt(0);

            if (usedWays.Count > 0)
            {
                while (usedWays.Count > 0)
                {
                    RealWorldTerrainOSMWay way = usedWays.FirstOrDefault(w => w.nodeRefs[0] == lastID);
                    bool r = false;
                    if (way == null)
                    {
                        way = usedWays.FirstOrDefault(w => w.nodeRefs.Last() == lastID);
                        if (way == null) break;

                        r = true;
                    }

                    lastID = r ? way.nodeRefs[0] : way.nodeRefs.Last();

                    List<Vector3> wayPoints = RealWorldTerrainOSMUtils.GetWorldPointsFromWay(way, nodes, globalContainer);

                    if (r) wayPoints.Reverse();

                    points.AddRange(wayPoints.Skip(1));
                    usedWays.Remove(way);
                }
            }

            if (points.Count < 3) return;

            if (points.First() == points.Last())
            {
                points.RemoveAt(0);
                if (points.Count < 3) return;
            }

            CreateRAMRiverArea(rel, container, points);
#endif
        }

        public static void CreateRAMRiverArea(RealWorldTerrainOSMWay way, GameObject container, RealWorldTerrainContainer globalContainer)
        {
#if RAM2019
            List<Vector3> points = RealWorldTerrainOSMUtils.GetWorldPointsFromWay(way, nodes, globalContainer);

            if (points.Count < 3)
            {
                Debug.Log(way.id);

                return;
            }

            if (points.First() == points.Last())
            {
                points.RemoveAt(0);
                if (points.Count < 3) return;
            }

            CreateRAMRiverArea(way, container, points);
#endif
        }

        public static void CreateRAMRiverArea(RealWorldTerrainOSMBase osm, GameObject container, List<Vector3> points)
        {
#if RAM2019
            GameObject go = RealWorldTerrainUtils.CreateGameObject(container, "River " + osm.id);
            LakePolygon lakePolygon = go.AddComponent<LakePolygon>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            lakePolygon.oldProfile = lakePolygon.currentProfile = prefs.ramAreaProfile;
            if (prefs.ramAreaProfile != null) meshRenderer.sharedMaterial = lakePolygon.oldMaterial = prefs.ramAreaProfile.lakeMaterial;

            lakePolygon.maximumTriangleSize = 10000;
            lakePolygon.yOffset = 2;
            lakePolygon.points = points;
            lakePolygon.GeneratePolygon();

            go.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(osm);
#endif
        }

        public static void CreateRAMRiverStream(RealWorldTerrainOSMWay way, GameObject container, RealWorldTerrainContainer globalContainer)
        {
#if RAM2019
            List<Vector3> points = RealWorldTerrainOSMUtils.GetWorldPointsFromWay(way, nodes, globalContainer);
            if (points.Count < 3) return;

            GameObject go = RealWorldTerrainUtils.CreateGameObject(container, "River " + way.id);
            RamSpline spline = go.AddComponent<RamSpline>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            spline.oldProfile = spline.currentProfile = prefs.ramSplineProfile;
            if (prefs.ramSplineProfile != null) meshRenderer.sharedMaterial = spline.oldMaterial = prefs.ramSplineProfile.splineMaterial;

            foreach (Vector3 p in points) spline.AddPoint(p);

            spline.GenerateSpline();

            go.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);
#endif
        }

        private static void CreateRiver(RealWorldTerrainOSMRelation rel, GameObject container, RealWorldTerrainContainer globalContainer, Material defMaterial)
        {
            List<Vector3> points = new List<Vector3>();

            List<RealWorldTerrainOSMWay> usedWays = new List<RealWorldTerrainOSMWay>();

            foreach (RealWorldTerrainOSMRelationMember member in rel.members)
            {
                if (member.type != "way") continue;

                RealWorldTerrainOSMWay way;
                if (!ways.TryGetValue(member.reference, out way)) continue;

                usedWays.Add(way);
            }

            points.AddRange(RealWorldTerrainOSMUtils.GetWorldPointsFromWay(usedWays[0], nodes, globalContainer));
            string lastID = usedWays[0].nodeRefs.Last();
            usedWays.RemoveAt(0);

            if (usedWays.Count > 0)
            {
                while (usedWays.Count > 0)
                {
                    RealWorldTerrainOSMWay way = usedWays.FirstOrDefault(w => w.nodeRefs[0] == lastID);
                    bool r = false;
                    if (way == null)
                    {
                        way = usedWays.FirstOrDefault(w => w.nodeRefs.Last() == lastID);
                        if (way == null) break;

                        r = true;
                    }

                    lastID = r? way.nodeRefs[0] : way.nodeRefs.Last();

                    List<Vector3> wayPoints = RealWorldTerrainOSMUtils.GetWorldPointsFromWay(way, nodes, globalContainer);

                    if (r) wayPoints.Reverse();

                    points.AddRange(wayPoints.Skip(1));
                    usedWays.Remove(way);
                }
            }

            if (points.Count < 3)
            {
                Debug.Log(rel.id);

                return;
            }

            if (points.First() == points.Last())
            {
                points.RemoveAt(0);
                if (points.Count < 3) return;
            }

            CreateRiver(rel, container, defMaterial, points);
        }

        private static void CreateRiver(RealWorldTerrainOSMWay way, GameObject container, RealWorldTerrainContainer globalContainer, Material defMaterial)
        {
            List<Vector3> points = RealWorldTerrainOSMUtils.GetWorldPointsFromWay(way, nodes, globalContainer);

            if (points.Count < 3)
            {
                Debug.Log(way.id);

                return;
            }

            if (points.First() == points.Last())
            {
                points.RemoveAt(0);
                if (points.Count < 3) return;
            }

            CreateRiver(way, container, defMaterial, points);
        }

        private static void CreateRiver(RealWorldTerrainOSMBase osm, GameObject container, Material defMaterial, List<Vector3> points)
        {
            List<Vector3> vertices = new List<Vector3>(points.Count);
            List<Vector2> uv = new List<Vector2>(points.Count);
            List<Vector3> normals = new List<Vector3>(points.Count);
            List<Vector2> flatPoints = new List<Vector2>(points.Count);

            Vector4 b = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 v = points[i];
                vertices.Add(v);
                normals.Add(Vector3.up);
                flatPoints.Add(new Vector2(v.x, v.z));
                if (v.x < b.x) b.x = v.x;
                if (v.z < b.y) b.y = v.z;
                if (v.x > b.z) b.z = v.x;
                if (v.z > b.w) b.w = v.z;
            }

            float ox = b.z - b.x;
            float oz = b.w - b.y;

            Vector3 position = new Vector3((b.x + b.z) / 2, vertices[0].y, (b.y + b.w) / 2);

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] -= position;
                Vector3 v = vertices[i];
                uv.Add(new Vector2((v.x - b.x) / ox, (v.z - b.y) / oz));
            }

            int[] triangles = RealWorldTerrainUtils.Triangulate(flatPoints).ToArray();
            bool reversed = RealWorldTerrainUtils.IsClockWise(vertices[triangles[0]], vertices[triangles[1]], vertices[triangles[2]]);
            if (reversed) triangles = triangles.Reverse().ToArray();

            string name = "River " + osm.id;

            GameObject meshGO = RealWorldTerrainUtils.CreateGameObject(container, name);
            meshGO.transform.localPosition = position;

            Mesh mesh = new Mesh
            {
                name = meshGO.name,
                vertices = vertices.ToArray(),
                uv = uv.ToArray(),
                normals = normals.ToArray(),
                triangles = triangles.ToArray()
            };

            mesh.RecalculateBounds();

            Material material;
            if (defMaterial != null) material = new Material(defMaterial);
            else
            {
                if (emptyMaterial == null) emptyMaterial = new Material(Shader.Find("Standard"));
                material = emptyMaterial;
            }

            MeshFilter meshFilter = RealWorldTerrainOSMUtils.AppendMesh(meshGO, mesh, material, name);
            meshGO.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(osm);

            riversToSave.Add(meshFilter);
        }

        public static void Dispose()
        {
            nodes = null;
            ways = null;
            relations = null;
            loaded = false;
            emptyMaterial = null;
        }

        public static void Download()
        {
            if (!prefs.generateRivers || File.Exists(compressedFilename)) return;
            if (File.Exists(filename))
            {
                byte[] data = File.ReadAllBytes(filename);
                OnDownloadComplete(ref data);
            }
            else
            {
                RealWorldTerrainDownloadItemUnityWebRequest item = new RealWorldTerrainDownloadItemUnityWebRequest(url)
                {
                    filename = filename,
                    averageSize = 600000,
                    exclusiveLock = RealWorldTerrainOSMUtils.OSMLocker,
                    ignoreRequestProgress = true
                };
                item.OnData += OnDownloadComplete;
            }
        }

        public static void Generate(RealWorldTerrainContainer baseContainer)
        {
            if (!loaded)
            {
                RealWorldTerrainOSMUtils.LoadOSM(compressedFilename, out nodes, out ways, out relations, false);
                loaded = true;
            }

            baseContainer.generatedRivers = true;
            GameObject container = new GameObject("Rivers");
            container.transform.parent = baseContainer.gameObject.transform;
            container.transform.localPosition = Vector3.zero;

            Material mat = prefs.riverMaterial;
            if (mat == null) RealWorldTerrainEditorUtils.FindMaterial("Default-River-Material.mat");

            riversToSave = new List<MeshFilter>();

            RealWorldTerrainOSMRelation[] areas = relations.Where(r => r.HasTag("natural", "water")).ToArray();
            RealWorldTerrainOSMWay[] wayAreas = ways.Where(r => r.Value.HasTag("natural", "water")).Select(r => r.Value).ToArray();
            KeyValuePair<string, RealWorldTerrainOSMWay>[] splines = ways.Where(w => w.Value.HasTags("waterway", "stream", "river")).ToArray();

            float total = areas.Length + wayAreas.Length + splines.Length;

            for (int i = 0; i < areas.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Please wait", "Generating river", i / total))
                {
                    break;
                }

                var rel = areas[i];
                if (prefs.riverEngine == BUILTIN_RIVER_ENGINE) CreateRiver(rel, container, baseContainer, mat);
                else if (prefs.riverEngine == RAM2019_RIVER_ENGINE) CreateRAMRiverArea(rel, container, baseContainer);
            }

            for (int i = 0; i < wayAreas.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Please wait", "Generating river", i / total))
                {
                    break;
                }

                var way = wayAreas[i];
                if (prefs.riverEngine == BUILTIN_RIVER_ENGINE) CreateRiver(way, container, baseContainer, mat);
                else if (prefs.riverEngine == RAM2019_RIVER_ENGINE) CreateRAMRiverArea(way, container, baseContainer);
            }

            for (int i = 0; i < splines.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Please wait", "Generating river", (i + areas.Length) / total))
                {
                    break;
                }

                var way = splines[i].Value;
                if (prefs.riverEngine == RAM2019_RIVER_ENGINE) CreateRAMRiverStream(way, container, baseContainer);
            }

            if (riversToSave != null && riversToSave.Count > 0)
            {
                riverFolder = RealWorldTerrainWindow.container.folder + "/Rivers/";
                if (!Directory.Exists(riverFolder)) Directory.CreateDirectory(riverFolder);

                AssetDatabase.StartAssetEditing();

                foreach (MeshFilter filter in riversToSave)
                {
                    Mesh mesh = filter.sharedMesh;
                    string path = riverFolder + mesh.name + ".asset";
                    AssetDatabase.CreateAsset(mesh, path);
                }

                AssetDatabase.StopAssetEditing();

                foreach (MeshFilter filter in riversToSave)
                {
                    Mesh mesh = filter.sharedMesh;
                    string path = riverFolder + mesh.name + ".asset";
                    filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                }
            }

            EditorUtility.ClearProgressBar();

            RealWorldTerrainPhase.phaseComplete = true;
        }

        private static void OnDownloadComplete(ref byte[] data)
        {
            RealWorldTerrainOSMUtils.GenerateCompressedFile(data, ref nodes, ref ways, ref relations, compressedFilename);
        }
    }
}