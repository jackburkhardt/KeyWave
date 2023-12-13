/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static string bingAPI;
        private static string earthDataLogin;
        private static string earthDataPass;
        private static bool showElevationProvider = true;

        private static void BingMapsElevationExtraFields()
        {
            if (bingAPI == null) bingAPI = RealWorldTerrainPrefs.LoadPref("BingAPI", "");
            EditorGUILayout.HelpBox("Public Windows App or Public Windows Phone App have the 50.000 transaction within 24 hours. With the other chooses there's only 125.000 transactions within a year and the key will expire if exceeding it.", MessageType.Info);
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            bingAPI = EditorGUILayout.TextField("Bing Maps API key", bingAPI);
            if (EditorGUI.EndChangeCheck())
            {
                if (bingAPI == "") RealWorldTerrainPrefs.DeletePref("BingAPI");
                else RealWorldTerrainPrefs.SetPref("BingAPI", bingAPI);
            }

            if (string.IsNullOrEmpty(bingAPI))
            {
                GUILayout.Box(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            if (GUILayout.Button("Create Key", GUILayout.ExpandWidth(false))) Process.Start("http://msdn.microsoft.com/en-us/library/ff428642.aspx");
            GUILayout.EndHorizontal();
        }

        private static void ElevationProviderUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showElevationProvider = EditorGUILayout.Foldout(showElevationProvider, "Elevation Provider");

            if (!showElevationProvider)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            prefs.elevationProvider = (RealWorldTerrainElevationProvider) EditorGUILayout.EnumPopup(prefs.elevationProvider);
            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM)
            {
                EditorGUILayout.HelpBox("The latitude range is 60°S - 60°N.\nThe resolution of the source data is 3 arc seconds(about 90 meters).", MessageType.Info);
            }
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30)
            {
                EditorGUILayout.HelpBox("The latitude range is 60°S - 60°N.\nThe resolution of the source data is 1 arc seconds(about 30 meters).", MessageType.Info);
            }
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps /*|| prefs.elevationProvider == RealWorldTerrainElevationProvider.ArcGIS*/)
            {
                EditorGUILayout.HelpBox("Global coverage.\nThe resolution of the source data:\nUSA - 10 meters, Global 56°S - 60°N - 90 meters, other (including poles) - 900 meters.", MessageType.Info);
            }
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.Mapbox)
            {
                EditorGUILayout.HelpBox("Global coverage.\nThe resolution of the source data up to 5 meters.", MessageType.Info);
            }

            ElevationProviderExtraFields();

            EditorGUILayout.EndVertical();
        }

        private static void ElevationProviderExtraFields()
        {
            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM || prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30)
            {
                prefs.ignoreSRTMErrors = EditorGUILayout.Toggle("Ignore SRTM errors", prefs.ignoreSRTMErrors);
            }

            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps) BingMapsElevationExtraFields();
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.Mapbox) MapboxElevationExtraFields();
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30) SRTM30ExtraFields();
        }

        private static void MapboxElevationExtraFields()
        {
            MapboxAccessToken();
            GUILayout.Space(10);
        }

        private static void SRTM30ExtraFields()
        {
            if (earthDataLogin == null || earthDataPass == null)
            {
                earthDataLogin = RealWorldTerrainPrefs.LoadPref("EarthDataLogin", "");
                earthDataPass = RealWorldTerrainPrefs.LoadPref("EarthDataPass", "");
            }

            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            earthDataLogin = EditorGUILayout.TextField("EarthData username", earthDataLogin);
            if (EditorGUI.EndChangeCheck())
            {
                if (earthDataLogin == "") RealWorldTerrainPrefs.DeletePref("EarthDataLogin");
                else RealWorldTerrainPrefs.SetPref("EarthDataLogin", earthDataLogin);
            }

            if (string.IsNullOrEmpty(earthDataLogin))
            {
                GUILayout.Box(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            earthDataPass = EditorGUILayout.TextField("EarthData password", earthDataPass);
            if (EditorGUI.EndChangeCheck())
            {
                if (earthDataPass == "") RealWorldTerrainPrefs.DeletePref("EarthDataPass");
                else RealWorldTerrainPrefs.SetPref("EarthDataPass", earthDataPass);
            }

            if (string.IsNullOrEmpty(earthDataPass))
            {
                GUILayout.Button(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Register on EarthData", GUILayout.ExpandWidth(true))) Process.Start("https://urs.earthdata.nasa.gov/users/new");
            GUILayout.Space(10);
        }
    }
}