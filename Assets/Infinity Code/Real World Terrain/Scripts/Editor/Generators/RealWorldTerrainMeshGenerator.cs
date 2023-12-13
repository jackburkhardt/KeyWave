/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.IO;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainMeshGenerator
    {
        private static Mesh mesh;
        private static int lastY;
        private static float curDepth;
        private static float nextZ;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static bool GenerateElevation(MeshData data, int hr, float thf, double y2, double y1, Vector3 s, double x1, double x2, double minElevation, double scaledRange, float nodataDepth, int thi, RealWorldTerrainTimer timer)
        {
            double mx1, mx2, my1, my2;
            RealWorldTerrainUtils.LatLongToMercat(x1, y2, out mx1, out my1);
            RealWorldTerrainUtils.LatLongToMercat(x2, y1, out mx2, out my2);

            double thx = (mx2 - mx1) / thf;
            double thy = (my2 - my1) / thf;

            Vector3[] vertices = data.vertices;
            Vector2[] uv = data.uv;
            int[] triangles = data.triangles;

            for (int hy = lastY; hy < hr; hy++)
            {
                float ry = hy / thf;
                double py = hy * thy + my1;

                int iy = hy * hr;
                float vy = hy * s.z;

                for (int hx = 0; hx < hr; hx++)
                {
                    float rx = hx / thf;
                    double px = hx * thx + mx1;

                    double elevation = RealWorldTerrainElevationGenerator.GetElevation(px, py);
                    int v = iy + hx;
                    float cy = float.MinValue;
                    if (Math.Abs(elevation - double.MinValue) > double.Epsilon) cy = (float)((elevation - minElevation) / scaledRange);
                    else if (!prefs.generateUnderWater) cy = nodataDepth;
                    else RealWorldTerrainElevationGenerator.hasUnderwater = true;

                    vertices[v] = new Vector3(hx * s.x, cy, vy);
                    uv[v] = new Vector2(rx, ry);

                    if (hx < thi && hy < thi)
                    {
                        int mv = (hy * thi + hx) * 6;
                        triangles[mv] = v;
                        triangles[mv + 1] = v + hr;
                        triangles[mv + 2] = v + 1;
                        triangles[mv + 3] = v + 1;
                        triangles[mv + 4] = v + hr;
                        triangles[mv + 5] = v + hr + 1;
                    }
                }

                lastY = hy + 1;
                if (timer.seconds > 1)
                {
                    RealWorldTerrainPhase.phaseProgress = hy / (float)hr;
                    return false;
                }
            }
            return true;
        }

        private static bool GenerateUnderWater(MeshData data, double minElevation, double elevationRange, Vector3 s, int hr, RealWorldTerrainTimer timer)
        {
            Vector3[] vertices = data.vertices;

            while (RealWorldTerrainElevationGenerator.hasUnderwater)
            {
                bool newHasUnderwater = false;
                bool fillMaxDepth = false;
                double prevDepth = (curDepth - minElevation) / elevationRange;
                curDepth -= RealWorldTerrainElevationGenerator.depthStep;
                if (curDepth <= prefs.nodataValue)
                {
                    curDepth = prefs.nodataValue;
                    fillMaxDepth = true;
                }

                float cDepth = (float)((curDepth - minElevation) / elevationRange * s.y);

                for (int hy = 0; hy < hr; hy++)
                {
                    bool ignoreTop = false;
                    int cy = hy * hr;
                    for (int hx = 0; hx < hr; hx++)
                    {
                        int cx = cy + hx;
                        if (Math.Abs(vertices[cx].y - float.MinValue) < float.Epsilon)
                        {
                            bool ignoreLeft = hx > 0 && vertices[cx - 1].y != prevDepth;
                            if (fillMaxDepth || RealWorldTerrainElevationGenerator.IsSingleDistance(hx, hy, ignoreLeft, ignoreTop))
                            {
                                vertices[cx].y = cDepth;
                                ignoreTop = true;
                            }
                            else
                            {
                                newHasUnderwater = true;
                                ignoreTop = false;
                            }
                        }
                        else ignoreTop = false;
                    }
                }

                RealWorldTerrainElevationGenerator.hasUnderwater = newHasUnderwater;
                if (timer.seconds > 1) return false;
            }
            return true;
        }

        public static void GenerateMesh(RealWorldTerrainItem item)
        {
            MeshData data = item["meshdata"] as MeshData;
            
            mesh = new Mesh
            {
                vertices = data.vertices,
                triangles = data.triangles,
                uv = data.uv
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            string id = item.name;
            string filename = Path.Combine(item.container.folder, id + ".asset");

            AssetDatabase.CreateAsset(mesh, filename);
            AssetDatabase.SaveAssets();

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public static void GenerateVertices(RealWorldTerrainItem item)
        {
            int hr = prefs.heightmapResolution;

            int thi = hr - 1;

            int verticesCount = hr * hr;

            MeshData data = new MeshData()
            {
                vertices = new Vector3[verticesCount],
                triangles = new int[thi * thi * 6],
                uv = new Vector2[verticesCount],
            };

            item["meshdata"] = data;

            double tx = item.leftLongitude;
            double ty = item.topLatitude;
            double bx = item.rightLongitude;
            double by = item.bottomLatitude;
            double minElevation = item.minElevation;
            double elevationRange = item.maxElevation - minElevation;

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            float thf = thi;
            Vector3 s = item.size;
            s.x /= thf;
            s.z /= thf;

            double scaledRange = elevationRange / s.y;
            float nodataDepth = (float)((prefs.nodataValue - minElevation) / scaledRange);

            if (!GenerateElevation(data, hr, thf, by, ty, s, tx, bx, minElevation, scaledRange, nodataDepth, thi, timer)) return;
            if (!GenerateUnderWater(data, minElevation, elevationRange, s, hr, timer)) return;

            lastY = 0;
            curDepth = 0;

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public static void InstantiateMeshes(RealWorldTerrainItem item)
        {
            MeshData data = item["meshdata"] as MeshData;
            Material mat = data.material;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));

                string matFilename = Path.Combine(item.container.folder, item.name) + ".mat";
                AssetDatabase.CreateAsset(mat, matFilename);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                data.material = mat = AssetDatabase.LoadAssetAtPath<Material>(matFilename);
            }

            string id = item.name;
            string filename = Path.Combine(item.container.folder, id + ".asset");
            Vector3 position = new Vector3(0, 0, nextZ);

            GameObject GO = new GameObject(id);
            GO.transform.parent = item.transform;
            GO.transform.localPosition = position;

            mesh = AssetDatabase.LoadAssetAtPath(filename, typeof(Mesh)) as Mesh;
            item.meshFilter = GO.AddComponent<MeshFilter>();
            item.meshFilter.sharedMesh = mesh;
            MeshCollider cl = GO.AddComponent<MeshCollider>();
            cl.sharedMesh = mesh;
            MeshRenderer meshRenderer = GO.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = mat;

            nextZ = cl.bounds.max.z - item.transform.position.z;

            lastY = 0;
            nextZ = 0;
            data.material = null;
            item["meshdata"] = null;

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public class MeshData
        {
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uv;
            public Material material;
        }
    }
}