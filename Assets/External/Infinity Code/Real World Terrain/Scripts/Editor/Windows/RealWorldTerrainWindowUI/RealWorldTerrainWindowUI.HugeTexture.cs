/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
#if HUGETEXTURE
        private static readonly string[] labelsHugeTexturePageSize = { "256", "512", "1024", "2048", "4096", "8192" };
        private static readonly int[] valuesHugeTexturePageSize = { 256, 512, 1024, 2048, 4096, 8192 };
#endif

        private static void HugeTextureGUI()
        {
#if HUGETEXTURE
            EditorGUILayout.HelpBox("Important: when using Huge Texture, you cannot use Terrain Layers.", MessageType.Info);

            TextureSourceUI();

            prefs.hugeTexturePageSize = EditorGUILayout.IntPopup("Page Size", prefs.hugeTexturePageSize, labelsHugeTexturePageSize, valuesHugeTexturePageSize);
            prefs.hugeTextureCols = EditorGUILayout.IntField("Cols", prefs.hugeTextureCols);
            prefs.hugeTextureRows = EditorGUILayout.IntField("Rows", prefs.hugeTextureRows);

            if (prefs.hugeTextureCols < 1) prefs.hugeTextureCols = 1;
            if (prefs.hugeTextureRows < 1) prefs.hugeTextureRows = 1;

            long width = prefs.hugeTexturePageSize * prefs.hugeTextureCols;
            long height = prefs.hugeTexturePageSize * prefs.hugeTextureRows;
            EditorGUILayout.LabelField("Width", width.ToString());
            EditorGUILayout.LabelField("Height", height.ToString());
            EditorGUILayout.LabelField("Total Pages", (prefs.hugeTextureCols * prefs.hugeTextureCols).ToString());

            if (width * height * 3 > 2147483648L)
            {
                EditorGUILayout.HelpBox("Width * Height * 3 must be less than or equal to 2 GB (2 147 483 648 bytes).", MessageType.Error);
            }

            TextureMaxLevelUI();
#else
            EditorGUILayout.HelpBox("HugeTexture is not in the project.", MessageType.Warning);
            if (GUILayout.Button("Huge Texture")) Process.Start("https://assetstore.unity.com/packages/tools/input-management/huge-texture-163576?aid=1100liByC&pubref=rwt_ht_asset");
#endif
        }
    }
}