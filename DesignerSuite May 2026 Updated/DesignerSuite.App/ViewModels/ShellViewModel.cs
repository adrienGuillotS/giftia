using DesignerSuite.App.Mvvm;
using KeychainQuickDesigner.Module;
using MahApps.Metro.IconPacks;
using PSQuickDesigner.Views;
using QuickDesinger2023;
using System.Collections.ObjectModel;
using DesignerSuite.App.Pages;

namespace DesignerSuite.App.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        public ObservableCollection<MenuItem> Menu { get; } = new();

        public ObservableCollection<MenuItem> OptionsMenu { get; } = new();

        public ShellViewModel()
        {
            // Build the menus
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.MugHotSolid },
                Label = "Quick Designer 2023",
                NavigationType = typeof(MainPage),
                NavigationDestination = new Uri(
                "pack://application:,,,/QuickDesigner2023.Module;component/MainPage.xaml",
                UriKind.Absolute)
            });
            Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.MugSaucerSolid },
                Label = "Quick Designer 2025",
                NavigationType = typeof(QuickDesigner2025.MainPage),
                NavigationDestination = new Uri(
                "pack://application:,,,/QuickDesigner2025.Module;component/MainPage.xaml",
                UriKind.Absolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.KeySolid },
                Label = "Keychain Quick Designer",
                NavigationType = typeof(KeychainDesignerView),
                NavigationDestination = new Uri(
                    "pack://application:,,,/KeychainDesignerView.Module;component/KeychainDesignerView.xaml",
                    UriKind.Absolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FontAwesomeBrands },
                Label = "PS Quick Designer",
                NavigationType = typeof(PhotoshopQuickDesignerView),
                NavigationDestination = new Uri(
                    "pack://application:,,,/PhotoshopQuickDesigner.Module;component/Views/PhotoshopQuickDesignerView.xaml",
                    UriKind.Absolute)
            });

            //
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.CakeCandlesSolid },
                Label = "Birth Certificate Designer",
                NavigationType = typeof(BirthdayCertificateDesigner2023.Views.MainPage),
                NavigationDestination = new Uri(
                    "pack://application:,,,/BirthdayCertificateDesigner2023;component/Views/MainPage.xaml",
                    UriKind.Absolute)
            });

            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FilePdfSolid },
                Label = "PDF Label Sorter",
                NavigationType = typeof(PdfLabelSorterPage),
                NavigationDestination = new Uri(
                    "pack://application:,,,/DesignerSuite.App;component/Pages/PdfLabelSorterPage.xaml",
                    UriKind.Absolute)
            });
        }
    }
}
