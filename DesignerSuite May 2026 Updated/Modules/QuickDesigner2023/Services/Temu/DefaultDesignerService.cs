using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Temu;
using QuickDesigner2023.Module.Interfaces;
using QuickDesigner2023.Module.Models;
using Svg;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using Corel.Interop.VGCore;
using Layer = Corel.Interop.VGCore.Layer;

namespace QuickDesigner2023.Module.Services.Temu
{
    public class DefaultDesignerService : CoreldrawDesignerService
    {
        #region Private Fields
        private const double PageWidth = 210;
        private const double PageHeight = 297;
        private const double ImageMaxWidth = 80;
        private const double ImageMaxHeight = 95;
        private const double MaxTextWidth = 70;

        private const double MarginLeft = 45;
        private const double MarginTop = (PageHeight - ImageMaxHeight * 3) / 2;
        private const double MarginRight = 40;
        private const double MarginBottom = MarginTop;

        private string extractPath = string.Empty;
        private string processedZipFilesPath = string.Empty;


        private Dictionary<string, Dictionary<string, List<string>>> inputTexts;
        private Dictionary<string, string> svgToSnapMapping;
        private readonly int zipCount;

        #endregion


        public DefaultDesignerService(Application corelApp, int zipCount) : base(corelApp)
        {
            inputTexts = new Dictionary<string, Dictionary<string, List<string>>>();
            svgToSnapMapping = new Dictionary<string, string>();
            this.zipCount = zipCount;
        }

        public override double ImportImageDataIntoCDR(Layer masterLayer, string destinationFolderPath, ICsvRecord csvPositioningData, string zipFilePath, double positionY)
        {
            Debug.WriteLine("Default SKU");
            extractPath = Path.Combine(destinationFolderPath, "temp");
            double newPositionY = positionY;

            if (csvPositioningData is DefaultNonCustomizedRecord positioningData)
            {

                IImageDataExtractorService imageDataExtractorService = new TemuMugImageDataExtractorService();
                var data = (TemuMugDataItem)imageDataExtractorService.ExtractImages(zipFilePath, extractPath);


                ArgumentNullException.ThrowIfNull(data);

                var positionX = MarginLeft;
                double actualImageX = positionX;
                double actualImageY = positioningData.PositionY;

                string orderId = Path.GetFileNameWithoutExtension(zipFilePath).ExtractOrderNumber();

                // insert order id from zip file name into the row
                masterLayer.CreateArtisticText(-150, positioningData.PositionY + 36, orderId
                    , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                    , "Arial", 30);

                string imagePath;
                if (!string.IsNullOrWhiteSpace(data?.ImageName))
                {
                    imagePath = Path.Combine(extractPath, data.ImageName);
                    if (File.Exists(imagePath))
                    {
                        var image = ImportImage(masterLayer, imagePath);

                        if (image != null)
                        {

                            image = ResizeShapeWithDesiredDimensions(masterLayer, image, ImageMaxWidth, ImageMaxHeight);
                            image.SetPositionEx(cdrReferencePoint.cdrCenter, positionX, positioningData.PositionY);
                            positionX += ImageMaxWidth + MarginRight;
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(data?.SvgImageName))
                {
                    imagePath = Path.Combine(extractPath, data.SvgImageName);
                    if (File.Exists(imagePath))
                    {
                        var image = ImportImage(masterLayer, imagePath);

                        if (image != null)
                        {

                            image = ResizeShapeWithDesiredDimensions(masterLayer, image, ImageMaxWidth, ImageMaxHeight);
                            image.SetPositionEx(cdrReferencePoint.cdrCenter, positionX, positioningData.PositionY);

                            positionX += ImageMaxWidth + MarginRight;
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(data?.SnapshotImageName))
                {
                    imagePath = Path.Combine(extractPath, data.SnapshotImageName);
                    if (File.Exists(imagePath))
                    {
                        var image = ImportImage(masterLayer, imagePath);

                        if (image != null)
                        {

                            image = ResizeShapeWithDesiredDimensions(masterLayer, image, ImageMaxWidth, ImageMaxHeight);
                            image.SetPositionEx(cdrReferencePoint.cdrCenter, positionX, positioningData.PositionY);

                            positionX += ImageMaxWidth + MarginRight;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(data.InputValue))
                {
                    string font = !string.IsNullOrWhiteSpace(data.FontFamily) ? data.FontFamily : "Times New Roman";


                    var text = masterLayer.CreateArtisticText(actualImageX, actualImageY,
                        data.InputValue
                        , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                        , font, 30, Alignment: cdrAlignment.cdrCenterAlignment);

                    string? color = !string.IsNullOrWhiteSpace(data.ColorValue) ? data.ColorValue : null;
                    if (color != null && color != "")
                    {
                        //.FromArgb(Convert.ToInt32(color.Replace("#", ""), 16))
                        Color c = masterLayer.Color;
                        c.HexValue = color;
                        text.Fill.ApplyUniformFill(c);
                    }
                }
                if (!string.IsNullOrWhiteSpace(data.RightSideInputValue))
                {
                    string font = !string.IsNullOrWhiteSpace(data.FontFamily) ? data.FontFamily : "Times New Roman";


                    var text = masterLayer.CreateArtisticText(actualImageX + ImageMaxWidth + MarginRight, actualImageY,
                        data.RightSideInputValue
                        , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                        , font, 30, Alignment: cdrAlignment.cdrCenterAlignment);

                    string? color = !string.IsNullOrWhiteSpace(data.RightSideColorValue) ? data.RightSideColorValue : null;
                    if (color != null && color != "")
                    {
                        //.FromArgb(Convert.ToInt32(color.Replace("#", ""), 16))
                        Color c = masterLayer.Color;
                        c.HexValue = color;
                        text.Fill.ApplyUniformFill(c);
                    }
                }
            }
            return newPositionY;
        }

        public Shape ResizeShapeWithDesiredDimensions(Layer masterLayer, Shape shape, double desiredWidth, double desiredHeight)
        {
            // Get current dimensions
            double currentWidth = shape.SizeWidth;
            double currentHeight = shape.SizeHeight;

            // Step 1: Scale Width to Fit Desired Width, Maintaining Aspect Ratio
            double scalingFactorWidth = desiredWidth / currentWidth;
            double newWidth = desiredWidth;
            double newHeight = currentHeight * scalingFactorWidth;

            // Step 3: Resize the Shape to Fit the Desired Dimensions
            shape.SetSize(newWidth, newHeight);
            return shape;
        }

        private ImageDataXml GetImagesFromZip(string zipFilePath, string destinationFolderPath)
        {
            var imageDataXml = new ImageDataXml();
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                            || entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                            )
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath, true);
                        }
                    }
                    else if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath, true);

                            imageDataXml = ImageDataXml.ReadDataFromXmlFile(destinationPath);
                        }
                    }
                }
            }

            return imageDataXml;
        }

    }
}
