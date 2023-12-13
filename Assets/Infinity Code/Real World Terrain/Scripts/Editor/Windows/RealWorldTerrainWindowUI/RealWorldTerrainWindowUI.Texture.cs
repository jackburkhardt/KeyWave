/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public static partial class RealWorldTerrainWindowUI
    {
        private static readonly string[] labelsTextureSize = { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192", "12288", "16384", "20480", "26624" };
        private static readonly int[] valuesTextureSize = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 12288, 16384, 20480, 26624 };

        private static int providerIndex;
        private static string[] providersTitle;
        private static RealWorldTerrainTextureProviderManager.Provider[] providers;
        private static bool showCustomProviderTokens;
        private static bool showTextures = true;

        public static void InitTextureProviders()
        {
            providersTitle = RealWorldTerrainTextureProviderManager.GetProvidersTitle();
            providers = RealWorldTerrainTextureProviderManager.GetProviders();

            if (prefs.mapType == null) prefs.mapType = RealWorldTerrainTextureProviderManager.FindMapType(prefs.mapTypeID);
            providerIndex = prefs.mapType.provider.index;
        }

        private static void PrecalculateMaxLevel()
        {
            int tx = 0;
            int ty = 0;
            int textureLevel = 0;

            if (prefs.textureResultType == RealWorldTerrainTextureResultType.regularTexture)
            {
                tx = prefs.textureSize.x * prefs.terrainCount.x / 256;
                ty = prefs.textureSize.y * prefs.terrainCount.y / 256;
            }
            else if (prefs.textureResultType == RealWorldTerrainTextureResultType.hugeTexture)
            {
                tx = prefs.hugeTexturePageSize * prefs.hugeTextureCols * prefs.terrainCount.x / 256;
                ty = prefs.hugeTexturePageSize * prefs.hugeTextureRows * prefs.terrainCount.y / 256;
            }
            else if (prefs.textureResultType == RealWorldTerrainTextureResultType.terrainLayers)
            {
                tx = prefs.controlTextureResolution * prefs.terrainCount.x / 256;
                ty = prefs.controlTextureResolution * prefs.terrainCount.y / 256;
            }

            for (int z = 5; z < 22; z++)
            {
                double stx, sty, etx, ety;
                RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, z, out stx, out sty);
                RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, z, out etx, out ety);

                if (etx < stx) etx += 1 << z;

                if (etx - stx > tx && ety - sty > ty)
                {
                    textureLevel = z;
                    break;
                }
            }

            if (textureLevel == 0) textureLevel = 22;

            EditorGUILayout.HelpBox("Texture level = " + textureLevel + " will be used.", MessageType.Info);
        }

        private static void RegularTextureUI()
        {
            TextureSourceUI();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Texture width");
            prefs.textureSize.x = EditorGUILayout.IntPopup(prefs.textureSize.x, labelsTextureSize, valuesTextureSize);
            GUILayout.Label("height");
            prefs.textureSize.y = EditorGUILayout.IntPopup(prefs.textureSize.y, labelsTextureSize, valuesTextureSize);
            DrawHelpButton("Texture size for each terrains.");
            EditorGUILayout.EndHorizontal();

            if (prefs.textureSize.x > 8192 || prefs.textureSize.y > 8192)
            {
#if !HUGETEXTURE
                EditorGUILayout.HelpBox("To use textures with side sizes larger than 8192 px, you must have a Huge Texture asset in the project.", MessageType.Error);
                if (GUILayout.Button("Huge Texture")) Process.Start("https://assetstore.unity.com/packages/tools/input-management/huge-texture-163576");
#else 
                EditorGUILayout.HelpBox("To use textures with side sizes larger than 8192 px switch to Result Type - Huge Texture.", MessageType.Error);
                if (GUILayout.Button("Set Result Type - Huge Texture"))
                {
                    prefs.textureResultType = RealWorldTerrainTextureResultType.hugeTexture;
                    prefs.hugeTexturePageSize = 1024;
                    prefs.hugeTextureCols = Mathf.CeilToInt(prefs.textureSize.x / (float)prefs.hugeTexturePageSize);
                    prefs.hugeTextureRows = Mathf.CeilToInt(prefs.textureSize.y / (float)prefs.hugeTexturePageSize);
                }
#endif
            }

            prefs.textureMipMaps = Toggle(prefs.textureMipMaps, "Mipmaps", false, "Mipmaps are lists of progressively smaller versions of an image, used to optimize performance on real-time 3D engines. Objects that are far away from the Camera use smaller Texture versions.");

            EditorGUILayout.BeginHorizontal();
            prefs.textureFileType = (RealWorldTerrainTextureFileType)EnumPopup("Format", prefs.textureFileType, "Texture format. PNG is highly recommended to avoid double quality loss due to compression.");
            if (prefs.textureFileType == RealWorldTerrainTextureFileType.jpg) prefs.textureFileQuality = EditorGUILayout.IntSlider("Quality", prefs.textureFileQuality, 0, 100);
            EditorGUILayout.EndHorizontal();

            TextureMaxLevelUI();
        }

        private static void TextureUI()
        {
            prefs.textureResultType = (RealWorldTerrainTextureResultType)EnumPopup("Result Type", prefs.textureResultType, "The type of the resulting texture.");

            if (prefs.textureResultType == RealWorldTerrainTextureResultType.regularTexture) RegularTextureUI();
            else if (prefs.textureResultType == RealWorldTerrainTextureResultType.hugeTexture) HugeTextureGUI();
            else if (prefs.textureResultType == RealWorldTerrainTextureResultType.terrainLayers) TerrainLayersUI();

            EditorGUILayout.Space();
        }

        private static void TextureMaxLevelUI()
        {
            List<string> levels = new List<string> { "Auto" };
            for (int i = 5; i < 23; i++) levels.Add(i.ToString());
            int index = prefs.maxTextureLevel;
            if (index != 0) index -= 4;
            index = EditorGUILayout.Popup("Max level", index, levels.ToArray());
            prefs.maxTextureLevel = index;
            if (index != 0) prefs.maxTextureLevel += 4;
            else
            {
                PrecalculateMaxLevel();
                prefs.reduceTextures = Toggle(prefs.reduceTextures, "Reduce size of textures, with no levels of tiles?", true,
                    "Reducing the size of the texture, reduces the time texture generation and memory usage.");
            }
        }

        private static void TextureSourceUI()
        {
            TextureProviderUI();

            if (prefs.mapType.provider.types.Length > 1)
            {
                GUIContent[] availableTypes = prefs.mapType.provider.types.Select(t => new GUIContent(t.title)).ToArray();
                int mti = prefs.mapType.index;
                EditorGUI.BeginChangeCheck();
                mti = EditorGUILayout.Popup(new GUIContent("Type", "Type of map texture"), mti, availableTypes);
                if (EditorGUI.EndChangeCheck())
                {
                    prefs.mapType = prefs.mapType.provider.types[mti];
                    prefs.mapTypeID = prefs.mapType.fullID;
                }
            }

            TextureProviderExtraFields();
            TextureProviderHelp();
        }

        private static void TextureProviderExtraFields()
        {
            RealWorldTerrainTextureProviderManager.IExtraField[] extraFields = prefs.mapType.extraFields;
            if (extraFields != null)
            {
                foreach (RealWorldTerrainTextureProviderManager.IExtraField field in extraFields)
                {
                    if (field is RealWorldTerrainTextureProviderManager.ToggleExtraGroup) TextureProviderToggleExtraGroup(field as RealWorldTerrainTextureProviderManager.ToggleExtraGroup);
                    else if (field is RealWorldTerrainTextureProviderManager.ExtraField) TextureProviderExtraField(field as RealWorldTerrainTextureProviderManager.ExtraField);
                }
            }

            extraFields = prefs.mapType.provider.extraFields;
            if (extraFields != null)
            {
                foreach (RealWorldTerrainTextureProviderManager.IExtraField field in extraFields)
                {
                    if (field is RealWorldTerrainTextureProviderManager.ToggleExtraGroup) TextureProviderToggleExtraGroup(field as RealWorldTerrainTextureProviderManager.ToggleExtraGroup);
                    else if (field is RealWorldTerrainTextureProviderManager.ExtraField) TextureProviderExtraField(field as RealWorldTerrainTextureProviderManager.ExtraField);
                }
            }
        }

        private static void TextureProviderExtraField(RealWorldTerrainTextureProviderManager.ExtraField field)
        {
            field.value = EditorGUILayout.TextField(field.title, field.value);
        }

        private static void TextureProviderHelp()
        {
            string[] help = prefs.mapType.help;
            if (help != null)
            {
                foreach (string field in help)
                {
                    EditorGUILayout.HelpBox(field, MessageType.Info);
                }
            }

            help = prefs.mapType.provider.help;
            if (help != null)
            {
                foreach (string field in help)
                {
                    EditorGUILayout.HelpBox(field, MessageType.Info);
                }
            }
        }

        private static void TextureProviderToggleExtraGroup(RealWorldTerrainTextureProviderManager.ToggleExtraGroup @group)
        {
            @group.value = EditorGUILayout.Toggle(@group.title, @group.value);
            EditorGUI.BeginDisabledGroup(@group.value);

            if (@group.fields != null)
            {
                foreach (RealWorldTerrainTextureProviderManager.IExtraField field in @group.fields)
                {
                    if (field is RealWorldTerrainTextureProviderManager.ToggleExtraGroup) TextureProviderToggleExtraGroup(field as RealWorldTerrainTextureProviderManager.ToggleExtraGroup);
                    else if (field is RealWorldTerrainTextureProviderManager.ExtraField) TextureProviderExtraField(field as RealWorldTerrainTextureProviderManager.ExtraField);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void TextureProviderUI()
        {
            EditorGUI.BeginChangeCheck();
            providerIndex = Popup("Provider", providerIndex, providersTitle, "Source of tiles for generating texture.");
            if (EditorGUI.EndChangeCheck())
            {
                prefs.mapType = providers[providerIndex].types[0];
                prefs.mapTypeID = prefs.mapType.fullID;
            }

            if (prefs.mapType.isCustom)
            {
                prefs.textureProviderURL = EditorGUILayout.TextField(prefs.textureProviderURL);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                showCustomProviderTokens = Foldout(showCustomProviderTokens, "Available tokens");
                if (showCustomProviderTokens)
                {
                    GUILayout.Label("{zoom}");
                    GUILayout.Label("{x}");
                    GUILayout.Label("{y}");
                    GUILayout.Label("{quad}");
                    GUILayout.Space(10);
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}