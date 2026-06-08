using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using KeychainQuickDesigner.Module.Models;
using KeychainQuickDesigner.Module.Services;

namespace KeychainQuickDesigner.Module.Interfaces
{
    public interface IZipProcessingService
    {
        WorkingModeEnum WorkingMode { get; set; }
        Task ProcessZipsAsync(
            string zipFolderPath,
            string destinationFolderPath,
            IEnumerable<OrderDataCSV> positioningData,
            IProgress<int> progress,
            ICorelDrawService corelService,
            IImagePlacementService imageService,
            ISvgImportService svgService);
    }
}
