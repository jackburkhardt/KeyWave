/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// This class contains basic information about the building.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RealWorldTerrainBuilding : MonoBehaviour
    {
        /// <summary>
        /// The height of the walls.
        /// </summary>
        public float baseHeight;

        /// <summary>
        /// Array of base vertices.
        /// </summary>
        public Vector3[] baseVertices;

        /// <summary>
        /// Reference to RealWorldTerrainContainer instance.
        /// </summary>
        public RealWorldTerrainContainer container;

        /// <summary>
        /// ID of the building
        /// </summary>
        public string id;

        /// <summary>
        /// Indicates that roof normals is inverted.
        /// </summary>
        public bool invertRoof;

        /// <summary>
        /// Indicates that walls normals is inverted.
        /// </summary>
        public bool invertWall;

        /// <summary>
        /// Height of roof.
        /// </summary>
        public float roofHeight;

        /// <summary>
        /// Type of roof.
        /// </summary>
        public RealWorldTerrainRoofType roofType;

        /// <summary>
        /// Whether to generate the wall?
        /// </summary>
        public bool generateWall;

        /// <summary>
        /// Material of the roof.
        /// </summary>
        public Material roofMaterial;

        public float startHeight = 0;

        /// <summary>
        /// Size of a tile texture in meters.
        /// </summary>
        public Vector2 tileSize = new Vector2(30, 30);

        public Vector2 uvOffset = Vector2.zero;

        /// <summary>
        /// Material of the wall.
        /// </summary>
        public Material wallMaterial;

        private MeshFilter _meshFilter;

        /// <summary>
        /// Reference to MeshFilter of the building.
        /// </summary>
        public MeshFilter meshFilter
        {
            get
            {
                if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
                return _meshFilter;
            }
        }

        /// <summary>
        /// Reference to MeshFilter of roof.
        /// </summary>
        [Obsolete("Use meshFilter instead.")]
        public MeshFilter roof
        {
            get { return meshFilter; }
        }

        /// <summary>
        /// Reference to MeshFilter of wall.
        /// </summary>
        [Obsolete("Use meshFilter instead.")]
        public MeshFilter wall
        {
            get { return meshFilter; }
        }

        private void CreateRoofDome(List<Vector3> vertices, List<int> triangles)
        {
            Vector3 roofTopPoint = Vector3.zero;
            roofTopPoint = vertices.Aggregate(roofTopPoint, (current, point) => current + point) / vertices.Count;
            roofTopPoint.y = (baseHeight + roofHeight) * container.scale.y;
            int vIndex = vertices.Count;

            for (int i = 0; i < vertices.Count; i++)
            {
                int p1 = i;
                int p2 = i + 1;
                if (p2 >= vertices.Count) p2 -= vertices.Count;

                triangles.AddRange(new[] { p1, p2, vIndex });
            }

            vertices.Add(roofTopPoint);
        }

        private void CreateRoofMesh(List<Vector3> vertices, out List<Vector2> uv, out List<int> triangles)
        {
            List<Vector2> roofPoints = CreateRoofVertices(vertices);
            triangles = CreateRoofTriangles(vertices, roofPoints);

            if (invertRoof) triangles.Reverse();

            float minX = vertices.Min(p => p.x);
            float minZ = vertices.Min(p => p.z);
            float maxX = vertices.Max(p => p.x);
            float maxZ = vertices.Max(p => p.z);
            float offX = maxX - minX;
            float offZ = maxZ - minZ;

            uv = vertices.Select(v => new Vector2((v.x - minX) / offX, (v.z - minZ) / offZ)).ToList();
        }

        private List<int> CreateRoofTriangles(List<Vector3> vertices, List<Vector2> roofPoints)
        {
            List<int> triangles = new List<int>();
            if (roofType == RealWorldTerrainRoofType.flat)
            {
                int[] trs = RealWorldTerrainTriangulator.Triangulate(roofPoints);
                if (trs != null) triangles.AddRange(trs);
            }
            else if (roofType == RealWorldTerrainRoofType.dome)
            {
                CreateRoofDome(vertices, triangles);
            }
            return triangles;
        }

        private List<Vector2> CreateRoofVertices(List<Vector3> vertices)
        {
            Vector3[] targetVertices = new Vector3[baseVertices.Length];
            Array.Copy(baseVertices, targetVertices, baseVertices.Length);

            if (container.prefs.buildingBottomMode == RealWorldTerrainBuildingBottomMode.followTerrain)
            {
                Vector3 tp = transform.position;
                RealWorldTerrainItem terrainItem = container.GetItemByWorldPosition(baseVertices[0] + tp);
                if (terrainItem != null)
                {
                    TerrainData t = terrainItem.terrainData;

                    Vector3 offset = tp - terrainItem.transform.position;

                    for (int i = 0; i < targetVertices.Length; i++)
                    {
                        Vector3 v = targetVertices[i];
                        Vector3 localPos = offset + v;
                        float y = t.GetInterpolatedHeight(localPos.x / t.size.x, localPos.z / t.size.z);
                        v.y = terrainItem.transform.position.y + y - tp.y;
                        targetVertices[i] = v;
                    }
                }
            }

            List<Vector2> roofPoints = new List<Vector2>();
            float topPoint = targetVertices.Max(v => v.y) + baseHeight * container.scale.y;
            foreach (Vector3 p in targetVertices)
            {
                Vector3 tv = new Vector3(p.x, topPoint, p.z);
                Vector2 rp = new Vector2(p.x, p.z);

                vertices.Add(tv);
                roofPoints.Add(rp);
            }

            return roofPoints;
        }

        private void CreateWallMesh(List<Vector3> vertices, List<Vector2> uv, out List<int> triangles)
        {
            List<Vector3> wv = new List<Vector3>();
            List<Vector2> wuv = new List<Vector2>();
            bool reversed = CreateWallVertices(wv, wuv);
            if (invertWall) reversed = !reversed;
            triangles = CreateWallTriangles(wv, vertices.Count, reversed);
            vertices.AddRange(wv);
            uv.AddRange(wuv);
        }

        private List<int> CreateWallTriangles(List<Vector3> vertices, int offset, bool reversed)
        {
            List<int> triangles = new List<int>();
            for (int i = 0; i < vertices.Count / 4; i++)
            {
                int p1 = i * 4;
                int p2 = i * 4 + 2;
                int p3 = i * 4 + 3;
                int p4 = i * 4 + 1;

                if (p2 >= vertices.Count) p2 -= vertices.Count;
                if (p3 >= vertices.Count) p3 -= vertices.Count;

                p1 += offset;
                p2 += offset;
                p3 += offset;
                p4 += offset;

                if (reversed)
                {
                    triangles.AddRange(new[] { p1, p4, p3, p1, p3, p2 });
                }
                else
                {
                    triangles.AddRange(new[] { p2, p3, p1, p3, p4, p1 });
                }
            }
            return triangles;
        }

        private bool CreateWallVertices(List<Vector3> vertices, List<Vector2> uv)
        {
            Vector3[] targetVertices = new Vector3[baseVertices.Length];
            Array.Copy(baseVertices, targetVertices, baseVertices.Length);

            if (container.prefs.buildingBottomMode == RealWorldTerrainBuildingBottomMode.followTerrain)
            {
                Vector3 tp = transform.position;
                RealWorldTerrainItem terrainItem = container.GetItemByWorldPosition(baseVertices[0] + tp);
                if (terrainItem != null)
                {
                    TerrainData t = terrainItem.terrainData;

                    Vector3 offset = tp - terrainItem.transform.position;

                    for (int i = 0; i < targetVertices.Length; i++)
                    {
                        Vector3 v = targetVertices[i];
                        Vector3 localPos = offset + v;
                        float y = t.GetInterpolatedHeight(localPos.x / t.size.x, localPos.z / t.size.z);
                        v.y = terrainItem.transform.position.y + y - tp.y;
                        targetVertices[i] = v;
                    }
                }
            }

            float topPoint = targetVertices.Max(v => v.y) + baseHeight * container.scale.y;

            float startY = startHeight * container.scale.y;
            float offsetY = startY < 0 ? startY : 0;

            for (int i = 0; i < targetVertices.Length; i++)
            {
                Vector3 p1 = targetVertices[i];
                Vector3 p2 = i < targetVertices.Length - 1 ? targetVertices[i + 1] : targetVertices[0];
                if (p1.y < startY) p1.y = startY;
                if (p2.y < startY) p2.y = startY;
                p1.y += offsetY;
                p2.y += offsetY;
                vertices.Add(p1);
                vertices.Add(new Vector3(p1.x, topPoint, p1.z));
                vertices.Add(p2);
                vertices.Add(new Vector3(p2.x, topPoint, p2.z));
            }

            float totalDistance = 0;
            float bottomPoint = float.MaxValue;

            for (int i = 0; i < vertices.Count / 4; i++)
            {
                int i1 = Mathf.RoundToInt(Mathf.Repeat(i * 4, vertices.Count));
                int i2 = Mathf.RoundToInt(Mathf.Repeat((i + 1) * 4, vertices.Count));
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                v1.y = v2.y = 0;
                totalDistance += (v1 - v2).magnitude;
                if (bottomPoint > targetVertices[i].y) bottomPoint = targetVertices[i].y;
            }

            Vector3 lv1 = vertices[vertices.Count - 4];
            Vector3 lv2 = vertices[0];
            lv1.y = lv2.y = 0;
            totalDistance += (lv1 - lv2).magnitude;

            float currentDistance = 0;
            float nextU = 0;
            float uMul = totalDistance / tileSize.x;
            float vMax = topPoint / tileSize.y;
            float vMinMul = container.scale.y * tileSize.y;

            for (int i = 0; i < vertices.Count / 4; i++)
            {
                int i1 = Mathf.RoundToInt(Mathf.Repeat(i * 4, vertices.Count));
                int i2 = Mathf.RoundToInt(Mathf.Repeat((i + 1) * 4, vertices.Count));
                float curU = nextU;
                uv.Add(new Vector2(curU * uMul + uvOffset.x, (vertices[i * 4].y - bottomPoint) / vMinMul + uvOffset.y));
                uv.Add(new Vector2(curU * uMul + uvOffset.x, vMax + uvOffset.y));

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                v1.y = v2.y = 0;
                currentDistance += (v1 - v2).magnitude;
                nextU = currentDistance / totalDistance;

                uv.Add(new Vector2(nextU * uMul + uvOffset.x, (vertices[i * 4 + 2].y - bottomPoint) / vMinMul + uvOffset.y));
                uv.Add(new Vector2(nextU * uMul + uvOffset.x, vMax + uvOffset.y)); 
            }

            int southIndex = -1;
            float southZ = float.MaxValue;

            for (int i = 0; i < targetVertices.Length; i++)
            {
                if (targetVertices[i].z < southZ)
                {
                    southZ = targetVertices[i].z;
                    southIndex = i;
                }
            }

            int prevIndex = southIndex - 1;
            if (prevIndex < 0) prevIndex = targetVertices.Length - 1;

            int nextIndex = southIndex + 1;
            if (nextIndex >= targetVertices.Length) nextIndex = 0;

            float angle1 = RealWorldTerrainUtils.Angle2D(targetVertices[southIndex], targetVertices[nextIndex]);
            float angle2 = RealWorldTerrainUtils.Angle2D(targetVertices[southIndex], targetVertices[prevIndex]);

            return angle1 < angle2;
        }

        public void Generate()
        {
            Mesh mesh;
            if (meshFilter.sharedMesh != null) mesh = meshFilter.sharedMesh;
            else
            {
                mesh = new Mesh();
                mesh.name = "Building " + id;
                mesh.subMeshCount = 2;
                meshFilter.sharedMesh = mesh;
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv;
            List<int> roofTriangles;
            List<int> wallTriangles = null;

            CreateRoofMesh(vertices, out uv, out roofTriangles);
            if (generateWall) CreateWallMesh(vertices, uv, out wallTriangles);

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(roofTriangles, 0);
            if (generateWall) mesh.SetTriangles(wallTriangles, 1);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GetComponent<MeshRenderer>().materials = new[]
            {
                roofMaterial,
                wallMaterial,
            };
        }
    }
}