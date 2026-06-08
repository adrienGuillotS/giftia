using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DesignerSuite.Core.Utilities
{
    public static class AsinsExtractorHelper
    {
        public static Dictionary<string, List<string>> GetZipFilesGroupedByAsin(string folderPath)
        {
            var asinToZipMapping = new Dictionary<string, List<string>>();
            var zipFiles = Directory.GetFiles(folderPath, "*.zip");

            foreach (var zipFilePath in zipFiles)
            {
                var asin = ExtractAsinFromZip(zipFilePath);
                if (string.IsNullOrEmpty(asin))
                    continue;

                if (!asinToZipMapping.ContainsKey(asin))
                {
                    asinToZipMapping[asin] = new List<string>();
                }

                asinToZipMapping[asin].Add(zipFilePath);
            }

            return asinToZipMapping;
        }

        public static Dictionary<string, List<string>> GetZipFilesGroupedBySkus(string folderPath)
        {
            var asinToZipMapping = new Dictionary<string, List<string>>();
            var zipFiles = Directory.GetFiles(folderPath, "*.zip");

            foreach (var zipFilePath in zipFiles)
            {
                var asin = ExtractAsinFromZip(zipFilePath);
                if (string.IsNullOrEmpty(asin))
                    continue;

                if (!asinToZipMapping.ContainsKey(asin))
                {
                    asinToZipMapping[asin] = new List<string>();
                }

                asinToZipMapping[asin].Add(zipFilePath);
            }

            return asinToZipMapping;
        }

        private static string ExtractAsinFromZip(string zipFilePath)
        {
            using (var archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var reader = new StreamReader(entry.Open()))
                        {
                            var content = reader.ReadToEnd();
                            var asin = ExtractAsinFromXml(content);
                            if (!string.IsNullOrEmpty(asin))
                                return asin;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static string ExtractAsinFromXml(string xmlContent)
        {
            var xmlDoc = XDocument.Parse(xmlContent);
            var asinElement = xmlDoc.Descendants("asin").FirstOrDefault();
            return asinElement != null ? asinElement.Value : string.Empty;
        }
    }
}
