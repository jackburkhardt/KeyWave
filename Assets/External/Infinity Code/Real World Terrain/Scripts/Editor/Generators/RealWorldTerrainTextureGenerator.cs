/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

#if HUGETEXTURE
using InfinityCode.HugeTexture.Editors;
#endif

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainTextureGenerator
    {
        const int countThreads = 16;

        public static Color32[] colors;
        public static bool ready;
        public static RealWorldTerrainTextureGenerator[] reqTextures;
        public static List<RealWorldTerrainTextureGenerator> textures;
        private static ThreadGenerator[] threads;

        private static Texture2D generatedTexture;
        private static RealWorldTerrainTextureGenerator lastTexture;
        private static int lastX;
        private static int lastTextureIndex;
        private static int maxTextureLevel;
        private static int maxZoomLevel;
        private static string textureFilename;
        private static int textureWidth;
        private static int textureHeight;

        public bool exist;

        private readonly int tileX;
        private readonly int tileY;
        private readonly int zoom;
        private readonly string levelFolder;
        public bool loaded;
        private string path;
        private Color32[] textureColors;
        private double leftMercator;
        private double topMercator;
        private double rightMercator;
        private double bottomMercator;
        private double mercatorWidth;
        private double mercatorHeight;

#if RTP
        public static List<Texture2D> rtpTextures;
#endif

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private bool downloaded
        {
            get { return File.Exists(path); }
        }

        private RealWorldTerrainTextureGenerator(int tileX, int tileY, int zoom)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.zoom = zoom;

            double cx, cy;
            RealWorldTerrainUtils.TileToLatLong(tileX, tileY, zoom, out cx, out cy);
            RealWorldTerrainUtils.LatLongToMercat(cx, cy, out leftMercator, out topMercator);
            RealWorldTerrainUtils.TileToLatLong(tileX + 1, tileY + 1, zoom, out cx, out cy);
            RealWorldTerrainUtils.LatLongToMercat(cx, cy, out rightMercator, out bottomMercator);

            mercatorWidth = rightMercator - leftMercator;
            mercatorHeight = bottomMercator - topMercator;

            levelFolder = Path.Combine(RealWorldTerrainEditorUtils.textureCacheFolder, zoom.ToString());
            if (!Directory.Exists(levelFolder)) Directory.CreateDirectory(levelFolder);

            string filename = prefs.mapType.filePrefix + tileX + "x" + tileY + ".jpg";
            path = Path.Combine(levelFolder, filename);
            string url;
            if (!prefs.mapType.isCustom) url = prefs.mapType.GetURL(zoom, tileX, tileY);
            else url = prefs.mapType.GetURL(zoom, tileX, tileY, prefs.textureProviderURL);

            if (!File.Exists(path))
            {
                new RealWorldTerrainDownloadItemUnityWebRequest(url)
                {
                    filename = path,
                    averageSize = RealWorldTerrainUtils.AVERAGE_TEXTURE_SIZE
                };
            }
        }

        private bool Contains(double x)
        {
            return x >= leftMercator && x <= rightMercator;
        }

        private bool Contains(double x, double y)
        {
            return x >= leftMercator && x <= rightMercator && y >= topMercator && y <= bottomMercator;
        }

        public static void Dispose()
        {
            if (textures != null)
            {
                foreach (RealWorldTerrainTextureGenerator texture in textures) texture.DisposeItem();
                textures = null;
            }

            ready = false;
            reqTextures = null;
        }

        public void DisposeItem()
        {
            Unload();
            generatedTexture = null;
            lastTexture = null;
            lastX = 0;
            textureColors = null;
            textureFilename = string.Empty;
            colors = null;
        }

        public static void GenerateHugeTexture(RealWorldTerrainItem item)
        {
#if HUGETEXTURE
            if (string.IsNullOrEmpty(textureFilename))
            {
                if (!ready) Prepare();

                lastX = 0;
                RealWorldTerrainPhase.phaseProgress = 0;
                lastTexture = null;
                reqTextures = textures.Where(t => t.Intersects(item)).OrderByDescending(t => t.zoom).ThenBy(t => t.tileX).ThenByDescending(t => t.tileY).ToArray();
                if (reqTextures.Length == 0) maxTextureLevel = 3;
                else maxZoomLevel = reqTextures[0].zoom;

                textureWidth = prefs.hugeTexturePageSize * prefs.hugeTextureCols;
                textureHeight = prefs.hugeTexturePageSize * prefs.hugeTextureRows;

                if (colors == null || colors.Length != textureWidth * textureHeight) colors = new Color32[textureWidth * textureHeight];

                textureFilename = Path.Combine(RealWorldTerrainWindow.container.folder, item.name + "r" + textureWidth + "x" + textureHeight + ".hugeraw");

                if (RealWorldTerrainWindow.generateInThread)
                {
                    foreach (RealWorldTerrainTextureGenerator t in reqTextures) t.LoadTexture();
                }
            }

            double tsx = textureWidth - 1;
            double tsy = textureHeight - 1;

            double x1 = item.leftMercator;
            double y1 = item.topMercator;
            double x2 = item.rightMercator;
            double y2 = item.bottomMercator;

            double rx = x2 - x1;
            double ry = y2 - y1;

            double sx = rx / tsx;
            double sy = ry / tsy;

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            if (!RealWorldTerrainWindow.generateInThread)
            {
                for (int ty = 0; ty < textureHeight; ty++)
                {
                    double py = y2 - sy * ty;
                    lastTextureIndex = -1;

                    for (int tx = lastX; tx < textureWidth; tx++)
                    {
                        double px = sx * tx + x1;
                        colors[ty * textureWidth + tx] = GetTextureColor(px, py);
                    }

                    lastX = ty;
                    RealWorldTerrainPhase.phaseProgress = ty / (float)textureHeight;
                    if (timer.seconds > 1) return;
                }
            }
            else if (threads == null)
            {
                threads = new ThreadGenerator[countThreads];
                int w = textureWidth / countThreads;
                for (int i = 0; i < countThreads; i++)
                {
                    threads[i] = new HugeTextureThreadGenerator
                    {
                        start = w * i,
                        end = w * (i + 1),
                        sx = sx,
                        sy = sy,
                        x1 = x1,
                        y2 = y2
                    };
                    new Thread(threads[i].Start).Start();
                }
                return;
            }
            else
            {
                bool isCompleted = true;
                float progress = 0;
                for (int i = 0; i < countThreads; i++)
                {
                    if (!threads[i].completed) isCompleted = false;
                    progress += threads[i].progress;
                }

                RealWorldTerrainPhase.phaseProgress = progress / countThreads;
                if (!isCompleted) return;

                for (int i = 0; i < countThreads; i++) threads[i].Dispose();
                threads = null;
            }

            foreach (RealWorldTerrainTextureGenerator t in reqTextures) t.Unload();
            reqTextures = null;

            FileInfo info = new FileInfo(textureFilename);
            if (!info.Directory.Exists) info.Directory.Create();

            FileStream stream = File.Open(textureFilename, FileMode.Create);

            for (int i = 0; i < colors.Length; i++)
            {
                Color32 clr = colors[i];
                stream.WriteByte(clr.r);
                stream.WriteByte(clr.g);
                stream.WriteByte(clr.b);
            }

            stream.Close();

            AssetDatabase.Refresh();

            HugeRawImporter importer = AssetImporter.GetAtPath(textureFilename) as HugeRawImporter;
            if (importer != null)
            {
                SerializedObject so = new SerializedObject(importer);
                so.FindProperty("pageSize").intValue = prefs.hugeTexturePageSize;
                so.FindProperty("cols").intValue = prefs.hugeTextureCols;
                so.FindProperty("rows").intValue = prefs.hugeTextureRows;
                so.FindProperty("originalWidth").intValue = textureWidth;
                so.FindProperty("originalHeight").intValue = textureHeight;
                so.ApplyModifiedProperties();
                importer.SaveAndReimport();
            }

            string matPath = textureFilename.Substring(0, textureFilename.LastIndexOf("."));
            if (File.Exists(matPath + ".mat"))
            {
                int index = 1;
                while (File.Exists(matPath + "_" + index + ".mat"))
                {
                    index++;
                }

                matPath += "_" + index;
            }

            matPath += ".mat";

            Shader shader;

            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null) shader = Shader.Find("Huge Texture/Diffuse Array");
            else shader = Shader.Find("Shader Graphs/HugeTexturePBR");

            Material mat = new Material(shader);
            mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2DArray>(textureFilename));
            mat.SetInt("_Cols", prefs.hugeTextureCols);
            mat.SetInt("_Rows", prefs.hugeTextureRows);

            AssetDatabase.CreateAsset(mat, matPath);

            AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (item.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                item.terrain.materialTemplate = mat;
            }
            else if (item.prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                Renderer[] rs = item.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rs) r.sharedMaterial = mat;
            }
#endif

            RealWorldTerrainPhase.phaseComplete = true;
            textureFilename = string.Empty;
            item.generatedTextures = true;
        }

        public static void GenerateTexture(RealWorldTerrainItem item)
        {
            if (string.IsNullOrEmpty(textureFilename))
            {
                if (!ready) Prepare();

                lastX = 0;
                RealWorldTerrainPhase.phaseProgress = 0;
                lastTexture = null;
                reqTextures = textures.Where(t => t.Intersects(item)).OrderByDescending(t => t.zoom).ThenBy(t => t.tileX).ThenByDescending(t => t.tileY).ToArray();
                if (reqTextures.Length == 0)
                {
                    maxTextureLevel = 3;
                }
                else
                {
                    maxZoomLevel = reqTextures[0].zoom;
                }
                
                textureWidth = prefs.textureSize.x;
                textureHeight = prefs.textureSize.y;

                if (prefs.reduceTextures && maxZoomLevel < maxTextureLevel)
                {
                    for (int i = maxZoomLevel; i < maxTextureLevel; i++)
                    {
                        if (textureWidth <= 128 || textureHeight <= 128) break;
                        textureWidth /= 2;
                        textureHeight /= 2;
                    }
                }

                if (colors == null || colors.Length != textureHeight) colors = new Color32[textureHeight];

                generatedTexture = new Texture2D(textureWidth, textureHeight);
                textureFilename = Path.Combine(RealWorldTerrainWindow.container.folder, item.name + "r" + textureWidth + "x" + textureHeight + "." + prefs.textureFileType);

                if (RealWorldTerrainWindow.generateInThread)
                {
                    foreach (RealWorldTerrainTextureGenerator t in reqTextures) t.LoadTexture();
                }
            }

            double tsx = textureWidth - 1;
            double tsy = textureHeight - 1;

            double x1 = item.leftMercator;
            double y1 = item.topMercator;
            double x2 = item.rightMercator;
            double y2 = item.bottomMercator;

            double rx = x2 - x1;
            double ry = y2 - y1;

            double sx = rx / tsx;
            double sy = ry / tsy;

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            if (!RealWorldTerrainWindow.generateInThread)
            {
                for (int tx = lastX; tx < textureWidth; tx++)
                {
                    double px = sx * tx + x1;
                    lastTextureIndex = -1;

                    for (int ty = 0; ty < textureHeight; ty++)
                    {
                        double py = y2 - sy * ty;
                        colors[ty] = GetTextureColor(px, py);
                    }

                    generatedTexture.SetPixels32(tx, 0, 1, textureHeight, colors);
                    lastX = tx;
                    RealWorldTerrainPhase.phaseProgress = tx / (float)textureWidth;
                    if (timer.seconds > 1) return;
                }
            }
            else if (threads == null)
            {
                threads = new ThreadGenerator[countThreads];
                int w = textureWidth / countThreads;
                for (int i = 0; i < countThreads; i++)
                {
                    threads[i] = new TextureThreadGenerator
                    {
                        start = w * i,
                        end = w * (i + 1),
                        sx = sx,
                        sy = sy,
                        x1 = x1,
                        y2 = y2
                    };
                    new Thread(threads[i].Start).Start();
                }
                return;
            }
            else
            {
                bool isCompleted = true;
                float progress = 0;
                for (int i = 0; i < countThreads; i++)
                {
                    if (!threads[i].completed) isCompleted = false;
                    progress += threads[i].progress;
                }

                RealWorldTerrainPhase.phaseProgress = progress / countThreads;
                if (!isCompleted) return;

                for (int i = 0; i < countThreads; i++)
                {
                    TextureThreadGenerator tg = threads[i] as TextureThreadGenerator;
                    Color32[][] cs = tg.colors;
                    for (int j = 0; j < cs.Length; j++)
                    {
                        generatedTexture.SetPixels32(tg.start + j, 0, 1, textureHeight, cs[j]);
                    }
                    tg.Dispose();
                }
                threads = null;
            }

            foreach (RealWorldTerrainTextureGenerator t in reqTextures) t.Unload();
            reqTextures = null;

            generatedTexture.Apply();

            FileInfo info = new FileInfo(textureFilename);
            if (!info.Directory.Exists) info.Directory.Create();

            if (prefs.textureFileType == RealWorldTerrainTextureFileType.png) File.WriteAllBytes(textureFilename, generatedTexture.EncodeToPNG());
            else File.WriteAllBytes(textureFilename, generatedTexture.EncodeToJPG(prefs.textureFileQuality));

            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(textureFilename) as TextureImporter;
            if (importer != null)
            {
                importer.mipmapEnabled = true;
                importer.isReadable = true;
                importer.mipmapEnabled = prefs.textureMipMaps;
                importer.maxTextureSize = Mathf.Max(textureWidth, textureHeight);
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            Object.DestroyImmediate(generatedTexture);

            generatedTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(textureFilename, typeof(Texture2D));

            if (item.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
#if !RTP
                Vector3 terrainSize = item.terrain.terrainData.size;
                Vector2 tileSize = new Vector2(terrainSize.x, terrainSize.z);

                TerrainLayer l = new TerrainLayer
                {
                    diffuseTexture = generatedTexture,
                    tileSize = tileSize
                };

                string path = Path.Combine(RealWorldTerrainWindow.container.folder, item.name + "r" + textureWidth + "x" + textureHeight + ".terrainlayer");
                AssetDatabase.CreateAsset(l, path);
                AssetDatabase.Refresh();
                l = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);

                item.terrain.terrainData.terrainLayers = new[] { l };

                float[,,] alphamap = new float[prefs.controlTextureResolution, prefs.controlTextureResolution, 1];
                for (int x = 0; x < prefs.controlTextureResolution; x++)
                {
                    for (int y = 0; y < prefs.controlTextureResolution; y++)
                    {
                        alphamap[x, y, 0] = 1;
                    }
                }
                item.terrain.terrainData.SetAlphamaps(0, 0, alphamap);
#else
                if (rtpTextures == null || rtpTextures.Count != 12)
                {
                    rtpTextures = new List<Texture2D>();
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Dirt.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Dirt Height.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Dirt Normal.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Grass.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Grass Height.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Grass Normal.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("GrassRock.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("GrassRock Height.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("GrassRock Normal.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Cliff.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Cliff Height.png", typeof(Texture2D)));
                    rtpTextures.Add((Texture2D)RealWorldTerrainEditorUtils.FindAndLoad("Cliff Normal.png", typeof(Texture2D)));
                }

                TerrainLayer[] tls = new TerrainLayer[4];

                for (int i = 0; i < 4; i++)
                {
                    tls[i] = new TerrainLayer { diffuseTexture = rtpTextures[i * 3] };
                }

                item.terrain.terrainData.terrainLayers = tls;

                ReliefTerrain reliefTerrain = item.gameObject.GetComponent<ReliefTerrain>();
                if (reliefTerrain == null) reliefTerrain = item.gameObject.AddComponent<ReliefTerrain>();
                reliefTerrain.InitArrays();
                reliefTerrain.ColorGlobal = generatedTexture;
                reliefTerrain.globalSettingsHolder.GlobalColorMapDistortByPerlin = 0;
#endif
            }
            else if (item.prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                Renderer[] rs = item.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rs) r.sharedMaterial.mainTexture = generatedTexture;
            }

            RealWorldTerrainPhase.phaseComplete = true;
            textureFilename = string.Empty;
            item.generatedTextures = true;
        }

        private Color32 GetColor(double x, double y)
        {
            if (!loaded) LoadTexture();

            double px = (x - leftMercator) / mercatorWidth * RealWorldTerrainUtils.TILE_SIZE;
            double py = (bottomMercator - y) / mercatorHeight * RealWorldTerrainUtils.TILE_SIZE;

            int ix = (int) px;
            if (ix < 0) ix = 0;
            else if (ix >= RealWorldTerrainUtils.TILE_SIZE) ix = RealWorldTerrainUtils.TILE_SIZE - 1;

            float rx = (float)(px - ix);
            int nx = ix + 1;
            if (nx >= RealWorldTerrainUtils.TILE_SIZE) nx -= 1;

            int iy = (int) py;
            if (iy < 0) iy = 0;
            else if (iy >= RealWorldTerrainUtils.TILE_SIZE) iy = RealWorldTerrainUtils.TILE_SIZE - 1;

            float ry = (float)(py - iy);
            int ny = iy + 1;
            if (ny >= RealWorldTerrainUtils.TILE_SIZE) ny -= 1;

            Color32 c1 = textureColors[iy * RealWorldTerrainUtils.TILE_SIZE + ix];
            Color32 c2 = textureColors[iy * RealWorldTerrainUtils.TILE_SIZE + nx];
            Color32 c3 = textureColors[ny * RealWorldTerrainUtils.TILE_SIZE + ix];
            Color32 c4 = textureColors[ny * RealWorldTerrainUtils.TILE_SIZE + nx];

            Color32 c5 = Color32.LerpUnclamped(c1, c2, rx);
            Color32 c6 = Color32.LerpUnclamped(c3, c4, rx);

            return Color32.LerpUnclamped(c5, c6, ry);
        }

        private static Color32 GetTextureColor(double x, double y)
        {
            if (lastTexture != null && lastTexture.zoom == maxZoomLevel && lastTexture.Contains(x, y)) return lastTexture.GetColor(x, y);

            for (int i = lastTextureIndex + 1; i < reqTextures.Length; i++)
            {
                RealWorldTerrainTextureGenerator t = reqTextures[i];
                if (t.Contains(x, y))
                {
                    lastTextureIndex = t.zoom == maxZoomLevel? i: -1;
                    lastTexture = t;
                    return t.GetColor(x, y);
                }
            }

            return Color.red;
        }

        public static void Init()
        {
            int textureLevel;
            if (prefs.maxTextureLevel == 0)
            {
                int tx = 0;
                int ty = 0;
                textureLevel = 0;

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
            }
            else textureLevel = prefs.maxTextureLevel;

            maxTextureLevel = textureLevel;

            textures = new List<RealWorldTerrainTextureGenerator>();
            for (int z = textureLevel; z > 4; z--)
            {
                double bx, by, ex, ey;
                RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, z, out bx, out by);
                RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, z, out ex, out ey);

                for (int x = (int)bx; x <= (int)ex; x++)
                {
                    for (int y = (int)by; y <= (int)ey; y++)
                    {
                        RealWorldTerrainTextureGenerator texture = new RealWorldTerrainTextureGenerator(x, y, z);
                        textures.Add(texture);
                    }
                }
            }
        }

        private bool Intersects(RealWorldTerrainMonoBase item)
        {
            double x1 = item.leftMercator;
            double y1 = item.topMercator;
            double x2 = item.rightMercator;
            double y2 = item.bottomMercator;

            bool xIn = leftMercator >= x1 && leftMercator <= x2 || rightMercator >= x1 && rightMercator <= x2;
            bool yIn = topMercator >= y1 && topMercator <= y2 || bottomMercator >= y1 && bottomMercator <= y2;
            bool xOut = leftMercator <= x1 && rightMercator >= x2;
            bool yOut = topMercator <= y1 && bottomMercator >= y2;

            return xIn && (yIn || yOut) || xOut && (yIn || yOut);
        }

        private bool Intersects(double x1, double y1, double x2, double y2)
        {
            bool xIn = leftMercator >= x1 && leftMercator <= x2 || rightMercator >= x1 && rightMercator <= x2;
            bool yIn = topMercator >= y1 && topMercator <= y2 || bottomMercator >= y1 && bottomMercator <= y2;
            bool xOut = leftMercator <= x1 && rightMercator >= x2;
            bool yOut = topMercator <= y1 && bottomMercator >= y2;

            return xIn && (yIn || yOut) || xOut && (yIn || yOut);
        }

        private void Load()
        {
            if (textureColors != null || !downloaded) return;
            if (new FileInfo(path).Length == 0)
            {
                RealWorldTerrainUtils.SafeDeleteFile(path);
                return;
            }

            if (zoom > maxZoomLevel) maxZoomLevel = zoom;

            exist = true;
        }

        private void LoadTexture()
        {
            Texture2D texture = new Texture2D(RealWorldTerrainUtils.TILE_SIZE, RealWorldTerrainUtils.TILE_SIZE);
            texture.LoadImage(File.ReadAllBytes(path));
            texture.wrapMode = TextureWrapMode.Clamp;
            textureColors = texture.GetPixels32();
            UnityEngine.Object.DestroyImmediate(texture);
            loaded = true;
        }

        private static void Prepare()
        {
            if (ready) return;
            foreach (RealWorldTerrainTextureGenerator texture in textures) texture.Load();
            textures.RemoveAll(t => !t.exist);
            ready = true;
        }

        private string ReplaceToken(Match match)
        {
            string v = match.Value.ToLower().Trim('{', '}');
            if (v == "zoom") return zoom.ToString();
            if (v == "x") return tileX.ToString();
            if (v == "y") return tileY.ToString();
            if (v == "quad") return RealWorldTerrainUtils.TileToQuadKey(tileX, tileY, zoom);
            return v;
        }

        private void Unload()
        {
            if (!loaded) return;
            textureColors = null;
            loaded = false;
        }

        private abstract class ThreadGenerator
        {
            public bool completed;
            public int start;
            public int end;
            public double sx;
            public double x1;
            public double y2;
            public double sy;
            public float progress;

            private RealWorldTerrainTextureGenerator lt;
            protected int lti;

            public virtual void Dispose()
            {
                lt = null;
            }

            protected Color32 GetTextureColor2(double x, double y)
            {
                if (lt != null && lt.zoom == maxZoomLevel && lt.Contains(x, y)) return lt.GetColor(x, y);

                for (int i = lti + 1; i < reqTextures.Length; i++)
                {
                    RealWorldTerrainTextureGenerator t = reqTextures[i];
                    if (!t.Contains(x, y)) continue;

                    lti = t.zoom == maxZoomLevel ? i : -1;
                    lt = t;
                    return t.GetColor(x, y);
                }

                return Color.red;
            }

            public abstract void Start();
        }

        private class TextureThreadGenerator: ThreadGenerator
        {
            public Color32[][] colors;

            public override void Dispose()
            {
                base.Dispose();
                colors = null;
            }

            public override void Start()
            {
                colors = new Color32[end - start][];

                for (int tx = start; tx < end; tx++)
                {
                    Color32[] cs = colors[tx - start] = new Color32[textureHeight];
                    double px = sx * tx + x1;
                    lti = -1;

                    for (int ty = 0; ty < textureHeight; ty++)
                    {
                        double py = y2 - sy * ty;
                        cs[ty] = GetTextureColor2(px, py);
                    }

                    progress = (tx - start) / (float)(end - start);
                }

                progress = 1;
                completed = true;
            }
        }

        private class HugeTextureThreadGenerator: ThreadGenerator
        {
            public override void Start()
            {
                for (int ty = 0; ty < textureHeight; ty++)
                {
                    double py = y2 - sy * ty;
                    lti = -1;
                    int ry = (textureHeight - ty - 1) * textureWidth;

                    for (int tx = start; tx < end; tx++)
                    {
                        double px = sx * tx + x1;
                        colors[ry + tx] = GetTextureColor2(px, py);
                    }

                    progress = ty / (float)textureHeight;
                }

                progress = 1;
                completed = true;
            }
        }
    }
}