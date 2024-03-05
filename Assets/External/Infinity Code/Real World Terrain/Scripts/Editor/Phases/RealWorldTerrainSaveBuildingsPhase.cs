/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainSaveBuildingsPhase : RealWorldTerrainPhase
    {
        public static List<SavableRenderer> renderersToSave
        {
            get
            {
                if (_renderersToSave == null) _renderersToSave = new List<SavableRenderer>();
                return _renderersToSave;
            }
        }

        public static Dictionary<MeshFilter, string> filtersToSave
        {
            get
            {
                if (_filtersToSave == null) _filtersToSave = new Dictionary<MeshFilter, string>();
                return _filtersToSave;
            }
        }

        private static Dictionary<MeshFilter, string> _filtersToSave;
        private static List<SavableRenderer> _renderersToSave;

        public override string title
        {
            get { return "Save Buildings..."; }
        }

        public override void Enter()
        {
            try
            {
                if (prefs.buildingGenerator != 0) return;
                
                AssetDatabase.StartAssetEditing();

                if (renderersToSave != null)
                {
                    foreach (var savableRenderer in renderersToSave)
                    {
                        try
                        {
                            Material material = savableRenderer.renderer.sharedMaterials[savableRenderer.index];
                            AssetDatabase.CreateAsset(material, savableRenderer.path);
                        }
                        catch
                        {

                        }
                    }
                }

                if (filtersToSave != null)
                {
                    foreach (var pair in filtersToSave)
                    {
                        try
                        {
                            AssetDatabase.CreateAsset(pair.Key.sharedMesh, pair.Value);
                        }
                        catch
                        {

                        }
                    }
                }

                AssetDatabase.StopAssetEditing();

                if (renderersToSave != null)
                {
                    foreach (var savableRenderer in renderersToSave)
                    {
                        try
                        {
                            savableRenderer.renderer.sharedMaterials[savableRenderer.index] = AssetDatabase.LoadAssetAtPath<Material>(savableRenderer.path);
                        }
                        catch
                        {

                        }
                    }
                }

                if (filtersToSave != null)
                {
                    foreach (var pair in filtersToSave)
                    {
                        try
                        {
                            pair.Key.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pair.Value);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
                throw;
            }

            _renderersToSave = null;
            _filtersToSave = null;

            Complete();
        }

        public class SavableRenderer
        {
            public string path;
            public int index;
            public MeshRenderer renderer;
        }
    }
}