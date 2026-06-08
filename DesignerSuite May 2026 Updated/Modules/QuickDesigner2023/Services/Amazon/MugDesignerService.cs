using DesignerSuite.Core.Utilities;
using MahApps.Metro.Controls;
using QuickDesigner2023.Module.Interfaces;
using QuickDesigner2023.Module.Models;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Corel.Interop.VGCore;
using Color = Corel.Interop.VGCore.Color;

namespace QuickDesigner2023.Module.Services.Amazon
{
    public class MugDesignerService : CoreldrawDesignerService
    {

        public MugDesignerService(Application corelApp) : base(corelApp)
        {

        }
        public override double ImportImageDataIntoCDR(Layer masterLayer, string destinationFolderPath, ICsvRecord csvPositioningData, string zipFilePath, double positionY)
        {
            Debug.WriteLine("Mug custom SKU");

            double newPositionY = positionY;
            if (csvPositioningData is MugCsvRecord positioningData)
            {
                string extractPath = destinationFolderPath + @"\temp\";

                var jsonFile = ZipFileHelper.ExtractZipContentAndGetJsonFile(zipFilePath, destinationFolderPath, extractPath);

                ArgumentNullException.ThrowIfNullOrWhiteSpace(jsonFile);

                string json = ReadFile(jsonFile);
                var (left, right, coaster) = MugCustomizationProcessor.ProcessCustomizationData(json);

                // insert order id from zip file name into the row
                masterLayer.CreateArtisticText(positioningData.OrderNumberX, positioningData.OrderNumberY,
                    Path.GetFileNameWithoutExtension(zipFilePath).Split('_')[0]
                    , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                    , "Arial", 30, Alignment: cdrAlignment.cdrCenterAlignment);

                ProcessSurface(masterLayer, left, extractPath,
                    positioningData.LeftPreviewImageX,
                    positioningData.LeftPreviewImageY,
                    positioningData.LeftSnapImageX,
                    positioningData.LeftSnapImageY,
                    95,
                    positioningData.ImageWidth,
                    positioningData.TextWidth,
                    positioningData.LeftTextX,
                    positioningData.LeftTextY);



                ProcessSurface(masterLayer, right, extractPath,
                      positioningData.RightPreviewImageX,
                      positioningData.RightPreviewImageY,
                      positioningData.RightSnapImageX,
                      positioningData.RightSnapImageY,
                      95,
                      positioningData.ImageWidth,
                      positioningData.TextWidth,
                      positioningData.RightTextX,
                      positioningData.RightTextY);

                var imageShape = ProcessSurface(masterLayer, coaster, extractPath,
                       positioningData.LeftSnapImageX - 140,
                       positioningData.LeftPreviewImageY - 10,
                       positioningData.LeftSnapImageX,
                       positioningData.LeftSnapImageY,
                       positioningData.ImageWidth,
                       positioningData.ImageWidth,
                       positioningData.TextWidth,
                       positioningData.LeftTextX,
                       positioningData.LeftTextY);
                if (imageShape != null)
                    newPositionY = imageShape.BottomY;
            }
            return newPositionY;
        }

        private Shape ProcessSurface(Layer masterLayer, SurfaceBase mugSide,
            string destinationFolderPath,
           double previewImagePositionX,
           double previewImagePositionY,
           double snapImagePositionX,
           double snapImagePositionY,
           double mugImageWidth,
           double snapImageWidth,
           double textWidth,
           double textPositionX,
           double textPositionY)
        {
            Shape mainImageShape = null;
            if (!string.IsNullOrWhiteSpace(mugSide.ImageName))
            {
                string imageFileName = Path.Combine(destinationFolderPath, mugSide.ImageName);

                if (File.Exists(imageFileName))
                {
                    mainImageShape = ImportImage(masterLayer, imageFileName);

                    mainImageShape = ResizeShapeByWidth(masterLayer, mainImageShape, mugImageWidth);
                    mainImageShape.SetPositionEx(cdrReferencePoint.cdrCenter, previewImagePositionX, previewImagePositionY);
                }
            }
            Shape snapImageShape = null;
            if (!string.IsNullOrEmpty(mugSide.SnapImage))
            {
                string snapImageName = Path.Combine(destinationFolderPath, mugSide.SnapImage);

                if (File.Exists(snapImageName))
                {
                    snapImageShape = ImportImage(masterLayer, snapImageName);

                    snapImageShape = ResizeShapeByWidth(masterLayer, snapImageShape, snapImageWidth);
                    snapImageShape.SetPositionEx(cdrReferencePoint.cdrCenter, snapImagePositionX, snapImagePositionY);
                }


            }
            if (mugSide?.AllTextEntries?.Count > 0)
            {
                foreach (var entry in mugSide.AllTextEntries)
                {
                    if (!string.IsNullOrEmpty(entry.InputValue))
                    {
                        string font = !string.IsNullOrWhiteSpace(entry.FontFamily) ? entry.FontFamily : "Arial";
                        string fillColor = entry.Fill != null ? entry.Fill : "#000000";
                        var textShape = DrawText(masterLayer, font, entry.InputValue, fillColor, textPositionX, textPositionY, textWidth);

                        textShape = WrappingTextHelper.ConvertParaToArtistic(textShape);
                        textShape.Text.AlignProperties.Alignment = cdrAlignment.cdrCenterAlignment;
                        ResizeShapeByWidth(masterLayer, textShape, textWidth);
                        if (snapImageShape != null)
                        {
                            var yPos = snapImageShape.BottomY - ((textShape.SizeHeight / 2) + 3);
                            textShape.SetPositionEx(cdrReferencePoint.cdrMiddleLeft, snapImageShape.PositionX, yPos);

                            // Group icon and text, then group with box
                            // _ = CreateShapeRangeCenteredAligned(snapImageShape, textShape, cdrAlignType.cdrAlignHCenter).Group();
                        }
                        else
                        {
                            textShape.SetPositionEx(cdrReferencePoint.cdrCenter, textPositionX, textPositionY);
                        }
                    }
                }
            }
            return mainImageShape;
        }
        private ShapeRange CreateShapeRangeCenteredAligned(Shape box, Shape group, cdrAlignType cdrAlignType = cdrAlignType.cdrAlignVCenter | cdrAlignType.cdrAlignHCenter)
        {
            var range = this.corelApp.CreateShapeRange();
            range.Add(group);
            range.Add(box);
            range.AddToSelection();
            range.AlignToGrid(cdrAlignType);
            range.RemoveFromSelection();
            return range;
        }
    }
}
