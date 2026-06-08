using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Temu;
using KeychainQuickDesigner.Module;
using KeychainQuickDesigner.Module.Interfaces;
using KeychainQuickDesigner.Module.Models;
using KeychainQuickDesigner.ModuleDocker.Interfaces;
using System.IO;
using System.IO.Compression;
using Corel.Interop.VGCore;

namespace KeychainQuickDesigner.Module.Services
{
    public class ZipProcessingService(IFileReaderService readerService) : IZipProcessingService
    {
        private readonly IFileReaderService _readerService = readerService;

        public WorkingModeEnum WorkingMode { get; set; } = WorkingModeEnum.Amazon;

        public async Task ProcessZipsAsync(
            string zipFolderPath,
            string destinationFolderPath,
            IEnumerable<OrderDataCSV> positioningData,
            IProgress<int> progress,
            ICorelDrawService _corelService,
            IImagePlacementService imageService,
            ISvgImportService svgService)
        {
            string processedZipFilesPath = Path.Combine(zipFolderPath, "processed");
            string extractPath = Path.Combine(destinationFolderPath, "temp");

            Directory.CreateDirectory(processedZipFilesPath);
            Directory.CreateDirectory(extractPath);

            var zipFiles = Directory.GetFiles(zipFolderPath, "*.zip")
                .Select(f => new FileInfo(f))
                .OrderBy(fi => fi.Name)   // .ThenBy(fi => fi.Name)               
                .Select(fi => fi.FullName)
                .ToArray();

            var positioningList = positioningData.ToList();
            var positioningEnumerator = GetPositioningDataEnumerator(positioningList);

            Document? currentDoc = null;
            Layer? masterLayer = null;
            var processedFiles = new List<string>();
            int currentDocIndex = 0;
            var total = Math.Max(zipFiles.Length, 1);
            for (int i = 0; i < zipFiles.Length; i++)
            {
                if (i % 3 == 0)
                {
                    if (currentDoc != null)
                    {
                        svgService.DeleteSvgLayer(currentDoc);
                        _corelService.SaveDocument(currentDoc, destinationFolderPath, Enums.DocumentExportTypeEnum.Cdr);
                        MoveProcessedFiles(processedFiles, processedZipFilesPath);
                        processedFiles.Clear();
                    }

                    currentDoc = _corelService.CreateNewDocument($"{++currentDocIndex}_{Guid.NewGuid()}");
                    masterLayer = _corelService.CreateMasterLayer(currentDoc);
                    svgService.ImportSvgIcons(currentDoc, WorkingMode);
                }

                EmptyTempDirectory(extractPath);

                KeychainOrderDataItem? imageData = ExtractFromZip(zipFiles[i], extractPath, WorkingMode);

                if (masterLayer == null)
                {
                    throw new InvalidOperationException("Master layer is null. Ensure the document and master layer are properly initialized.");
                }

                if (imageData == null)
                {
                    throw new InvalidOperationException($"Image data could not be extracted from zip file: {zipFiles[i]}");
                }

                if (!positioningEnumerator.MoveNext())
                    throw new InvalidOperationException("Positioning data exhausted");

                var position = positioningEnumerator.Current;

                // order id
                var orderId = WorkingMode == WorkingModeEnum.Amazon ? Path.GetFileNameWithoutExtension(zipFiles[i]).Split('_')[0] :
                    Path.GetFileNameWithoutExtension(zipFiles[i]).ExtractOrderNumber(); ;

                imageService.PlaceImages(masterLayer, imageData, zipFiles[i], extractPath, position, orderId);

                processedFiles.Add(zipFiles[i]);
                progress.Report((i + 1) * 100 / total);
            }

            if (currentDoc != null)
            {
                svgService.DeleteSvgLayer(currentDoc);
                _corelService.SaveDocument(currentDoc, destinationFolderPath, Enums.DocumentExportTypeEnum.Cdr);
                MoveProcessedFiles(processedFiles, processedZipFilesPath);
            }
            positioningEnumerator.Dispose();
            _corelService.Refresh();
            progress.Report(100);
            await Task.CompletedTask;
        }

        private KeychainOrderDataItem? ExtractFromZip(string zipPath, string extractPath, WorkingModeEnum workingMode)
        {
            KeychainOrderDataItem? orderDataItem;
            switch (workingMode)
            {
                case WorkingModeEnum.Amazon:
                    orderDataItem = ExtractFromAmazonOrderZip(zipPath, extractPath);
                    break;
                case WorkingModeEnum.Temu:
                    orderDataItem = (KeychainOrderDataItem)new TemuKeychainImageDataExtractorService().ExtractImages(zipPath, extractPath);
                    break;
                default:
                    throw new NotImplementedException($"{workingMode} is not implemented");
            }
            return orderDataItem;
        }

        private KeychainOrderDataItem? ExtractFromAmazonOrderZip(string zipPath, string extractPath)
        {
            KeychainOrderDataItem? imageData = null;
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    var destPath = Path.Combine(extractPath, entry.FullName);
                    if (!destPath.StartsWith(extractPath)) continue;

                    var directoryPath = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    entry.ExtractToFile(destPath, true);

                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        imageData = (KeychainOrderDataItem)_readerService.ReadDataFromFile(destPath);
                    }
                }
            }
            return imageData;
        }

        private static void MoveProcessedFiles(IEnumerable<string> files, string processedZipFilesPath)
        {
            foreach (var file in files)
            {
                try
                {
                    var dest = Path.Combine(processedZipFilesPath, Path.GetFileName(file));
                    //if (File.Exists(dest)) File.Delete(dest);
                    File.Move(file, dest);
                }
                catch { /* log if needed */ }
            }
        }

        private static void EmptyTempDirectory(string extractPath)
        {
            if (!Directory.Exists(extractPath)) return;
            foreach (var f in Directory.GetFiles(extractPath))
            {
                try { File.Delete(f); } catch { }
            }
        }

        private static IEnumerator<OrderDataCSV> GetPositioningDataEnumerator(List<OrderDataCSV> data)
        {
            int index = 0;
            while (true)
            {
                yield return data[index];
                index = (index + 1) % data.Count;
            }
        }
    }
}
