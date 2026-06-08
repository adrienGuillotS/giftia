using DesignerSuite.Core.Utilities;
using QuickDesigner2023.Module.Interfaces;
using QuickDesigner2023.Module.Models;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Corel.Interop.VGCore;
using cdrReferencePoint = Corel.Interop.VGCore.cdrReferencePoint;
using Layer = Corel.Interop.VGCore.Layer;

namespace QuickDesigner2023.Module.Services.Amazon
{
    public class KeychainDesignerService : CoreldrawDesignerService
    {
        private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex EscapeNewlineRegex = new(@"(?<!\r)\n", RegexOptions.Compiled);
        private const double BoxWidth = 25.6;
        private const int BoxHeight = 34;
        private const int GroupNewHeight = 65;
        private const string SvgIconsLayerName = "SVG Layer";

        public KeychainDesignerService(Application corelApp) : base(corelApp)
        {

        }

        public override double ImportImageDataIntoCDR(Corel.Interop.VGCore.Layer layer, string destinationFolderPath, ICsvRecord csvPositioningData, string zipFilePath, double positionY)
        {
            Debug.WriteLine("Keychain custom Asin");

            double newPositionY = positionY;
            if (csvPositioningData is KeychainCsvRecord position)
            {
                string extractPath = destinationFolderPath + @"\temp\";
                OrderData? data = ExtractFromZip(zipFilePath, extractPath);

                ArgumentNullException.ThrowIfNull(data);

                // insert order id from zip file name into the row
                layer.CreateArtisticText(position.OrderNumberX, position.OrderNumberY,
                    Path.GetFileNameWithoutExtension(zipFilePath).Split('_')[0]
                    , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                    , "Arial", 30, Alignment: cdrAlignment.cdrCenterAlignment);

                var imageShape = ImportMainImage(layer, data, extractPath, position);

                ImportSnapshotImage(layer, data, extractPath, position);

                // text message
                if (!string.IsNullOrWhiteSpace(data.TextMessage))
                {
                    CreateTextElement(layer, data, position);
                }
                else if (!string.IsNullOrWhiteSpace(data.SvgIconName))
                {
                    // Draw a Rectangle
                    Shape box = CreateEmptyRectangle(position);

                    double boxPadding = box.Outline.Width + 1;

                    var icon = DuplicateSvgIcon(data.SvgIconName, box.CenterX, box.TopY - boxPadding);

                    if (icon != null)
                    {
                        icon.MoveToLayer(layer);
                        var group = CreateShapeRangeCenteredAligned(icon, box).Group();
                        group.SetPositionEx(cdrReferencePoint.cdrCenter, position.TextX, position.TextY);

                        SetShapeSizeWithRespectToHeight(GroupNewHeight, group);
                    }
                }

                if (imageShape != null)
                    newPositionY = imageShape.BottomY;
            }
            return newPositionY;
        }
        public static void SetShapeSizeWithRespectToHeight(double newHeight, Shape shape)
        {
            // Suppose shape is your Shape object
            double currentHeight = shape.OriginalHeight;
            double currentWidth = shape.SizeWidth;

            // Calculate scale factor
            double scaleFactor = newHeight / currentHeight;

            // Scale proportionally
            shape.SizeHeight = currentHeight * scaleFactor;
            shape.SizeWidth = currentWidth * scaleFactor;
        }
        public  Shape? DuplicateSvgIcon(string iconName, double x, double y)
        {
            var icon = corelApp.ActiveDocument.ActivePage.Layers
                .Cast<Layer>()
                .FirstOrDefault(l => l.Name == SvgIconsLayerName)
                ?.Shapes.Cast<Shape>()
                .FirstOrDefault(s => s.Name.Equals(iconName, StringComparison.OrdinalIgnoreCase));

            var dup = icon?.Duplicate();
            dup?.SetPositionEx(cdrReferencePoint.cdrTopMiddle, x, y);
            return dup ?? null;
        }

        private void ImportSnapshotImage(Layer layer, OrderData data, string destinationPath, KeychainCsvRecord position)
        {
            if (string.IsNullOrWhiteSpace(data.SnapshotImageName)) return;
            // snapshot
            var snapPath = Path.Combine(destinationPath, data.SnapshotImageName);
            if (File.Exists(snapPath))
            {
                var image = ImportImage(layer, snapPath);
                image = ResizeShape(image, position.PreviewImageWidth, isWidth: true);
                image.SetPositionEx(cdrReferencePoint.cdrCenter, position.PreviewImageX, position.PreviewImageY);
            }
        }
        private Shape? ImportMainImage(Layer layer, OrderData data, string destinationPath, KeychainCsvRecord position)
        {
            if (string.IsNullOrWhiteSpace(data.ImageName)) return null;
            // main image
            var mainImagePath = Path.Combine(destinationPath, data.ImageName);
            if (File.Exists(mainImagePath))
            {
                var image = ImportImage(layer, mainImagePath);
                image = ResizeShape(image, position.ActualImageHeight);
                image.SetPositionEx(cdrReferencePoint.cdrCenter, position.ActualImageX, position.ActualImageY);
                image.Name = image.Name;
                return image;
            }
            return null;
        }
        public Shape ResizeShape(Shape shape, double size, bool isWidth = false)
        {
            var ratio = isWidth ? size / shape.SizeWidth : size / shape.SizeHeight;
            shape.SetSize(
                isWidth ? size : shape.SizeWidth * ratio,
                isWidth ? shape.SizeHeight * ratio : size
            );
            return shape;
        }
        
        private OrderData? ExtractFromZip(string zipPath, string extractPath)
        {
            OrderData? imageData = null;
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    var destPath = Path.Combine(extractPath, entry.FullName);
                    if (!destPath.StartsWith(extractPath)) continue;

                    var directoryPath = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    entry.ExtractToFile(destPath, true);

                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        imageData = ReadDataFromFile(destPath);
                    }
                }
            }
            return imageData;
        }

        public OrderData ReadDataFromFile(string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException(xmlFilePath);
            }

            XDocument xmlDoc = XDocument.Load(xmlFilePath);

            var imageName = xmlDoc.Descendants("image")
                                  .Where(image => image.Descendants("snapshot").Descendants("imageName").FirstOrDefault() == null)
                                  .SelectMany(image => image.Descendants("imageName"))
                                  .Select(x => x.Value)
                                  .FirstOrDefault();

            var snapshotImageName = xmlDoc.Descendants("snapshot")
                                           .Descendants("imageName")
                                           .Select(x => x.Value)
                                           .FirstOrDefault();

            var inputValue = xmlDoc.Descendants("inputValue")
                                    .Select(x => x.Value)
                                    .FirstOrDefault();

            var svgIcon = xmlDoc.Descendants("displayValue")
                                .Select(x => x.Value)
                                .FirstOrDefault();

            var fontFamily = xmlDoc.Descendants("fontSelection")
                                   .Descendants("family")
                                   .Select(x => x.Value)
                                   .FirstOrDefault();

            var colorValue = xmlDoc.Descendants("colorSelection")
                                   .Descendants("value")
                                   .Select(x => x.Value)
                                   .FirstOrDefault();

            OrderData imageData = new()
            {
                ImageName = imageName ?? string.Empty,
                SnapshotImageName = snapshotImageName ?? string.Empty,
                SvgIconName = string.IsNullOrEmpty(svgIcon) || svgIcon.ToLower().Equals("null") ? string.Empty : svgIcon,
                TextMessage = inputValue != null ? WhitespaceRegex.Replace(inputValue, " ").Trim() : string.Empty,
                FontFamily = fontFamily,
                ColorValue = colorValue
            };
            if (inputValue?.StartsWith("𝑔𝒾𝑔𝒾", StringComparison.OrdinalIgnoreCase) == true)
            {
                Debug.WriteLine(inputValue);
            }
            return imageData;
        }

        #region Private Methods
        private Shape CreateEmptyRectangle(KeychainCsvRecord position)
        {
            var box = corelApp.ActiveLayer.CreateRectangle2(
                position.TextX,
                position.TextY,
                BoxWidth,
                BoxHeight);
            box.OrderToBack();
            box.Fill.ApplyNoFill();
            box.Outline.SetProperties(0.5);
            box.SetPositionEx(cdrReferencePoint.cdrCenter, position.TextX, position.TextY);
            return box;
        }

        private void CreateTextElement(Layer layer, OrderData data, KeychainCsvRecord position)
        {
            Shape box = CreateEmptyRectangle(position);
            Shape? icon = null;

            double boxPadding = box.Outline.Width + 1;
            double textShapeMaxAllowedHeight = position.TextIconGroupMaxHeight;

            double textShapeXPosition = position.TextX;
            double textShapeYPosition = position.TextY;

            if (!string.IsNullOrWhiteSpace(data.SvgIconName))
            {
                // Place icon centered in the box
                icon = DuplicateSvgIcon(data.SvgIconName, box.CenterX, box.TopY - boxPadding);
                // Updated code to fix CS8602: Dereference of a possibly null reference.
                if (icon != null)
                {
                    icon.MoveToLayer(layer);
                    // Adjust available height for text (icon + padding)
                    textShapeMaxAllowedHeight = position.TextIconGroupMaxHeight - (icon.SizeHeight + (boxPadding * 2));

                    // Position text BELOW the icon
                    textShapeYPosition = icon.BottomY - 2.5;
                }
            }

            var font = data.FontFamily ?? "Arial";
            var cleanText = data.TextMessage != null
                ? EscapeNewlineRegex.Replace(data.TextMessage, "\r\n")
                : string.Empty;

            // Create text below the icon
            var textShape = WrappingTextHelper.CreateAutoFittedParagraphText(
                corelApp.ActiveDocument,
                textShapeXPosition,
                textShapeYPosition,
                position.TextShapeMaxWidth,
                textShapeMaxAllowedHeight,
                cleanText,
                font,
                12
            );
            cdrReferencePoint referencePoint = !string.IsNullOrWhiteSpace(data.ColorValue) ? cdrReferencePoint.cdrTopLeft :
                cdrReferencePoint.cdrTopMiddle;

            textShape = WrappingTextHelper.ConvertParaToArtistic(textShape);
            textShape.SetPositionEx(referencePoint, textShapeXPosition, textShapeYPosition);
            textShape.Text.AlignProperties.Alignment = cdrAlignment.cdrCenterAlignment;
            textShape.Name = cleanText.Replace("\r\n", "");

            if (!string.IsNullOrWhiteSpace(data.ColorValue))
            {
                var color = layer.Color;
                color.HexValue = data.ColorValue;
                textShape.Fill.ApplyUniformFill(color);
            }

            if (!string.IsNullOrWhiteSpace(data.SvgIconName) && icon != null)
            {
                // Group icon and text, then group with box
                var group = CreateShapeRangeCenteredAligned(icon, textShape, cdrAlignType.cdrAlignHCenter).Group();
                ResizeGroupToWidthWithMaxHeight(group, position.TextShapeMaxWidth, position.TextIconGroupMaxHeight);

                group = CreateShapeRangeCenteredAligned(box, group).Group();
                group.SetPositionEx(cdrReferencePoint.cdrCenter, position.TextX, position.TextY);
                group.Name = ShapePrefixes.Box;
                SetShapeSizeWithRespectToHeight(GroupNewHeight, group);
            }
            else
            {
                ResizeGroupToWidthWithMaxHeight(textShape, position.TextShapeMaxWidth, position.TextIconGroupMaxHeight);
                var group = CreateShapeRangeCenteredAligned(box, textShape).Group();
                group.SetPositionEx(cdrReferencePoint.cdrCenter, position.TextX, position.TextY);
                group.Name = ShapePrefixes.Box;
                SetShapeSizeWithRespectToHeight(GroupNewHeight, group);
            }
        }

        private ShapeRange CreateShapeRangeCenteredAligned(Shape box, Shape group, cdrAlignType cdrAlignType = cdrAlignType.cdrAlignVCenter | cdrAlignType.cdrAlignHCenter)
        {
            var range = corelApp.CreateShapeRange();
            range.Add(group);
            range.Add(box);
            range.AddToSelection();
            range.AlignToGrid(cdrAlignType);
            range.RemoveFromSelection();
            return range;
        }

        private static void ResizeGroupToWidthWithMaxHeight(Shape group, double targetWidth, double maxHeight)
        {
            maxHeight -= 3;
            // Current group dimensions
            double currentWidth = group.SizeWidth;
            double currentHeight = group.SizeHeight;

            // Calculate the scale factor required to reach target width
            double scaleFactor = targetWidth / currentWidth;

            // Predict new height after scaling
            double newHeight = currentHeight * scaleFactor;

            // If new height exceeds the max height, adjust scale factor based on height instead
            if (newHeight > maxHeight)
            {
                scaleFactor = maxHeight / currentHeight;
            }

            // Apply scaling proportionally (same factor for X and Y to maintain aspect ratio)
            group.Stretch(scaleFactor, scaleFactor);
        }


        private Shape CreateShapeGroup(Shape shape1, Shape shape2)
        {
            var range = corelApp.CreateShapeRange();
            range.Add(shape1);
            range.Add(shape2);
            return range.Group();
        }


        private static void ProcessParagraphTextInGroup(Shape shape)
        {
            // If the shape itself is paragraph text
            if (shape.Type == cdrShapeType.cdrTextShape &&
                shape.Text.Type == cdrTextType.cdrParagraphText)
            {
                shape.Text.FitTextToFrame();
            }

            // If the shape is a group, go deeper
            if (shape.Type == cdrShapeType.cdrGroupShape)
            {
                foreach (Shape subShape in shape.Shapes)
                {
                    ProcessParagraphTextInGroup(subShape);
                }
            }
        }



        #endregion
    }

    public static class ShapePrefixes
    {
        public const string Box = "Group of 2 Objects";
        public const string MainImage = "main_image_";
    }
}
