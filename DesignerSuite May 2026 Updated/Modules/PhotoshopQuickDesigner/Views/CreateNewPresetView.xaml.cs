using MahApps.Metro.Controls;
using PSQuickDesigner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PSQuickDesigner.Views
{
    /// <summary>
    /// Interaction logic for CreateNewPresetView.xaml
    /// </summary>
    public partial class CreateNewPresetView : MetroWindow
    {
        public CreateNewPresetView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var mainViewModel = new CreateNewPresetViewModel();

            // Assuming your view model is set as the DataContext
            DataContext = mainViewModel;

            NewPortraitRB.IsChecked = true;
        }

        private void SaveNewPresetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewPresetsTB.Text))
            {
                MessageBox.Show("Preset name is required", "PS Quick Designer", MessageBoxButton.OK, MessageBoxImage.Information);
                NewPresetsTB.Focus();
            }
            else
                this.DialogResult = true;
        }
    }
}
