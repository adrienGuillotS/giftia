using CsvHelper;
using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Amazon;
using DesignerSuite.Core.Services.Temu;
using DesignerSuite.Core.Utilities;
using QuickDesigner2023.Module.Enums;
using QuickDesigner2023.Module.Interfaces;
using QuickDesigner2023.Module.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace QuickDesigner2023.Module
{
    public class AsinsExtractor

    {
        public const string MugCsvFileName = "Mugs.csv";
        public const string PhoneCaseCsvFileName = "PhoneCases.csv";
        public const string KeychainCsvFileName = "Keychains.csv";
        public const string AsinFolderCsvFileName = "Asin Folder.csv";

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

            string workingModeSpecificFilder = CoreHelper.GetWorkingModeString(workingMode);
            string resourcesPath = Path.Combine(CoreHelper.GetAppAssemblyPath(), "Resources", workingModeSpecificFilder, "QuickDesigner2023");

            var mugRecords = ExtractImageDataFromCsv<MugCsvRecord>(Path.Combine(resourcesPath, MugCsvFileName))
                             ?? throw new InvalidOperationException("No mug data found in the CSV file.");

            var phoneCaseRecords = ExtractImageDataFromCsv<PhoneCaseCsvRecord>(Path.Combine(resourcesPath, PhoneCaseCsvFileName))
                                   ?? throw new InvalidOperationException("No phone case data found in the CSV file.");

            var keychainRecords = ExtractImageDataFromCsv<KeychainCsvRecord>(Path.Combine(resourcesPath, KeychainCsvFileName))
                                   ?? throw new InvalidOperationException("No keychain data found in the CSV file.");

            // Index ASINs for faster lookup instead of repeatedly filtering with .Where()
            var mugLookup = mugRecords.GroupBy(r => r.ASIN).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var phoneLookup = phoneCaseRecords.GroupBy(r => r.ASIN).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var keychainLookup = keychainRecords.GroupBy(r => r.ASIN).ToDictionary(g => g.Key, g => g.AsEnumerable());

            var asinZipCdrRecords = new List<AsinZipCDR>(asinGroups.Count);

            foreach (var (asin, zipFiles) in asinGroups)
            {
                if (mugLookup.TryGetValue(asin, out var mugCsv))
                {
                    asinZipCdrRecords.Add(new AsinZipCDR
                    {
                        ASIN = asin,
                        AsinType = AsinType.Mug,
                        ZipFiles = zipFiles,
                        CsvRecords = mugCsv
                    });
                }
                else if (phoneLookup.TryGetValue(asin, out var phoneCsv))
                {
                    asinZipCdrRecords.Add(new AsinZipCDR
                    {
                        ASIN = asin,
                        AsinType = AsinType.PhoneCase,
                        ZipFiles = zipFiles,
                        CsvRecords = phoneCsv
                    });
                }
                else if (keychainLookup.TryGetValue(asin, out var keychainCsv))
                {
                    asinZipCdrRecords.Add(new AsinZipCDR
                    {
                        ASIN = asin,
                        AsinType = AsinType.Keychain,
                        ZipFiles = zipFiles,
                        CsvRecords = keychainCsv
                    });
                }
                else
                {
                    asinZipCdrRecords.Add(new AsinZipCDR
                    {
                        ASIN = asin,
                        AsinType = AsinType.None,
                        ZipFiles = zipFiles,
                        CsvRecords = new List<DefaultNonCustomizedRecord>()
                        {
                            new DefaultNonCustomizedRecord(){ RowNumber=1, PositionY=243.5},
                            new DefaultNonCustomizedRecord(){ RowNumber=2, PositionY=148.5},
                            new DefaultNonCustomizedRecord(){ RowNumber=3, PositionY=53.5},

                        }
                    });
                }
            }

            return asinZipCdrRecords;
        }

        #region Private Methods

        private static List<T> ExtractImageDataFromCsv<T>(string csvFilePath)
            where T : ICsvRecord
        {
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<T>().ToList();
            }
        }

        public static List<AsinFolderRecord> ReadAsinFolders(string csvFilePath)
        {
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<AsinFolderRecord>().ToList();
            }
        }

        //private static List<AsinZipCDR> ProcessImageRecordsBySize(
        //    List<MugCsvRecord> imageRecords,
        //    Dictionary<string, List<string>> asinToZipMapping,
        //    string size)
        //{
        //    var sizeFilteredRecords = imageRecords.Where(record => record.SizeCaption == size).ToList();
        //    var asinZipCdrList = new List<AsinZipCDR>();

        //    foreach (var asinKey in asinToZipMapping.Keys)
        //    {
        //        var zipFiles = asinToZipMapping[asinKey];
        //        var asinFilteredRecords = sizeFilteredRecords.Where(record => record.ASIN == asinKey).ToList();
        //        if (asinFilteredRecords.Count == 0)
        //            continue;

        //        var zipFileQueue = new Queue<string>(zipFiles);

        //        while (zipFileQueue.Count > 0)
        //        {
        //            var currentZipChunk = zipFileQueue.Take(asinFilteredRecords.Count).ToList();
        //            var asinZipRecords = new List<AsinZipOrder>();

        //            for (int i = 0; i < asinFilteredRecords.Count; i++)
        //            {
        //                if (i >= currentZipChunk.Count)
        //                    break;

        //                var zipFile = currentZipChunk[i];
        //                asinZipRecords.Add(new AsinZipOrder
        //                {
        //                    ImageData = asinFilteredRecords[i],
        //                    ZipFilePath = zipFile
        //                });
        //            }

        //            asinZipCdrList.Add(new AsinZipCDR { AsinZipOrders = asinZipRecords });

        //            // Remove processed zip files from queue
        //            foreach (var _ in currentZipChunk)
        //            {
        //                zipFileQueue.Dequeue();
        //            }
        //        }
        //    }

        //    return asinZipCdrList;
        //}

        #endregion
    }
}
