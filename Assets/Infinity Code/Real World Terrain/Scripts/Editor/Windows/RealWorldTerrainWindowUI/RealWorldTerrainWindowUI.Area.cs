/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static bool showCoordinates = true;
        private static int coordinateMode = 0;

        private static string utmTLLatZone;
        private static int utmTLLngZone;
        private static int utmTLEasting;
        private static int utmTLNorthing;

        private static string utmBRLatZone;
        private static int utmBRLngZone;
        private static int utmBREasting;
        private static int utmBRNorthing;

        private static void AnchorUI()
        {
            prefs.useAnchor = Toggle(prefs.useAnchor, "Use Anchor", true, "The coordinates of the anchor point at which there will be a zero position in the scene.");
            if (!prefs.useAnchor) return;

            prefs.anchorLatitude = DoubleField("Latitude", prefs.anchorLatitude, "Latitude of the Anchor. \nValues: -90 to 90.", "http://en.wikipedia.org/wiki/Latitude");
            prefs.anchorLongitude = DoubleField("Longitude", prefs.anchorLongitude, "Longitude of the Anchor. \nValues: -180 to 180.", "http://en.wikipedia.org/wiki/Longitude");
        }

        private static void ApplyUTMValues()
        {
            RealWorldTerrainUTM.ToLngLat(utmTLLatZone, utmTLLngZone, utmTLEasting, utmTLNorthing, out prefs.leftLongitude, out prefs.topLatitude);
            RealWorldTerrainUTM.ToLngLat(utmBRLatZone, utmBRLngZone, utmBREasting, utmBRNorthing, out prefs.rightLongitude, out prefs.bottomLatitude);
            coordinateMode = 0;
        }

        private static void AreaUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showCoordinates = EditorGUILayout.Foldout(showCoordinates, "Area");
            if (!showCoordinates)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            prefs.title = EditorGUILayout.TextField("Title", prefs.title);
            GUILayout.Space(10);

            CoordinatesUI();
            AnchorUI();
            AreaButtonsUI();

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
        }

        private static void AreaButtonsUI()
        {
            GUI.SetNextControlName("InsertCoordsButton");
            if (GUILayout.Button("Insert the coordinates from the clipboard")) InsertCoords();
            if (GUILayout.Button("Run the helper")) RunHelper();
            if (prefs.resultType == RealWorldTerrainResultType.terrain && GUILayout.Button("Get the best settings for the specified coordinates")) RealWorldTerrainSettingsGeneratorWindow.OpenWindow();
            if (GUILayout.Button("Show Open Street Map"))
            {
                Vector2 center;
                int zoom;
                RealWorldTerrainUtils.GetCenterPointAndZoom(new[] {prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude, prefs.bottomLatitude}, out center, out zoom);
                Process.Start(string.Format(RealWorldTerrainCultureInfo.numberFormat, "http://www.openstreetmap.org/#map={0}/{1}/{2}", zoom, center.y, center.x));
            }
        }

        private static void CoordinatesUI()
        {
            EditorGUI.BeginChangeCheck();
            coordinateMode = GUILayout.Toolbar(coordinateMode, new[] {"Decimal", "UTM"});
            if (EditorGUI.EndChangeCheck() && coordinateMode == 1) InitUTMValues();

            if (coordinateMode == 0) DecimalCoordinatesUI();
            else UTMCoordinatesUI();
        }

        private static void DecimalCoordinatesUI()
        {
            GUILayout.Label("Top-Left");
            EditorGUI.indentLevel++;
            prefs.topLatitude = DoubleField("Latitude", prefs.topLatitude, "Latitude of the Top-Left corner of the area. \nValues: -90 to 90.", "http://en.wikipedia.org/wiki/Latitude");
            prefs.leftLongitude = DoubleField("Longitude", prefs.leftLongitude, "Longitude of the Top-Left corner of the area. \nValues: -180 to 180.", "http://en.wikipedia.org/wiki/Longitude");
            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            GUILayout.Label("Bottom-Right");
            EditorGUI.indentLevel++;
            prefs.bottomLatitude = DoubleField("Latitude", prefs.bottomLatitude, "Latitude of the Bottom-Right corner of the area. \nValues: -90 to 90.", "http://en.wikipedia.org/wiki/Latitude");
            prefs.rightLongitude = DoubleField("Longitude", prefs.rightLongitude, "Longitude of the Bottom-Right corner of the area. \nValues: -180 to 180.", "http://en.wikipedia.org/wiki/Longitude");
            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            if (prefs.topLatitude > 60 || prefs.bottomLatitude < -60)
            {
                if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM || prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30)
                {
                    EditorGUILayout.HelpBox("SRTM and SRTM30 only contain data in the latitude range 60 to -60. Please select another Elevation Provider.", MessageType.Error);
                }
                else if (/*prefs.elevationProvider == RealWorldTerrainElevationProvider.ArcGIS || */prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps)
                {
                    EditorGUILayout.HelpBox("For latitudes north of 60 degrees or south of -60 degrees, ArcGIS and Bing Maps contain data with an accuracy of 900 meters per value, which is very small for a good result. Try using Mapbox first.", MessageType.Warning);
                }
            }
        }

        private static void InitUTMValues()
        {
            double e, n;
            RealWorldTerrainUTM.ToUTM(prefs.leftLongitude, prefs.topLatitude, out utmTLLatZone, out utmTLLngZone, out e, out n);
            utmTLEasting = (int) Math.Round(e);
            utmTLNorthing = (int) Math.Round(n);

            RealWorldTerrainUTM.ToUTM(prefs.rightLongitude, prefs.bottomLatitude, out utmBRLatZone, out utmBRLngZone, out e, out n);
            utmBREasting = (int) Math.Round(e);
            utmBRNorthing = (int) Math.Round(n);
        }

        public static void InsertCoords()
        {
            GUI.FocusControl("InsertCoordsButton");
            string nodeStr = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(nodeStr)) return;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(nodeStr);
                XmlNode node = doc.FirstChild;
                if (node.Name != "Coords" || node.Attributes == null) return;

                prefs.leftLongitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "tlx");
                prefs.topLatitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "tly");
                prefs.rightLongitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "brx");
                prefs.bottomLatitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "bry");

                if (prefs.useAnchor)
                {
                    if (EditorUtility.DisplayDialog("Remove Anchor", "Remove anchor point from previous settings?", "Remove", "Keep"))
                    {
                        prefs.useAnchor = false;
                        double mx1, my1, mx2, my2;
                        RealWorldTerrainUtils.LatLongToMercat(prefs.leftLongitude, prefs.topLatitude, out mx1, out my1);
                        RealWorldTerrainUtils.LatLongToMercat(prefs.rightLongitude, prefs.bottomLatitude, out mx2, out my2);
                        mx1 = (mx2 + mx1) / 2;
                        my1 = (my2 + my1) / 2;
                        RealWorldTerrainUtils.MercatToLatLong(mx1, my1, out prefs.anchorLongitude, out prefs.anchorLatitude);
                    }
                }

                XmlNodeList POInodes = node.SelectNodes("//POI");
                prefs.POI = new List<RealWorldTerrainPOI>();
                foreach (XmlNode n in POInodes) prefs.POI.Add(new RealWorldTerrainPOI(n));

                prefs.Save();
            }
            catch { }
        }

        public static void RunHelper()
        {
            string helperPath = "file://" + Directory.GetFiles(Application.dataPath, "RWT_Helper.html", SearchOption.AllDirectories)[0].Replace('\\', '/');
            if (Application.platform == RuntimePlatform.OSXEditor) helperPath = helperPath.Replace(" ", "%20");
            prefs.Save();
            Application.OpenURL(helperPath);
        }

        private static void UTMCoordinatesUI()
        {
            GUILayout.Label("Top-Left");
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();

            utmTLLngZone = EditorGUILayout.IntField(utmTLLngZone);
            utmTLLatZone = EditorGUILayout.TextField(utmTLLatZone);
            utmTLEasting = EditorGUILayout.IntField(utmTLEasting);
            utmTLNorthing = EditorGUILayout.IntField(utmTLNorthing);

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            GUILayout.Label("Bottom-Right");
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();

            utmBRLngZone = EditorGUILayout.IntField(utmBRLngZone);
            utmBRLatZone = EditorGUILayout.TextField(utmBRLatZone);
            utmBREasting = EditorGUILayout.IntField(utmBREasting);
            utmBRNorthing = EditorGUILayout.IntField(utmBRNorthing);

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;

            if (GUILayout.Button("Apply")) ApplyUTMValues();

            GUILayout.Space(10);
        }
    }
}