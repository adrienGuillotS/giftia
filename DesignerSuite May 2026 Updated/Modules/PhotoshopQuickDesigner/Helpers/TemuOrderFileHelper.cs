using DesignerSuite.Core.Models;
using DesignerSuite.Core.Utilities;
using PSQuickDesigner;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoshopQuickDesigner.Module.Helpers
{
    public static class TemuOrderFileHelper
    {
        public static List<OrderDocuement> ExtractImages(string zipPath, string destPath)
        {
            Directory.CreateDirectory(destPath);

            var images = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string preview = null;

            string text1 = "", text2 = "", color1 = "", color2 = "";
            using (var zipArvhive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in zipArvhive.Entries)
                {
                    if (IsImage(entry.FullName))
                    {
                        var normalized = entry.FullName.NormalizeFileName();
                        var outPath = SaveEntry(entry, destPath, normalized);

                        var file = normalized.ToLowerInvariant();

                        if (file.Contains("preview_image"))
                            preview = normalized;
                        else if (file.Contains("image1") || file.Contains("image_1"))
                            images["1"] = normalized;
                        else if (file.Contains("image2") || file.Contains("image_2"))
                            images["2"] = normalized;
                    }
                    else if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        string json = SaveAndRead(entry, destPath);

                        var info = TextColorExtractor.Extract(json);
                        if (info?.Count > 0)
                        {
                            text1 = info[0].Text;
                            color1 = info[0].ColorHex;

                            if (info.Count > 1)
                            {
                                text2 = info[1].Text;
                                color2 = info[1].ColorHex;
                            }
                        }
                    }
                }
            }
            string orderId = Path.GetFileNameWithoutExtension(zipPath).ExtractOrderNumber();
            var list = new List<OrderDocuement>();

            bool isSideTwoPresent = !string.IsNullOrWhiteSpace(images.GetValueOrDefault("2")) || !string.IsNullOrWhiteSpace(text2);

            if (!string.IsNullOrWhiteSpace(images.GetValueOrDefault("1")) || !string.IsNullOrWhiteSpace(text1))
                list.Add(Create(orderId, isSideTwoPresent ? 1 : 0, images.GetValueOrDefault("1"), preview, text1, color1));

            if (isSideTwoPresent)
                list.Add(Create(orderId, 2, images.GetValueOrDefault("2"), preview, text2, color2));

            return list;
        }

        private static bool IsImage(string n) =>
            n.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || n.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || n.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);

        private static string SaveEntry(ZipArchiveEntry e, string dest, string file)
        {
            string full = Path.Combine(dest, file);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            e.ExtractToFile(full, true);
            return full;
        }

        private static string SaveAndRead(ZipArchiveEntry entry, string dest)
        {
            var full = SaveEntry(entry, dest, entry.FullName);
            return File.ReadAllText(full);
        }

        private static OrderDocuement Create(string orderId, int idx, string img, string preview, string txt, string color)
        {
            return new OrderDocuement
            {
                FontFamily = "Times New Roman",
                DocumentName = idx == 0 ? orderId : $"{orderId}-{idx}",
                MainImage = img,
                SmallImage = preview,
                Text = txt,
                FontColorHex = color
            };
        }
    }
}
