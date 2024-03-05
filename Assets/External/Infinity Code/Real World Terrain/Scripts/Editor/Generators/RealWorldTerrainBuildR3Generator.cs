/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

#if BUILDR3
using BuildRCities;
using BuildRCities.Scene;
using JSpace;
#endif

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainBuildR3Generator
    {
        public static List<string> alreadyCreated;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static void CreateBuilding(RealWorldTerrainContainer globalContainer, RealWorldTerrainOSMWay way)
        {
#if BUILDR3
            float minLng, minLat, maxLng, maxLat;
            List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(way, RealWorldTerrainBuildingGenerator.nodes, out minLng, out minLat, out maxLng, out maxLat);

            if (maxLng < prefs.leftLongitude ||
                maxLat < prefs.bottomLatitude ||
                minLng > prefs.rightLongitude ||
                minLat > prefs.topLatitude) return;

            if (points.Count < 3) return;

            if (points.First() == points.Last())
            {
                points.Remove(points.Last());
                if (points.Count < 3) return;
            }

            RealWorldTerrainBuildingGenerator.GetGlobalPoints(points, globalContainer);

            for (int i = 0; i < points.Count; i++)
            {
                int prev = i - 1;
                if (prev < 0) prev = points.Count - 1;

                int next = i + 1;
                if (next >= points.Count) next = 0;

                Vector3 p = points[i];
                Vector3 pp = points[prev];
                if ((pp - p).sqrMagnitude < 1f)
                {
                    points.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((points[next] - p).sqrMagnitude < 1f)
                {
                    points.RemoveAt(next);
                    continue;
                }

                float a1 = RealWorldTerrainUtils.Angle2D(pp, p);
                float a2 = RealWorldTerrainUtils.Angle2D(p, points[next]);

                if (Mathf.Abs(a1 - a2) < 5)
                {
                    points.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            if (points.Count < 3) return;

            Vector3 centerPoint = Vector3.zero;
            centerPoint = points.Aggregate(centerPoint, (current, point) => current + point) / points.Count;
            centerPoint.y = points.Min(p => p.y);

            int southIndex = -1;
            float southZ = float.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                points[i] -= centerPoint;

                if (points[i].z < southZ)
                {
                    southZ = points[i].z;
                    southIndex = i;
                }
            }

            int prevIndex = southIndex - 1;
            if (prevIndex < 0) prevIndex = points.Count - 1;

            int nextIndex = southIndex + 1;
            if (nextIndex >= points.Count) nextIndex = 0;

            float angle1 = RealWorldTerrainUtils.Angle2D(points[southIndex], points[nextIndex]);
            float angle2 = RealWorldTerrainUtils.Angle2D(points[southIndex], points[prevIndex]);

            if (angle1 < angle2) points.Reverse();

            GameObject house = RealWorldTerrainUtils.CreateGameObject(RealWorldTerrainBuildingGenerator.houseContainer, way.id);
            house.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);
            house.AddComponent<RealWorldTerrainBuildRBuilding>();
            Building building = house.AddComponent<Building>();

            Volume volume = building.content.AddNewVolume();

            int numberOfFloors = prefs.buildingFloorLimits.Random();

            if (way.HasTagKey("building:levels"))
            {
                int l;
                if (int.TryParse(way.GetTagValue("building:levels"), out l)) numberOfFloors = l;
            }

            if (numberOfFloors > 1) numberOfFloors -= 1;

            volume.Initialise(points.Select(p => new VolumePoint(new Vector2Fixed(p.x, p.z))).Reverse().ToList(), numberOfFloors, Random.Range(2.5f, 4f));

            volume.roof.hasDormers = true;
            volume.roof.floorDepth = Random.Range(0, 0.25f);
            volume.roof.minimumDormerSpacing = Random.Range(0.25f, 1f);
            volume.roof.dormerStyle.dormerHeight = volume.roof.height * 0.9f;

            if (prefs.buildR3Materials != null && prefs.buildR3Materials.Count > 0)
            {
                RealWorldTerrainBuildR3Material material = prefs.buildR3Materials[Random.Range(0, prefs.buildR3Materials.Count)];

                if (material.roofTexture != null) volume.roof.floorDynamicTextureId = material.roofTexture.content.buid;
                volume.roof.type = material.roofType;

                if (material.wallFacade != null)
                {
                    for (int i = 0; i < points.Count; i++) volume.SetExternalFacade(i, material.wallFacade.content);
                }
            }

            if (prefs.buildR3Collider) building.detailLevel.buildingColliderLod = BuildRLodLevels.Lod0;
            building.MarkModified();

            house.transform.position = centerPoint;
            house.AddComponent<VisualPart>();
#endif
        }

        public static void Generate(RealWorldTerrainContainer globalContainer)
        {
            if (!RealWorldTerrainBuildingGenerator.loaded)
            {
                RealWorldTerrainBuildingGenerator.Load();

                if (RealWorldTerrainBuildingGenerator.ways.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }

                if (RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
                {
                    RealWorldTerrainItem item = RealWorldTerrainWindow.generateTarget as RealWorldTerrainItem;
                    RealWorldTerrainBuildingGenerator.baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings " + item.x + "x" + (item.container.terrainCount.y - item.y - 1));
                    RealWorldTerrainBuildingGenerator.baseContainer.transform.position = item.transform.position;
                }
                else RealWorldTerrainBuildingGenerator.baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings");

                RealWorldTerrainBuildingGenerator.houseContainer = RealWorldTerrainUtils.CreateGameObject(RealWorldTerrainBuildingGenerator.baseContainer, "Houses");
                globalContainer.generatedBuildings = true;

                if (RealWorldTerrainBuildingGenerator.ways.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }

                alreadyCreated = new List<string>();
            }

            EditorUtility.DisplayProgressBar("Generate Buildings", "", 0);

            for (int i = RealWorldTerrainPhase.index; i < RealWorldTerrainBuildingGenerator.ways.Count; i++)
            {
                RealWorldTerrainOSMWay way = RealWorldTerrainBuildingGenerator.ways.Values.ElementAt(i);
                if (alreadyCreated.Contains(way.id)) continue;
                if (way.GetTagValue("building") == "bridge") continue;
                string layer = way.GetTagValue("layer");
                if (!String.IsNullOrEmpty(layer) && Int32.Parse(layer) < 0) continue;

                CreateBuilding(globalContainer, way);
                alreadyCreated.Add(way.id);

                float progress = (i + 1) / (float)RealWorldTerrainBuildingGenerator.ways.Count;
                if (EditorUtility.DisplayCancelableProgressBar("Generate Buildings " + (progress * 100).ToString("F2") + "%", "", progress)) break;
            }

            EditorUtility.ClearProgressBar();

            alreadyCreated = null;
            RealWorldTerrainPhase.phaseComplete = true;
        }
    }
}