using DesignerSuite.Core.Models;
using DesignerSuite.Core.Utilities;
using QuickDesigner2023.Module.Interfaces;
using QuickDesigner2023.Module.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Corel.Interop.VGCore;

namespace QuickDesigner2023.Module.Services
{
    public abstract class CoreldrawDesignerService
    {
        #region Private Fields
        private const double PageWidth = 210;
        private const double PageHeight = 297;
        private const double ImageMaxWidth = 80;
        private const double ImageMaxHeight = 95;
        private const double MaxTextWidth = 70;

        private const double MarginLeft = 5;
        private const double MarginTop = (PageHeight - (ImageMaxHeight * 3)) / 2;
        private const double MarginRight = 40;
        private const double MarginBottom = MarginTop;
        #endregion



        private static readonly Regex EscapeNewlineRegex = new(@"(?<!\r)\n", RegexOptions.Compiled);

        public Corel.Interop.VGCore.Application corelApp;
        public CoreldrawDesignerService(Application corelApp)
        {
            this.corelApp = corelApp;
        }

        public virtual Shape ImportImage(Layer masterLayer, string imageFilePath)
        {
            Shape image;

            StructImportOptions sio = corelApp.CreateStructImportOptions();
            sio.MaintainLayers = true;

            var filters = masterLayer.ImportEx(imageFilePath, Options: sio);
            filters.Finish();

            image = masterLayer.FindShape(Path.GetFileName(imageFilePath));

            return image;
        }
        public virtual Shape ResizeShape(Shape shape, double size, bool isWidth = false)
        {
            var ratio = isWidth ? size / shape.SizeWidth : size / shape.SizeHeight;
            shape.SetSize(
                isWidth ? size : shape.SizeWidth * ratio,
                isWidth ? shape.SizeHeight * ratio : size
            );
            return shape;
        }

        public virtual Shape ResizeShapeByWidth(Layer masterLayer, Shape shape, double desiredWidth)
        {
            // Current dimensions
            double currentWidth = shape.SizeWidth;
            double currentHeight = shape.SizeHeight;

            // Scaling factor based on width
            double scaleFactor = desiredWidth / currentWidth;

            // Maintain aspect ratio
            double newWidth = desiredWidth;
            double newHeight = currentHeight * scaleFactor;

            // Apply new size
            shape.SetSize(newWidth, newHeight);

            return shape;
        }
        public virtual async Task<string> ReadFileAsync(string jsonFile)
        {
            string json = string.Empty;
            using (FileStream stream = new(jsonFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new(stream))
            {
                json = await reader.ReadToEndAsync();
            }

            return json;
        }

        public virtual string ReadFile(string jsonFile)
        {
            string json = string.Empty;
            using (FileStream stream = new(jsonFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new(stream))
            {
                json = reader.ReadToEnd();
            }

            return json;
        }

        public virtual Shape DrawText(Layer masterLayer, string font, string text, string fill, double textPositionX, double textPositionY, double maxWidth)
        {
            var cleanText = text != null
               ? EscapeNewlineRegex.Replace(text, "\r\n")
               : string.Empty;
            var textShape = WrappingTextHelper.CreateAutoFittedParagraphText(
               masterLayer.Application.ActiveDocument,
               textPositionX,
               textPositionY,
               maxWidth,
               maxWidth,
               cleanText,
               font,
               30f
           );

            string? color = !string.IsNullOrWhiteSpace(fill) ? fill : null;
            if (!string.IsNullOrWhiteSpace(color))
            {
                Color c = masterLayer.Color;
                c.HexValue = color;
                textShape.Fill.ApplyUniformFill(c);
            }
            return textShape;
        }
        public virtual void CreateGuideLines(Document document)
        {
            //Horizontal Guides
            double y = PageHeight - MarginTop;
            document.ActiveLayer.CreateGuide(0, y, PageWidth, y);
            for (int i = 0; i < 7; i++)
            {
                y -= ImageMaxHeight / 2;
                document.ActiveLayer.CreateGuide(0, y, PageWidth, y);
            }
            //Vertical Guides
            double x = MarginLeft;
            for (int i = 0; i < 6; i++)
            {
                document.ActiveLayer.CreateGuide(x, 0, x, PageHeight);
                x += ImageMaxWidth;
                document.ActiveLayer.CreateGuide(x, 0, x, PageHeight);
                x += MarginRight;
            }
        }

        public abstract double ImportImageDataIntoCDR(
            Layer masterLayer,
            string destinationFolderPath,
            ICsvRecord positioningData,
            string zipFilePath, double positionY);


    }
}
