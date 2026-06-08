using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Models;
using DesignerSuite.Core.Services;
using DesignerSuite.Core.Services.Temu;
using DesignerSuite.Core.Utilities;
using QuickDesigner2023.Module.Interfaces;
using QuickDesigner2023.Module.Models;
using QuickDesigner2023.Module.Static;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Corel.Interop.VGCore;

namespace QuickDesigner2023.Module.Services.Temu
{
    public class PhoneCaseDesignerService : CoreldrawDesignerService
    {
        private static readonly Regex EscapeNewlineRegex = new(@"(?<!\r)\n", RegexOptions.Compiled);

        public PhoneCaseDesignerService(Application corelApp) : base(corelApp)
        {

        }

        public override double ImportImageDataIntoCDR(Layer masterLayer, string destinationFolderPath, ICsvRecord csvPositioningData, string zipFilePath, double positionY)
        {
            Debug.WriteLine("Phone Case custom SKU");

            double newPositionY = positionY;
            if (csvPositioningData is PhoneCaseCsvRecord positioningData)
            {
                string extractPath = destinationFolderPath + @"\temp\";

                IImageDataExtractorService imageDataExtractorService = new TemuImageDataExtractorService();
                var data = (OrderDataItem)imageDataExtractorService.ExtractImages(zipFilePath, extractPath);

                ArgumentNullException.ThrowIfNull(data);

                //PhoneCaseData phoneCase = PhoneCaseProcessor.ProcessPhoneCaseData(jsonString);
                string orderId = Path.GetFileNameWithoutExtension(zipFilePath).ExtractOrderNumber();

                // insert order id from zip file name into the row
                masterLayer.CreateArtisticText(positioningData.OrderNumberX, positioningData.OrderNumberY,
                    orderId
                    , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                    , "Arial", 30, Alignment: cdrAlignment.cdrCenterAlignment);

                PhoneCaseData phoneCase = new PhoneCaseData
                {
                    UploadedImage = data.ImageName,
                    PreviewImage = data.SnapshotImageName,
                    InputValue = data.InputValue?.RemoveNoText(),
                    Text = data.InputValue?.RemoveNoText(),
                    Fill = data.ColorValue,
                    FontFamily = data.FontFamily
                };

                var imageShape = ProcessSurface(masterLayer, phoneCase, positioningData, extractPath);

                if (imageShape != null)
                    newPositionY = imageShape.BottomY;
            }
            return newPositionY;
        }

        private Shape ProcessSurface(Layer masterLayer, PhoneCaseData phoneCaseData,
          PhoneCaseCsvRecord positioningData, string extractedDataFolderPath)
        {
            Shape? mainImageShape = null;
            if (!string.IsNullOrWhiteSpace(phoneCaseData.UploadedImage))
            {
                string imageFileName = Path.Combine(extractedDataFolderPath, phoneCaseData.UploadedImage);

                if (File.Exists(imageFileName))
                {
                    mainImageShape = ImportImage(masterLayer, imageFileName);

                    mainImageShape = ResizeShape(mainImageShape, positioningData.MainImageHeight, false);
                    mainImageShape.SetPositionEx(cdrReferencePoint.cdrCenter, positioningData.ImageX, positioningData.ImageY);
                }
            }
            Shape? snapImageShape = null;
            if (!string.IsNullOrEmpty(phoneCaseData.PreviewImage))
            {
                string snapImageName = Path.Combine(extractedDataFolderPath, phoneCaseData.PreviewImage);

                if (File.Exists(snapImageName))
                {
                    snapImageShape = ImportImage(masterLayer, snapImageName);

                    snapImageShape = ResizeShape(snapImageShape, positioningData.PreviewImageWidth, true);
                    snapImageShape.SetPositionEx(cdrReferencePoint.cdrCenter, positioningData.PreviewImageX, positioningData.PreviewImageY);
                }


            }
            if (!string.IsNullOrEmpty(phoneCaseData.InputValue))
            {
                string font = !string.IsNullOrWhiteSpace(phoneCaseData.FontFamily) ? phoneCaseData.FontFamily : "Arial";

                double textPositionX = positioningData.ImageX;
                double textPositionY = positioningData.ImageY;

                var textShape = DrawText(masterLayer, font, phoneCaseData.InputValue, phoneCaseData.Fill, textPositionX, textPositionY, positioningData.TextWidth);

                textShape = WrappingTextHelper.ConvertParaToArtistic(textShape);
                textShape.Text.AlignProperties.Alignment = cdrAlignment.cdrCenterAlignment;
                ResizeShapeByWidth(masterLayer, textShape, positioningData.TextWidth);
                if (mainImageShape != null)
                {
                    var yPos = mainImageShape.BottomY - (textShape.SizeHeight / 2 - 15);
                    textShape.SetPositionEx(cdrReferencePoint.cdrBottomMiddle, positioningData.ImageX, yPos);

                    // Group icon and text, then group with box
                    // _ = CreateShapeRangeCenteredAligned(snapImageShape, textShape, cdrAlignType.cdrAlignHCenter).Group();
                }
                else
                {
                    textShape.SetPositionEx(cdrReferencePoint.cdrCenter, textPositionX, textPositionY);
                }
            }

            if (positioningData.ModelName is not null)
            {
                // insert model name
                var modelShape = masterLayer.CreateArtisticText(positioningData.ModelTextX, positioningData.ModelTextY,
                       positioningData.ModelName
                       , cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed
                       , "Arial", 30, Alignment: cdrAlignment.cdrCenterAlignment);
                modelShape.Text.AlignProperties.Alignment = cdrAlignment.cdrCenterAlignment;
                modelShape.RotateEx(90, positioningData.ModelTextX, positioningData.ModelTextY);
                modelShape.SetPositionEx(cdrReferencePoint.cdrCenter, positioningData.ModelTextX, positioningData.ModelTextY);
            }
            return mainImageShape;
        }
    }
}
