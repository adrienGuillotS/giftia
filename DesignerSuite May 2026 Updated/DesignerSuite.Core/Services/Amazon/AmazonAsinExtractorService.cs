using DesignerSuite.Core.Interfaces;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace DesignerSuite.Core.Services.Amazon
{

    public class AmazonAsinExtractorService : IZipOrderExtractorService
    {
        public Dictionary<string, List<string>> GetZipFilesGrouped(string folderPath)
        {
            var asinToZipMapping = new Dictionary<string, List<string>>();
            var zipFiles = Directory.GetFiles(folderPath, "*.zip");

            foreach (var zipFilePath in zipFiles)
            {
                var asin = ExtractKeyFromZip(zipFilePath);
                if (string.IsNullOrEmpty(asin))
                    continue;

                if (!asinToZipMapping.ContainsKey(asin))
                    asinToZipMapping[asin] = new List<string>();

                asinToZipMapping[asin].Add(zipFilePath);
            }

            return asinToZipMapping;
        }

        public string ExtractKeyFromZip(string zipFilePath)
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
                            var xmlDoc = XDocument.Parse(content);
                            var asinElement = xmlDoc.Descendants("asin").FirstOrDefault();
                            if (asinElement != null)
                                return asinElement.Value;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }

}
