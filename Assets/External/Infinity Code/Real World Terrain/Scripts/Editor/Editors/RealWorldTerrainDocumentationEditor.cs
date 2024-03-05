/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof(RealWorldTerrainDocumentation))]
    public class RealWorldTerrainDocumentationEditor : Editor
    {
        private static void DrawDocumentation()
        {
            if (GUILayout.Button("Local Documentation"))
            {
                RealWorldTerrainLinks.OpenLocalDocumentation();
            }

            if (GUILayout.Button("Online Documentation"))
            {
                RealWorldTerrainLinks.OpenDocumentation();
            }

            GUILayout.Space(10);
        }

        private static void DrawExtra()
        {
            if (GUILayout.Button("Changelog"))
            {
                RealWorldTerrainLinks.OpenChangelog();
            }

            GUILayout.Space(10);
        }

        private new static void DrawHeader()
        {
            GUILayout.Label("Real World Terrain", RealWorldTerrainEditorUtils.centeredLabel);
            GUILayout.Label("version: " + RealWorldTerrainWindow.version, RealWorldTerrainEditorUtils.centeredLabel);
            GUILayout.Space(10);
        }

        private void DrawRateAndReview()
        {
            EditorGUILayout.HelpBox("Please don't forget to leave a review on the store page if you liked Tree Tool, this helps us a lot!", MessageType.Warning);

            if (GUILayout.Button("Rate & Review"))
            {
                RealWorldTerrainLinks.OpenReviews();
            }
        }

        private void DrawSupport()
        {
            if (GUILayout.Button("Support"))
            {
                RealWorldTerrainLinks.OpenSupport();
            }

            if (GUILayout.Button("Forum"))
            {
                RealWorldTerrainLinks.OpenForum();
            }

            if (GUILayout.Button("Discord"))
            {
                RealWorldTerrainLinks.OpenDiscord();
            }

            GUILayout.Space(10);
        }

        public override void OnInspectorGUI()
        {
            DrawHeader();
            DrawDocumentation();
            DrawExtra();
            DrawSupport();
            DrawRateAndReview();
        }
    }
}