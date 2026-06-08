using DesignerSuite.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DesignerSuite.Core.Services.Temu
{
    
    public class TemuSkuExtractorService : IZipOrderExtractorService
    {
        public Dictionary<string, List<string>> GetZipFilesGrouped(string folderPath)
        {
            var skuToZipMapping = new Dictionary<string, List<string>>();
            var zipFiles = Directory.GetFiles(folderPath, "*.zip");

            foreach (var zipFilePath in zipFiles)
            {
                var sku = ExtractKeyFromZip(zipFilePath);
                if (string.IsNullOrEmpty(sku))
                    continue;

                if (!skuToZipMapping.ContainsKey(sku))
                    skuToZipMapping[sku] = new List<string>();

                skuToZipMapping[sku].Add(zipFilePath);
            }

            return skuToZipMapping;
        }

        public string ExtractKeyFromZip(string zipFilePath)
        {
            string fileName = Path.GetFileName(zipFilePath);
            var match = Regex.Match(fileName, @"SKU\s*ID\s*-\s*(\d+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }

}
