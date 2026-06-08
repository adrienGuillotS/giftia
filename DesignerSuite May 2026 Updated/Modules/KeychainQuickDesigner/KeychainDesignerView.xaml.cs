using Corel.Interop.VGCore;
using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Static;
using DesignerSuite.Core.Utilities;
using DesignerSuite.Core.Utilities.Corel;
using KeychainQuickDesigner.Module.Constants;
using KeychainQuickDesigner.Module.Enums;
using KeychainQuickDesigner.Module.Helpers;
using KeychainQuickDesigner.Module.Models;
using KeychainQuickDesigner.Module.Services;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace KeychainQuickDesigner.Module
{
    public partial class KeychainDesignerView : System.Windows.Controls.Page
    {
        public KeychainDesignerView() => InitializeComponent();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            ZipsFolderPathTxt.Text = @"G:\Testing Workspace\Umar\DesignerSuite\Keychain Zip files for Nabeel";
            CDRFilesLocationTxt.Text = @"G:\Testing Workspace\Umar\DesignerSuite\Keychain Zip files for Nabeel\Destination";
#endif
            if (!string.IsNullOrEmpty(AppDefaultDirectories.KeychainQuickDesigner))
            {
                string defaultPath = AppDefaultDirectories.KeychainQuickDesigner;
                ZipsFolderPathTxt.Text = defaultPath;
                CDRFilesLocationTxt.Text = defaultPath;
            }
        }

        #region Browse Buttons
        private void ImagesBrowse_Click(object sender, RoutedEventArgs e) =>
            BrowseFolder(ZipsFolderPathTxt);

        private void CDRBrowse_Click(object sender, RoutedEventArgs e) =>
            BrowseFolder(CDRFilesLocationTxt);

        private static void BrowseFolder(TextBox targetBox)
        {
            var dialog = new OpenFolderDialog();
            if (dialog?.ShowDialog() == true)
                targetBox.Text = dialog.FolderName;
        }
        #endregion

        #region Step Handlers
        private async void PerformAmazonStepOneBtn_Click(object sender, RoutedEventArgs e) =>
            await ExecuteStepOneAsync(WorkingModeEnum.Amazon);

        private async void PerformTemuStepOneButton_Click(object sender, RoutedEventArgs e) =>
            await ExecuteStepOneAsync(WorkingModeEnum.Temu);

        private async void PerformAmazonStepTwoButton_Click(object sender, RoutedEventArgs e) =>
            await ExecuteStepTwoAsync(WorkingModeEnum.Amazon);

        private async void PerformTemuStepTwoButton_Click(object sender, RoutedEventArgs e) =>
            await ExecuteStepTwoAsync(WorkingModeEnum.Temu);
        #endregion

        #region Quick Actions
        private async void CopyAmazonMainImagesBtn_Click(object sender, RoutedEventArgs e) =>
            await CopyMainImagesToSeparateFile(WorkingModeEnum.Amazon);

        private async void CopyTemuMainImagesBtn_Click(object sender, RoutedEventArgs e) =>
            await CopyMainImagesToSeparateFile(WorkingModeEnum.Temu);

        private async void CopyAmazonTextBoxShapesButton_Click(object sender, RoutedEventArgs e) =>
            await CopyTextBoxesToSeparateFile(WorkingModeEnum.Amazon);

        private async void CopyTemuTextBoxShapesButton_Click(object sender, RoutedEventArgs e) =>
            await CopyTextBoxesToSeparateFile(WorkingModeEnum.Temu);

        private async void EnlargeAmazonTextBoxShapesButton_Click(object sender, RoutedEventArgs e) =>
            await EnlargeTextBoxShapes(WorkingModeEnum.Amazon);

        private async void EnlargeTemuTextBoxShapesButton_Click(object sender, RoutedEventArgs e) =>
            await EnlargeTextBoxShapes(WorkingModeEnum.Temu);

        private async void ExportAmazonPdfButton_Click(object sender, RoutedEventArgs e) =>
            await ExportPdfFile(WorkingModeEnum.Amazon);

        private async void ExportTemuPdfButton_Click(object sender, RoutedEventArgs e) =>
            await ExportPdfFile(WorkingModeEnum.Temu);
        #endregion

        #region Core Logic
        private async Task ExecuteStepOneAsync(WorkingModeEnum mode)
        {
            if (!ValidatePaths()) return;

            string zipPath = ZipsFolderPathTxt.Text;
            string destPath = CDRFilesLocationTxt.Text;

            await RunWithUiLockAsync(async progress =>
            {
                var (corel, svg, pos, img, zip) = CreateServices(mode);
                var positioning = pos.LoadOrderDataLocationForCdrFile();

                await zip.ProcessZipsAsync(zipPath, destPath, positioning, progress, corel, img, svg);
                ShowMessage("Step one files are generated successfully!");
            });
        }

        private async Task ExecuteStepTwoAsync(WorkingModeEnum mode)
        {
            try
            {
                SetControlsEnabled(false);
                await CopyMainImagesToSeparateFile(mode);
                await CopyTextBoxesToSeparateFile(mode);
                await EnlargeTextBoxShapes(mode);
                await ExportPdfFile(mode);
                ShowMessage("Step 2 completed successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async Task CopyMainImagesToSeparateFile(WorkingModeEnum mode)
        {
            var pos = new PositioningDataService(mode);
            var locationData = pos.LoadImageLocationForCdrFile();
            await RunCopyShapesTask(locationData, ShapePrefixes.MainImage,
                locationData.Count, 210, 297, DocumentExportTypeEnum.Cdr, Corel.Interop.VGCore.cdrShapeType.cdrBitmapShape, mode);
        }

        private async Task CopyTextBoxesToSeparateFile(WorkingModeEnum mode)
        {
            var pos = new PositioningDataService(mode);
            var positioningData = pos.LoadTextBoxLocationForSvgFile();
            await RunCopyShapesTask(positioningData, ShapePrefixes.Box,
                positioningData.Count, 220, 220, DocumentExportTypeEnum.Svg, Corel.Interop.VGCore.cdrShapeType.cdrGroupShape, mode);
        }

        private async Task EnlargeTextBoxShapes(WorkingModeEnum mode)
        {
            if (!ValidateCdrPath()) return;

            string dest = CDRFilesLocationTxt.Text;
            await RunWithUiLockAsync(async progress =>
            {
                var (corel, _, _, img, _) = CreateServices(mode);
                await img.EnlargeTextBoxShapes(dest, ShapePrefixes.Box, 65, progress);
            });
        }

        private async Task RunCopyShapesTask<T>(
            List<T> shapeLocations, string shapePrefix, int maxShapes,
            double width, double height, DocumentExportTypeEnum exportType,
            cdrShapeType shapeType, WorkingModeEnum mode)
            where T : IShapeLocation
        {
            if (!ValidateCdrPath()) return;

            string dest = CDRFilesLocationTxt.Text;
            await RunWithUiLockAsync(async progress =>
            {
                var (_, _, _, img, _) = CreateServices(mode);
                string postfix = $"{DateTime.Now:MMddyyyy} Keychain";

                await img.CopyShapesToSeparateFiles(dest, shapeLocations, shapePrefix,
                    maxShapes, width, height, exportType, progress, postfix, shapeType);
            });
        }

        private async Task ExportPdfFile(WorkingModeEnum mode)
        {
            if (!ValidateCdrPath()) return;

            string dest = CDRFilesLocationTxt.Text;
            string tempPath = System.IO.Path.Combine(dest, "Temp", "Temp PDFs");

            await RunWithUiLockAsync(async progress =>
            {
                var (corel, _, _, _, _) = CreateServices(mode);
                var files = await Task.Run(() => corel.ExportDocumentsToTemporaryPdfs(dest, tempPath, progress));

                if (files.Count == 0)
                {
                    ShowMessage("No CDR files found to export.");
                    return;
                }

                PdfOperations.MergePdfFiles(files, dest, progress);
                ShowMessage("Files exported to PDF successfully!");
            });
        }
        #endregion

        #region Utilities
        private bool ValidatePaths() =>
            ValidatePath(CDRFilesLocationTxt.Text, "CDR files") &&
            ValidatePath(ZipsFolderPathTxt.Text, "zip files");

        private bool ValidateCdrPath() =>
            ValidatePath(CDRFilesLocationTxt.Text, "CDR files");

        private bool ValidatePath(string path, string label)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                ShowMessage($"Please select {label} location");
                return false;
            }
            return true;
        }

        private async Task RunWithUiLockAsync(Func<IProgress<int>, Task> action)
        {
            try
            {
                SetControlsEnabled(false);
                var progress = new Progress<int>(p => Progress_Bar.Value = p);
                await action(progress);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private static (CorelDrawService, SvgImportService, PositioningDataService,
                        ImagePlacementService, ZipProcessingService)
            CreateServices(WorkingModeEnum mode)
        {
            var app = GetCorelApplicationOrThrow();
            var corel = new CorelDrawService(app);
            var svg = new SvgImportService(app);
            var pos = new PositioningDataService(mode);
            var img = new ImagePlacementService(corel, svg);
            var zip = new ZipProcessingService(new XmlFileReaderService()) { WorkingMode = mode };
            return (corel, svg, pos, img, zip);
        }

        private static Corel.Interop.VGCore.Application GetCorelApplicationOrThrow()
        {
            var connector = new CorelConnector();
            if (!connector.ConnectToCorel() || connector.CorelApp is null)
                throw new InvalidOperationException("CorelDRAW is not started or not compatible with this software.");
            return connector.CorelApp;
        }

        private void SetControlsEnabled(bool enabled)
        {
            bool state = enabled;
            ZipsFolderPathTxt.IsEnabled =
            CDRFilesLocationTxt.IsEnabled =
            ImagesBrowse.IsEnabled =
            CDRBrowse.IsEnabled =
            PerformAmazonStepOneBtn.IsEnabled =
            PerformTemuStepOneButton.IsEnabled =
            PerformAmazonStepTwoButton.IsEnabled =
            PerformTemuStepTwoButton.IsEnabled =
            CopyAmazonMainImagesBtn.IsEnabled =
            CopyTemuMainImagesBtn.IsEnabled =
            CopyAmazonTextBoxShapesButton.IsEnabled =
            CopyTemuTextBoxShapesButton.IsEnabled =
            EnlargeAmazonTextBoxShapesButton.IsEnabled =
            EnlargeTemuTextBoxShapesButton.IsEnabled =
            ExportAmazonPdfButton.IsEnabled =
            ExportTemuPdfButton.IsEnabled = state;

            if (state) Progress_Bar.Value = 0;
        }

        private static void ShowMessage(string msg) =>
            MessageBox.Show(msg, "KeyChain Designer", MessageBoxButton.OK, MessageBoxImage.Information);
        #endregion
    }
}
