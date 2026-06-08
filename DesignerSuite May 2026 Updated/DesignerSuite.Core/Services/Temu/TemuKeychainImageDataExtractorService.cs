namespace DesignerSuite.Core.Services.Temu
{
    using DesignerSuite.Core.Interfaces;
    using DesignerSuite.Core.Models;
    using DesignerSuite.Core.Utilities;
    using System;
    using System.IO;
    using System.IO.Compression;

    public class TemuKeychainImageDataExtractorService : IImageDataExtractorService
    {
        public IOrderDataItem ExtractImages(string zipFilePath, string destinationFolderPath)
        {
            var orderDataItem = new KeychainOrderDataItem();

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Extract images
                    if (
                        entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        || entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                        || entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        var normalizedFileName = entry.FullName.NormalizeFileName();
                        var destinationPath = Path.GetFullPath(Path.Combine(destinationFolderPath, normalizedFileName));

                        // Ensure the extraction stays within the destination folder
                        if (destinationPath.StartsWith(destinationFolderPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath, overwrite: true);

                            string lowerName = normalizedFileName.ToLowerInvariant();
                            if (lowerName.Contains("preview_image"))
                            {
                                orderDataItem.SnapshotImageName = normalizedFileName;
                                continue;
                            }
                            else if (lowerName.Contains("image1") || lowerName.Contains("image_1"))
                            {
                                orderDataItem.ImageName = normalizedFileName;
                            }
                            else if (lowerName.Contains("image2") || lowerName.Contains("image_2"))
                            {
                                orderDataItem.SvgImageName = normalizedFileName;
                            }
                        }
                    }
                    // Extract and read JSON metadata
                    else if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.GetFullPath(
                            Path.Combine(destinationFolderPath, entry.FullName));

                        if (destinationPath.StartsWith(destinationFolderPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath, overwrite: true);
                            string json = File.ReadAllText(destinationPath);

                            List<TextColorInfo> textColorInfo = TextColorExtractor.Extract(json);
                            if (textColorInfo != null && textColorInfo.Count > 0)
                            {
                                orderDataItem.InputValue = textColorInfo[0].Text?.RemoveNoText();
                                orderDataItem.ColorValue = textColorInfo[0].ColorHex;
                                if (textColorInfo.Count == 2)
                                {
                                    orderDataItem.SvgIconName = textColorInfo[1].Text;
                                }
                            }
                            orderDataItem.FontFamily = "Times New Roman";
                        }
                    }
                }
            }

            return orderDataItem;
        }
    }

}
