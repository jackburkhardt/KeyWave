/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public abstract class RealWorldTerrainElevationGenerator
    {
        /// <summary>
        /// (Mercator X (0-1), Mercator Y (0-1), elevation value or double.MinValue if unknown, return modified elevation value or null) Called when an elevation value is received from the selected provider. Return null to use the value from the provider.
        /// </summary>
        public static Func<double, double, double, double?> OnElevationRetrieved;

        /// <summary>
        /// (Mercator X (0-1), Mercator Y (0-1), return Elevation Value or null) Allows you to intercept receiving elevation value from the selected provider. Return null to get the value from the selected provider.
        /// </summary>
        public static Func<double, double, double?> OnGetElevation;

        /// <summary>
        /// Allows you to intercept the range of elevation values for the generated area.
        /// </summary>
        public static OnGetElevationRangeDelegate OnGetElevationRange;

        public delegate void OnGetElevationRangeDelegate(out double minEl, out double maxEl);

        public static List<RealWorldTerrainElevationGenerator> elevations;
        public static float[,] tdataHeightmap;

        public static float depthStep;
        protected static RealWorldTerrainElevationGenerator lastElevation;
        protected static int lastX;
        protected static int mapSize;
        protected static int mapSize2;

        public static bool hasUnderwater;
        private static TerrainData tdata;

        public short[,] heightmap;
        public bool unziped;
        private static bool hasKnownValue;
        private static int countUnknownValues;
        private static bool hasMercatorBounds;
        private static double mx1, my1, mx2, my2;


        protected static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public virtual bool Contains(double X, double Y)
        {
            return false;
        }

        private bool Contains(Vector2 point)
        {
            return Contains(point.x, point.y);
        }

        public static void Dispose()
        {
            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps)
            {
                RealWorldTerrainBingElevationGenerator.Dispose();
            }
            /*else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.ArcGIS)
            {
                RealWorldTerrainArcGISElevationGenerator.Dispose();
            }*/
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM)
            {
                RealWorldTerrainSRTMElevationGenerator.Dispose();
            }
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30)
            {
                RealWorldTerrainSRTM30ElevationGenerator.Dispose();
            }

            hasMercatorBounds = false;
            tdataHeightmap = null;
            tdata = null;
            elevations = null;
            lastElevation = null;
            lastX = 0;
            countUnknownValues = 0;
        }

        public static void GenerateHeightMap(RealWorldTerrainItem item)
        {
            if (tdata == null) InitTData(item);

            double mx1 = item.leftMercator, my1 = item.topMercator, mx2 = item.rightMercator, my2 = item.bottomMercator;

            double minElevation = item.minElevation;
            double elevationRange = item.maxElevation - minElevation;

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            int r = tdata.heightmapResolution;

            double thx = (mx2 - mx1) / (r - 1);
            double thy = (my2 - my1) / (r - 1);

            for (int hx = lastX; hx < r; hx++)
            {
                double px = hx * thx + mx1;

                for (int hy = 0; hy < r; hy++)
                {
                    double py = hy * thy + my1;

                    double elevation = GetElevation(px, py);
                    int chy = r - hy - 1;

                    if (Math.Abs(elevation - double.MinValue) > double.Epsilon)
                    {
                        tdataHeightmap[chy, hx] = (float)((elevation - minElevation) / elevationRange);
                        hasKnownValue = true;
                    }
                    else if (!prefs.generateUnderWater)
                    {
                        tdataHeightmap[chy, hx] = (float)(-minElevation / elevationRange);
                        hasKnownValue = true;
                    }
                    else
                    {
                        hasUnderwater = true;
                        tdataHeightmap[chy, hx] = float.MinValue;
                        countUnknownValues++;
                    }
                }
                lastX = hx + 1;
                RealWorldTerrainPhase.phaseProgress = hx / (float)r;
                if (timer.seconds > 1) return;
            }

            float underwaterValue = (float)((prefs.nodataValue - minElevation) / elevationRange);

            if (hasKnownValue && hasUnderwater)
            {
                float[,] newHeightmap = new float[tdataHeightmap.GetLength(0), tdataHeightmap.GetLength(1)];
                int totalUnknownValues = countUnknownValues;
                bool showProgress = prefs.heightmapResolution > 256;

                byte[,] neighbormap = new byte[newHeightmap.GetLength(0), newHeightmap.GetLength(1)];
                byte[,] newNeighbormap = new byte[newHeightmap.GetLength(0), newHeightmap.GetLength(1)];

                for (int hx = 0; hx < r; hx++)
                {
                    for (int hy = 0; hy < r; hy++)
                    {
                        if (tdataHeightmap[hy, hx] > float.MinValue) neighbormap[hy, hx] = 1; // Has value
                        else
                        {
                            int countVals;
                            GetNeighborHeightValues(tdataHeightmap, hy, hx, out countVals);
                            if (countVals > 0) neighbormap[hy, hx] = 2; // Has neighbors
                            else neighbormap[hy, hx] = 0; // Has no neighbors
                        }

                        newNeighbormap[hy, hx] = neighbormap[hy, hx];
                    }
                }

                float pnc = (prefs.heightmapResolution - 1) / 32f / r;

                while (countUnknownValues > 0)
                {
                    if (showProgress)
                    {
                        float progress = (totalUnknownValues - countUnknownValues) / (float) totalUnknownValues;
                        if (EditorUtility.DisplayCancelableProgressBar("Generate Underwater Area", (progress * 100).ToString("F2") + "%", progress))
                        {
                            tdata = null;
                            EditorUtility.ClearProgressBar();
                            RealWorldTerrainWindow.CancelCapture();
                            return;
                        }
                    }

                    for (int hx = 0; hx < r; hx++)
                    {
                        for (int hy = 0; hy < r; hy++)
                        {
                            byte b = neighbormap[hy, hx];
                            if (b < 2)
                            {
                                newHeightmap[hy, hx] = tdataHeightmap[hy, hx];
                                continue;
                            }

                            int countVals;
                            float h = GetNeighborHeightValues(tdataHeightmap, hy, hx, out countVals);
                            if (countVals < 3)
                            {
                                newHeightmap[hy, hx] = tdataHeightmap[hy, hx];
                                continue;
                            }

                            float noise = (Mathf.PerlinNoise(pnc * hx, pnc * hy) * 3 + 1) / 4;
                            h -= depthStep * noise;
                            if (h < underwaterValue) h = underwaterValue;
                            newHeightmap[hy, hx] = h;
                            countUnknownValues--;

                            for (int cx = Mathf.Max(hx - 1, 0); cx < Mathf.Min(hx + 2, r); cx++)
                            {
                                for (int cy = Mathf.Max(hy - 1, 0); cy < Mathf.Min(hy + 2, r); cy++)
                                {
                                    if (newNeighbormap[cy, cx] == 0) newNeighbormap[cy, cx] = 2;
                                }
                            }

                            newNeighbormap[hy, hx] = 1;
                        }
                    }

                    float[,] tmp = tdataHeightmap;
                    tdataHeightmap = newHeightmap;
                    newHeightmap = tmp;

                    for (int hx = 0; hx < r; hx++)
                    {
                        for (int hy = 0; hy < r; hy++) neighbormap[hy, hx] = newNeighbormap[hy, hx];
                    }
                }

                if (showProgress) EditorUtility.ClearProgressBar();
            }
            else if (hasUnderwater)
            {
                for (int hx = 0; hx < r; hx++)
                {
                    for (int hy = 0; hy < r; hy++)
                    {
                        tdataHeightmap[hy, hx] = underwaterValue;
                    }
                }
            }

            lastX = 0;
            tdata.SetHeights(0, 0, tdataHeightmap);
            tdata = null;
            hasUnderwater = false;
            hasKnownValue = false;
            countUnknownValues = 0;

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public static double GetElevation(double x, double y, bool allowWaterTexture = true)
        {
            if (OnGetElevation != null)
            {
                double? r = OnGetElevation(x, y);
                if (r.HasValue) return r.Value;
            }

            bool useWaterTexture = allowWaterTexture && prefs.generateUnderWater && prefs.waterDetectionTexture != null;
            if (useWaterTexture)
            {
                try
                {
                    if (!hasMercatorBounds)
                    {
                        RealWorldTerrainUtils.LatLongToMercat(prefs.leftLongitude, prefs.topLatitude, out mx1, out my1);
                        RealWorldTerrainUtils.LatLongToMercat(prefs.rightLongitude, prefs.bottomLatitude, out mx2, out my2);
                        hasMercatorBounds = true;
                    }

                    double rx = (x - mx1) / (mx2 - mx1);
                    double ry = (my2 - y) / (my2 - my1);
                    Color c = prefs.waterDetectionTexture.GetPixelBilinear((float)rx, (float)ry);
                    if (c.r > 0.9f && c.g > 0.9f && c.b > 0.9f)
                    {
                        if (OnElevationRetrieved != null)
                        {
                            double? nv = OnElevationRetrieved(x, y, double.MinValue);
                            if (nv.HasValue) return nv.Value;
                        }

                        return double.MinValue;
                    }
                }
                catch
                {
                    
                }
            }

            double v = double.MinValue;

            if (lastElevation != null && lastElevation.Contains(x, y))
            {
                v = lastElevation.GetElevationValue(x, y);
            }
            else
            {
                foreach (RealWorldTerrainElevationGenerator el in elevations)
                {
                    if (el.Contains(x, y))
                    {
                        lastElevation = el;
                        v = el.GetElevationValue(x, y);
                        break;
                    }
                }
            }

            if (useWaterTexture && Math.Abs(v - double.MinValue) < double.Epsilon) v = 0;

            if (OnElevationRetrieved != null)
            {
                double? nv = OnElevationRetrieved(x, y, v);
                if (nv.HasValue) v = nv.Value;
            }

            return v;
        }

        public static void GetElevationRange(out double minEl, out double maxEl)
        {
            if (OnGetElevationRange != null)
            {
                OnGetElevationRange(out minEl, out maxEl);
                return;
            }

            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM) RealWorldTerrainSRTMElevationGenerator.GetSRTMElevationRange(out minEl, out maxEl);
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30) RealWorldTerrainSRTM30ElevationGenerator.GetSRTMElevationRange(out minEl, out maxEl);
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.Mapbox) RealWorldTerrainMapboxElevationGenerator.GetMapboxElevationRange(out minEl, out maxEl);
            //else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.ArcGIS) RealWorldTerrainArcGISElevationGenerator.GetArcGISElevationRange(out minEl, out maxEl);
            else RealWorldTerrainBingElevationGenerator.GetBingElevationRange(out minEl, out maxEl);
        }

        public virtual double GetElevationValue(double x, double y)
        {
            return double.MinValue;
        }

        private short GetFixedValue(int X, int Y)
        {
            short v = GetValue(X, Y);
            if (v == short.MinValue) v = 0;
            return v;
        }

        private static float GetNeighborHeightValues(float[,] heightmap, int y, int x, out int countVals)
        {
            float v;
            float val = 0;
            countVals = 0;
            int l1 = heightmap.GetLength(0) - 1;
            int l2 = heightmap.GetLength(1) - 1;
            if (y > 0)
            {
                v = heightmap[y - 1, x];
                if (v > float.MinValue)
                {
                    val += v * 2;
                    countVals += 2;
                }

                if (x > 0)
                {
                    v = heightmap[y - 1, x - 1];
                    if (v > float.MinValue)
                    {
                        val += v;
                        countVals++;
                    }
                }
                if (x < l2)
                {
                    v = heightmap[y - 1, x + 1];
                    if (v > float.MinValue)
                    {
                        val += v;
                        countVals++;
                    }
                }
            }
            if (y < l1)
            {
                v = heightmap[y + 1, x];
                if (v > float.MinValue)
                {
                    val += v;
                    countVals++;
                }

                if (x > 0)
                {
                    v = heightmap[y + 1, x - 1];
                    if (v > float.MinValue)
                    {
                        val += v;
                        countVals++;
                    }
                }
                if (x < l2)
                {
                    v = heightmap[y + 1, x + 1];
                    if (v > float.MinValue)
                    {
                        val += v;
                        countVals++;
                    }
                }
            }
            if (x > 0)
            {
                v = heightmap[y, x - 1];
                if (v > float.MinValue)
                {
                    val += v;
                    countVals++;
                }
            }
            if (x < l2)
            {
                v = heightmap[y, x + 1];
                if (v > float.MinValue)
                {
                    val += v;
                    countVals++;
                }
            }

            if (countVals > 0) val /= countVals;
            return val;
        }

        protected double GetSmoothElevation(
            double xs1, double xs2, double xp1, double ys1, double ys2, double yp1, int ix, int iy,
            double ox, int ixp1, double oy, int iyp1, double oxy, int ixs1, int iys1, int ixp2, int iyp2)
        {
            double result = xs1 * xs2 * xp1 * ys1 * ys2 * yp1 * 0.25 * GetFixedValue(ix, iy);
            result -= ox * xp1 * xs2 * ys1 * ys2 * yp1 * 0.25 * GetFixedValue(ixp1, iy);
            result -= oy * xs1 * xs2 * xp1 * yp1 * ys2 * 0.25 * GetFixedValue(ix, iyp1);
            result += oxy * xp1 * xs2 * yp1 * ys2 * 0.25 * GetFixedValue(ixp1, iyp1);
            result -= ox * xs1 * xs2 * ys1 * ys2 * yp1 / 12.0 * GetFixedValue(ixs1, iy);
            result -= oy * xs1 * xs2 * xp1 * ys1 * ys2 / 12.0 * GetFixedValue(ix, iys1);
            result += oxy * xs1 * xs2 * yp1 * ys2 / 12.0 * GetFixedValue(ixs1, iyp1);
            result += oxy * xp1 * xs2 * ys1 * ys2 / 12.0 * GetFixedValue(ixp1, iys1);
            result += ox * xs1 * xp1 * ys1 * ys2 * yp1 / 12.0 * GetFixedValue(ixp2, iy);
            result += oy * xs1 * xs2 * xp1 * ys1 * yp1 / 12.0 * GetFixedValue(ix, iyp2);
            result += oxy * xs1 * xs2 * ys1 * ys2 / 36.0 * GetFixedValue(ixs1, iys1);
            result -= oxy * xs1 * xp1 * yp1 * ys2 / 12.0 * GetFixedValue(ixp2, iyp1);
            result -= oxy * xp1 * xs2 * ys1 * yp1 / 12.0 * GetFixedValue(ixp1, iyp2);
            result -= oxy * xs1 * xp1 * ys1 * ys2 / 36.0 * GetFixedValue(ixp2, iys1);
            result -= oxy * xs1 * xs2 * ys1 * yp1 / 36.0 * GetFixedValue(ixs1, iyp2);
            result += oxy * xs1 * xp1 * ys1 * yp1 / 36.0 * GetFixedValue(ixp2, iyp2);
            return result;
        }

        protected virtual short GetValue(int x, int y)
        {
            if (x < 0) x = 0;
            else if (x > mapSize2) x = mapSize2;
            if (y < 0) y = 0;
            else if (y > mapSize2) y = mapSize2;
            short v = heightmap[x, y];
            return v;
        }

        private static void InitTData(RealWorldTerrainItem item)
        {
            tdata = item.terrain.terrainData;
            tdata.baseMapResolution = prefs.baseMapResolution;
            tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
            tdata.heightmapResolution = prefs.heightmapResolution;
            tdata.size = item.size;
            if (tdataHeightmap == null) tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];
            hasUnderwater = false;
            lastX = 0;
            countUnknownValues = 0;
        }

        public static bool IsSingleDistance(int X, int Y, bool ignoreLeft, bool ignoreTop)
        {
            int r = tdata.heightmapResolution;
            if (!ignoreTop && Y > 0 && Math.Abs(tdataHeightmap[Y - 1, X] - float.MinValue) > float.Epsilon) return true;
            if (Y < r - 1 && Math.Abs(tdataHeightmap[Y + 1, X] - float.MinValue) > float.Epsilon) return true;
            if (!ignoreLeft && X > 0 && Math.Abs(tdataHeightmap[Y, X - 1] - float.MinValue) > float.Epsilon) return true;
            if (X < r - 1 && Math.Abs(tdataHeightmap[Y, X + 1] - float.MinValue) > float.Epsilon) return true;
            return false;
        }

        public virtual void UnzipHeightmap()
        {
            
        }
    }
}