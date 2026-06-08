using CsvHelper;
using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Utilities;
using KeychainQuickDesigner.Module.Interfaces;
using KeychainQuickDesigner.Module.Models;
using System.Globalization;
using System.IO;

namespace KeychainQuickDesigner.Module.Services
{
    public class PositioningDataService : IPositioningDataService
    {
        public const string StepOnePostionsCsvFileName = "step_1_positions.csv";
        public const string ImageLocationsForCdrFile = "image_locations_for_cdr_file.csv";
        public const string TextBoxLocationsForSvfFile = "text_box_locations_for_svg_file.csv";


        private readonly string _resourcesPath;

        public PositioningDataService(WorkingModeEnum workingMode)
        {
            string? assemblyDir = Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new InvalidOperationException("Unable to determine the assembly directory.");
            _resourcesPath = Path.Combine(assemblyDir, "Resources", CoreHelper.GetWorkingModeString(workingMode), "CsvFiles");
        }

        public List<OrderDataCSV> LoadOrderDataLocationForCdrFile()
        {
            return LoadFromCsv<OrderDataCSV>(StepOnePostionsCsvFileName);
        }

        public List<ImageLocation> LoadImageLocationForCdrFile()
        {
            return LoadFromCsv<ImageLocation>(ImageLocationsForCdrFile);
        }

        public List<TextBoxLocation> LoadTextBoxLocationForSvgFile()
        {
            return LoadFromCsv<TextBoxLocation>(TextBoxLocationsForSvfFile);
        }

        public List<T> LoadFromCsv<T>(string fileName)
        {
            string csvFilePath = Path.Combine(_resourcesPath, fileName);

            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
#pragma warning disable IDE0305 // Simplify collection initialization
            return csv.GetRecords<T>().ToList();
#pragma warning restore IDE0305 // Simplify collection initialization
        }
    }

}
