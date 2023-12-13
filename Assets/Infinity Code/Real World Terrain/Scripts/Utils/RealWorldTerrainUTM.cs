/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Text.RegularExpressions;

namespace InfinityCode.RealWorldTerrain
{
    public static class RealWorldTerrainUTM
    {
        const double flattening = 298.257223563;
        const double equatorialRadius = 6378137;

        private static double DegToRad(double deg)
        {
            double pi = Math.PI;
            return deg / 180 * pi;
        }

        private static double FootpointLatitude(double y)
        {
            const double r = equatorialRadius;
            const double v1 = r * (1.0 - 1.0 / flattening);
            const double x = (r - v1) / (r + v1);
            double v2 = (r + v1) / 2.0 * (1.0 + Math.Pow(x, 2.0) / 4.0 + Math.Pow(x, 4.0) / 64.0);
            double v3 = y / v2;
            double v4 = 3.0 * x / 2.0 + -27.0 * Math.Pow(x, 3.0) / 32.0 + 269.0 * Math.Pow(x, 5.0) / 512.0;
            double v5 = 21.0 * Math.Pow(x, 2.0) / 16.0 + -55.0 * Math.Pow(x, 4.0) / 32.0;
            double v6 = 151.0 * Math.Pow(x, 3.0) / 96.0 + -417.0 * Math.Pow(x, 5.0) / 128.0;
            double v7 = 1097.0 * Math.Pow(x, 4.0) / 512.0;
            return v3 + v4 * Math.Sin(2.0 * v3) + v5 * Math.Sin(4.0 * v3) + v6 * Math.Sin(6.0 * v3) + v7 * Math.Sin(8.0 * v3);
        }

        private static void Geodetic_To_UPS(double lng, double lat, out string latZone, out int lngZone, out double easting, out double northing)
        {
            if (Math.Abs(lng + 180.0) < double.Epsilon) lng = 180.0;

            double v1 = lat * Math.PI / 180.0;
            double v2 = lng * Math.PI / 180.0;
            const double x = 1.0 / flattening;
            const double v3 = equatorialRadius * (1.0 - x);
            double v4 = Math.Sqrt(2.0 * x - Math.Pow(x, 2.0));
            double v5 = 2.0 * (equatorialRadius * equatorialRadius) / v3 * Math.Pow((1.0 - v4) / (1.0 + v4), v4 / 2.0) * Math.Tan(Math.PI / 4.0 - Math.Abs(v1) / 2.0) * Math.Pow((1.0 + v4 * Math.Sin(Math.Abs(v1))) / (1.0 - v4 * Math.Sin(Math.Abs(v1))), v4 / 2.0);
            double v6 = 2000000.0 + 0.994 * v5 * Math.Sin(v2);
            double v7 = lat <= 0.0 ? 2000000.0 + 0.994 * v5 * Math.Cos(v2) : 2000000.0 - 0.994 * v5 * Math.Cos(v2);
            string str = lat < 0.0 ? (lng >= 0.0 || lng == -180.0 || lat == -90.0 ? "B" : "A") : (lng >= 0.0 || lng == -180.0 || lat == 90.0 ? "Z" : "Y");
            easting = v6;
            northing = v7;
            lngZone = 0;
            latZone = str;
        }

        private static double RadToDeg(double rad)
        {
            return rad / Math.PI * 180.0;
        }

        public static void ToLngLat(string latZone, int lngZone, double easting, double northing, out double lng, out double lat)
        {
            if (new Regex("[AaBbYyZz]").IsMatch(latZone))
            {
                UPS_To_Geodetic(latZone, easting, northing, out lng, out lat);
                return;
            }
            bool isNorthHemisphere = new Regex("[CcDdEeFfGgHhJjKkLlMm]").IsMatch(latZone);
            double v1 = easting - 500000.0;
            const double v2 = 0.9996;
            double x = v1 / v2;
            if (isNorthHemisphere) northing -= 10000000.0;
            UTMtoLatLong(x, northing / v2, UTMCentralMeridian(lngZone), out lng, out lat);
        }

        public static void ToUTM(double lng, double lat, out string latZone, out int lngZone, out double easting, out double northing)
        {
            if (lat < -80.0 || lat > 84.0)
            {
                Geodetic_To_UPS(lng, lat, out latZone, out lngZone, out easting, out northing);
                return;
            }

            const double c0 = 0.9996;
            const double c1 = Math.PI / 180.0;
            const double c2 = 1.0 / flattening;
            const double c3 = equatorialRadius * (1.0 - c2);
            const double c4 = 1.0 - c3 / equatorialRadius * (c3 / equatorialRadius);

            int v1 = (int)Math.Floor(lng / 6.0 + 31.0);
            string str = lat >= -72.0 ? (lat >= -64.0 ? (lat >= -56.0 ? (lat >= -48.0 ? (lat >= -40.0 ? (lat >= -32.0 ? (lat >= -24.0 ? (lat >= -16.0 ? (lat >= -8.0 ? (lat >= 0.0 ? (lat >= 8.0 ? (lat >= 16.0 ? (lat >= 24.0 ? (lat >= 32.0 ? (lat >= 40.0 ? (lat >= 48.0 ? (lat >= 56.0 ? (lat >= 64.0 ? (lat >= 72.0 ? "X" : "W") : "V") : "U") : "T") : "S") : "R") : "Q") : "P") : "N") : "M") : "L") : "K") : "J") : "H") : "G") : "F") : "E") : "D") : "C";
            double v2 = Math.Sqrt(1.0 - Math.Pow(c3, 2.0) / Math.Pow(equatorialRadius, 2.0));
            double v3 = lat * c1;
            double v4 = 3.0 + 6.0 * (1.0 + Math.Floor((lng + 180.0) / 6.0) - 1.0) - 180.0;
            double v5 = v2 * v2 / (1.0 - Math.Pow(v2, 2.0));
            double v6 = equatorialRadius / Math.Sqrt(1.0 - Math.Pow(v2 * Math.Sin(v3), 2.0));
            double v7 = Math.Pow(Math.Tan(v3), 2.0);
            double v8 = v5 * Math.Pow(Math.Cos(v3), 2.0);
            double v9 = (lng - v4) * c1 * Math.Cos(v3);
            double v10 = (v3 * (1.0 - c4 * (0.25 + c4 * (3.0 / 64.0 + 5.0 * c4 / 256.0))) - Math.Sin(2.0 * v3) * (c4 * (0.375 + c4 * (3.0 / 32.0 + 45.0 * c4 / 1024.0))) + Math.Sin(4.0 * v3) * (c4 * c4 * (15.0 / 256.0 + c4 * 45.0 / 1024.0)) - Math.Sin(6.0 * v3) * (c4 * c4 * c4 * 0.0113932291666667)) * equatorialRadius;
            double v11 = c0 * v6 * v9 * (1.0 + v9 * v9 * ((1.0 - v7 + v8) / 6.0 + v9 * v9 * (5.0 - 18.0 * v7 + v7 * v7 + 72.0 * v8 - 58.0 * v5) / 120.0)) + 500000.0;
            double v12 = c0 * (v10 + v6 * Math.Tan(v3) * (v9 * v9 * (0.5 + v9 * v9 * ((5.0 - v7 + 9.0 * v8 + 4.0 * v8 * v8) / 24.0 + v9 * v9 * (61.0 - 58.0 * v7 + v7 * v7 + 600.0 * v8 - 330.0 * v5) / 720.0))));
            latZone = str;
            lngZone = v1 == 61? 1: v1;
            easting = v11;
            northing = v12 < 0.0? 10000000.0 + v12: v12;
        }

        private static void UPS_To_Geodetic(string latZone, double easting, double northing, out double lng, out double lat)
        {
            const double x = 1.0 / flattening;
            const double v1 = equatorialRadius * (1.0 - x);
            double v2 = Math.Sqrt(2 * x - Math.Pow(x, 2));
            double v3 = (easting - 2000000) / 0.994;
            double v4 = (northing - 2000000) / 0.994;
            double v5 = v4;
            if (v4 == 0.0) v4 = 1;

            bool flag = latZone.ToUpper() != "Z" && latZone.ToUpper() != "Y";

            double d1;
            double d2;
            if (flag)
            {
                d1 = Math.PI + Math.Atan(v3 / v5);
                d2 = Math.PI + Math.Atan(v3 / v4);
            }
            else
            {
                d1 = Math.PI - Math.Atan(v3 / v5);
                d2 = Math.PI - Math.Atan(v3 / v4);
            }
            double v6 = 2.0 * Math.Pow(equatorialRadius, 2.0) / v1 * Math.Pow((1.0 - v2) / (1.0 + v2), v2 / 2.0);
            double v7 = Math.Abs(v4);
            double v8 = Math.Abs(Math.Cos(d2));
            double v9 = v6 * v8;
            double y = Math.Log(v7 / v9) / Math.Log(Math.E) * -1.0;
            double v10 = 2.0 * Math.Atan(Math.Pow(Math.E, y)) - Math.PI / 2.0;
            double d3;
            double v11;
            double v12;
            for (d3 = 0.0; Math.Abs(v10 - d3) > 1E-07 && !double.IsInfinity(v10); v10 -= v11 / v12)
            {
                d3 = v10;
                double d4 = (1.0 + Math.Sin(v10)) / (1.0 - Math.Sin(v10)) * Math.Pow((1.0 - v2 * Math.Sin(v10)) / (1.0 + v2 * Math.Sin(v10)), v2);
                v11 = -y + 0.5 * Math.Log(d4);
                v12 = (1.0 - Math.Pow(v2, 2.0)) / ((1.0 - Math.Pow(v2, 2.0) * Math.Pow(Math.Sin(v10), 2.0)) * Math.Cos(v10));
            }
            if (!double.IsInfinity(v10)) d3 = v10;
            lat = !double.IsNaN(d3) ? d3 * (180.0 / Math.PI) : 90.0;
            if (flag) lat *= -1.0;
            lng = !double.IsNaN(d1) ? d1 * (180.0 / Math.PI) : 0.0;
            if (easting < 2000000.0) lng = (180.0 - lng % 180.0) * -1.0;
            else if (lng > 180.0) lng -= 180.0;
            else if (lng < -180.0) lng += 180.0;
            if (((northing < 2000000.0 ? 0 : v3 == 0.0 ? 1 : 0) & (flag ? 1 : 0)) != 0) lng = 0.0;
            if (northing < 2000000.0 && v3 == 0.0 && !flag) lng = 0.0;
        }

        private static double UTMCentralMeridian(double zone)
        {
            return DegToRad(zone * 6 - 183);
        }

        private static void UTMtoLatLong(double x, double y, double zone, out double lng, out double lat)
        {
            const double r = equatorialRadius;
            const double x2 = r * (1 - 1 / flattening);
            const double v1 = (r * r - x2 * x2) / (x2 * x2);

            double fpLat = FootpointLatitude(y);
            double x3 = Math.Cos(fpLat);
            double v2 = Math.Pow(x3, 2);
            double v3 = v1 * v2;
            double v4 = Math.Pow(r, 2) / (x2 * Math.Sqrt(v3 + 1));
            double v5 = v4;
            double v6 = Math.Tan(fpLat);
            double v7 = v6 * v6;
            double v8 = v7 * v7;
            double v9 = 1 / (v5 * x3);
            double v10 = v5 * v4;
            double v11 = v6 / (2 * v10);
            double v12 = v10 * v4;
            double v13 = 1 / (6 * v12 * x3);
            double v14 = v12 * v4;
            double v15 = v6 / (24 * v14);
            double v16 = v14 * v4;
            double v17 = 1 / (120 * v16 * x3);
            double v18 = v16 * v4;
            double v19 = v6 / (720 * v18);
            double v20 = v18 * v4;
            double v21 = 1.0 / (5040 * v20 * x3);
            double v22 = v6 / (40320 * (v20 * v4));
            double v23 = -1 - v3;
            double v24 = -1 - 2 * v7 - v3;
            double v25 = 5 + 3 * v7 + 6 * v3 - 6 * v7 * v3 - 3 * (v3 * v3) - 9 * v7 * (v3 * v3);
            double v26 = 5 + 28 * v7 + 24 * v8 + 6 * v3 + 8 * v7 * v3;
            double v27 = -61 - 90 * v7 - 45 * v8 - 107 * v3 + 162 * v7 * v3;
            double v28 = -61 - 662 * v7 - 1320 * v8 - 720 * (v8 * v7);
            double v29 = 1385 + 3633 * v7 + 4095 * v8 + 1575 * (v8 * v7);
            double rad1 = fpLat + v11 * v23 * (x * x) + v15 * v25 * Math.Pow(x, 4) + v19 * v27 * Math.Pow(x, 6) + v22 * v29 * Math.Pow(x, 8);
            double rad2 = zone + v9 * x + v13 * v24 * Math.Pow(x, 3) + v17 * v26 * Math.Pow(x, 5) + v21 * v28 * Math.Pow(x, 7);
            lat = RadToDeg(rad1);
            lng = RadToDeg(rad2);
            if (lat > 90) lat = 90;
            else if (lat < -90) lat = -90;
            if (lng > 180) lng = 180;
            else if (lng < -180) lng = -180;
        }
    }
}