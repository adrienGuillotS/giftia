using KeychainQuickDesigner.Module.Constants;
using KeychainQuickDesigner.Module.Enums;
using KeychainQuickDesigner.Module.Helpers;
using KeychainQuickDesigner.Module.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Corel.Interop.VGCore;
using Application = Corel.Interop.VGCore.Application;
using Page = Corel.Interop.VGCore.Page;

namespace KeychainQuickDesigner.Module.Services
{
    public class CorelDrawService(Application app) : ICorelDrawService
    {
        public Application CorelApp { get; } = app ?? throw new ArgumentNullException(nameof(app));

        public Document CreateNewDocument(string name, double pageWidth, double pageHeight)
        {
            var doc = CorelApp.CreateDocument();
            doc.Name = name;
            doc.Activate();
            doc.Unit = cdrUnit.cdrMillimeter;
            doc.ActivePage.SetSize(pageWidth, pageHeight);
            doc.Rulers.VUnits = doc.Rulers.HUnits = cdrUnit.cdrMillimeter;
            return doc;
        }

        public Layer CreateMasterLayer(Document doc)
        {
            return doc.ActivePage.CreateLayer("Master Layer");
        }

        public Shape ImportImage(Layer layer, string path)
        {
            var options = CorelApp.CreateStructImportOptions();
            options.MaintainLayers = true;

            var filter = layer.ImportEx(path, Options: options);
            filter.Finish();

            return layer.FindShape(System.IO.Path.GetFileName(path));
        }

        public Shape ResizeShape(Shape shape, double size, bool isWidth = false)
        {
            var ratio = isWidth ? size / shape.SizeWidth : size / shape.SizeHeight;
            shape.SetSize(
                isWidth ? size : shape.SizeWidth * ratio,
                isWidth ? shape.SizeHeight * ratio : size
            );
            return shape;
        }

        public void AlignShapes(Shape a, Shape b)
        {
            var rangeA = CorelApp.CreateShapeRange();
            rangeA.Add(a);

            var rangeB = CorelApp.CreateShapeRange();
            rangeB.Add(b);

            rangeA.AlignRangeToShapeRange(cdrAlignType.cdrAlignHCenter, rangeB);
            rangeA.AlignRangeToShapeRange(cdrAlignType.cdrAlignVCenter, rangeB);
        }
        public void SaveCdrFile(Document doc, string destinationFolder)
        {
            if (doc == null || string.IsNullOrWhiteSpace(destinationFolder))
                return;

            //Task.Run(() =>
            //{
            try
            {
                // Turn off UI redraw for speed
                CorelApp.Optimization = true;

                // Build file path
                string filePath = Path.Combine(destinationFolder, doc.Name + ".cdr");

                // If doc already saved once, Save() is faster than SaveAs()
                if (!string.IsNullOrWhiteSpace(doc.FileName) &&
                    string.Equals(Path.GetExtension(doc.FileName), ".cdr", StringComparison.OrdinalIgnoreCase))
                {
                    doc.Save();
                }
                else
                {
                    doc.SaveAs(filePath);
                }
                doc?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving {doc.Name}: {ex.Message}");
            }
            finally
            {
                // Turn UI redraw back on
                CorelApp.Optimization = false;
                CorelApp.Refresh();
            }
            //});
        }


        public void SaveDocument(Document document, string destinationFolder, DocumentExportTypeEnum format)
        {
            if (document == null) { throw new NullReferenceException("Document cannot be null."); }

            if (format == DocumentExportTypeEnum.Cdr)
            {
                SaveCdrFile(document, destinationFolder);
                // ComHelper.SafeRelease(document);
                ComHelper.ForceGC();
            }
            else if (format == DocumentExportTypeEnum.Svg)
            {
                string fileName = Path.Combine(destinationFolder, document.Name);
                string svgPath = fileName + ".svg";
                document.Export(svgPath, cdrFilter.cdrSVG, cdrExportRange.cdrAllPages);
                document?.Close();
            }
            else
            {
                throw new ArgumentException($"Unsupported export format: {format}");
            }
        }

        public void Refresh()
        {
            CorelApp.Refresh();
        }
        public List<string> ExportDocumentsToTemporaryPdfs(string cdrFilesDirectoryPath, string tempFolderPath, IProgress<int> progress)
        {
            if (string.IsNullOrWhiteSpace(cdrFilesDirectoryPath))
                throw new ArgumentException("Output PDF path cannot be null or empty.", nameof(cdrFilesDirectoryPath));

            List<string> tempPdfPaths = [];

            try
            {
                // Get files using more efficient pattern matching
                var matchingFiles = Directory.EnumerateFiles(cdrFilesDirectoryPath, "*.cdr")
                    .Where(file => AppConstants.FileNameRegex.IsMatch(Path.GetFileNameWithoutExtension(file)))
                    .ToList();

                int totalFiles = matchingFiles.Count;
                if (totalFiles == 0)
                {
                    progress.Report(100);
                    return tempPdfPaths;
                }

                // Ensure the temporary folder exists
                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }
                else
                {
                    ClearTemporaryFolder(tempFolderPath);
                }

                for (int j = 0; j < totalFiles; j++)
                {
                    Document doc = CorelApp.OpenDocument(matchingFiles[j]);

                    try
                    {
                        FitPageToContent(doc);

                        // Define PDF export settings
                        doc.PDFSettings.ColorMode = Corel.Interop.VGCore.pdfColorMode.pdfCMYK; // Example: CMYK color mode
                        doc.PDFSettings.ComplexFillsAsBitmaps = true;
                        doc.PDFSettings.FountainSteps = 256; // Higher value for smoother gradients
                        doc.PDFSettings.EmbedFonts = true; // Embed fonts for consistent viewing
                        doc.PDFSettings.BitmapCompression = pdfBitmapCompressionType.pdfJPEG; // JPEG compression for images
                        doc.PDFSettings.JPEGQualityFactor = 80; // JPEG quality (0-100)
                        //doc.PrintSettings.FileMode = PrnFileMode.prnSingleFile;
                        //doc.PrintSettings.Layout.Placement = PrnPlaceType.prnPlaceFitToPage;

                        // Construct a unique temporary file name for each PDF
                        string safeDocName = SanitizeFileName(doc.Name);
                        string tempPdfPath = System.IO.Path.Combine(tempFolderPath, $"{safeDocName}_{Guid.NewGuid()}.pdf");

                        Debug.WriteLine($"Exporting '{doc.Name}' to '{tempPdfPath}'...");
                        doc.PublishToPDF(tempPdfPath);

                        tempPdfPaths.Add(tempPdfPath);
                        Debug.WriteLine($"Successfully exported '{doc.Name}'.");

                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception($"Error exporting document '{doc.Name}': {ex.Message}");
                    }
                    finally
                    {
                        if (doc != null)
                        {
                            doc?.Close();
                        }
                        progress?.Report((j + 1) * 100 / totalFiles);
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return tempPdfPaths;
        }


        public void FitPageToContent(Document doc)
        {
            Page page = doc.ActivePage;
            ShapeRange shapes = page.Shapes.All();
            var boundingBox = shapes.BoundingBox;

            double currentPageWidth = page.SizeWidth;
            double rightmostExtent = boundingBox.Right;

            // If content extends beyond current page width, increase page width
            if (rightmostExtent > currentPageWidth)
            {
                double newWidth = currentPageWidth + rightmostExtent + 1; // Add 1 unit for buffer
                page.SetSize(newWidth, page.SizeHeight + 3);
            }
        }


        private static void ClearTemporaryFolder(string tempFolderPath)
        {
            // Clear existing files in the temporary folder
            foreach (var file in System.IO.Directory.GetFiles(tempFolderPath, "*.pdf"))
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"Error deleting file '{file}': {ex.Message}");
                }
            }
        }

        // Helper method to sanitize file names for paths
        private static string SanitizeFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
