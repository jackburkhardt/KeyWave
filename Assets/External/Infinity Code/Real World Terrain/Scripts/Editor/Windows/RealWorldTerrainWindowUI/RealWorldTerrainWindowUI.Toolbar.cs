/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.IO;
using System.Text;
using System.Xml;
using InfinityCode.RealWorldTerrain.Editors;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static void ToolbarUI()
        {
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);

            GUILayout.BeginHorizontal();

            ToolbarFileUI(buttonStyle);

            if (GUILayout.Button("History", buttonStyle, GUILayout.ExpandWidth(false)))
            {
                RealWorldTerrainHistoryWindow.OpenWindow();
            }

            ToolbarUpdateUI(buttonStyle);

            if (GUILayout.Button("Settings", buttonStyle, GUILayout.ExpandWidth(false))) RealWorldTerrainSettingsWindow.OpenWindow();

            ToolbarHelpUI(buttonStyle);

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private static void ToolbarFileUI(GUIStyle buttonStyle)
        {
            if (GUILayout.Button("File", buttonStyle, GUILayout.ExpandWidth(false)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Import Prefs"), false, () =>
                {
                    string filename = EditorUtility.OpenFilePanel("Import Prefs", Application.dataPath, "xml");
                    if (!string.IsNullOrEmpty(filename)) prefs.LoadFromXML(filename);
                });
                menu.AddItem(new GUIContent("Export Prefs"), false, () =>
                {
                    string filename = EditorUtility.SaveFilePanel("Import Prefs", Application.dataPath, "Prefs", "xml");
                    if (!string.IsNullOrEmpty(filename)) File.WriteAllText(filename, prefs.ToXML(new XmlDocument()).OuterXml, Encoding.UTF8);
                });
                menu.ShowAsContext();
            }
        }

        private static void ToolbarHelpUI(GUIStyle buttonStyle)
        {
            if (GUILayout.Button("Help", buttonStyle, GUILayout.ExpandWidth(false)))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Documentation"), false, RealWorldTerrainLinks.OpenLocalDocumentation);
                menu.AddItem(new GUIContent("Changelog"), false, RealWorldTerrainLinks.OpenChangelog);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Asset Store"), false, RealWorldTerrainLinks.OpenAssetStore);
                menu.AddItem(new GUIContent("Homepage"), false, RealWorldTerrainLinks.OpenHomepage);
                menu.AddItem(new GUIContent("Support"), false, RealWorldTerrainLinks.OpenSupport);
                menu.AddItem(new GUIContent("Forum"), false, RealWorldTerrainLinks.OpenForum);
                menu.AddItem(new GUIContent("Discord"), false, RealWorldTerrainLinks.OpenDiscord);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Check Updates"), false, RealWorldTerrainUpdaterWindow.OpenWindow);
                menu.AddItem(new GUIContent("About"), false, RealWorldTerrainAboutWindow.OpenWindow);

                menu.ShowAsContext();
            }
        }

        private static void ToolbarUpdateUI(GUIStyle buttonStyle)
        {
            if (!RealWorldTerrainUpdaterWindow.hasNewVersion)
            {
                GUILayout.Label("", buttonStyle);
                return;
            }

            Color defColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
            if (GUILayout.Button("New version available!!! Click here to update.", buttonStyle))
            {
                wnd.Close();
                RealWorldTerrainUpdaterWindow.OpenWindow();
            }

            GUI.backgroundColor = defColor;
        }
    }
}