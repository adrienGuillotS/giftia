using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Temu;
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

namespace QuickDesigner2023.Module.Services.Temu
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

                IImageDataExtractorService imageDataExtractorService = new TemuMugImageDataExtractorService();
                var orderDataItem = (TemuMugDataItem)imageDataExtractorService.ExtractImages(zipFilePath, extractPath);


                ArgumentNullException.ThrowIfNull(orderDataItem);

                string orderId = Path.GetFileNameWithoutExtension(zipFilePath).ExtractOrderNumber();

                // insert order id from zip file name into the row
                masterLayer.CreateArtisticText(positioningData.OrderNumberX, positioningData.OrderNumberY,
                    orderId
                    , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                    , "Times New Roman", 30, Alignment: cdrAlignment.cdrCenterAlignment);

                LeftSide leftSide = new LeftSide
                {
                    Fill = orderDataItem.ColorValue,
                    FontFamily = orderDataItem.FontFamily,
                    ImageName = orderDataItem.SnapshotImageName,
                    SnapImage = orderDataItem.ImageName,
                    Text = orderDataItem.InputValue?.RemoveNoText(),
                    InputValue = orderDataItem.InputValue?.RemoveNoText()
                };
                ProcessSurface(masterLayer, leftSide, extractPath,
                    positioningData.LeftPreviewImageX,
                    positioningData.LeftPreviewImageY,
                    positioningData.LeftSnapImageX,
                    positioningData.LeftSnapImageY,
                    95,
                    positioningData.ImageWidth,
                    positioningData.TextWidth,
                    positioningData.LeftTextX,
                    positioningData.LeftTextY);

                RightSide right = new RightSide
                {
                    Fill = orderDataItem.RightSideColorValue,
                    FontFamily = orderDataItem.FontFamily,
                    SnapImage = orderDataItem.SvgImageName,
                    Text = orderDataItem.RightSideInputValue?.RemoveNoText(),
                    InputValue = orderDataItem.RightSideInputValue?.RemoveNoText()
                };


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
            if (!string.IsNullOrEmpty(mugSide.Text))
            {
                string font = !string.IsNullOrWhiteSpace(mugSide.FontFamily) ? mugSide.FontFamily : "Arial";

                var textShape = DrawText(masterLayer, font, mugSide.Text, mugSide.Fill, textPositionX, textPositionY, textWidth);

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
