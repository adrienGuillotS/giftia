using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Static
{
    public static class AppDefaultDirectories
    {
        public static string PhotoshopQuickDesigner { get; set; } = string.Empty;
        public static string KeychainQuickDesigner { get; set; } = string.Empty;
        public static string BirthdayCertificateDesigner { get; set; } = string.Empty;
        public static string QuickDesigner2023 { get; set; } = string.Empty;
        public static string QuickDesigner2025 { get; set; } = string.Empty;

        public static void SetDefaultDirectory(string appTitle, string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(appTitle))
                throw new ArgumentException("App title cannot be null or empty.", nameof(appTitle));

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            switch (appTitle.ToLowerInvariant())
            {
                case AppTitles.BirthdayCertificateDesigner:
                    BirthdayCertificateDesigner = directoryPath;
                    break;

                case AppTitles.KeychainQuickDesigner:
                    KeychainQuickDesigner = directoryPath;
                    break;

                case AppTitles.PhotoshopQuickDesigner:
                    PhotoshopQuickDesigner = directoryPath;
                    break;

                case AppTitles.QuickDesigner2023:
                    QuickDesigner2023 = directoryPath;
                    break;

                case AppTitles.QuickDesigner2025:
                    QuickDesigner2025 = directoryPath;
                    break;

                default:
                    throw new NotImplementedException($"No default directory mapping found for '{appTitle}'.");
            }
        }
    }
}
