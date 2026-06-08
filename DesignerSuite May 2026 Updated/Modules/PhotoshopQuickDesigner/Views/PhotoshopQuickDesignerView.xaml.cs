using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Static;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Photoshop;
using PSQuickDesigner.Enums;
using PSQuickDesigner.Models;
using PSQuickDesigner.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using WindowsAPICodePack.Dialogs;
using Application = Photoshop.Application;
using MessageBox = System.Windows.MessageBox;
using Orientation = PSQuickDesigner.Enums.Orientation;

namespace PSQuickDesigner.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PhotoshopQuickDesignerView : Page
    {
        public PhotoshopQuickDesignerView()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            ZipFolderPathCB.Text = @"G:\Testing Workspace\Umar\Temu\Temu Pillow\Bug";
#endif
            if (!string.IsNullOrEmpty(AppDefaultDirectories.PhotoshopQuickDesigner))
            {
                ZipFolderPathCB.Text = AppDefaultDirectories.PhotoshopQuickDesigner;
            }

            var mainViewModel = new MainViewModel();

            // Assuming your view model is set as the DataContext
            DataContext = mainViewModel;

            mainViewModel.Presets = new System.Collections.ObjectModel.ObservableCollection<Preset>();

            var savedPresets = PresetDetailsHelper.GetSavedPresets();
            if (savedPresets != null)
            {
                foreach (var item in savedPresets)
                {
                    mainViewModel.Presets.Add(item);
                }

                mainViewModel.SelectedPreset = mainViewModel.Presets?.FirstOrDefault();
            }
            else
            {
                mainViewModel.SelectedPreset = new Preset();
            }
            PortraitRB.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
                ZipFolderPathCB.Text = selectedFolderPath;
            }
        }


        public void EnableAllControls(bool isEnabled)
        {
            AutomateTemuBtn.IsEnabled = AutomateAmazonBtn.IsEnabled = BrowseBtn.IsEnabled = isEnabled;
            presetExpander.IsEnabled = isEnabled;
            SmallImageWidthTB.IsEnabled = SmallImageHeightTB.IsEnabled = isEnabled;
        }

        private void SavedPresetsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Preset preset)
            {
                //SelectComboBoxItem(preset.Fill, FillCB);

                ArtboardChB.IsChecked = preset.Artboards?.Count > 0;

            }
        }


        private void CreateNewPresetBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateNewPresetView createNewPreset = new CreateNewPresetView();
            if (createNewPreset.ShowDialog() == true)
            {
                var createNewPresetVM = (CreateNewPresetViewModel)createNewPreset.DataContext;
                if (createNewPresetVM.SelectedPreset != null)
                {
                    var mainViewModel = ((MainViewModel)this.DataContext);
                    mainViewModel.Presets.Add(createNewPresetVM.SelectedPreset);
                    mainViewModel.SelectedPreset = mainViewModel.Presets.FirstOrDefault(p => p.Name.Equals(createNewPresetVM.SelectedPreset.Name));
                    PresetDetailsHelper.SavePresets(new Root() { Presets = mainViewModel.Presets.ToList() });
                }
                //MessageBox.Show("New Preset has been created!", "PS Quick Designer",
                //    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void AutomateTemuBtn_Click(object sender, RoutedEventArgs e)
        {
            await ProcessAutomationAsync(
                sender,
                workingMode: DesignerSuite.Core.Enums.WorkingModeEnum.Temu
            );
        }

        private async void AutomateAmazonBtn_Click(object sender, RoutedEventArgs e)
        {
            await ProcessAutomationAsync(
                sender,
                workingMode: DesignerSuite.Core.Enums.WorkingModeEnum.Amazon
            );
        }

        private async Task ProcessAutomationAsync(object sender, WorkingModeEnum workingMode)
        {
            string zipFolderPath = ZipFolderPathCB.Text;

            // Validate folder
            if (!ValidateZipFolderPath(zipFolderPath, sender))
                return;

            var button = sender as Button;
            string? originalContent = button?.Content?.ToString();

            try
            {
                EnableAllControls(false);
                if (button != null) button.Content = "Processing...";

                var viewModel = (MainViewModel)this.DataContext;

                // If artboard is enabled but no preset selected
                if (viewModel.SelectedPreset == null && ArtboardChB.IsChecked == true)
                {
                    var artboards = new List<Artboard>
            {
                new Artboard { Bottom = 1890, Right = 1417, Left = 0, Top = 0 }
            };
                }

                // Run the processing on background thread
                await Task.Run(() =>
                {
                    if (workingMode == WorkingModeEnum.Temu)
                        viewModel.StartProcessing(zipFolderPath, WorkingModeEnum.Temu);
                    else
                        viewModel.StartProcessing(zipFolderPath);
                });

                MessageBox.Show(
                    "Automation has been completed successfully.",
                    this.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableAllControls(true);
                if (button != null) button.Content = originalContent;
            }
        }

        private bool ValidateZipFolderPath(string zipFolderPath, object sender)
        {
            if (string.IsNullOrWhiteSpace(zipFolderPath))
            {
                MessageBox.Show(
                    "Please select the folder containing source Zip files.",
                    "Browse Zip Files",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Button_Click(sender, null);
                return false;
            }

            if (!Directory.Exists(zipFolderPath))
            {
                MessageBox.Show(
                    "Source Zip files folder could not be found.",
                    "Browse Zip Files",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Button_Click(sender, null);
                return false;
            }

            return true;
        }

    }
}
