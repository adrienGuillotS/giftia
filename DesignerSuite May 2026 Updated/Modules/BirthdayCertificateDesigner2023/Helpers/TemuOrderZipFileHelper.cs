using BirthdayCertificateDesigner2023.Models;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BirthdayCertificateDesigner2023.Module.Helpers
{
    public static class TemuOrderZipFileHelper
    {
        public static Order GetCustomerDataFromJsonInsideZip(string zipFilePath, string destinationFolderPath)
        {
            destinationFolderPath = Path.GetFullPath(destinationFolderPath);
           
            var order = new Order();
            order.OrderId = Path.GetFileNameWithoutExtension(zipFilePath).ExtractOrderNumber();
            order.CertificateInfos = new List<CertificateInfo>();
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
                            if (textColorInfo.Count < 3)
                            {
                                throw new ArgumentException($"{entry.FullName} must contain Name of the person, Date of birth and the City information.");
                            }
                            if (textColorInfo != null && textColorInfo.Count == 3)
                            {
                                CertificateInfo certificateInfo = new CertificateInfo();
                                certificateInfo.FileName = destinationPath;
                                certificateInfo.PersonName = textColorInfo[0].Text;

                                if (textColorInfo.Count == 3)
                                {
                                    certificateInfo.DateOfBirth = textColorInfo[1].Text;
                                    certificateInfo.City = textColorInfo[2].Text;
                                }
                                order.CertificateInfos.Add(certificateInfo);
                            }
                        }
                    }
                }
            }

            return order;
        }
    }
}
