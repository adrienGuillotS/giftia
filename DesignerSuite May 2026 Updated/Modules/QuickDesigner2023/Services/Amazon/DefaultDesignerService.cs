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

namespace QuickDesigner2023.Module.Services.Amazon
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
        private const double MarginTop = (PageHeight - (ImageMaxHeight * 3)) / 2;
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

        public override double ImportImageDataIntoCDR(
        Layer masterLayer,
        string destinationFolderPath,
        ICsvRecord csvPositioningData,
        string zipFilePath,
        double positionY)
        {
            Debug.WriteLine("Default Asin");

            extractPath = Path.Combine(destinationFolderPath, "temp");

            if (csvPositioningData is not DefaultNonCustomizedRecord positioningData)
                return positionY;

            var imageDataXml = GetImagesFromZip(zipFilePath, destinationFolderPath)
                               ?? throw new Exception("No image data found in the xml file");

            double positionX = MarginLeft;
            double actualImageX = positionX;
            double actualImageY = positioningData.PositionY;

            // Order Id
            masterLayer.CreateArtisticText(
                -150,
                actualImageY + 36,
                Path.GetFileNameWithoutExtension(zipFilePath).Split('_')[0],
                cdrTextLanguage.cdrLanguageNone,
                cdrTextCharSet.cdrCharSetMixed,
                "Arial",
                30);

            // Import all images
            ImportImages(masterLayer, imageDataXml.MainImages, ref positionX, actualImageY);
            ImportImages(masterLayer, imageDataXml.SnapshotImageNames, ref positionX, actualImageY);

            // Add text overlays
            AddTextValues(masterLayer, imageDataXml, actualImageX, actualImageY);

            return positionY;
        }

        private void ImportImages(
            Layer masterLayer,
            IEnumerable<string> imageNames,
            ref double positionX,
            double positionY)
        {
            if (imageNames == null)
                return;

            foreach (var imageFileName in imageNames)
            {
                string imagePath = Path.Combine(extractPath, imageFileName);

                if (!File.Exists(imagePath))
                    continue;

                var image = ImportImage(masterLayer, imagePath);

                if (image == null)
                    continue;

                image = ResizeShapeWithDesiredDimensions(
                    masterLayer,
                    image,
                    ImageMaxWidth,
                    ImageMaxHeight);

                image.SetPositionEx(
                    cdrReferencePoint.cdrCenter,
                    positionX,
                    positionY);

                positionX += ImageMaxWidth + MarginRight;
            }
        }

        private void AddTextValues(
         Layer masterLayer,
         dynamic imageDataXml,
         double x,
         double startY)
        {
            var inputValues = imageDataXml.InputValues;
            var colorValues = imageDataXml.ColorValues;
            var fontFamilies = imageDataXml.FontFamilies;

            if (inputValues == null || inputValues?.Count == 0)
                return;

            double currentY = startY;
            const double padding = 10; // space between texts

            for (int i = 0; i < inputValues?.Count; i++)
            {
                string input = inputValues[i];

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                // Safe font access
                string font = "Arial";

                if (fontFamilies != null &&
                    i < fontFamilies?.Count &&
                    !string.IsNullOrWhiteSpace(fontFamilies?[i]))
                {
                    font = fontFamilies[i];
                }

                var text = masterLayer.CreateArtisticText(
                    x,
                    currentY,
                    input,
                    cdrTextLanguage.cdrLanguageNone,
                    cdrTextCharSet.cdrCharSetMixed,
                    font,
                    30,
                    Alignment: cdrAlignment.cdrCenterAlignment);

                // Safe color access
                if (colorValues != null &&
                    i < colorValues?.Count &&
                    !string.IsNullOrWhiteSpace(colorValues?[i]))
                {
                    Color color = masterLayer.Color;
                    color.HexValue = colorValues[i];

                    text.Fill.ApplyUniformFill(color);
                }

                // Move next text below current text dynamically
                currentY = text.BottomY - padding;
            }
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
