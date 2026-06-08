using Corel.Interop.VGCore;
using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Amazon;
using DesignerSuite.Core.Services.Temu;
using DesignerSuite.Core.Static;
using DesignerSuite.Core.Utilities;
using DesignerSuite.Core.Utilities.Corel;
using QuickDesigner2025;
using QuickDesigner2025.Modals;
using Svg;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Corel.Interop.VGCore;
using WindowsAPICodePack.Dialogs;
using Color = Corel.Interop.VGCore.Color;
using corel = Corel.Interop.VGCore;
using Path = System.IO.Path;
using Shape = Corel.Interop.VGCore.Shape;

namespace QuickDesigner2025
{
    public partial class MainPage : System.Windows.Controls.Page
    {
        #region Private Fields

        private corel.Application corelApp;
        private string extractPath = string.Empty;
        private string processedZipFilesPath = string.Empty;

        private int counter = 0;
        #endregion

        #region Constructor

        public MainPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            ZipsFolderPathTxt.Text = @"G:\Testing Workspace\Umar\Temu\Temu Slate";
            CDRFilesLocationTxt.Text = @"G:\Testing Workspace\Umar\Temu\Temu Slate\Destination Folder";
#endif
            if (!string.IsNullOrEmpty(AppDefaultDirectories.QuickDesigner2025))
            {
                ZipsFolderPathTxt.Text = AppDefaultDirectories.QuickDesigner2025;
                CDRFilesLocationTxt.Text = AppDefaultDirectories.QuickDesigner2025;
            }
        }

        private void ImagesBrowse_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select Source Zip Files Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok &&
                !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                string selectedFolderPath = dialog.FileName;

                // Assuming ZipFolderPathCB is a TextBox or ComboBox in your XAML
                ZipsFolderPathTxt.Text = selectedFolderPath;
            }
        }

        private void CDRBrowse_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select CDR Destination Files Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok &&
                !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                string selectedFolderPath = dialog.FileName;

                // Assuming ZipFolderPathCB is a TextBox or ComboBox in your XAML
                CDRFilesLocationTxt.Text = selectedFolderPath;
            }
        }

        private async void GenerateBtn_Click(object sender, RoutedEventArgs e)
        {
            WorkingModeEnum workingMode = WorkingModeEnum.Amazon;
            bool flowControl = await InvokeGenerateAction(workingMode);
            if (!flowControl)
            {
                return;
            }
        }


        private async void GenerateTemuBtn_Click(object sender, RoutedEventArgs e)
        {
            WorkingModeEnum workingMode = WorkingModeEnum.Temu;
            bool flowControl = await InvokeGenerateAction(workingMode);
            if (!flowControl)
            {
                return;
            }
        }

        private static Corel.Interop.VGCore.Application GetCorelApplicationOrThrow()
        {
            var connector = new CorelConnector();

            if (!connector.ConnectToCorel() || connector.CorelApp is null)
            {
                throw new InvalidOperationException(
                    "CorelDRAW is not started or not compatible with this software.");
            }

            return connector.CorelApp;
        }

        public void StartTask(BgTask bgTask)
        {
            if (bgTask == null)
                return;
            ICUITaskManager taskManager = corelApp.FrameWork.TaskManager;
            try
            {
                taskManager.RunInBackground(cuiTaskPriority.kSoon, bgTask);
            }
            catch (Exception erro)
            {
                corelApp.FrameWork.ShowMessageBox(erro.Message);
            }
        }

        #endregion

        #region Methods

        private void Generate(Progress<object> progress, List<AsinZipCDR> ordersCDR, string destinationFolderPath,
            WorkingModeEnum workingMode = WorkingModeEnum.Amazon)
        {
            int zipCount = 0;
            foreach (var orderCdr in ordersCDR)
            {
                double width = orderCdr.AsinZipOrders.FirstOrDefault().ImageData.PageWidth;
                double height = orderCdr.AsinZipOrders.FirstOrDefault().ImageData.PageHeight;

                Document document = NewDocument(corelApp, ConvertCmToMm(height), ConvertCmToMm(width));
                List<string> processedZipFiles = new List<string>();


                foreach (AsinZipOrder order in orderCdr.AsinZipOrders)
                {
                    Layer layer = document.ActiveLayer;

                    CoreHelper.EmptyTempDirectory(extractPath);
                    Directory.CreateDirectory(extractPath);

                    OrderDataItem? imageDataFromXml = null;
                    IImageDataExtractorService imageDataExtractorService =
                        workingMode == WorkingModeEnum.Amazon ? new AmazonImageDataExtractorService() :
                        new TemuImageDataExtractorService();

                    string orderId = workingMode == WorkingModeEnum.Amazon ? Path.GetFileNameWithoutExtension(order.ZipFilePath).Split('_')[0] :
                        Path.GetFileNameWithoutExtension(order.ZipFilePath).ExtractOrderNumber();

                    imageDataFromXml = (OrderDataItem)imageDataExtractorService.ExtractImages(order.ZipFilePath, extractPath);
                    if (imageDataFromXml == default || imageDataFromXml == null)
                    {
                        throw new Exception("No image data found in the xml file");
                    }


                    ImportImageDataIntoCDR(layer, extractPath, order.ImageData,
                        imageDataFromXml, order.ZipFilePath, orderId);

                    ReportProgress(progress, ((100 - 10) / ordersCDR.Count) * zipCount);
                    zipCount++;
                    processedZipFiles.Add(order.ZipFilePath);
                }

                StartTask(new BgTask(corelApp, document, destinationFolderPath));
                MoveZipFiles(processedZipFiles);
                corelApp.Refresh();
            }
        }

        private void ImportImageDataIntoCDR(Layer masterLayer, string destinationFolderPath, ImageDataCSV positioningData, OrderDataItem imageDataXml, string zipFilePath, string orderId)
        {
            // insert order id from zip file name into the row
            masterLayer.CreateArtisticText(positioningData.OrderNumberX, positioningData.OrderNumberY,
                orderId
                , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                , "Arial", 30, Alignment: cdrAlignment.cdrCenterAlignment);


            string mainImageFile = Path.Combine(destinationFolderPath, imageDataXml.ImageName);

            if (File.Exists(mainImageFile))
            {
                var image = ImportImage(masterLayer, mainImageFile);

                image = ResizeShapeWithDesiredDimensions(masterLayer, image, positioningData.ActualImageWidth, positioningData.ActualImageHeight);
                image.SetPositionEx(cdrReferencePoint.cdrCenter, positioningData.ActualImageX, positioningData.ActualImageY);
            }

            string snapImageFile = Path.Combine(destinationFolderPath, imageDataXml.SnapshotImageName);

            if (File.Exists(snapImageFile))
            {
                var image = ImportImage(masterLayer, snapImageFile);

                image = ResizeShapeWithDesiredDimensions(masterLayer, image, positioningData.PreviewImageWidth, positioningData.PreviewImageWidth);
                image.SetPositionEx(cdrReferencePoint.cdrCenter, positioningData.PreviewImageX, positioningData.PreviewImageY);
            }

            if (!string.IsNullOrWhiteSpace(imageDataXml.InputValue))
            {
                string font = !string.IsNullOrWhiteSpace(imageDataXml.FontFamily) ? imageDataXml.FontFamily : "Arial";


                var text = masterLayer.CreateArtisticText(positioningData.ActualImageX, positioningData.ActualImageY,
                    imageDataXml.InputValue
                    , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                    , font, 30, Alignment: cdrAlignment.cdrCenterAlignment);

                string color = !string.IsNullOrWhiteSpace(imageDataXml.ColorValue) ? imageDataXml.ColorValue : null;
                if (color != null && color != "")
                {
                    //.FromArgb(Convert.ToInt32(color.Replace("#", ""), 16))
                    Color c = masterLayer.Color;
                    c.HexValue = color;
                    text.Fill.ApplyUniformFill(c);
                }
            }
        }
        private Shape ImportImage(Layer masterLayer, string imageFilePath)
        {
            Shape image;

            StructImportOptions sio = corelApp.CreateStructImportOptions();
            sio.MaintainLayers = true;

            var filters = masterLayer.ImportEx(imageFilePath, Options: sio);
            filters.Finish();

            image = masterLayer.FindShape(Path.GetFileName(imageFilePath));

            return image;
        }


        public Shape ResizeShapeWithDesiredDimensions(Layer masterLayer, Shape shape, double desiredWidth, double desiredHeight)
        {
            // Get current dimensions
            double currentWidth = shape.SizeWidth;
            double currentHeight = shape.SizeHeight;

            // Step 1: Scale Width to Fit Desired Width, Maintaining Aspect Ratio
            double scalingFactorWidth = desiredWidth / currentWidth;
            double newWidth = desiredWidth;
            double newHeight = currentHeight * scalingFactorWidth;

            //// Step 2: If Height is Smaller than Desired Height, Adjust Based on Height
            //if (newHeight < desiredHeight)
            //{
            //    double scalingFactorHeight = desiredHeight / currentHeight;
            //    newHeight = desiredHeight;
            //    newWidth = currentWidth * scalingFactorHeight;
            //}

            // Step 3: Resize the Shape to Fit the Desired Dimensions
            shape.SetSize(newWidth, newHeight);

            //// Step 4: If the Resized Shape Exceeds Desired Dimensions, Crop the Excess
            //if (newWidth > desiredWidth || newHeight > desiredHeight)
            //{
            //    shape = CropImageWithIntersection(shape, desiredWidth, desiredHeight);
            //}
            return shape;
        }

        static Shape CropImageWithIntersection(Shape image, double desiredWidth, double desiredHeight)
        {
            // 1. Create a rectangle with the desired dimensions
            Layer layer = image.Layer;
            Shape cropRect = layer.CreateRectangle2(image.PositionX - desiredWidth / 2, image.PositionY - desiredHeight / 2, desiredWidth, desiredHeight);

            // 2. Align the rectangle to the center of the image
            cropRect.SetPositionEx(cdrReferencePoint.cdrCenter, image.PositionX, image.PositionY);

            // 3. Intersect the rectangle with the image
            Shape croppedImage = cropRect.Intersect(image, false, false);

            return croppedImage;
        }

        private string[] GetZipFiles(string directoryPath)
        {
            Directory.CreateDirectory(processedZipFilesPath);
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            return directory.GetFiles("*.zip").OrderBy(p => p.CreationTime).Select(f => f.FullName).ToArray();
        }
        private OrderDataItem GetImagesFromZip(string zipFilePath, string destinationFolderPath)
        {
            var imageDataXml = new OrderDataItem();
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                            )
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName.NormalizeFileName()));

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath);
                        }
                    }
                    else if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath);

                            imageDataXml = OrderDataItem.ReadDataFromXmlFile(destinationPath);
                        }
                    }
                }
            }

            return imageDataXml;
        }
        static double ConvertCmToMm(double centimeters) { return centimeters * 10; }

        private double GetMaxWidth(string imageFilePath, double maxWidth, double maxTextWidth)
        {
            SvgDocument document = SvgDocument.Open(imageFilePath);
            if (document.GetXML().Contains("<text") && !document.GetXML().Contains("<image"))
            {
                return maxTextWidth;
            }
            return maxWidth;
        }



        private Document NewDocument(corel.Application application, double height = 297, double width = 200)
        {
            Document document = application.CreateDocument();
            document.Name = ++counter + "_" + Guid.NewGuid().ToString();
            document.Activate();
            document.Unit = cdrUnit.cdrMillimeter;
            document.ReferencePoint = cdrReferencePoint.cdrCenter;
            document.ActivePage.SetSize(width, height);
            document.Rulers.VUnits = document.Rulers.HUnits = cdrUnit.cdrMillimeter;
            return document;
        }

        private void MoveZipFiles(List<string> zipFiles)
        {
            foreach (string zipFile in zipFiles)
            {
                try
                {
                    File.Move(zipFile, processedZipFilesPath + Path.GetFileName(zipFile), false);
                }
                catch (IOException ex)
                {
                    ReportProgressError($"Failed to move {Path.GetFileName(zipFile)} to destination folder.{Environment.NewLine}{ex.Message}");
                }
            }
            zipFiles.RemoveAll(s => true);
        }

        private void ReportProgressError(string errorMessage)
        {
            RunOnUIThread(() =>
            {
                ErrorConsoleRichTxt.AppendText($"{errorMessage}");
                ErrorConsoleRichTxt.AppendText(Environment.NewLine);

            });
        }

        private static bool IsHexColorCode(string input)
        {
            // Define a regular expression pattern for a valid hex color code
            string pattern = @"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";

            // Use Regex.IsMatch to check if the input matches the pattern
            return Regex.IsMatch(input, pattern);
        }

        private void RunOnUIThread(Action body, DispatcherPriority priority = DispatcherPriority.Render)
        {
            this.Dispatcher.BeginInvoke(body, priority);
        }

        private void ReportProgress(IProgress<object> progress, object value)
        {
            RunOnUIThread(() =>
            {
                progress.Report(value);
            });
        }

        private void DisableEveryThing()
        {
            ZipsFolderPathTxt.IsEnabled =
                ImagesBrowse.IsEnabled =
                CDRFilesLocationTxt.IsEnabled =
                CDRBrowse.IsEnabled =
                GenerateBtn.IsEnabled =
                GenerateTemuBtn.IsEnabled =
                false;
            ErrorConsoleRichTxt.Document.Blocks.Clear();
            counter = 0;
        }

        private void EnableEveryThing()
        {
            ZipsFolderPathTxt.IsEnabled =
                ImagesBrowse.IsEnabled =
                CDRFilesLocationTxt.IsEnabled =
                CDRBrowse.IsEnabled =
                GenerateBtn.IsEnabled =
                GenerateTemuBtn.IsEnabled =
                true;
            Progress_Bar.Value = 0;
        }

        private async Task<bool> InvokeGenerateAction(WorkingModeEnum workingMode)
        {
            if (CDRFilesLocationTxt.Text == "")
            {
                MessageBox.Show("Please select CDR files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (ZipsFolderPathTxt.Text == "")
            {
                MessageBox.Show("Please select zip files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            try
            {

                DisableEveryThing();
                var progress = new Progress<object>(v =>
                {
                    if (v is int value)
                    {
                        Progress_Bar.Value = value;
                    }
                    else
                    {
                        ErrorConsoleRichTxt.AppendText($"{v}{Environment.NewLine}");
                    }
                });
                processedZipFilesPath = ZipsFolderPathTxt.Text + @"\processed\";
                extractPath = CDRFilesLocationTxt.Text + @"\temp\";
                string zipFolderPath = ZipsFolderPathTxt.Text;
                string cdrFilesText = CDRFilesLocationTxt.Text;
                if (GetZipFiles(zipFolderPath)?.Length > 0)
                {
                    List<AsinZipCDR> records = await AsinsExtractor.LoadAsync(zipFolderPath, workingMode);
                    await Task.Run(() =>
                    {
                        corelApp = GetCorelApplicationOrThrow();
                        Generate(progress, records, cdrFilesText, workingMode);
                    });
                    MessageBox.Show("Files generated successfully!", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No zip File found inside '{zipFolderPath}'", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                EnableEveryThing();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                EnableEveryThing();
            }

            return true;
        }
        #endregion
    }

}
