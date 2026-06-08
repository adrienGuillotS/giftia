
using Corel.Interop.VGCore;
using KeychainQuickDesigner.Module.Enums;
using KeychainQuickDesigner.Module.Models;
using DesignerSuite.Core.Models;

namespace KeychainQuickDesigner.Module
{
    public interface IImagePlacementService
    {
        void PlaceImages(Layer layer,
            IOrderDataItem dataItem,
            string zipPath,
            string destinationPath,
            OrderDataCSV position, string orderId);

        Task CopyShapesToSeparateFiles<T>(
               string destinationFolder,
               List<T> imageLocations,
               string shapeNamePrefix,
               int maxShapesPerDoc,
               double pageWidth,
               double pageHeight,
               DocumentExportTypeEnum exportFormat,
               IProgress<int> progress,
               string fileNamePostfix, cdrShapeType cdrShapeTypeToSearch) where T : IShapeLocation;

        Task EnlargeTextBoxShapes(
               string destinationFolder,
               string shapeNamePrefix,
               double newHeight,
               IProgress<int> progress);
    }
}