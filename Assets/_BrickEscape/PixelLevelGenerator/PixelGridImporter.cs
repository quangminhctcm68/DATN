// Doc Texture2D -> ImageGrid (crop/downsample, alpha/background rule).
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 1, 7.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public class PixelGridImportConfig
    {
        public ImportMode Mode = ImportMode.ExactPixel;
        public float AlphaThreshold = 0.5f;
        public BackgroundRule BackgroundRule = BackgroundRule.TransparentIsVoid;
        public Color32 VoidBackgroundColor = new Color32(255, 255, 255, 255);
        public int DownsampleFactor = 2;
        public RectInt CropRect;
        public bool Quantize = false;
    }

    public static class PixelGridImporter
    {
        /// <summary>
        /// Kiem tra texture co dat yeu cau import khong (Point filter, no mipmap, Read/Write enabled).
        /// Tra ve danh sach ly do sai, rong neu OK.
        /// </summary>
        public static List<string> CheckImportSettings(Texture2D texture)
        {
            var problems = new List<string>();
            if (texture == null)
            {
                problems.Add("Chua chon Texture2D.");
                return problems;
            }

            string path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                problems.Add("Khong tim thay TextureImporter cho asset nay.");
                return problems;
            }

            if (!importer.isReadable) problems.Add("Read/Write chua bat (Read/Write Enabled).");
            if (importer.filterMode != FilterMode.Point) problems.Add("FilterMode khong phai Point.");
            if (importer.mipmapEnabled) problems.Add("Mipmap dang bat, can tat de giu pixel-art sac net.");
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                problems.Add("Texture dang bi compress; nen dat Uncompressed de doc mau chinh xac.");

            return problems;
        }

        /// <summary>
        /// Tu sua import setting sau khi GD xac nhan bam "Fix Import Settings".
        /// KHONG duoc goi tu dong ma khong co xac nhan cua GD (dac ta muc 1, 7.1).
        /// </summary>
        public static void FixImportSettings(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        public static ImageGrid Import(Texture2D texture, PixelGridImportConfig config, out List<string> errors)
        {
            errors = new List<string>();
            if (texture == null)
            {
                errors.Add("Texture rong.");
                return null;
            }

            int srcW = texture.width;
            int srcH = texture.height;

            int startX = 0, startY = 0, regionW = srcW, regionH = srcH;
            if (config.Mode == ImportMode.Crop)
            {
                startX = Mathf.Clamp(config.CropRect.x, 0, srcW - 1);
                startY = Mathf.Clamp(config.CropRect.y, 0, srcH - 1);
                regionW = Mathf.Clamp(config.CropRect.width, 1, srcW - startX);
                regionH = Mathf.Clamp(config.CropRect.height, 1, srcH - startY);
            }

            Color32[] pixels;
            try
            {
                pixels = texture.GetPixels32();
            }
            catch (UnityException)
            {
                errors.Add("Texture khong doc duoc (Read/Write Enabled = false). Bam Fix Import Settings.");
                return null;
            }

            System.Func<int, int, Color32> sample = (sx, sy) =>
            {
                sx = Mathf.Clamp(sx, 0, srcW - 1);
                sy = Mathf.Clamp(sy, 0, srcH - 1);
                // Unity texture data la bottom-left origin; anh "y tang xuong" theo quy uoc tai lieu
                // nen dao truc y khi doc.
                int flippedY = srcH - 1 - sy;
                return pixels[flippedY * srcW + sx];
            };

            ImageGrid grid;
            if (config.Mode == ImportMode.Downsample)
            {
                int factor = Mathf.Max(1, config.DownsampleFactor);
                int outW = Mathf.CeilToInt(regionW / (float)factor);
                int outH = Mathf.CeilToInt(regionH / (float)factor);
                grid = new ImageGrid(outW, outH) { Mode = config.Mode };
                for (int ox = 0; ox < outW; ox++)
                {
                    for (int oy = 0; oy < outH; oy++)
                    {
                        var dominant = DominantColorInBlock(sample, startX + ox * factor, startY + oy * factor, factor, factor, regionW, regionH);
                        grid.Cells[ox, oy] = BuildCell(dominant, config);
                    }
                }
            }
            else
            {
                // ExactPixel hoac Crop (crop roi dung exact pixel)
                grid = new ImageGrid(regionW, regionH) { Mode = config.Mode };
                for (int ox = 0; ox < regionW; ox++)
                {
                    for (int oy = 0; oy < regionH; oy++)
                    {
                        Color32 c = sample(startX + ox, startY + oy);
                        grid.Cells[ox, oy] = BuildCell(c, config);
                    }
                }
            }

            // Thu thap distinct source colors tren cac cell board (bo void).
            var seen = new HashSet<Color32ArgbKey>();
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid.Cells[x, y];
                    if (cell.IsVoid) continue;
                    var key = new Color32ArgbKey(cell.SourceColor);
                    if (seen.Add(key))
                        grid.DistinctColors.Add(cell.SourceColor);
                }
            }

            if (grid.Width > 50)
                errors.Add($"Ket qua {grid.Width} cot vuot gioi han 50 cot. Crop hoac downsample anh.");
            if (grid.Width < 1 || grid.Height < 1)
                errors.Add("Kich thuoc grid khong hop le (width/height < 1).");

            return grid;
        }

        private static ImageCell BuildCell(Color32 c, PixelGridImportConfig config)
        {
            float alpha = c.a / 255f;
            bool isVoid;
            if (config.BackgroundRule == BackgroundRule.TransparentIsVoid)
            {
                isVoid = alpha <= config.AlphaThreshold;
            }
            else
            {
                bool matchesBg = c.r == config.VoidBackgroundColor.r && c.g == config.VoidBackgroundColor.g &&
                                  c.b == config.VoidBackgroundColor.b;
                isVoid = matchesBg || alpha <= config.AlphaThreshold;
            }

            if (isVoid) return ImageCell.Void;

            return new ImageCell
            {
                IsVoid = false,
                SourceColor = c,
                SourcePaletteIndex = -1 // duoc gan lai o ColorPaletteService
            };
        }

        private static Color32 DominantColorInBlock(System.Func<int, int, Color32> sample, int startX, int startY, int w, int h, int regionW, int regionH)
        {
            var counts = new Dictionary<Color32ArgbKey, int>();
            Color32ArgbKey best = default;
            int bestCount = -1;
            for (int dx = 0; dx < w; dx++)
            {
                for (int dy = 0; dy < h; dy++)
                {
                    int sx = startX + dx;
                    int sy = startY + dy;
                    if (dx >= regionW || dy >= regionH) continue;
                    var c = sample(sx, sy);
                    var key = new Color32ArgbKey(c);
                    counts.TryGetValue(key, out int cur);
                    cur++;
                    counts[key] = cur;
                    if (cur > bestCount)
                    {
                        bestCount = cur;
                        best = key;
                    }
                }
            }
            return bestCount < 0 ? new Color32(0, 0, 0, 0) : best.ToColor32();
        }

        private struct Color32ArgbKey : System.IEquatable<Color32ArgbKey>
        {
            public byte R, G, B, A;
            public Color32ArgbKey(Color32 c) { R = c.r; G = c.g; B = c.b; A = c.a; }
            public Color32 ToColor32() => new Color32(R, G, B, A);
            public bool Equals(Color32ArgbKey other) => R == other.R && G == other.G && B == other.B && A == other.A;
            public override bool Equals(object obj) => obj is Color32ArgbKey k && Equals(k);
            public override int GetHashCode() => (R << 24) | (G << 16) | (B << 8) | A;
        }
    }
}