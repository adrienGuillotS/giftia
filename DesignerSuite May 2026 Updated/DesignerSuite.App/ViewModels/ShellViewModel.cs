using DesignerSuite.App.Mvvm;
using MahApps.Metro.IconPacks;
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
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ColumnsSolid },
                Label = "Dashboard",
                NavigationType = typeof(DashboardPage),
                NavigationDestination = new Uri(
                    "pack://application:,,,/DesignerSuite.App;component/Pages/DashboardPage.xaml",
                    UriKind.Absolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.MugHotSolid },
                Label = "Quick Designer 2023",
                NavigationType = typeof(DashboardPage),
                NavigationDestination = new Uri(
                "pack://application:,,,/QuickDesigner2023.Module;component/MainPage.xaml",
                UriKind.Absolute)
            });
            Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.MugSaucerSolid },
                Label = "Quick Designer 2025",
                NavigationType = typeof(DashboardPage),
                NavigationDestination = new Uri(
                "pack://application:,,,/QuickDesigner2025.Module;component/MainPage.xaml",
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
