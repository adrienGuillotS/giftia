using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Amazon;
using DesignerSuite.Core.Services.Temu;
using DesignerSuite.Core.Static;
using DesignerSuite.Core.Utilities;
using DesignerSuite.Core.Utilities.Corel;
using MahApps.Metro.Controls;
using QuickDesigner2023.Module;
using QuickDesigner2023.Module.Interfaces;
using QuickDesigner2023.Module.Models;
using QuickDesigner2023.Module.Services;
using QuickDesigner2023.Module.Static;
using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.XPath;
using Corel.Interop.VGCore;
using WindowsAPICodePack.Dialogs;
using Path = System.IO.Path;

namespace QuickDesinger2023
{
    public partial class MainPage : System.Windows.Controls.Page
    {
        #region Private Fields

        private Corel.Interop.VGCore.Application corelApp;

        private const double PageWidth = 210;
        private const double PageHeight = 297;
        private const double ImageMaxWidth = 80;
        private const double ImageMaxHeight = 95;
        private const double MaxTextWidth = 70;

        private const double MarginLeft = 5;
        private const double MarginTop = (PageHeight - (ImageMaxHeight * 3)) / 2;
        private const double MarginRight = 40;
        private const double MarginBottom = MarginTop;

        private string extractPath = string.Empty;
        private string processedZipFilesPath = string.Empty;

        private int counter = 0;

        private Dictionary<string, Dictionary<string, List<string>>> inputTexts;
        private Dictionary<string, string> svgToSnapMapping;

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
            ZipsFolderPathTxt.Text = @"D:\Testing Workspace\Umar\Bulk Order Processor\Test Downloaded Files\New Files\New folder";
            CDRFilesLocationTxt.Text = @"D:\Testing Workspace\Umar\Bulk Order Processor\Test Downloaded Files\New Files\New folder";
#endif
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
                ZipsFolderPathTxt.Text = ZipsFolderPathTxt.Text.TrimEnd(Path.DirectorySeparatorChar);
                CDRFilesLocationTxt.Text = CDRFilesLocationTxt.Text.TrimEnd(Path.DirectorySeparatorChar);

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
                        Generate(progress, zipFolderPath, cdrFilesText, records, workingMode);
                    });
                    if (Directory.GetFiles(processedZipFilesPath).Count() > 0)
                    {
                        ZipsFolderPathTxt.Text = processedZipFilesPath;
                        _ = MoveZipFiles(processedZipFilesPath, cdrFilesText, workingMode);
                    }
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

        private string[] GetZipFiles(string directoryPath)
        {
            Directory.CreateDirectory(processedZipFilesPath);
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            return directory.GetFiles("*.zip").OrderBy(p => p.CreationTime).Select(f => f.FullName).ToArray();
        }

       
        private Document NewDocument(Corel.Interop.VGCore.Application application)
        {
            Document document = application.CreateDocument();
            document.Name = ++counter + "_" + Guid.NewGuid().ToString();
            document.Activate();
            document.Unit = cdrUnit.cdrMillimeter;
            document.ActivePage.SetSize(210, 297);
            document.Rulers.VUnits = document.Rulers.HUnits = cdrUnit.cdrMillimeter;
            return document;
        }
        private void Generate(IProgress<object> progress, string zipFolder, string destinationFolderPath,
            List<AsinZipCDR> asinZipCDRs, WorkingModeEnum workingMode = WorkingModeEnum.Amazon)
        {
            List<string> processedZipFiles = new List<string>();
            Document document = null;
            double positionX = 0;
            double positionY = 0;

            ISvgImportService svgService = new SvgImportService(corelApp);
            Layer? masterLayer = null;
            int zipCount = 1;
            int rowIndex = 1;
            string[] zipFiles = GetZipFiles(zipFolder);

            var yPositions = new Dictionary<int, double>
                {
                    { 1, PageHeight - MarginTop + ImageMaxHeight }, // First row
                    { 2, 92.5 }, // Second row
                    { 3, 6.0 }   // Third row
                };
            bool isKeychainSignIconsLoaded = false;
            foreach (string zipFilePath in zipFiles)
            {
                if ((zipCount - 1) % 3 == 0)
                {
                    rowIndex = 1;
                    if (document != null)
                    {
                        // if keychain sign icons are loaded, delete them
                        if (isKeychainSignIconsLoaded)
                            svgService?.DeleteSvgLayer(document);
                        CreateGuideLines(document.ActiveLayer);
                        StartTask(new BgTask(corelApp, document, destinationFolderPath));
                        MoveZipFiles(processedZipFiles);
                    }
                    document = NewDocument(corelApp);
                    masterLayer = document.ActivePage.ActiveLayer;
                    positionX = MarginLeft;
                    //positionY = PageHeight + MaxHeight;
                    positionY = yPositions[rowIndex];
                    isKeychainSignIconsLoaded = false;
                }
                if (document == null)
                {
                    document = NewDocument(corelApp);
                    masterLayer = document.ActivePage.ActiveLayer;
                }

                if (masterLayer == null)
                {
                    throw new InvalidOperationException("Master layer is null. Ensure the document and master layer are properly initialized.");
                }

                Directory.CreateDirectory(extractPath);
                CoreHelper.EmptyTempDirectory(extractPath);

                var order = asinZipCDRs.FirstOrDefault(item => item.ZipFiles.Contains(zipFilePath));

                ArgumentNullException.ThrowIfNull(order);

                List<ICsvRecord>? CsvRecords = order?.CsvRecords?.ToList();

                switch (order?.AsinType)
                {
                    case QuickDesigner2023.Module.Enums.AsinType.Mug:

                        if (CsvRecords == null)
                        {
                            throw new ArgumentNullException($"Customized ASIN ({order.ASIN}) mug positioning data cannot be null.");
                        }
                        ICsvRecord? currentRowPositioningData = CsvRecords.FirstOrDefault(item => item.RowNumber == rowIndex) ?? throw new ArgumentNullException("Mug row number cannot be empty or null");

                        CoreldrawDesignerService designerService = workingMode == WorkingModeEnum.Amazon ?
                            new QuickDesigner2023.Module.Services.Amazon.MugDesignerService(corelApp) : new QuickDesigner2023.Module.Services.Temu.MugDesignerService(corelApp);
                        positionY = designerService.ImportImageDataIntoCDR(masterLayer, destinationFolderPath, currentRowPositioningData, zipFilePath, positionY);

                        break;
                    case QuickDesigner2023.Module.Enums.AsinType.PhoneCase:
                        if (CsvRecords == null)
                        {
                            throw new ArgumentNullException($"Customized ASIN ({order.ASIN}) phone case positioning data cannot be null.");
                        }
                        currentRowPositioningData = CsvRecords.FirstOrDefault(item => item.RowNumber == rowIndex) ?? throw new ArgumentNullException("Phone case row number cannot be empty or null");

                        designerService = workingMode == WorkingModeEnum.Amazon ? new QuickDesigner2023.Module.Services.Amazon.PhoneCaseDesignerService(corelApp) :
                            new QuickDesigner2023.Module.Services.Temu.PhoneCaseDesignerService(corelApp);
                        positionY = designerService.ImportImageDataIntoCDR(masterLayer, destinationFolderPath, currentRowPositioningData, zipFilePath, positionY);
                        break;
                    case QuickDesigner2023.Module.Enums.AsinType.Keychain:
                        if (CsvRecords == null)
                        {
                            throw new ArgumentNullException($"Customized ASIN ({order.ASIN}) keychain positioning data cannot be null.");
                        }
                        // if keychain sign icons are not already loaded, load them and 
                        // set the boolean flag as true
                        if (!isKeychainSignIconsLoaded)
                        {
                            svgService?.ImportSvgIcons(document, workingMode);
                            isKeychainSignIconsLoaded = true;
                        }
                        currentRowPositioningData = CsvRecords.FirstOrDefault(item => item.RowNumber == rowIndex) ?? throw new ArgumentNullException("Keychain row number cannot be empty or null");

                        designerService = workingMode == WorkingModeEnum.Amazon ?
                             new QuickDesigner2023.Module.Services.Amazon.KeychainDesignerService(corelApp) : new QuickDesigner2023.Module.Services.Temu.KeychainDesignerService(corelApp);
                        positionY = designerService.ImportImageDataIntoCDR(masterLayer, destinationFolderPath, currentRowPositioningData, zipFilePath, positionY);
                        break;
                    case QuickDesigner2023.Module.Enums.AsinType.None:
                        if (CsvRecords == null)
                        {
                            throw new ArgumentNullException($"Non Customized ASIN ({order.ASIN}) positioning data cannot be null.");
                        }

                        currentRowPositioningData = CsvRecords.FirstOrDefault(item => item.RowNumber == rowIndex) ?? throw new ArgumentNullException("Non customized row number cannot be empty or null");
                        designerService = workingMode == WorkingModeEnum.Amazon ?
                            new QuickDesigner2023.Module.Services.Amazon.DefaultDesignerService(corelApp, zipCount) : new QuickDesigner2023.Module.Services.Temu.DefaultDesignerService(corelApp, zipCount);

                        positionY = designerService.ImportImageDataIntoCDR(masterLayer, destinationFolderPath, currentRowPositioningData, zipFilePath, positionY);

                        // HandleFilesWithoutCustomAsins(progress, destinationFolderPath, document, ref positionX, ref positionY, zipCount, zipFilePath, masterLayer, rowIndex == 1);
                        break;
                    default:
                        throw new NotImplementedException($"'{order?.AsinType}' is not implemented");
                }

                rowIndex++;
                processedZipFiles.Add(zipFilePath);
                ReportProgress(progress, ((100 - 10) / zipFiles.Length) * zipCount);
                zipCount++;

            }
            if (document != null)
            {
                // if keychain sign icons are loaded, delete them
                if (isKeychainSignIconsLoaded)
                    svgService?.DeleteSvgLayer(document);
                CreateGuideLines(document.ActiveLayer);
                StartTask(new BgTask(corelApp, document, destinationFolderPath));
                MoveZipFiles(processedZipFiles);
            }
            corelApp.Refresh();
            ReportProgress(progress, 100);
        }

        public void QuitCorelDraw()
        {
            try
            {
                // Optional: Close all open documents before quitting
                foreach (Document doc in corelApp.Documents)
                {
                    doc.Close();
                }

                // Quit CorelDRAW
                corelApp.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error quitting CorelDRAW: " + ex.Message);
            }
        }
       
        private void MoveZipFiles(List<string> zipFiles)
        {
            foreach (string zipFile in zipFiles)
            {
                try
                {
                    File.Move(zipFile, processedZipFilesPath + Path.GetFileName(zipFile));

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error moving {zipFile}: {ex.Message}");
                }
            }
            zipFiles.RemoveAll(s => true);
        }
        private void CreateGuideLines(Layer layer)
        {
            //Horizontal Guides
            double y = PageHeight - MarginTop;
            layer.CreateGuide(0, y, PageWidth, y);
            for (int i = 0; i < 7; i++)
            {
                y -= ImageMaxHeight / 2;
                layer.CreateGuide(0, y, PageWidth, y);
            }
            //Vertical Guides
            double x = MarginLeft;
            for (int i = 0; i < 6; i++)
            {
                layer.CreateGuide(x, 0, x, PageHeight);
                x += ImageMaxWidth;
                layer.CreateGuide(x, 0, x, PageHeight);
                x += MarginRight;
            }
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
                SortZipFilesBtn.IsEnabled =
                CDRFilesLocationTxt.IsEnabled =
                CDRBrowse.IsEnabled =
                GenerateBtn.IsEnabled =
                GenerateTemuBtn.IsEnabled =
                moveTemuZipFilesBtn.IsEnabled =
                false;
            ErrorConsoleRichTxt.Document.Blocks.Clear();
            counter = 0;
        }

        private void EnableEveryThing()
        {
            ZipsFolderPathTxt.IsEnabled =
                ImagesBrowse.IsEnabled =
                SortZipFilesBtn.IsEnabled =
                CDRFilesLocationTxt.IsEnabled =
                CDRBrowse.IsEnabled =
                GenerateBtn.IsEnabled =
                GenerateTemuBtn.IsEnabled =
                moveTemuZipFilesBtn.IsEnabled =
                true;
            Progress_Bar.Value = 0;
        }
        #endregion


        private string MoveZipFiles(List<string> zipFiles, string zipFilesLocation, string folderName)
        {
            string targetDir = string.Empty;
            int maxPerFolder = 30;
            var sortedFiles = zipFiles
                    .OrderBy(File.GetLastWriteTime)
                    .ToList();

            int totalFiles = sortedFiles.Count;

            // Split into batches of up to 30 files each
            var batches = sortedFiles
                .Select((file, index) => new { file, index })
                .GroupBy(x => totalFiles > maxPerFolder ? x.index / maxPerFolder : 0);

            foreach (var batch in batches)
            {
                // Determine subfolder name (A, B, C...) only if needed
                string subFolderName = totalFiles > maxPerFolder
                    ? ((char)('A' + batch.Key)).ToString()
                    : string.Empty;

                // Build target path
                targetDir = Path.Combine(zipFilesLocation, folderName, subFolderName);
                Directory.CreateDirectory(targetDir);

                foreach (var item in batch)
                {
                    var zip = item.file;

                    try
                    {
                        string destPath = Path.Combine(targetDir, Path.GetFileName(zip));
                        File.Copy(zip, destPath, overwrite: true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error moving {zip}: {ex.Message}");
                    }
                }
            }

            return targetDir;
        }

        private void MoveZipFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            string zipFilesLocation = ZipsFolderPathTxt.Text;
            if (string.IsNullOrWhiteSpace(zipFilesLocation) || !Path.Exists(zipFilesLocation))
            {
                MessageBox.Show("Please select zip files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string zipDestinationLocation = CDRFilesLocationTxt.Text;
            if (string.IsNullOrWhiteSpace(zipDestinationLocation))
            {
                MessageBox.Show("Please select output files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                CDRBrowse_Click(sender, e);
                return;
            }

            bool flowControl = MoveZipFiles(zipFilesLocation, zipDestinationLocation, WorkingModeEnum.Amazon);
            if (!flowControl)
            {
                return;
            }
        }

        private bool MoveZipFiles(string zipFilesLocation, string zipDestinationLocation, WorkingModeEnum workingMode)
        {

            IZipOrderExtractorService zipOrderExtractor;
            Dictionary<string, List<string>> asinGroups;
            switch (workingMode)
            {
                case WorkingModeEnum.Amazon:
                    zipOrderExtractor = new AmazonAsinExtractorService();
                    asinGroups = zipOrderExtractor.GetZipFilesGrouped(zipFilesLocation);
                    break;
                case WorkingModeEnum.Temu:
                    zipOrderExtractor = new TemuSkuExtractorService();
                    asinGroups = zipOrderExtractor.GetZipFilesGrouped(zipFilesLocation);
                    break;
                default:
                    throw new NotImplementedException($"The working mode '{workingMode}' is not supported.");
            }

            if (asinGroups == null || asinGroups?.Count == 0)
            {
                MessageBox.Show("No zip file is found.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string workingModeSpecificFilder = CoreHelper.GetWorkingModeString(workingMode);

            string asinFolderCsvFilePath = Path.Combine(CoreHelper.GetAppAssemblyPath(), "Resources", workingModeSpecificFilder, "QuickDesigner2023", AsinsExtractor.AsinFolderCsvFileName);
            List<AsinFolderRecord> asinsFolders = AsinsExtractor.ReadAsinFolders(asinFolderCsvFilePath);


            foreach (var asinFilesKeyValuePair in asinGroups)
            {
                string asin = asinFilesKeyValuePair.Key;
                var zipFiles = asinFilesKeyValuePair.Value;

                if (zipFiles == null || zipFiles.Count == 0)
                    continue;

                var order = asinsFolders.FirstOrDefault(item => item.Asins.Contains(asin));
                if (order == null)
                    continue;

                var folderName = order.FolderName;
                var lastTargetDir = MoveZipFiles(zipFiles, zipDestinationLocation, folderName);
                if (!string.IsNullOrEmpty(lastTargetDir) && Directory.Exists(lastTargetDir))
                {
                    AppDefaultDirectories.SetDefaultDirectory(order.AppTitle, lastTargetDir);
                }

            }

            return true;
        }

        private void MoveTemuZipFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            string zipFilesLocation = ZipsFolderPathTxt.Text;
            if (string.IsNullOrWhiteSpace(zipFilesLocation) || !Path.Exists(zipFilesLocation))
            {
                MessageBox.Show("Please select zip files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string zipDestinationLocation = CDRFilesLocationTxt.Text;
            if (string.IsNullOrWhiteSpace(zipDestinationLocation))
            {
                MessageBox.Show("Please select output files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                CDRBrowse_Click(sender, e);
                return;
            }

            bool flowControl = MoveZipFiles(zipFilesLocation, zipDestinationLocation, WorkingModeEnum.Temu);
            if (!flowControl)
            {
                return;
            }
        }
    }
}
