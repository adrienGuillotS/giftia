using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;

namespace BirthdayCertificateDesigner2023
{
    public enum DateDialogResult
    {
        Ignore,
        Continue,
        SaveAndContinue
    }

    /// <summary>
    /// Interaction logic for BirthDateErrorDialog.xaml
    /// </summary>
    public partial class BirthDateErrorDialog : MetroWindow
    {
        public DateDialogResult Result;
        public DateTime? DateOfBirth { get; private set; }
        private readonly string _invalidDateString;

        public BirthDateErrorDialog(string invalidDateString)
        {
            InitializeComponent();
            _invalidDateString = invalidDateString;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableButtons(false);
            InvalidDateTextBox.Text = _invalidDateString;
        }

        private void SaveAndContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = DateDialogResult.SaveAndContinue;
            DialogResult = true;
        }
        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = DateDialogResult.Continue;
            DialogResult = true;
        }

        private void IgnoreBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = DateDialogResult.Ignore;
            DialogResult = true;
        }
        private void DateControl_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableButtons(DateControl.SelectedDate != null);
            DateOfBirth = DateControl.SelectedDate;
        }
        void EnableButtons(bool enable)
        {
            ContinueBtn.IsEnabled = enable;
            SaveAndContinueBtn.IsEnabled = enable;
        }

    }
}
