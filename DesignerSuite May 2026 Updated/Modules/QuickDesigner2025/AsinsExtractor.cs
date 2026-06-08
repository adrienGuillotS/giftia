using CsvHelper;
using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Amazon;
using DesignerSuite.Core.Services.Temu;
using DesignerSuite.Core.Utilities;
using QuickDesigner2025.Modals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QuickDesigner2025
{
    public class AsinsExtractor
    {
        public const string CsvFilePath = "Test2025.csv";

        public static async Task<List<AsinZipCDR>> LoadAsync(string zipFolderPath, WorkingModeEnum workingMode = WorkingModeEnum.Amazon)
        {
            return await Task.Run(() => Load(zipFolderPath, workingMode));
        }

        public static List<AsinZipCDR> Load(string zipFolderPath, WorkingModeEnum workingMode)
        {
            if (!Directory.Exists(zipFolderPath))
                throw new DirectoryNotFoundException($"The specified path does not exist: {zipFolderPath}");

            IZipOrderExtractorService zipOrderExtractor;
            Dictionary<string, List<string>> asinGroups;
            switch (workingMode)
            {
                case WorkingModeEnum.Amazon:
                    zipOrderExtractor = new AmazonAsinExtractorService();
                    asinGroups = zipOrderExtractor.GetZipFilesGrouped(zipFolderPath);
                    break;
                case WorkingModeEnum.Temu:
                    zipOrderExtractor = new TemuSkuExtractorService();
                    asinGroups = zipOrderExtractor.GetZipFilesGrouped(zipFolderPath);
                    break;
                default:
                    throw new NotImplementedException($"The working mode '{workingMode}' is not supported.");
            }

            string assemblyDir = CoreHelper.GetAppAssemblyPath();

            string csvFile = Path.Combine(assemblyDir, "Resources", CoreHelper.GetWorkingModeString(workingMode), "QuickDesigner2025", CsvFilePath);

            var imageRecords = ExtractImageDataFromCsv(csvFile);

            if (imageRecords == null || imageRecords.Count == 0)
                throw new InvalidOperationException("No image data found in the CSV file.");

            var asinZipCdrRecords = new List<AsinZipCDR>();

            // Process records by sizes
            var sizes = new[] { "15cm", "20cm", "30cm" };
            foreach (var size in sizes)
            {
                asinZipCdrRecords.AddRange(ProcessImageRecordsBySize(imageRecords, asinGroups, size));
            }

            return asinZipCdrRecords;
        }



        #region Private Methods

        private static List<ImageDataCSV> ExtractImageDataFromCsv(string csvFilePath = CsvFilePath)
        {
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<ImageDataCSV>().ToList();
            }
        }

        private static List<AsinZipCDR> ProcessImageRecordsBySize(
            List<ImageDataCSV> imageRecords,
            Dictionary<string, List<string>> asinToZipMapping,
            string size)
        {
            var sizeFilteredRecords = imageRecords.Where(record => record.SizeCaption == size).ToList();
            var asinZipCdrList = new List<AsinZipCDR>();

            foreach (var asinKey in asinToZipMapping.Keys)
            {
                var zipFiles = asinToZipMapping[asinKey];
                var asinFilteredRecords = sizeFilteredRecords.Where(record => record.ASIN == asinKey).ToList();
                if (asinFilteredRecords.Count == 0)
                    continue;

                var zipFileQueue = new Queue<string>(zipFiles);

                while (zipFileQueue.Count > 0)
                {
                    var currentZipChunk = zipFileQueue.Take(asinFilteredRecords.Count).ToList();
                    var asinZipRecords = new List<AsinZipOrder>();

                    for (int i = 0; i < asinFilteredRecords.Count; i++)
                    {
                        if (i >= currentZipChunk.Count)
                            break;

                        var zipFile = currentZipChunk[i];
                        asinZipRecords.Add(new AsinZipOrder
                        {
                            ImageData = asinFilteredRecords[i],
                            ZipFilePath = zipFile
                        });
                    }

                    asinZipCdrList.Add(new AsinZipCDR { AsinZipOrders = asinZipRecords });

                    // Remove processed zip files from queue
                    foreach (var _ in currentZipChunk)
                    {
                        zipFileQueue.Dequeue();
                    }
                }
            }

            return asinZipCdrList;
        }

        #endregion
    }

}
