/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    public static class RealWorldTerrainLinks
    {
        public const string assetStore = "https://assetstore.unity.com/packages/tools/terrain/real-world-terrain-8752";
        public const string changelog = "https://infinity-code.com/products_update/get-changelog.php?asset=Real%20World%20Terrain&from=1.0";
        public const string discord = "https://discord.gg/2XRWwPgZK4";
        public const string documentation = "https://infinity-code.com/documentation/real-world-terrain.html";
        public const string forum = "https://forum.infinity-code.com";
        public const string homepage = "https://infinity-code.com/assets/real-world-terrain";
        public const string reviews = assetStore + "/reviews";
        public const string support = "mailto:support@infinity-code.com?subject=Real%20World%20Terrain";
        public const string youtube = "https://www.youtube.com/playlist?list=PL2QU1uhBMew9F_7iwf_gKjuenEpQcYD-K";

        public static void Open(string url)
        {
            Application.OpenURL(url);
        }

        public static void OpenAssetStore()
        {
            Open(assetStore);
        }

        public static void OpenChangelog()
        {
            Open(changelog);
        }

        public static void OpenDiscord()
        {
            Open(discord);
        }

        public static void OpenDocumentation()
        {
            OpenDocumentation(null);
        }

        public static void OpenDocumentation(string anchor)
        {
            string url = documentation;
            if (!string.IsNullOrEmpty(anchor)) url += "#" + anchor;
            Open(url);
        }

        public static void OpenForum()
        {
            Open(forum);
        }

        public static void OpenHomepage()
        {
            Open(homepage);
        }

        public static void OpenLocalDocumentation()
        {
            string url = RealWorldTerrainEditorUtils.assetPath + "Documentation/Content/Documentation-Content.html";
            Application.OpenURL(url);
        }

        public static void OpenReviews()
        {
            Open(reviews);
        }

        public static void OpenSupport()
        {
            Open(support);
        }

        public static void OpenYouTube()
        {
            Open(youtube);
        }
    }
}