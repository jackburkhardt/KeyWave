/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainBuildingGenerator
    {
        private const int multiRequestZoom = 12;

        public static Func<List<Vector3>, RealWorldTerrainOSMWay, Dictionary<string, RealWorldTerrainOSMNode>, bool> OnGenerateBuilding;

        public static GameObject baseContainer;
        public static GameObject houseContainer;

        private static Material defHouseRoofMaterial;
        private static Material defHouseWallMaterial;

        public static Dictionary<string, RealWorldTerrainOSMNode> nodes;
        public static Dictionary<string, RealWorldTerrainOSMWay> ways;
        public static List<RealWorldTerrainOSMRelation> relations;
        public static bool loaded;

        private static string url
        {
            get
            {
                string format = string.Format(RealWorldTerrainCultureInfo.numberFormat, "(way['building']({0},{1},{2},{3});relation['building']({0},{1},{2},{3}););out;>;out skel qt;", prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude);
                return RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(format);
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
                return Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("buildings_{0}_{1}_{2}_{3}.osm", prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude));
            }
        }

        public static string compressedFilename
        {
            get
            {
                return filename + "c";
            }
        }

        private static void AddHole(RealWorldTerrainContainer globalContainer, List<Vector3> input, RealWorldTerrainOSMWay hole)
        {
            List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(hole, nodes);
            if (points.Count < 3) return;
            if (points.First() == points.Last())
            {
                points.Remove(points.Last());
                if (points.Count < 3) return;
            }

            GetGlobalPoints(points, globalContainer);

            for (int i = 0; i < points.Count; i++)
            {
                int prev = i - 1;
                if (prev < 0) prev = points.Count - 1;

                int next = i + 1;
                if (next >= points.Count) next = 0;

                if ((points[prev] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((points[next] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(next);
                    continue;
                }

                float a1 = RealWorldTerrainUtils.Angle2D(points[prev], points[i]);
                float a2 = RealWorldTerrainUtils.Angle2D(points[i], points[next]);

                if (Mathf.Abs(a1 - a2) < 5)
                {
                    points.RemoveAt(i);
                    i--;
                }
            }

            if (points.Count < 3) return;

            if (!IsReversed(points))
            {
                points.Reverse();
            }

            float closestDistance = float.MaxValue;
            int closestIndex1 = -1;
            int closestIndex2 = -1;

            int holeCount = points.Count;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < holeCount; i++)
            {
                Vector3 p = points[i];
                float px = p.x;
                float pz = p.z;

                if (px < minX) minX = px;
                if (px > maxX) maxX = px;
                if (pz < minZ) minZ = pz;
                if (pz > maxZ) maxZ = pz;
            }

            float cx = (maxX + minX) / 2;
            float cz = (maxZ + minZ) / 2;

            for (int i = 0; i < input.Count; i++)
            {
                Vector3 p = input[i];
                float px = p.x;
                float pz = p.z;
                float distance = (px - cx) * (px - cx) + (pz - cz) * (pz - cz);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex1 = i;
                }
            }

            cx = input[closestIndex1].x;
            cz = input[closestIndex1].z;
            closestDistance = float.MaxValue;

            for (int i = 0; i < holeCount; i++)
            {
                Vector3 p = points[i];
                float px = p.x;
                float pz = p.z;
                float distance = (px - cx) * (px - cx) + (pz - cz) * (pz - cz);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex2 = i;
                }
            }

            int firstPartSize = holeCount - closestIndex2;
            input.Insert(closestIndex1, input[closestIndex1]);
            closestIndex1++;
            input.InsertRange(closestIndex1, points.Skip(closestIndex2).Take(firstPartSize));
            input.InsertRange(closestIndex1 + firstPartSize, points.Take(closestIndex2 + 1));
        }

        private static void AddHoles(RealWorldTerrainContainer globalContainer, RealWorldTerrainOSMWay way, List<Vector3> points)
        {
            for (int i = 0; i < way.holes.Count; i++)
            {
                RealWorldTerrainOSMWay hole = way.holes[i];
                if (hole.nodeRefs.First() == hole.nodeRefs.Last()) AddHole(globalContainer, points, hole);
                else
                {
                    while (true)
                    {
                        bool success = false;
                        for (int j = i + 1; j < way.holes.Count; j++)
                        {
                            RealWorldTerrainOSMWay h2 = way.holes[j];
                            if (hole.nodeRefs.Last() == h2.nodeRefs.First())
                            {
                                hole.nodeRefs.AddRange(h2.nodeRefs.Skip(1));
                                way.holes.RemoveAt(j);
                                success = true;
                                break;
                            }

                            if (hole.nodeRefs.Last() == h2.nodeRefs.Last())
                            {
                                h2.nodeRefs.Reverse();
                                hole.nodeRefs.AddRange(h2.nodeRefs.Skip(1));
                                way.holes.RemoveAt(j);
                                success = true;
                                break;
                            }
                        }

                        if (!success) break;

                        if (hole.nodeRefs.First() == hole.nodeRefs.Last())
                        {
                            AddHole(globalContainer, points, hole);
                            break;
                        }
                    }
                }
            }
        }

        private static void AnalyzeHouseRoofType(RealWorldTerrainOSMWay way, ref float baseHeight,
            ref RealWorldTerrainRoofType roofType, ref float roofHeight)
        {
            string roofShape = way.GetTagValue("roof:shape");
            string roofHeightStr = way.GetTagValue("roof:height");
            string minHeightStr = way.GetTagValue("min_height");
            if (!string.IsNullOrEmpty(roofShape))
            {
                if ((roofShape == "dome" || roofShape == "pyramidal") && !String.IsNullOrEmpty(roofHeightStr))
                {
                    GetHeightFromString(roofHeightStr, ref roofHeight);
                    baseHeight -= roofHeight;
                    roofType = RealWorldTerrainRoofType.dome;
                }
            }
            else if (!string.IsNullOrEmpty(roofHeightStr))
            {
                GetHeightFromString(roofHeightStr, ref roofHeight);
                baseHeight -= roofHeight;
                roofType = RealWorldTerrainRoofType.dome;
            }
            else if (!string.IsNullOrEmpty(minHeightStr))
            {
                float totalHeight = baseHeight;
                GetHeightFromString(minHeightStr, ref baseHeight);
                roofHeight = totalHeight - baseHeight;
                roofType = RealWorldTerrainRoofType.dome;
            }
        }

        private static void AnalyzeHouseTags(RealWorldTerrainOSMWay way, ref Material wallMaterial, ref Material roofMaterial,
            ref float baseHeight, bool useDefaultMaterials, ref bool saveMaterials)
        {
            string heightStr = way.GetTagValue("height");
            string levelsStr = way.GetTagValue("building:levels");
            GetHeightFromString(heightStr, ref baseHeight);
            if (string.IsNullOrEmpty(heightStr))
            {
                if (!string.IsNullOrEmpty(levelsStr))
                {
                    float h;
                    if (float.TryParse(levelsStr, NumberStyles.AllowDecimalPoint, RealWorldTerrainCultureInfo.cultureInfo, out h)) baseHeight = h * RealWorldTerrainWindow.prefs.buildingFloorHeight;
                }
                else baseHeight = RealWorldTerrainWindow.prefs.buildingFloorLimits.Random() * RealWorldTerrainWindow.prefs.buildingFloorHeight;
            }

            if (prefs.buildingUseColorTags)
            {
                string colorStr = way.GetTagValue("building:colour");
                if (useDefaultMaterials && !string.IsNullOrEmpty(colorStr))
                {
                    Color color = RealWorldTerrainUtils.StringToColor(colorStr);
                    if (color != wallMaterial.color)
                    {
                        if (!saveMaterials)
                        {
                            saveMaterials = true;
                            wallMaterial = Object.Instantiate(wallMaterial);
                            roofMaterial = Object.Instantiate(roofMaterial);
                        }
                        wallMaterial.color = roofMaterial.color = color;
                    }
                }
            }
        }

        private static void CreateHouse(RealWorldTerrainOSMWay way, RealWorldTerrainContainer globalContainer)
        {
            float minLng, minLat, maxLng, maxLat;
            List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(way, nodes, out minLng, out minLat, out maxLng, out maxLat);
            if (points.Count < 3) return;

            if (maxLng < prefs.leftLongitude ||
                maxLat < prefs.bottomLatitude ||
                minLng > prefs.rightLongitude ||
                minLat > prefs.topLatitude) return;

            if (points.First() == points.Last())
            {
                points.Remove(points.Last());
                if (points.Count < 3) return;
            }

            GetGlobalPoints(points, globalContainer);

            for (int i = 0; i < points.Count; i++)
            {
                int prev = i - 1;
                if (prev < 0) prev = points.Count - 1;

                int next = i + 1;
                if (next >= points.Count) next = 0;

                if ((points[prev] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((points[next] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(next);
                    continue;
                }

                float a1 = RealWorldTerrainUtils.Angle2D(points[prev], points[i]);
                float a2 = RealWorldTerrainUtils.Angle2D(points[i], points[next]);

                if (Mathf.Abs(a1 - a2) < 5)
                {
                    points.RemoveAt(i);
                    i--;
                }
            }

            if (points.Count < 3) return;
            if (IsReversed(points)) points.Reverse();

            if (OnGenerateBuilding != null)
            {
                try
                {
                    if (OnGenerateBuilding(points, way, nodes)) return;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return;
                }
            }

            Vector3 centerPoint = Vector3.zero;
            centerPoint = points.Aggregate(centerPoint, (current, point) => current + point) / points.Count;
            centerPoint.y = points.Min(p => p.y);

            if (way.holes != null) AddHoles(globalContainer, way, points);

            float baseHeight = 15;
            float roofHeight = 0;

            RealWorldTerrainBuildingMaterial buildingMaterial = null;
            Material wallMaterial;
            Material roofMaterial;
            bool saveMaterials = false;

            bool useDefaultMaterials = true;

            if (prefs.buildingMaterials == null || prefs.buildingMaterials.Count == 0)
            {
                wallMaterial = GetMaterialByTags("OSM-House-Wall-Material", way.tags, defHouseWallMaterial);
                roofMaterial = GetMaterialByTags("OSM-House-Roof-Material", way.tags, defHouseRoofMaterial);
            }
            else
            {
                useDefaultMaterials = false;
                int rnd = Random.Range(0, prefs.buildingMaterials.Count);
                buildingMaterial = prefs.buildingMaterials[rnd];
                wallMaterial = buildingMaterial.wall != null ? buildingMaterial.wall : defHouseWallMaterial;
                roofMaterial = buildingMaterial.roof != null ? buildingMaterial.roof : defHouseRoofMaterial;
            }

            wallMaterial = wallMaterial != null ? wallMaterial : defHouseWallMaterial;
            roofMaterial = roofMaterial != null ? roofMaterial : defHouseRoofMaterial;

            RealWorldTerrainRoofType roofType = RealWorldTerrainRoofType.flat;
            AnalyzeHouseTags(way, ref wallMaterial, ref roofMaterial, ref baseHeight, useDefaultMaterials, ref saveMaterials);
            AnalyzeHouseRoofType(way, ref baseHeight, ref roofType, ref roofHeight);

            Vector3[] baseVertices = points.Select(p => p - centerPoint).ToArray();

            GameObject houseGO = RealWorldTerrainUtils.CreateGameObject(houseContainer, "House " + way.id);
            houseGO.transform.position = centerPoint;

            RealWorldTerrainBuilding house = prefs.dynamicBuildings ? houseGO.AddComponent<RealWorldTerrainDynamicBuilding>() : houseGO.AddComponent<RealWorldTerrainBuilding>();
            house.baseHeight = baseHeight;
            house.baseVertices = baseVertices;
            house.startHeight = -prefs.buildingBasementDepth;
            house.container = RealWorldTerrainWindow.container;
            house.roofHeight = roofHeight;
            house.roofType = roofType;
            house.generateWall = true;
            house.wallMaterial = wallMaterial;
            house.roofMaterial = roofMaterial;
            if (buildingMaterial != null)
            {
                house.tileSize = buildingMaterial.tileSize;
            }
            house.id = way.id;

            if (way.HasTagKey("building"))
            {
                string buildingType = way.GetTagValue("building");
                if (buildingType == "roof") house.generateWall = false;
            }

            house.Generate();

            string folder = RealWorldTerrainWindow.container.folder + "/Buildings/";

            if (prefs.buildingSaveInResult)
            {
                if (saveMaterials)
                {
                    string materialsFolder = folder + "Materials/";
                    if (!Directory.Exists(materialsFolder)) Directory.CreateDirectory(materialsFolder);

                    string roofMaterialPath = materialsFolder + "House " + way.id + " Roof.mat";

                    MeshRenderer meshRenderer = house.GetComponent<MeshRenderer>();
                    RealWorldTerrainSaveBuildingsPhase.renderersToSave.Add(new RealWorldTerrainSaveBuildingsPhase.SavableRenderer
                    {
                        index = 0,
                        path = roofMaterialPath,
                        renderer = meshRenderer
                    });

                    if (house.generateWall)
                    {
                        string wallMaterialPath = materialsFolder + "House " + way.id + " Wall.mat";

                        RealWorldTerrainSaveBuildingsPhase.renderersToSave.Add(new RealWorldTerrainSaveBuildingsPhase.SavableRenderer
                        {
                            index = 1,
                            path = wallMaterialPath,
                            renderer = meshRenderer
                        });
                    }
                }

                if (!prefs.dynamicBuildings)
                {
                    string meshesPath = folder + "Meshes/";
                    if (!Directory.Exists(meshesPath)) Directory.CreateDirectory(meshesPath);

                    string wallMeshPath = meshesPath + "House " + way.id + ".asset";
                    RealWorldTerrainSaveBuildingsPhase.filtersToSave.Add(house.meshFilter, wallMeshPath);
                }
            }

            houseGO.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);
        }

        public static void Dispose()
        {
            loaded = false;

            ways = null;
            nodes = null;
            relations = null;

            defHouseRoofMaterial = null;
            defHouseWallMaterial = null;

            baseContainer = null;
            houseContainer = null;
        }

        public static void Download()
        {
            if (!prefs.generateBuildings) return;
            if (prefs.buildingSingleRequest) DownloadSingleRequest();
            else DownloadMultiRequests();
        }

        private static void DownloadMultiRequests()
        {
            double tlx, tly, brx, bry;

            RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, multiRequestZoom, out tlx, out tly);
            RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, multiRequestZoom, out brx, out bry);

            int itlx = (int)tlx;
            int itly = (int)tly;
            int ibrx = (int)brx + 1;
            int ibry = (int)bry + 1;

            int maxX = 1 << multiRequestZoom;
            if (itlx > ibrx) ibrx += maxX;

            for (int x = itlx; x < ibrx; x++)
            {
                int cx = x;
                if (cx >= maxX) cx -= maxX;

                for (int y = itly; y < ibry; y++)
                {
                    RealWorldTerrainUtils.TileToLatLong(cx, y, multiRequestZoom, out tlx, out tly);
                    RealWorldTerrainUtils.TileToLatLong(cx + 1, y + 1, multiRequestZoom, out brx, out bry);

                    string fn = Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("buildings_{0}_{1}_{2}_{3}.osm", bry, tlx, tly, brx));
                    string cfn = fn + "c";
                    if (File.Exists(cfn)) continue;
                    if (File.Exists(fn))
                    {
                        byte[] data = File.ReadAllBytes(fn);
                        OnDownloadComplete(ref data, cfn);
                    }
                    else
                    {
                        string format = string.Format(RealWorldTerrainCultureInfo.numberFormat, "(way['building']({0},{1},{2},{3});relation['building']({0},{1},{2},{3}););out;>;out skel qt;", bry, tlx, tly, brx);
                        string partURL = RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(format);

                        RealWorldTerrainDownloadItemUnityWebRequest item = new RealWorldTerrainDownloadItemUnityWebRequest(partURL)
                        {
                            filename = fn,
                            averageSize = 600000,
                            exclusiveLock = RealWorldTerrainOSMUtils.OSMLocker,
                            ignoreRequestProgress = true
                        };

                        item.OnData += delegate (ref byte[] data)
                        {
                            OnDownloadComplete(ref data, cfn);
                        };
                    }
                }
            }
        }

        private static void DownloadSingleRequest()
        {
            if (File.Exists(compressedFilename)) return;
            if (File.Exists(filename))
            {
                byte[] data = File.ReadAllBytes(filename);
                OnDownloadComplete(ref data, compressedFilename);
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

                item.OnData += delegate (ref byte[] data)
                {
                    OnDownloadComplete(ref data, compressedFilename);
                };
            }
        }

        public static string FixPathString(string path)
        {
            return RealWorldTerrainUtils.ReplaceString(path, new[] { ":", "/", "\\", "=" }, "-");
        }

        public static void Generate(RealWorldTerrainContainer globalContainer)
        {
            if (!loaded)
            {
                Load();

                if (ways.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }

                if (RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
                {
                    RealWorldTerrainItem item = RealWorldTerrainWindow.generateTarget as RealWorldTerrainItem;
                    baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings " + item.x + "x" + (item.container.terrainCount.y - item.y - 1));
                    baseContainer.transform.position = item.transform.position;
                }
                else baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings");
                houseContainer = RealWorldTerrainUtils.CreateGameObject(baseContainer, "Houses");

                defHouseWallMaterial = RealWorldTerrainEditorUtils.FindMaterial("Default-House-Wall-Material.mat");
                defHouseRoofMaterial = RealWorldTerrainEditorUtils.FindMaterial("Default-House-Roof-Material.mat");
                globalContainer.generatedBuildings = true;
            }

            GenerateHouses(globalContainer);

            if (!RealWorldTerrainPhase.phaseComplete) RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)(ways.Count);
            else RealWorldTerrainPhase.phaseProgress = 1;
        }

        private static void GenerateHouses(RealWorldTerrainContainer globalContainer)
        {
            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            while (RealWorldTerrainPhase.index < ways.Count)
            {
                if (timer.seconds > 1) return;

                RealWorldTerrainOSMWay way = ways.Values.ElementAt(RealWorldTerrainPhase.index);
                RealWorldTerrainPhase.index++;

                if (way.GetTagValue("building") == "bridge") continue;
                string layer = way.GetTagValue("layer");
                if (!string.IsNullOrEmpty(layer))
                {
                    int l;
                    if (int.TryParse(layer, out l) && l < 0) continue;
                }

                CreateHouse(way, globalContainer);
            }

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public static void GetGlobalPoints(List<Vector3> points, RealWorldTerrainContainer globalContainer)
        {
            bool hasNoValuePoints = false;
            bool[] noValues = new bool[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                bool success;
                Vector3 p = RealWorldTerrainEditorUtils.CoordsToWorldWithElevation(points[i], globalContainer, Vector3.zero, out success);
                noValues[i] = !success;
                if (!success) hasNoValuePoints = true;
                points[i] = p;
            }

            if (!hasNoValuePoints) return;

            float sy = 0;
            int cy = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (!noValues[i])
                {
                    sy += points[i].y;
                    cy++;
                }
            }

            if (cy > 0) sy /= cy;
            else sy = RealWorldTerrainWindow.prefs.nodataValue * globalContainer.scale.y;

            for (int i = 0; i < points.Count; i++)
            {
                if (noValues[i])
                {
                    Vector3 p = points[i];
                    p.y = sy;
                    points[i] = p;
                }
            }
        }

        public static void GetHeightFromString(string str, ref float height)
        {
            if (string.IsNullOrEmpty(str)) return;
            if (float.TryParse(str, NumberStyles.AllowDecimalPoint, RealWorldTerrainCultureInfo.cultureInfo, out height)) return;

            if (str.Substring(str.Length - 2, 2) == "cm")
            {
                float.TryParse(str.Substring(0, str.Length - 2), NumberStyles.AllowDecimalPoint, RealWorldTerrainCultureInfo.cultureInfo, out height);
                height /= 10;
            }
            else if (str.Substring(str.Length - 1, 1) == "m") float.TryParse(str.Substring(0, str.Length - 1), NumberStyles.AllowDecimalPoint, RealWorldTerrainCultureInfo.cultureInfo, out height);
        }

        public static Material GetMaterialByTags(string materialName, List<RealWorldTerrainOSMTag> tags, Material defaultMaterial)
        {
            if (RealWorldTerrainOSMUtils.projectMaterials == null)
            {
                RealWorldTerrainOSMUtils.projectMaterials = Directory.GetFiles("Assets", "*.mat", SearchOption.AllDirectories);
            }

            foreach (RealWorldTerrainOSMTag tag in tags)
            {
                string matName = string.Format("{0}({1}={2})", materialName, FixPathString(tag.key), FixPathString(tag.value));
                foreach (string projectMaterial in RealWorldTerrainOSMUtils.projectMaterials)
                {
                    if (projectMaterial.Contains(matName))
                    {
                        string assetFN = projectMaterial.Replace("\\", "/");
                        return AssetDatabase.LoadAssetAtPath<Material>(assetFN);
                    }
                }
            }

            return defaultMaterial;
        }

        private static bool IsReversed(List<Vector3> points)
        {
            float minZ = float.MaxValue;
            int i2 = -1;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p = points[i];
                if (p.z < minZ)
                {
                    minZ = p.z;
                    i2 = i;
                }
            }

            int i1 = i2 - 1;
            int i3 = i2 + 1;
            if (i1 < 0) i1 += points.Count;
            if (i3 >= points.Count) i3 -= points.Count;

            Vector3 p1 = points[i1];
            Vector3 p2 = points[i2];
            Vector3 p3 = points[i3];

            Vector3 s1 = p2 - p1;
            Vector3 s2 = p3 - p1;

            Vector3 side1 = s1;
            Vector3 side2 = s2;

            return Vector3.Cross(side1, side2).y <= 0;
        }

        public static void Load()
        {
            if (prefs.buildingSingleRequest) LoadSinglePart();
            else LoadMultiParts();
            loaded = true;
        }

        private static void LoadSinglePart()
        {
            RealWorldTerrainOSMUtils.LoadOSM(compressedFilename, out nodes, out ways, out relations);
        }

        private static void LoadMultiParts()
        {
            double tlx, tly, brx, bry;

            RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, multiRequestZoom, out tlx, out tly);
            RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, multiRequestZoom, out brx, out bry);

            int itlx = (int)tlx;
            int itly = (int)tly;
            int ibrx = (int)brx + 1;
            int ibry = (int)bry + 1;

            int maxX = 1 << multiRequestZoom;
            if (itlx > ibrx) ibrx += maxX;

            nodes = new Dictionary<string, RealWorldTerrainOSMNode>();
            ways = new Dictionary<string, RealWorldTerrainOSMWay>();
            relations = new List<RealWorldTerrainOSMRelation>();

            for (int x = itlx; x < ibrx; x++)
            {
                int cx = x;
                if (cx >= maxX) cx -= maxX;

                for (int y = itly; y < ibry; y++)
                {
                    RealWorldTerrainUtils.TileToLatLong(cx, y, multiRequestZoom, out tlx, out tly);
                    RealWorldTerrainUtils.TileToLatLong(cx + 1, y + 1, multiRequestZoom, out brx, out bry);

                    string fn = Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("buildings_{0}_{1}_{2}_{3}.osm", bry, tlx, tly, brx));
                    string cfn = fn + "c";

                    Dictionary<string, RealWorldTerrainOSMNode> ns;
                    Dictionary<string, RealWorldTerrainOSMWay> ws;
                    List<RealWorldTerrainOSMRelation> rs;
                    RealWorldTerrainOSMUtils.LoadOSM(cfn, out ns, out ws, out rs);

                    foreach (KeyValuePair<string, RealWorldTerrainOSMNode> pair in ns)
                    {
                        if (!nodes.ContainsKey(pair.Key)) nodes.Add(pair.Key, pair.Value);
                    }

                    foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ws)
                    {
                        if (!ways.ContainsKey(pair.Key)) ways.Add(pair.Key, pair.Value);
                    }
                }
            }
        }

        private static void OnDownloadComplete(ref byte[] data, string fn)
        {
            nodes = null;
            ways = null;
            relations = null;
            RealWorldTerrainOSMUtils.GenerateCompressedFile(data, ref nodes, ref ways, ref relations, fn);
        }
    }
}