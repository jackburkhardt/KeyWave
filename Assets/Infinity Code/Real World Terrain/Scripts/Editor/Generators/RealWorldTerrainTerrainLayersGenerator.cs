/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.RealWorldTerrain.Vector;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    using Feature = RealWorldTerrainVectorTile.Feature;
    using GeomType = RealWorldTerrainVectorTile.GeomType;
    using Layer = RealWorldTerrainVectorTile.Layer;
    using LPoint = RealWorldTerrainVectorTile.LPoint;
    using LPoints = List<RealWorldTerrainVectorTile.LPoint>;
    using LayerFeature = RealWorldTerrainVectorTerrainLayerFeature;

    public static class RealWorldTerrainTerrainLayersGenerator
    {
        private static Dictionary<LayerFeature, List<int>> dLayerIndices;
        private static List<LayerFeature> features;
        private static float[,,] map;
        private static int maxTextureLevel;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static Dictionary<ulong, RealWorldTerrainVectorTile> tiles;
        private static Vector3[] tempPoints;

        public static void Dispose()
        {
            if (tiles != null)
            {
                foreach (var pair in tiles) pair.Value.Dispose();
                tiles = null;
            }
        }

        private static void FillWater(int isx, int isy, int iex, int iey)
        {
            List<int> indices = null;
            Vector2 noiseOffset = Vector2.zero;
            float noiseScale = 16;
            int res = prefs.controlTextureResolution;

            for (int i = features.Count - 1; i >= 0; i--)
            {
                var feature = features[i];
                foreach (LayerFeature.Rule rule in feature.rules)
                {
                    if (rule.layer == RealWorldTerrainMapboxLayer.water)
                    {
                        noiseOffset = feature.noiseOffset / res;
                        noiseScale = feature.noiseScale / res;
                        indices = dLayerIndices[feature];
                        break;
                    }
                }
                if (indices != null) break;
            }

            if (indices == null) return;

            for (int y = isy; y < iey; y++)
            {
                int cy = res - y - 1;
                for (int x = isx; x < iex; x++)
                {
                    if (indices.Count > 1)
                    {
                        float noise = Mathf.PerlinNoise(noiseScale * x + noiseOffset.x, noiseScale * y + noiseOffset.y) * (indices.Count - 1);
                        int i1 = (int)noise;
                        int i2 = i1 + 1;
                        float v1 = 1 - (noise - i1);
                        map[cy, x, indices[i1]] = v1;
                        if (i2 < indices.Count) map[cy, x, indices[i2]] = 1 - v1;
                    }
                    else map[cy, x, indices[0]] = 1;
                }
            }
        }

        private static void FinalizeControlMap()
        {
            List<int>[] mapIndices = dLayerIndices.Values.ToArray();
            int res = prefs.controlTextureResolution;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    bool hasValue = false;
                    for (int i = mapIndices.Length - 1; i >= 0; i--)
                    {
                        List<int> ris = mapIndices[i];
                        for (int j = 0; j < ris.Count; j++)
                        {
                            if (map[y, x, ris[j]] > 0)
                            {
                                hasValue = true;
                                break;
                            }
                        }

                        if (hasValue)
                        {
                            for (int j = ris[0] - 1; j >= 0; j--) map[y, x, j] = 0;
                            break;
                        }
                    }
                }
            }
        }

        public static void Generate(RealWorldTerrainItem item)
        {
            int size = 1 << maxTextureLevel;
            int res = prefs.controlTextureResolution;

            List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
            terrainLayers.AddRange(prefs.vectorTerrainBaseLayers);
            dLayerIndices = new Dictionary<LayerFeature, List<int>>();
            features = new List<LayerFeature>(prefs.vectorTerrainLayers);

            InitFeatureLayers(terrainLayers);

            item.terrainData.terrainLayers = terrainLayers.ToArray();
            map = new float[res, res, terrainLayers.Count];

            InitBaseLayers();

            double rangeX = res / (item.rightMercator - item.leftMercator) / size;
            double rangeY = res / (item.bottomMercator - item.topMercator) / size;

            tempPoints = new Vector3[64];

            foreach (var pair in tiles)
            {
                RealWorldTerrainVectorTile tile = pair.Value;
                if (item.rightMercator * size < tile.x) continue;
                if (item.bottomMercator * size < tile.y) continue;
                if (item.leftMercator * size > tile.x + 1) continue;
                if (item.topMercator * size > tile.y + 1) continue;

                GenerateTile(item, tile, size, rangeX, rangeY);
            }

            FinalizeControlMap();

            item.terrainData.SetAlphamaps(0, 0, map);
            item.terrainData.SetBaseMapDirty();
            RealWorldTerrainPhase.phaseComplete = true;
        }

        private static void GenerateGeometry(LPoints points, ulong extent, double ex, double sx, double ey, double sy, int res, int isx, int isy, int iex, int iey, List<int> indices, Vector2 noiseOffset, float noiseScale)
        {
            int count = points.Count;
            if (count > tempPoints.Length) Array.Resize(ref tempPoints, Mathf.NextPowerOfTwo(count));

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < count; i++)
            {
                LPoint p = points[i];
                Vector3 wp = tempPoints[i] = new Vector3((float) (ex - sx) * p.x / extent + (float) sx, 0, (float) (ey - sy) * p.y / extent + (float) sy);
                if (wp.x < minX) minX = wp.x;
                if (wp.z < minZ) minZ = wp.z;
                if (wp.x > maxX) maxX = wp.x;
                if (wp.z > maxZ) maxZ = wp.z;
            }

            if (maxX < 0 || maxZ < 0) return;
            if (minX >= res || minZ >= res) return;

            int csx = isx;
            int csy = isy;
            int cex = iex;
            int cey = iey;

            if (csx < minX) csx = (int) minX;
            if (csy < minZ) csy = (int) minZ;
            if (cex > maxX) cex = Mathf.CeilToInt(maxX);
            if (cey > maxZ) cey = Mathf.CeilToInt(maxZ);

            bool isClockWise = RealWorldTerrainUtils.IsClockwise(tempPoints, count);
            if (!isClockWise)
            {
                for (int y = csy; y < cey; y++)
                {
                    int cy = res - y - 1;
                    for (int x = csx; x < cex; x++)
                    {
                        if (RealWorldTerrainUtils.IsPointInPolygon(tempPoints, count, x, y))
                        {
                            if (indices.Count > 1)
                            {
                                float noise = Mathf.PerlinNoise(noiseScale * x + noiseOffset.x, noiseScale * y + noiseOffset.y) * (indices.Count - 1);
                                int i1 = (int) noise;
                                int i2 = i1 + 1;
                                float v1 = 1 - (noise - i1);
                                map[cy, x, indices[i1]] = v1;
                                if (i2 < indices.Count) map[cy, x, indices[i2]] = 1 - v1;
                            }
                            else map[cy, x, indices[0]] = 1;
                        }
                    }
                }
            }
            else
            {
                for (int y = csy; y < cey; y++)
                {
                    int cy = res - y - 1;
                    for (int x = csx; x < cex; x++)
                    {
                        if (RealWorldTerrainUtils.IsPointInPolygon(tempPoints, count, x, y))
                        {
                            foreach (int i in indices) map[cy, x, i] = 0;
                        }
                    }
                }
            }
        }

        private static void GenerateLayer(RealWorldTerrainItem item, Layer layer, double ex, double sx, double ey, double sy, int isx, int isy, int iex, int iey)
        {
            int res = prefs.controlTextureResolution;

            for (int f = 0; f < layer.featureCount; f++)
            {
                Feature feature = layer.GetFeature(f);
                if (feature.geometryType == GeomType.POLYGON) GeneratePolygon(item, layer, ex, sx, ey, sy, isx, isy, iex, iey, feature, res);
                else if (layer.name == "road" && feature.geometryType == GeomType.LINESTRING) GenerateLineSting(item, layer, ex, sx, ey, sy, isx, isy, iex, iey, feature, res);
            }
        }

        private static void GenerateLine(LPoint p1, LPoint p2, ulong extent, double ex, double sx, double ey, double sy, int res, int isx, int isy, int iex, int iey, List<int> indices, Vector2 noiseOffset, float noiseScale, float width)
        {
            int count = 2;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            float hw = width / 2;
            float sqrHW = hw * hw;

            for (int i = 0; i < count; i++)
            {
                LPoint p = i == 0? p1: p2;
                Vector3 wp = tempPoints[i] = new Vector3((float)(ex - sx) * p.x / extent + (float)sx, 0, (float)(ey - sy) * p.y / extent + (float)sy);
                if (wp.x - hw < minX) minX = wp.x - hw;
                if (wp.z - hw < minZ) minZ = wp.z - hw;
                if (wp.x + hw > maxX) maxX = wp.x + hw;
                if (wp.z + hw > maxZ) maxZ = wp.z + hw;
            }

            if (maxX < 0 || maxZ < 0) return;
            if (minX >= res || minZ >= res) return;

            int csx = isx;
            int csy = isy;
            int cex = iex;
            int cey = iey;

            if (csx < minX) csx = (int)minX;
            if (csy < minZ) csy = (int)minZ;
            if (cex > maxX) cex = Mathf.CeilToInt(maxX);
            if (cey > maxZ) cey = Mathf.CeilToInt(maxZ);

            Vector2 wp1 = new Vector2(tempPoints[0].x, tempPoints[0].z);
            Vector2 wp2 = new Vector2(tempPoints[1].x, tempPoints[1].z);

            for (int y = csy; y < cey; y++)
            {
                int cy = res - y - 1;
                for (int x = csx; x < cex; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    Vector2 np = RealWorldTerrainUtils.NearestPointStrict(p, wp1, wp2);

                    if ((p - np).sqrMagnitude < sqrHW)
                    {
                        if (indices.Count > 1)
                        {
                            float noise = Mathf.PerlinNoise(noiseScale * x + noiseOffset.x, noiseScale * y + noiseOffset.y) * (indices.Count - 1);
                            int i1 = (int) noise;
                            int i2 = i1 + 1;
                            float v1 = 1 - (noise - i1);
                            map[cy, x, indices[i1]] = v1;
                            if (i2 < indices.Count) map[cy, x, indices[i2]] = 1 - v1;
                        }
                        else map[cy, x, indices[0]] = 1;
                    }
                }
            }
        }

        private static void GenerateLineSting(RealWorldTerrainItem item, Layer layer, double ex, double sx, double ey, double sy, int isx, int isy, int iex, int iey, Feature feature, int res)
        {
            var cl = GetVectorTerrainLayer(layer, feature);
            if (cl == null) return;

            float widthMul = 1;

            if (layer.name == "road")
            {
                Dictionary<string, object> properties = feature.GetProperties();
                object classObj, structObj, typeObj;
                if (properties.TryGetValue("class", out classObj))
                {
                    string @class = classObj as string;
                    if (@class == null) return;

                    string @struct;
                    if (properties.TryGetValue("structure", out structObj))
                    {
                        @struct = structObj as string;
                    }
                    else @struct = "No Structure";

                    string type;
                    if (properties.TryGetValue("type", out typeObj))
                    {
                        type = typeObj as string;
                    }
                    else type = "No Type";

                    if (@class == "tertiary")
                    {
                        
                    }
                    else if (@class == "secondary" || @class == "street")
                    {
                        widthMul = 0.5f;
                    }
                    else if (@class == "service")
                    {
                        {
                            if (@struct != null)
                            {
                                if (@struct == "tunnel") return;
                            }
                        }

                        widthMul = 0.5f;
                    }
                    else if (@class == "major_rail" || @class == "service_rail" || @class == "pedestrian" || @class == "path")
                    {
                        return;
                    }
                    else widthMul = 0.3f;
                }
                else return;
            }
            else return;

            Vector2 noiseOffset = cl.noiseOffset / res;
            float noiseScale = cl.noiseScale / res;

            List<int> indices = dLayerIndices[cl];

            float lineWidth = item.prefs.controlTextureResolution / 128f * widthMul;

            List<LPoints> geometry = feature.Geometry();
            if (geometry.Count == 0) return;

            foreach (LPoints points in geometry)
            {
                if (points.Count < 2) continue;

                for (int i = 1; i < points.Count; i++)
                {
                    LPoint p1 = points[i - 1];
                    LPoint p2 = points[i];

                    GenerateLine(p1, p2, layer.extent, ex, sx, ey, sy, res, isx, isy, iex, iey, indices, noiseOffset, noiseScale, lineWidth);
                }
            }
        }

        private static void GeneratePolygon(RealWorldTerrainItem item, Layer layer, double ex, double sx, double ey, double sy, int isx, int isy, int iex, int iey, Feature feature, int res)
        {
            var cl = GetVectorTerrainLayer(layer, feature);
            if (cl == null) return;

            if (layer.name == "road")
            {
                Dictionary<string, object> properties = feature.GetProperties();
                object layerObj;
                if (properties.TryGetValue("layer", out layerObj))
                {
                    string layerProp = layerObj.ToString();
                    if (int.Parse(layerProp) < 0) return;
                }
            }

            List<LPoints> geometry = feature.Geometry();
            if (geometry.Count == 0) return;

            Vector2 noiseOffset = cl.noiseOffset / res;
            float noiseScale = cl.noiseScale / res;

            List<int> indices = dLayerIndices[cl];

            foreach (LPoints points in geometry)
            {
                if (points.Count < 3) continue;

                GenerateGeometry(points, layer.extent, ex, sx, ey, sy, res, isx, isy, iex, iey, indices, noiseOffset, noiseScale);
            }
        }

        private static void GenerateTile(RealWorldTerrainItem item, RealWorldTerrainVectorTile tile, int size, double rangeX, double rangeY)
        {
            int res = prefs.controlTextureResolution;

            double sx = (tile.x - item.leftMercator * size) * rangeX;
            double ex = sx + rangeX;
            double sy = (tile.y - item.topMercator * size) * rangeY;
            double ey = sy + rangeY;

            int isx = (int) Math.Round(sx);
            int iex = (int) Math.Round(ex);
            int isy = (int) Math.Round(sy);
            int iey = (int) Math.Round(ey);

            if (isx < 0) isx = 0;
            if (isy < 0) isy = 0;
            if (iex >= res) iex = res;
            if (iey >= res) iey = res;

            tile.Load();

            List<string> layers = tile.GetLayerNames();

            if (layers == null || layers.Count == 0)
            {
                FillWater(isx, isy, iex, iey);
                tile.Dispose();
                return;
            }

            for (int l = 0; l < layers.Count; l++)
            {
                string layerName = layers[l];
                if (layerName.Contains("label")) continue;

                Layer layer = tile.GetLayer(layerName);

                GenerateLayer(item, layer, ex, sx, ey, sy, isx, isy, iex, iey);
            }

            tile.Dispose();
        }

        private static LayerFeature GetVectorTerrainLayer(Layer layer, Feature feature)
        {
            string layerName = layer.name;

            for (int i = features.Count - 1; i >= 0; i--)
            {
                var l = features[i];

                for (int j = 0; j < l.rules.Count; j++)
                {
                    var rule = l.rules[j];
                    if (rule.layerName != layerName) continue;

                    if (!rule.hasExtra || rule.extra == ~0) return l;

                    Dictionary<string, object> properties = feature.GetProperties();
                    object fClass;
                    if (!properties.TryGetValue("class", out fClass)) continue;

                    List<string> layerFeature;
                    if (rule.layer == RealWorldTerrainMapboxLayer.landuse)
                    {
                        layerFeature = LayerFeature.landuseNames;
                    }
                    else if (rule.layer == RealWorldTerrainMapboxLayer.landuse_overlay)
                    {
                        layerFeature = LayerFeature.landuseOverlayNames;
                    }
                    else if (rule.layer == RealWorldTerrainMapboxLayer.structure)
                    {
                        layerFeature = LayerFeature.structureNames;
                    }
                    else if (rule.layer == RealWorldTerrainMapboxLayer.waterway)
                    {
                        layerFeature = LayerFeature.waterwayNames;
                    }
                    else continue;

                    int index = layerFeature.IndexOf((string) fClass);
                    if (index == -1) continue;

                    BitArray ba = new BitArray(BitConverter.GetBytes(rule.extra));
                    if (ba.Get(index)) return l;
                }
            }

            return null;
        }

        public static void Init()
        {
            int textureLevel;
            if (prefs.maxTextureLevel == 0)
            {
                textureLevel = 0;

                int tx = prefs.controlTextureResolution * prefs.terrainCount.x / 256;
                int ty = prefs.controlTextureResolution * prefs.terrainCount.y / 256;

                for (int z = 5; z < 24; z++)
                {
                    double stx, sty, etx, ety;
                    RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, z, out stx, out sty);
                    RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, z, out etx, out ety);

                    if (etx < stx) etx += 1 << z;

                    if (etx - stx > tx && ety - sty > ty)
                    {
                        textureLevel = z;
                        break;
                    }
                }

                if (textureLevel == 0) textureLevel = 24;
            }
            else textureLevel = prefs.maxTextureLevel;

            maxTextureLevel = textureLevel;

            tiles = new Dictionary<ulong, RealWorldTerrainVectorTile>();

            int max = 1 << maxTextureLevel;

            double tlx, tly, brx, bry;
            RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, maxTextureLevel, out tlx, out tly);
            RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, maxTextureLevel, out brx, out bry);

            int itlx = (int) tlx;
            int itly = (int) tly;
            int ibrx = (int) Math.Ceiling(brx);
            int ibry = (int) Math.Ceiling(bry);

            if (itlx > ibrx) ibrx += max;

            for (int x = itlx; x < ibrx; x++)
            {
                int cx = x;
                if (cx >= max) cx -= max;

                for (int y = itly; y < ibry; y++)
                {
                    RealWorldTerrainVectorTile tile = new RealWorldTerrainVectorTile(cx, y, maxTextureLevel);
                    tile.Download();
                    tiles.Add(tile.key, tile);
                }
            }
        }

        private static void InitBaseLayers()
        {
            int countBaseLayers = prefs.vectorTerrainBaseLayers.Count;
            int res = prefs.controlTextureResolution;

            if (countBaseLayers > 1)
            {
                Vector2 noiseOffset = prefs.vectorTerrainBaseLayersNoiseOffset / res;
                float noiseScale = prefs.vectorTerrainBaseLayersNoiseScale / res;

                for (int x = 0; x < res; x++)
                {
                    for (int y = 0; y < res; y++)
                    {
                        for (int k = 0; k < countBaseLayers; k++)
                        {
                            float noise = Mathf.PerlinNoise(noiseScale * x + noiseOffset.x, noiseScale * y + noiseOffset.y) * (countBaseLayers - 1);
                            int i1 = (int) noise;
                            int i2 = i1 + 1;
                            float v1 = 1 - (noise - i1);
                            map[y, x, i1] = v1;
                            if (i2 < countBaseLayers) map[y, x, i2] = 1 - v1;
                            map[y, x, k] = 1;
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < res; x++)
                {
                    for (int y = 0; y < res; y++)
                    {
                        map[y, x, 0] = 1;
                    }
                }
            }
        }

        private static void InitFeatureLayers(List<TerrainLayer> terrainLayers)
        {
            int nextIndex = terrainLayers.Count;

            for (int i = 0; i < features.Count; i++)
            {
                LayerFeature layer = features[i];
                List<int> indices = new List<int>();

                foreach (TerrainLayer terrainLayer in layer.terrainLayers)
                {
                    if (terrainLayer != null)
                    {
                        int index = nextIndex++;
                        indices.Add(index);
                        terrainLayers.Add(terrainLayer);
                    }
                }

                if (indices.Count > 0) dLayerIndices.Add(layer, indices);
                else
                {
                    features.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}