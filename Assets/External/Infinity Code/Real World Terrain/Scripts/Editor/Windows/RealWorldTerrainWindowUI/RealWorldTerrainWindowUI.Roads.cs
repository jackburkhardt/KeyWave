/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if EASYROADS3D
using EasyRoads3Dv3;
#endif

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private const string EasyRoadsLabel = "EasyRoads3D";
        private const string RoadArchitectLabel = "Road Architect";

        private static List<string> availableRoadType;
        private static string[] roadTypeNames;

#if EASYROADS3D
        private static ERModularBase roadNetwork;
        private static bool needFindRoadNetwork = true;
        private static string[] erRoadTypeNames;
#endif

        private static void EasyRoads3DFields()
        {
            prefs.erSnapToTerrain = EditorGUILayout.Toggle("Snap to Terrain", prefs.erSnapToTerrain);

            if (Math.Abs(prefs.erWidthMultiplier - 1) > float.Epsilon && prefs.erGenerateConnection)
            {
                EditorGUILayout.HelpBox("If you select any connection, the width of the road will be reset to the default value.", MessageType.Warning);
            }

            prefs.roadTypeMode = (RealWorldTerrainRoadTypeMode)EditorGUILayout.EnumPopup("Mode", prefs.roadTypeMode);
            if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.simple)
            {
                prefs.erWidthMultiplier = EditorGUILayout.FloatField("Width Multiplier", prefs.erWidthMultiplier);
            }

            prefs.erGenerateConnection = EditorGUILayout.Toggle("Generate Connections", prefs.erGenerateConnection);
            if (prefs.erGenerateConnection)
            {
                EditorGUILayout.HelpBox("Important: the ability to generate connections is in beta. \nThis means that some roads may be not connected. \nWe and AndaSoft are working to improve this feature.", MessageType.Warning);
            }

            if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.simple)
            {
                prefs.roadTypes = (RealWorldTerrainRoadType)EditorGUILayout.EnumFlagsField("Road types", prefs.roadTypes);
            }
            else if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.advanced)
            {
#if EASYROADS3D
                if (roadNetwork == null && needFindRoadNetwork)
                {
                    roadNetwork = Object.FindObjectOfType<ERModularBase>();
                    needFindRoadNetwork = false;
                }

                if (roadNetwork == null)
                {
                    EditorGUILayout.HelpBox("Mode - Advanced requires a road network in the scene.", MessageType.Error);

                    if (GUILayout.Button("Find Road Network in scene"))
                    {
                        roadNetwork = Object.FindObjectOfType<ERModularBase>();
                    }

                    if (GUILayout.Button("Create Road Network"))
                    {
                        ERRoadNetwork newRoadNetwork = new ERRoadNetwork();
                        roadNetwork = newRoadNetwork.roadNetwork;
                    }
                }

                if (roadTypeNames == null) roadTypeNames = Enum.GetNames(typeof(RealWorldTerrainRoadType));

                if (roadNetwork == null)
                {
                    foreach (string name in roadTypeNames) EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(name));
                }
                else
                {
                    if (erRoadTypeNames == null) UpdateERRoadTypes();

                    if (GUILayout.Button("Update EasyRoad3D Road Types")) UpdateERRoadTypes();

                    if (erRoadTypeNames != null)
                    {
                        if (prefs.erRoadTypes == null) prefs.erRoadTypes = new string[roadTypeNames.Length];
                        if (prefs.erRoadTypes.Length != erRoadTypeNames.Length) Array.Resize(ref prefs.erRoadTypes, roadTypeNames.Length);

                        for (int i = 0; i < roadTypeNames.Length; i++)
                        {
                            int index = 0;
                            string roadTypeName = prefs.erRoadTypes[i];

                            if (!string.IsNullOrEmpty(roadTypeName))
                            {
                                for (int j = 0; j < erRoadTypeNames.Length; j++)
                                {
                                    if (erRoadTypeNames[j] == roadTypeName)
                                    {
                                        index = j;
                                        break;
                                    }
                                }
                            }

                            index = EditorGUILayout.Popup(ObjectNames.NicifyVariableName(roadTypeNames[i]), index, erRoadTypeNames);
                            if (index != 0) prefs.erRoadTypes[i] = erRoadTypeNames[index];
                            else prefs.erRoadTypes[i] = string.Empty;
                        }
                    }
                }
#endif
            }
        }

        private static void InitRoadEngines()
        {
            availableRoadType = new List<string>();
#if EASYROADS3D
            availableRoadType.Add(EasyRoadsLabel);
#endif
#if ROADARCHITECT
            availableRoadType.Add(RoadArchitectLabel);
#endif

        }

        private static void RoadArchitectFields()
        {
            prefs.roadTypeMode = RealWorldTerrainRoadTypeMode.simple;
            prefs.roadTypes = (RealWorldTerrainRoadType)EditorGUILayout.EnumFlagsField("Road types", prefs.roadTypes);
        }

        private static void RoadsUI()
        {
            if (availableRoadType.Count == 0)
            {
                prefs.generateRoads = false;
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateRoads = EditorGUILayout.Toggle("Generate roads", prefs.generateRoads);

            if (!prefs.generateRoads)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.HelpBox("If the selected area contains cities, generation of roads can take a VERY long time.", MessageType.Info);

            SelectRoadEngine();

            prefs.normalizeRoadDistances = EditorGUILayout.Toggle("Normalize Road Distances", prefs.normalizeRoadDistances);

            if (prefs.roadEngine == EasyRoadsLabel) EasyRoads3DFields();
            else if (prefs.roadEngine == RoadArchitectLabel) RoadArchitectFields();

            EditorGUILayout.EndVertical();
        }

        private static void SelectRoadEngine()
        {
            if (availableRoadType.Count == 1)
            {
                EditorGUILayout.LabelField("Road engine - " + availableRoadType[0]);
                prefs.roadEngine = availableRoadType[0];
            }
            else
            {
                int roadEngineIndex = availableRoadType.IndexOf(prefs.roadEngine);
                if (roadEngineIndex == -1) roadEngineIndex = 0;
                roadEngineIndex = EditorGUILayout.Popup("Road engine", roadEngineIndex, availableRoadType.ToArray());
                prefs.roadEngine = availableRoadType[roadEngineIndex];
            }
        }

        private static void UpdateERRoadTypes()
        {
#if EASYROADS3D
            ERRoadType[] erRoadTypes = roadNetwork.GetRoadTypes();
            if (erRoadTypes != null) erRoadTypeNames = new[] { "Ignore" }.Concat(erRoadTypes.Select(t => t.roadTypeName)).ToArray();
            else erRoadTypeNames = null;
#endif
        }
    }
}