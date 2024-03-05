/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    [Serializable]
    public class RealWorldTerrainVectorTerrainLayerFeature
    {
        public const float TerrainLayerLineHeight = 20;

        private static string[] _layerNames;
        private static List<string> _landuseNames;
        private static List<string> _landuseOverlayNames;
        private static List<string> _structureNames;
        private static List<string> _waterwayNames;

        public List<TerrainLayer> terrainLayers;
        public Vector2 noiseOffset = Vector2.zero;
        public float noiseScale = 16;
        public List<Rule> rules;

        [NonSerialized]
        private float? _height;

        public float height
        {
            get
            {
                if (!_height.HasValue) UpdateHeight();
                return _height.Value;
            }
        }

        public static List<string> landuseNames
        {
            get
            {
                if (_landuseNames == null) _landuseNames = Enum.GetNames(typeof(RealWorldTerrainMapboxLanduse)).ToList();
                return _landuseNames;
            }
        }

        public static List<string> landuseOverlayNames
        {
            get
            {
                if (_landuseOverlayNames == null) _landuseOverlayNames = Enum.GetNames(typeof(RealWorldTerrainMapboxLanduseOverlay)).ToList();
                return _landuseOverlayNames;
            }
        }

        public static string[] layerNames
        {
            get
            {
                if (_layerNames == null) _layerNames = Enum.GetNames(typeof(RealWorldTerrainMapboxLayer));
                return _layerNames;
            }
        }

        public static List<string> structureNames
        {
            get
            {
                if (_structureNames == null) _structureNames = Enum.GetNames(typeof(RealWorldTerrainMapboxStructure)).ToList();
                return _structureNames;
            }
        }

        public static List<string> waterwayNames
        {
            get
            {
                if (_waterwayNames == null) _waterwayNames = Enum.GetNames(typeof(RealWorldTerrainMapboxWaterway)).ToList();
                return _waterwayNames;
            }
        }

        public void UpdateHeight()
        {
            int rows = 3;

            if (terrainLayers == null) rows += 1;
            else if (terrainLayers.Count == 1) rows += 1;
            else rows += terrainLayers.Count + 2;

            if (rules != null)
            {
                foreach (Rule rule in rules) rows += rule.hasExtra ? 3 : 2;
            }

            _height = rows * TerrainLayerLineHeight + 5;
        }

        [Serializable]
        public class Rule
        {
            public RealWorldTerrainMapboxLayer layer = RealWorldTerrainMapboxLayer.building;
            public int extra = ~0;
            
            [NonSerialized]
            private string _layerName;

            public string layerName
            {
                get
                {
                    if (_layerName == null) _layerName = layer.ToString();
                    return _layerName;
                }
            }

            public bool hasExtra
            {
                get
                {
                    return layer == RealWorldTerrainMapboxLayer.landuse_overlay ||
                           layer == RealWorldTerrainMapboxLayer.landuse ||
                           layer == RealWorldTerrainMapboxLayer.waterway || 
                           layer == RealWorldTerrainMapboxLayer.structure;
                }
            }
        }
    }
}