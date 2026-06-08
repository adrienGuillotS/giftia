using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Services.Amazon
{
    using DesignerSuite.Core.Interfaces;
    using DesignerSuite.Core.Models;
    using System;
    using System.IO;
    using System.IO.Compression;

    public class AmazonImageDataExtractorService : IImageDataExtractorService
    {
        public IOrderDataItem ExtractImages(string zipFilePath, string destinationFolderPath)
        {
            var imageDataXml = new OrderDataItem();

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Extract image files
                    if (entry.FullName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                        || entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        || entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                        || entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.GetFullPath(
                            Path.Combine(destinationFolderPath, entry.FullName.NormalizeFileName()));

                        if (destinationPath.StartsWith(destinationFolderPath, StringComparison.Ordinal))
                            entry.ExtractToFile(destinationPath, overwrite: true);
                    }

                    // Extract and read XML metadata
                    else if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.GetFullPath(
                            Path.Combine(destinationFolderPath, entry.FullName));

                        if (destinationPath.StartsWith(destinationFolderPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath, overwrite: true);
                            imageDataXml = OrderDataItem.ReadDataFromXmlFile(destinationPath);
                        }
                    }
                }
            }

            return imageDataXml;
        }
    }

}
