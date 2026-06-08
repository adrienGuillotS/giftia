using System.IO;
using System.Runtime.InteropServices;
using Photoshop;
using PSQuickDesigner.Converters;
using PSQuickDesigner.Enums;
using Application = Photoshop.Application;

namespace PSQuickDesigner
{
    public static class PhotoshopHelper
    {
        public static float _mainImageWidthInCM = 39;
        public static float _mainImageHeightInCM = 39;
        public static float _smallImageWidthInCM = 10f;
        public static float _smallImageHeightInCM = 10f;
        public static float _smallImageRightMarginInCM = 1.5f;
        public static float _smallImageTopMarginInCM = 1.5f;
        public static PsUnits _psUnit = PsUnits.psCM;
        public static PsResolutions _psResolutions = PsResolutions.psPixelsPerCentimeter;

        #region preference Helpers
        public static void SetPsUnits(string units)
        {
            switch (units)
            {
                case "pixelsUnit":
                    _psUnit = PsUnits.psPixels;
                    break;
                case "inchesUnit":
                    _psUnit = PsUnits.psInches;
                    break;
                case "centimetersUnit":
                    _psUnit = PsUnits.psCM;
                    break;
                case "millimetersUnit":
                    _psUnit = PsUnits.psMM;
                    break;
                case "pointsUnit":
                    _psUnit = PsUnits.psPoints;
                    break;
                case "picasUnit":
                    _psUnit = PsUnits.psPicas;
                    break;
                default:
                    throw new ArgumentException($"No such '{units}' unit exist.");
            }
        }

        public static PsDocumentFill GetFillColor(string fillColor)
        {
            PsDocumentFill documentFill;
            switch (fillColor)
            {
                case "transparent":
                    documentFill = PsDocumentFill.psTransparent;
                    break;
                case "background":
                    documentFill = PsDocumentFill.psBackgroundColor;
                    break;
                default:
                    documentFill = PsDocumentFill.psWhite;
                    break;
            }
            return documentFill;
        }
        public static PsNewDocumentMode GetPsNewDocumentMode(string mode)
        {
            PsNewDocumentMode _psNewDocumentMode;
            switch (mode)
            {
                case "bitmap":
                    _psNewDocumentMode = PsNewDocumentMode.psNewBitmap;
                    break;
                case "grayscale":
                    _psNewDocumentMode = PsNewDocumentMode.psNewGray;
                    break;
                case "RGB":
                    _psNewDocumentMode = PsNewDocumentMode.psNewRGB;
                    break;
                case "CMYK":
                    _psNewDocumentMode = PsNewDocumentMode.psNewCMYK;
                    break;
                case "Lab":
                    _psNewDocumentMode = PsNewDocumentMode.psNewLab;
                    break;
                default:
                    throw new ArgumentException($"No such '{mode}' new document color mode exist.");
            }
            return _psNewDocumentMode;
        }
        #endregion

        public static async Task ImportImageAsync(string mainImage, string smallImage,
           string textContent, string fontName, double fontSize, string hexColor,
           double documentWidth, double documentHeight, string savePath,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] string name,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] float resolution,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] PsNewDocumentMode mode,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object initialFill,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object pixelAspectRatio,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object bitsPerChannel,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object colorProfileName)
        {
            await Task.Run(() =>
            {
                ImportImage(mainImage, smallImage, textContent, fontName, fontSize, hexColor,
                    documentWidth, documentHeight, savePath, name, resolution, mode, initialFill,
                    pixelAspectRatio, bitsPerChannel, colorProfileName);
            });
        }
        public static void ImportImage(string mainImage, string smallImage,
           string textContent, string fontFamily, double fontSize, string hexColor,
           double documentWidth, double documentHeight, string savePath,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] string name,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] float resolution,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] PsNewDocumentMode mode,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object initialFill,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object pixelAspectRatio,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object bitsPerChannel,
           [Optional][In][MarshalAs(UnmanagedType.Struct)] object colorProfileName)
        {
            Document targetDoc = null, sourceDoc = null;
            ArtLayer sourceLayer = null, targetLayer = null;
            Application photoshopApp = null;
            try
            {
                // Create a new Photoshop application instance
                photoshopApp = new Application();
                photoshopApp.Visible = true;
                // Set preferences
                photoshopApp.Preferences.RulerUnits = _psUnit;
                photoshopApp.DisplayDialogs = PsDialogModes.psDisplayNoDialogs;
                // Create a new target document
                targetDoc = photoshopApp.Documents.Add(documentWidth, documentHeight, resolution, name, mode, initialFill, pixelAspectRatio, bitsPerChannel, colorProfileName); // Adjust canvas size as needed

                // Open the main image
                if (File.Exists(mainImage))
                    ImportMainImage();

                if (!string.IsNullOrWhiteSpace(textContent))
                {
                    var postScriptName = string.Empty;
                    try
                    {
                        foreach (TextFont font in photoshopApp.Fonts)
                        {

                            if (font.Name.Equals(fontFamily, StringComparison.OrdinalIgnoreCase)
                                || font.Family.Equals(fontFamily, StringComparison.OrdinalIgnoreCase)
                                && font.Style.Equals("Regular"))
                            {
                                postScriptName = font.PostScriptName;
                                break;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(postScriptName)) throw new Exception();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"No such font family '{fontFamily}' exists in Photoshop.\n\n {ex.Message}");
                    }

                    InsertText(targetDoc, textContent, hexColor, postScriptName, fontSize);
                }

                if (File.Exists(smallImage))
                    ImportPreviewImage();

                string filePath = Path.Combine(savePath, name.ToString());
                targetDoc.SaveAs(filePath);

                // Close the document without saving changes (optional)
                targetDoc.Close(PsSaveOptions.psSaveChanges);

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                ReleaseComObject(sourceLayer);
                ReleaseComObject(targetLayer);
                ReleaseComObject(targetDoc);
                ReleaseComObject(sourceDoc);
                ReleaseComObject(photoshopApp);
            }

            void ImportMainImage()
            {
                sourceDoc = photoshopApp.Open(mainImage);

                // Get the first layer of the source document
                sourceLayer = sourceDoc.ArtLayers[1]; // Adjust the index as needed
                sourceLayer.Copy();

                photoshopApp.ActiveDocument = targetDoc;

                targetLayer = targetDoc.Paste();

                if (targetLayer != null)
                {// resize and center align

                    var targetWidth = UnitConverter.ConvertCentimeters(_mainImageWidthInCM, _psUnit, _psResolutions, resolution);
                    //   var imageHeight = UnitConverter.ConvertCentimeters(_mainImageHeightInCM, _psUnit, _psResolutions, resolution);

                    // Calculate the proportional height to maintain the aspect ratio
                    // int targetHeight = (int)(targetWidth * ((float)documentHeight / documentWidth));

                    //targetLayer.Resize(targetWidth, targetHeight, PsAnchorPosition.psMiddleCenter);
                    //targetDoc.ResizeImage()
                    //  targetDoc.ResizeImage(targetWidth, targetHeight, null, PsResampleMethod.psBicubic, PsAnchorPosition.psMiddleCenter, true);

                    double currentWidth = targetLayer.Bounds[2] - targetLayer.Bounds[0];
                    double currentHeight = targetLayer.Bounds[3] - targetLayer.Bounds[1];

                    // Calculate the scaling factor based on the desired width
                    double scaleFactor = targetWidth / currentWidth;

                    // Resize the layer maintaining aspect ratio
                    targetLayer.Resize(scaleFactor * 100, scaleFactor * 100, PsAnchorPosition.psMiddleCenter);
                    CenterAlignLayer(targetDoc, targetLayer);
                }

                sourceDoc.Close(PsSaveOptions.psDoNotSaveChanges);
            }

            void ImportPreviewImage()
            {
                var smallImageWidth = UnitConverter.ConvertCentimeters(_smallImageWidthInCM, _psUnit, _psResolutions, resolution);
                var smallImageHeight = UnitConverter.ConvertCentimeters(_smallImageHeightInCM, _psUnit, _psResolutions, resolution);
                var topMargin = UnitConverter.ConvertCentimeters(_smallImageTopMarginInCM, _psUnit, _psResolutions, resolution);
                var rightMargin = UnitConverter.ConvertCentimeters(_smallImageRightMarginInCM, _psUnit, _psResolutions, resolution);

                ImportSmallImage(smallImage, smallImageWidth, smallImageHeight, topMargin, rightMargin, targetDoc, photoshopApp);


                if (!Directory.Exists(savePath)) { Directory.CreateDirectory(savePath); }
            }
        }

        private static void ImportSmallImage(string image, double width, double height, double topMargin, double rightMargin, Document targetDoc, Application photoshopApp)
        {
            Document sourceDoc = null;
            ArtLayer sourceLayer = null, targetLayer = null;
            try
            {
                sourceDoc = photoshopApp.Open(image);
                sourceLayer = sourceDoc.ArtLayers[1];
                sourceLayer.Copy();

                photoshopApp.ActiveDocument = targetDoc;
                targetLayer = targetDoc.Paste();

                if (targetLayer != null)
                {
                    double scaleX = width / (targetLayer.Bounds[2] - targetLayer.Bounds[0]);
                    double scaleY = height / (targetLayer.Bounds[3] - targetLayer.Bounds[1]);

                    targetLayer.Resize(scaleX * 100, scaleY * 100, PsAnchorPosition.psMiddleCenter);

                    TopRightAlignLayer(targetDoc, targetLayer, topMargin, rightMargin);
                }

                sourceDoc.Close(PsSaveOptions.psDoNotSaveChanges);
            }
            finally
            {
                ReleaseComObject(sourceLayer);
                ReleaseComObject(targetLayer);
                ReleaseComObject(sourceDoc);
            }
        }

        private static void InsertText(Document targetDoc, string textContent, string hexColor, string fontPostScriptName,
               double fontSize)
        {
            ArtLayer textLayer = null;
            try
            {
                textLayer = targetDoc.ArtLayers.Add();
                textLayer.Kind = PsLayerKind.psTextLayer;

                textLayer.TextItem.Font = fontPostScriptName;
                textLayer.TextItem.Size = fontSize;
                textLayer.TextItem.Color.RGB.HexValue = hexColor.TrimStart('#');

                textLayer.TextItem.Contents = textContent;

                textLayer.TextItem.Justification = PsJustification.psCenter;

                CenterAlignLayer(targetDoc, textLayer);
            }
            finally
            {
                ReleaseComObject(textLayer);
            }
        }

        private static void CenterAlignLayer(Document targetDoc, ArtLayer layer)
        {
            double docWidth = targetDoc.Width;
            double docHeight = targetDoc.Height;
            double layerWidth = layer.Bounds[2] - layer.Bounds[0];
            double layerHeight = layer.Bounds[3] - layer.Bounds[1];

            double newX = (docWidth - layerWidth) / 2;
            double newY = (docHeight - layerHeight) / 2;

            layer.Translate(newX - layer.Bounds[0], newY - layer.Bounds[1]);
        }

        private static void TopRightAlignLayer(Document targetDoc, ArtLayer layer, double topMargin, double rightMargin)
        {
            double docWidth = targetDoc.Width;
            double layerWidth = layer.Bounds[2] - layer.Bounds[0];

            double newX = docWidth - layerWidth - rightMargin;
            double newY = topMargin;

            layer.Translate(newX - layer.Bounds[0], newY - layer.Bounds[1]);
        }

        private static void ReleaseComObject(object obj)
        {
            if (obj != null)
            {
                Marshal.ReleaseComObject(obj);
            }
        }

    }
}