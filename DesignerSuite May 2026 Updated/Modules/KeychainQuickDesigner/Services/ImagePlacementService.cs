using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Utilities;
using KeychainQuickDesigner.Module.Constants;
using KeychainQuickDesigner.Module.Enums;
using KeychainQuickDesigner.Module.Helpers;
using KeychainQuickDesigner.Module.Interfaces;
using KeychainQuickDesigner.Module.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Corel.Interop.VGCore;

namespace KeychainQuickDesigner.Module.Services
{
    public class ImagePlacementService(ICorelDrawService corel, ISvgImportService svgService) : IImagePlacementService
    {
        private readonly ICorelDrawService _corelService = corel ?? throw new ArgumentNullException(nameof(corel));
        private const double BoxWidth = 25.6;
        private const int BoxHeight = 34;
        private const int _throttleEveryNPastes = 3;
        private static readonly Regex EscapeNewlineRegex = new(@"(?<!\r)\n", RegexOptions.Compiled);

        public void PlaceImages(Layer layer, IOrderDataItem data, string zipPath, string destinationPath, OrderDataCSV position, string orderId)
        {
            KeychainOrderDataItem dataItem = (KeychainOrderDataItem)data;
            layer.CreateArtisticText(
                position.OrderNumberX, position.OrderNumberY, orderId,
                cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed,
                "Arial", 30, Alignment: cdrAlignment.cdrCenterAlignment
            );
            ImportMainImage(layer, dataItem, destinationPath, position);

            ImportSnapshotImage(layer, dataItem, destinationPath, position);

            // text message
            if (!string.IsNullOrWhiteSpace(dataItem.InputValue))
            {
                CreateTextElement(layer, dataItem, position);
            }
            else if (!string.IsNullOrWhiteSpace(dataItem.SvgIconName))
            {
                // Draw a Rectangle
                Shape box = CreateEmptyRectangle(position);

                double boxPadding = box.Outline.Width + 1;

                var icon = svgService.DuplicateSvgIcon(dataItem.SvgIconName, box.CenterX, box.TopY - boxPadding);

                if (icon != null)
                {
                    icon.MoveToLayer(layer);
                    var group = CreateShapeRangeCenteredAligned(icon, box).Group();
                    group.SetPositionEx(cdrReferencePoint.cdrCenter, position.TextX, position.TextY);
                }
            }
        }

        private void ImportSnapshotImage(Layer layer, IOrderDataItem data, string destinationPath, OrderDataCSV position)
        {
            if (string.IsNullOrWhiteSpace(data.SnapshotImageName)) return;
            // snapshot
            var snapPath = Path.Combine(destinationPath, data.SnapshotImageName);
            if (File.Exists(snapPath))
            {
                var image = _corelService.ImportImage(layer, snapPath);
                image = _corelService.ResizeShape(image, position.PreviewImageWidth, isWidth: true);
                image.SetPositionEx(cdrReferencePoint.cdrCenter, position.PreviewImageX, position.PreviewImageY);
            }
        }

        private void ImportMainImage(Layer layer, IOrderDataItem data, string destinationPath, OrderDataCSV position)
        {
            if (string.IsNullOrWhiteSpace(data.ImageName)) return;
            // main image
            var mainImagePath = Path.Combine(destinationPath, data.ImageName);
            if (File.Exists(mainImagePath))
            {
                var image = _corelService.ImportImage(layer, mainImagePath);
                image = _corelService.ResizeShape(image, position.ActualImageHeight);
                image.SetPositionEx(cdrReferencePoint.cdrCenter, position.ActualImageX, position.ActualImageY);
                image.Name = $"{ShapePrefixes.MainImage}{image.Name}";
            }
        }

        private Shape CreateEmptyRectangle(OrderDataCSV position)
        {
            var box = _corelService.CorelApp.ActiveLayer.CreateRectangle2(
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

        public async Task CopyShapesToSeparateFiles<T>(
                string destinationFolder,
                List<T> locations,
                string shapeNamePrefix,
                int maxShapesPerDoc,
                double pageWidth,
                double pageHeight,
                DocumentExportTypeEnum exportFormat,
                IProgress<int> progress,
                string fileNamePostfix, cdrShapeType cdrShapeTypeToSearch) where T : IShapeLocation
        {
            var positioningEnumerator = locations.GetEnumerator();
            bool hasMorePositions = positioningEnumerator.MoveNext();

            // Get files using more efficient pattern matching
            var matchingFiles = Directory.EnumerateFiles(destinationFolder, "*.cdr")
                .Where(file => AppConstants.FileNameRegex.IsMatch(Path.GetFileNameWithoutExtension(file)))
                .ToList();

            int totalFiles = matchingFiles.Count;
            if (totalFiles == 0)
            {
                progress.Report(100);
                return;
            }

            Document? outputDoc = null;
            int currentDocIndex = 1;
            int imagesInCurrentDoc = 0;

            for (int j = 0; j < totalFiles; j++)
            {
                Document? sourceDoc = null;
                try
                {
                    sourceDoc = _corelService.CorelApp.OpenDocument(matchingFiles[j]);
                    var matchingShapes = new List<Shape>();
                    switch (cdrShapeTypeToSearch)
                    {
                        case cdrShapeType.cdrBitmapShape:
                            matchingShapes = sourceDoc?.ActivePage?.Shapes?
                                .Cast<Shape>()
                                .Where(shape => shape != null &&
                                !string.IsNullOrEmpty(shape.Name)
                                && shape.Name.StartsWith(shapeNamePrefix)).ToList() ?? [];
                            break;
                        case cdrShapeType.cdrGroupShape:
                            Shapes? allShapes = sourceDoc?.ActivePage?.Shapes;
                            for (int i = 1; i <= allShapes?.Count; i++) // CorelDRAW collections are 1-based
                            {
                                Shape s = allShapes[i];
                                if (s != null && s.Type == cdrShapeType.cdrGroupShape)
                                {
                                    // Match the UI idea of "Group of 2 objects"
                                    if (s.Shapes != null && s.Shapes.Count == 2 &&
                                        (string.IsNullOrWhiteSpace(s.Name) || s.Name.Equals(shapeNamePrefix)))
                                        matchingShapes.Add(s);
                                }
                            }
                            break;
                        default:
                            throw new NotSupportedException($"Shape type '{cdrShapeTypeToSearch}' is not supported.");
                    }

                    if (matchingShapes == null || matchingShapes.Count == 0)
                    {
                        continue;
                    }
                    foreach (var shape in matchingShapes)
                    {
                        // Create new document when needed
                        if (outputDoc == null || imagesInCurrentDoc >= maxShapesPerDoc)
                        {
                            if (outputDoc != null)
                            {
                                _corelService.SaveDocument(outputDoc, destinationFolder, exportFormat);
                            }
                            outputDoc = _corelService.CreateNewDocument($"{currentDocIndex++}-{fileNamePostfix}", pageWidth, pageHeight);
                            outputDoc.ActivePage.SetSize(pageWidth, pageHeight);

                            imagesInCurrentDoc = 0;
                            positioningEnumerator.Dispose();
                            positioningEnumerator = locations.GetEnumerator();
                            hasMorePositions = positioningEnumerator.MoveNext();
                        }
                        //outputDoc.BeginCommandGroup("Paste Shape");
                        try
                        {
                            // Copy-paste with minimal activation
                            shape.Copy();
                            outputDoc.Activate();
                            var pastedShape = outputDoc.ActiveLayer.Paste();

                            try
                            {
                                // Position and count
                                if (!hasMorePositions)
                                    throw new InvalidOperationException("Positioning data exhausted");

                                pastedShape.SetPositionEx(cdrReferencePoint.cdrCenter,
                                    positioningEnumerator.Current.PositionX,
                                    positioningEnumerator.Current.PositionY);
                            }
                            finally
                            {
                                ComHelper.SafeRelease(pastedShape);
                            }
                        }
                        finally
                        {
                            // outputDoc.EndCommandGroup();
                        }

                        hasMorePositions = positioningEnumerator.MoveNext();
                        imagesInCurrentDoc++;

                        //Throttle slightly to keep Corel responsive/ stable
                        if (imagesInCurrentDoc % _throttleEveryNPastes == 0)
                            await Task.Delay(TimeSpan.FromSeconds(.5));
                    }

                }
                catch (Exception ex)
                {
                    // Log & continue with next file (don’t crash the whole batch)
                    Debug.WriteLine($"Error processing '{matchingFiles[j]}': {ex}");
                }
                finally
                {
                    // Close source doc ASAP to drop its undo & memory
                    if (sourceDoc != null)
                    {
                        try { sourceDoc?.Close(); } catch { /* ignore */ }
                        ComHelper.SafeRelease(sourceDoc);
                    }
                    progress.Report((j + 1) * 100 / totalFiles);
                    ComHelper.ForceGC();
                }
            }

            // Save final document
            if (outputDoc != null)
            {
                _corelService.SaveDocument(outputDoc, destinationFolder, exportFormat);
            }

            positioningEnumerator.Dispose();
            _corelService.Refresh();
            progress.Report(100);
        }

        public async Task EnlargeTextBoxShapes(
               string destinationFolder,
               string shapeNamePrefix,
               double newHeight,
               IProgress<int> progress)
        {

            // Get files using more efficient pattern matching
            var matchingFiles = Directory.EnumerateFiles(destinationFolder, "*.cdr")
                .Where(file => AppConstants.FileNameRegex.IsMatch(Path.GetFileNameWithoutExtension(file)))
                .ToList();

            int totalFiles = matchingFiles.Count;
            if (totalFiles == 0)
            {
                progress.Report(100);
                return;
            }

            int imagesInCurrentDoc = 0;

            for (int j = 0; j < totalFiles; j++)
            {
                Document? currentCdrDoc = null;
                try
                {
                    currentCdrDoc = _corelService.CorelApp.OpenDocument(matchingFiles[j]);
                    currentCdrDoc.Unit = cdrUnit.cdrMillimeter;
                    currentCdrDoc.Rulers.VUnits = currentCdrDoc.Rulers.HUnits = cdrUnit.cdrMillimeter;
                    // Find all main images in one operation
                    var matchingShapes = new List<Shape>();

                    Shapes allShapes = currentCdrDoc.ActivePage.Shapes;
                    for (int i = 1; i <= allShapes.Count; i++) // CorelDRAW collections are 1-based
                    {
                        Shape s = allShapes[i];
                        if (s != null && s.Type == cdrShapeType.cdrGroupShape)
                        {
                            // Match the UI idea of "Group of 2 objects"
                            if (s.Shapes != null && s.Shapes.Count == 2)
                                matchingShapes.Add(s);
                        }
                    }

                    currentCdrDoc.BeginCommandGroup("Enlarge Text Box Shapes");
                    foreach (var shape in matchingShapes)
                    {
                        try
                        {
                            SetShapeSizeWithRespectToHeight(newHeight, shape);

                            ProcessParagraphTextInGroup(shape);
                        }
                        finally
                        {
                            ComHelper.SafeRelease(shape);
                        }


                        if (imagesInCurrentDoc % _throttleEveryNPastes == 0)
                            await Task.Delay(TimeSpan.FromSeconds(.5));
                    }

                }
                catch (Exception ex)
                {
                    // Log & continue with next file (don’t crash the whole batch)
                    Debug.WriteLine($"Error processing '{matchingFiles[j]}': {ex}");
                }
                finally
                {
                    currentCdrDoc?.EndCommandGroup();
                    // Close source doc ASAP to drop its undo & memory
                    if (currentCdrDoc != null)
                    {
                        _corelService.SaveDocument(currentCdrDoc, destinationFolder);
                        ComHelper.SafeRelease(currentCdrDoc);
                    }
                    progress.Report((j + 1) * 100 / totalFiles);
                    ComHelper.ForceGC();
                }
            }

            _corelService.Refresh();
            progress.Report(100);
        }

        #region Private Methods

        private void CreateTextElement(Layer layer, IOrderDataItem dataItem, OrderDataCSV position)
        {
            KeychainOrderDataItem keychainOrderData = (KeychainOrderDataItem)dataItem;
            Shape box = CreateEmptyRectangle(position);
            Shape? icon = null;

            double boxPadding = box.Outline.Width + 1;
            double textShapeMaxAllowedHeight = position.TextIconGroupMaxHeight;

            double textShapeXPosition = position.TextX;
            double textShapeYPosition = position.TextY;
            bool pasteSvgIconNameText = false;
            if (!string.IsNullOrWhiteSpace(keychainOrderData.SvgIconName))
            {
                // Place icon centered in the box
                icon = svgService.DuplicateSvgIcon(keychainOrderData.SvgIconName, box.CenterX, box.TopY - boxPadding);
                // Updated code to fix CS8602: Dereference of a possibly null reference.
                if (icon != null)
                {
                    icon.MoveToLayer(layer);
                    // Adjust available height for text (icon + padding)
                    textShapeMaxAllowedHeight = position.TextIconGroupMaxHeight - (icon.SizeHeight + (boxPadding * 2));

                    // Position text BELOW the icon
                    textShapeYPosition = icon.BottomY - 2.5;
                }
                else if (!string.IsNullOrWhiteSpace(keychainOrderData.SvgIconName))
                {
                    pasteSvgIconNameText = true;// set it to true if svg icon not found in the svg icons file
                }
            }

            var font = keychainOrderData.FontFamily ?? "Arial";
            var cleanText = keychainOrderData.InputValue != null
                ? EscapeNewlineRegex.Replace(keychainOrderData.InputValue, "\r\n")
                : string.Empty;

            // Create text below the icon
            var textShape = WrappingTextHelper.CreateAutoFittedParagraphText(
                _corelService.CorelApp.ActiveDocument,
                textShapeXPosition,
                textShapeYPosition,
                position.TextShapeMaxWidth,
                textShapeMaxAllowedHeight,
                cleanText,
                font,
                12
            );
            cdrReferencePoint referencePoint = !string.IsNullOrWhiteSpace(keychainOrderData.ColorValue) ? cdrReferencePoint.cdrTopLeft :
                cdrReferencePoint.cdrTopMiddle;

            textShape = WrappingTextHelper.ConvertParaToArtistic(textShape);
            textShape.SetPositionEx(referencePoint, textShapeXPosition, textShapeYPosition);
            textShape.Text.AlignProperties.Alignment = cdrAlignment.cdrCenterAlignment;
            textShape.Name = cleanText.Replace("\r\n", "");

            if (pasteSvgIconNameText)
            {
                icon = WrappingTextHelper.CreateAutoFittedParagraphText(
                _corelService.CorelApp.ActiveDocument,
                textShape.LeftX,
                textShape.BottomY - 5,
                position.TextShapeMaxWidth,
                textShapeMaxAllowedHeight,
                keychainOrderData?.SvgIconName,
                font,
                12);

                icon = WrappingTextHelper.ConvertParaToArtistic(icon);
                icon.SetPositionEx(referencePoint, textShapeXPosition, textShape.BottomY - 1);
                icon.Text.AlignProperties.Alignment = cdrAlignment.cdrCenterAlignment;
                icon.Name = keychainOrderData?.SvgIconName.Replace("\r\n", "");

                if (!string.IsNullOrWhiteSpace(keychainOrderData?.ColorValue))
                {
                    var color = layer.Color;
                    color.HexValue = keychainOrderData.ColorValue;
                    icon.Fill.ApplyUniformFill(color);
                }
            }

            if (!string.IsNullOrWhiteSpace(keychainOrderData?.ColorValue))
            {
                var color = layer.Color;
                color.HexValue = keychainOrderData.ColorValue;
                textShape.Fill.ApplyUniformFill(color);
            }

            if (!string.IsNullOrWhiteSpace(keychainOrderData?.SvgIconName) && icon != null)
            {
                // Group icon and text, then group with box
                var group = CreateShapeRangeCenteredAligned(icon, textShape, cdrAlignType.cdrAlignHCenter).Group();
                ResizeGroupToWidthWithMaxHeight(group, position.TextShapeMaxWidth, position.TextIconGroupMaxHeight);

                group = CreateShapeRangeCenteredAligned(box, group).Group();
                group.SetPositionEx(cdrReferencePoint.cdrCenter, position.TextX, position.TextY);
                group.Name = ShapePrefixes.Box;
            }
            else
            {
                ResizeGroupToWidthWithMaxHeight(textShape, position.TextShapeMaxWidth, position.TextIconGroupMaxHeight);
                var group = CreateShapeRangeCenteredAligned(box, textShape).Group();
                group.SetPositionEx(cdrReferencePoint.cdrCenter, position.TextX, position.TextY);
                group.Name = ShapePrefixes.Box;
            }
        }

        private ShapeRange CreateShapeRangeCenteredAligned(Shape box, Shape group, cdrAlignType cdrAlignType = cdrAlignType.cdrAlignVCenter | cdrAlignType.cdrAlignHCenter)
        {
            var range = _corelService.CorelApp.CreateShapeRange();
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
            var range = _corelService.CorelApp.CreateShapeRange();
            range.Add(shape1);
            range.Add(shape2);
            return range.Group();
        }

        private static void SetShapeSizeWithRespectToHeight(double newHeight, Shape shape)
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
}