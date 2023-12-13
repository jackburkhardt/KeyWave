/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// Class of meta-information (key / value).
    /// </summary>
    [Serializable]
    public class RealWorldTerrainOSMMetaTag
    {
        /// <summary>
        /// Key title.
        /// </summary>
        public string title;

        /// <summary>
        /// Tag value.
        /// </summary>
        public string info;

        public bool CompareKeyOrValue(string value, bool searchInKey, bool searchInValue)
        {
            return (searchInKey && CompareString(title, value)) || (searchInValue && CompareString(info, value));
        }

        private bool CompareString(string v1, string v2)
        {
            if (v1 == null || v2 == null) return false;
            if (v2.Length > v1.Length) return false;

            for (int i = 0; i < v1.Length - v2.Length + 1; i++)
            {
                bool success = true;
                for (int j = 0; j < v2.Length; j++)
                {
                    char c1 = char.ToUpperInvariant(v1[i + j]);
                    char c2 = v2[j];
                    if (c1 != c2)
                    {
                        success = false;
                        break;
                    }
                }

                if (success) return true;
            }

            return false;
        }
    }
}