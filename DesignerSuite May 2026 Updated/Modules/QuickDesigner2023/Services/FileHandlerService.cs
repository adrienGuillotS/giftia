using Svg;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using Corel.Interop.VGCore;
namespace QuickDesigner2023.Module.Services
{
    public class OrderZipFileHandler
    {
        private readonly Application _corelApp;
        private Dictionary<string, Dictionary<string, List<string>>> _inputTexts;
        private Dictionary<string, string> _svgToSnapMapping;

        private readonly double _imageMaxWidth;
        private readonly double _imageMaxHeight;
        private readonly double _maxTextWidth;
        private readonly double _marginLeft;
        private readonly double _marginRight;
        private readonly double _pageWidth;
        private readonly string _extractPath;

        public OrderZipFileHandler(Application app, double imageMaxWidth, double imageMaxHeight, double maxTextWidth, double marginLeft, double marginRight, double pageWidth, string extractPath)
        {
            _corelApp = app;
            _inputTexts = new Dictionary<string, Dictionary<string, List<string>>>();
            _svgToSnapMapping = new Dictionary<string, string>();
            _imageMaxWidth = imageMaxWidth;
            _imageMaxHeight = imageMaxHeight;
            _maxTextWidth = maxTextWidth;
            _marginLeft = marginLeft;
            _marginRight = marginRight;
            _pageWidth = pageWidth;
            _extractPath = extractPath;
        }

        public void HandleFilesWithoutCustomAsins(
            IProgress<object> progress,
            string destinationFolderPath,
            Document document,
            ref double positionX,
            ref double positionY,
            int zipCount,
            string zipFilePath,
            Layer layer,
            bool isFirstRow)
        {
            List<string> imageFiles = GetImagesFromZip(zipFilePath, destinationFolderPath);

            if (_svgToSnapMapping?.Count == 3)
                LoadThirdSurface(layer, imageFiles, zipCount);

            bool isFirstImage = true;
            Shape image = null;

            foreach (string imageFilePath in imageFiles)
            {
                try
                {
                    image = LoadResizedImage(layer, imageFilePath, _imageMaxWidth, _imageMaxHeight, _maxTextWidth);
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, $"Failed to load '{Path.GetFileName(imageFilePath)}' from '{Path.GetFileName(zipFilePath)}' for '{document.Name}.cdr'");
                    continue;
                }

                double x = 0;
                double y = 0;

                if (isFirstImage)
                {
                    double orderIdPosY = positionY - 8;
                    if (isFirstRow)
                    {
                        positionY -= _imageMaxHeight;
                        orderIdPosY = positionY - 15;
                    }

                    positionX = _marginLeft;

                    if (image != null)
                    {
                        x = positionX + ((_imageMaxWidth - image.SizeWidth) / 2);
                        y = positionY;
                    }

                    isFirstImage = false;

                    // Insert order id (zip name prefix)
                    layer.CreateArtisticText(-150, orderIdPosY, Path.GetFileNameWithoutExtension(zipFilePath).Split('_')[0],
                        cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed, "Arial", 30);
                }
                else
                {
                    positionX += _imageMaxWidth + _marginRight;

                    if (image != null)
                    {
                        x = positionX + ((_imageMaxWidth - image.SizeWidth) / 2);
                        y = positionY;
                    }
                }

                image?.SetPosition(x, y);

                LoadTextFromXmlFile(layer, imageFilePath, positionX, positionY);
            }

            if (image != null)
                positionY = image.BottomY;
        }

        private void LoadTextFromXmlFile(Layer layer, string imageFilePath, double positionX, double positionY)
        {
            if (!_inputTexts.TryGetValue(Path.GetFileNameWithoutExtension(imageFilePath), out var textData))
                return;

            if (!textData.TryGetValue("text", out var texts) || texts.Count == 0)
                return;

            foreach (var textValue in texts.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                string fontFamily = textData.ContainsKey("family") ? textData["family"].FirstOrDefault() : "Arial";
                int verticalOffset = texts.Count > 1 ? 15 : 25;

                Shape text = layer.CreateArtisticText(positionX, positionY -= verticalOffset, textValue,
                    cdrTextLanguage.cdrLanguageNone, cdrTextCharSet.cdrCharSetMixed, fontFamily, 25);

                if (textData.TryGetValue("color", out var colors))
                {
                    string color = colors.FirstOrDefault();
                    if (!string.IsNullOrEmpty(color))
                    {
                        var c = layer.Color;
                        c.HexValue = color;
                        text.Fill.ApplyUniformFill(c);
                    }
                }
            }
        }

        private void LoadThirdSurface(Layer layer, List<string> imageFiles, int zipCount)
        {
            var yPositions = new Dictionary<int, double> { { 1, 235 }, { 2, 140 }, { 0, 46 } };

            var thirdMapping = _svgToSnapMapping.ElementAt(2);
            var svgImageFile = imageFiles.FirstOrDefault(i => i.EndsWith(thirdMapping.Key));
            var jpgImageFile = imageFiles.FirstOrDefault(i => i.EndsWith(thirdMapping.Value));

            // SVG
            var svgImage = LoadResizedImage(layer, svgImageFile, 80, 80, _maxTextWidth);
            PositionAndAddText(layer, svgImage, svgImageFile, -205, yPositions[zipCount % 3]);

            // JPG
            var jpgImage = LoadResizedImage(layer, jpgImageFile, 80, 80, _maxTextWidth);
            PositionAndAddText(layer, jpgImage, jpgImageFile, -56, yPositions[zipCount % 3]);

            imageFiles.RemoveAll(i => i == svgImageFile || i == jpgImageFile);
        }

        private void PositionAndAddText(Layer layer, Shape image, string filePath, double positionX, double positionY)
        {
            if (image != null)
            {
                var x = positionX - (image.SizeWidth / 2);
                var y = positionY + (image.SizeHeight / 2);
                image.SetPosition(x, y);
            }
            LoadTextFromXmlFile(layer, filePath, positionX - 40, positionY + 40);
        }

        private void ReportProgress(IProgress<object> progress, string message)
        {
            progress?.Report(message + Environment.NewLine);
        }

        private List<string> GetImagesFromZip(string zipFilePath, string destinationFolderPath)
        {
            Directory.CreateDirectory(_extractPath);
            EmptyTempDirectory(destinationFolderPath);
            List<string> otherFiles = new List<string>();
            List<string> svgFiles = new List<string>();
            List<string> imageFiles = new List<string>();
            List<string> inputTextsList = new List<string>();
            List<string> fontFamilyList = new List<string>();
            List<string> fontColorList = new List<string>();
            List<string> svgNameList = new List<string>();

            _inputTexts = new Dictionary<string, Dictionary<string, List<string>>>();
            _svgToSnapMapping = new Dictionary<string, string>();
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
                        string destinationPath = Path.GetFullPath(Path.Combine(_extractPath, entry.FullName));

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(_extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath);
                            if (Path.GetExtension(destinationPath).EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                            {
                                svgFiles.Add(destinationPath);
                            }
                            else
                            {
                                otherFiles.Add(destinationPath);
                            }
                            //imageFiles.Add(destinationPath);
                        }
                    }
                    else if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        //if (entry.FullName.Equals("32496115032842.xml"))
                        //{
                        //    Console.WriteLine("");
                        //}
                        string destinationPath = Path.GetFullPath(Path.Combine(_extractPath, entry.FullName));
                        if (destinationPath.StartsWith(_extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath);
                            string xmlFileData = File.ReadAllText(destinationPath).Replace("\n", "~`~").Replace("\r", "~`~");
                            Regex svgRegex = new Regex(@"<svg>(((?!<\/svg>).)*)<\/svg>|<svg\/>");
                            foreach (Match match in svgRegex.Matches(xmlFileData))
                            {
                                svgNameList.Add(match.Groups[1].Value.Replace("~`~", Environment.NewLine));
                            }
                            Regex inputRegex = new Regex(@"<inputValue>(((?!<\/inputValue>).)*)<\/inputValue>|<inputValue\/>");
                            foreach (Match match in inputRegex.Matches(xmlFileData))
                            {
                                inputTextsList.Add(match.Groups[1].Value.Replace("~`~", Environment.NewLine));
                            }
                            Regex familyRegex = new Regex(@"<family>(((?!<\/family>).)*)<\/family>");
                            foreach (Match match in familyRegex.Matches(xmlFileData))
                            {
                                string fontFamily = match.Groups[1].Value.Replace("~`~", Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(fontFamily))
                                    fontFamilyList.Add(fontFamily);
                            }
                            Regex colorRegex = new Regex(@"<value>(((?!<\/value>).)*)<\/value>");
                            foreach (Match match in colorRegex.Matches(xmlFileData))
                            {
                                var hexColor = match.Groups[1].Value.Replace("~`~", Environment.NewLine);
                                if (IsHexColorCode(hexColor))
                                    fontColorList.Add(hexColor);
                            }
                            // Reading xml document with XPath
                            var xpathDoc = new XPathDocument(destinationPath);
                            var xpathNav = xpathDoc.CreateNavigator();
                            // extracting svg to snapshot (jpg) mappings.
                            var children = xpathNav.Select("/data/customizationData/children");
                            while (children.MoveNext())
                            {
                                var svgNode = children.Current.SelectSingleNode("svg");
                                var snapNode = children.Current.SelectSingleNode("snapshot/imageName");
                                _svgToSnapMapping.Add(svgNode.Value, snapNode.Value);
                            }

                        }
                    }
                }
            }
            List<string> svgsText = new List<string>();
            foreach (string file in svgFiles)
            {
                imageFiles.Add(file);
                svgsText.Add(File.ReadAllText(file));
            }
            for (int i = 0; i < svgNameList.Count; i++)
            {

                Dictionary<string, List<string>> textData = new Dictionary<string, List<string>>();
                if (inputTextsList.Count > i)
                {
                    List<string> dataList = new List<string>();
                    if (svgNameList.Count == 1 && svgNameList.Count != inputTextsList.Count)
                    {
                        dataList = inputTextsList;
                    }
                    else
                    {
                        dataList.Add(System.Net.WebUtility.HtmlDecode(inputTextsList[i]));
                    }
                    textData.Add("text", dataList);
                }
                if (fontFamilyList.Count > i)
                {
                    var list = new List<string>()
                    {
                        fontFamilyList[i]
                    };
                    textData.Add("family", list);
                }
                if (fontColorList.Count > i)
                {
                    var list = new List<string>()
                    {
                        fontColorList[i]
                    };
                    textData.Add("color", list);
                }
                _inputTexts.Add(Path.GetFileNameWithoutExtension(svgNameList[i]), textData);
            }
            foreach (string file in otherFiles)
            {
                bool existsInSvg = false;
                foreach (string text in svgsText)
                {
                    if (text.Contains(Path.GetFileNameWithoutExtension(file)))
                    {
                        existsInSvg = true;
                    }
                }
                if (!existsInSvg)
                {
                    imageFiles.Add(file);
                }
            }
            return imageFiles;
        }

        private static bool IsHexColorCode(string input)
        {
            // Define a regular expression pattern for a valid hex color code
            string pattern = @"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";

            // Use Regex.IsMatch to check if the input matches the pattern
            return Regex.IsMatch(input, pattern);
        }

        private Shape LoadResizedImage(Layer masterLayer, string imageFilePath, double maxWidth, double maxHeight, double maxTextWidth)
        {
            double adjustedMaxWidth = maxWidth;
            if (imageFilePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                //if svg file contains only text
                string svgText = File.ReadAllText(imageFilePath);
                if (svgText.Contains("<text") && !svgText.Contains("<image"))
                {
                    return null;
                }
                adjustedMaxWidth = GetMaxWidth(imageFilePath, maxWidth, maxTextWidth);
                TransformImageFile(imageFilePath);
            }
            Shape image;

            StructImportOptions sio = _corelApp.CreateStructImportOptions();
            sio.MaintainLayers = true;

            var filters = masterLayer.ImportEx(imageFilePath, Options: sio);
            filters.Finish();

            image = masterLayer.FindShape(Path.GetFileName(imageFilePath));
            if (imageFilePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                if (image.SizeWidth > adjustedMaxWidth || image.SizeHeight > maxHeight)
                {

                    if (image.SizeWidth >= image.SizeHeight)
                    {
                        double diff = (image.SizeWidth - adjustedMaxWidth) / image.SizeWidth;

                        image.SizeWidth = adjustedMaxWidth;
                        var updatedHeight = (1 - diff) * image.SizeHeight;
                        image.SizeHeight = updatedHeight;
                    }
                    else
                    {
                        double diff = (image.SizeHeight - maxHeight) / image.SizeHeight;
                        image.SizeHeight = maxHeight;
                        image.SizeWidth = (1 - diff) * image.SizeWidth;
                    }
                }
                if (image.SizeWidth > adjustedMaxWidth)
                {
                    double diff = (image.SizeWidth - adjustedMaxWidth) / image.SizeWidth;
                    image.SizeWidth = adjustedMaxWidth;
                    image.SizeHeight = (1 - diff) * image.SizeHeight;
                }
                if (image.SizeHeight > maxHeight)
                {
                    double diff = (image.SizeHeight - maxHeight) / image.SizeHeight;
                    image.SizeHeight = maxHeight;
                    image.SizeWidth = (1 - diff) * image.SizeWidth;
                }
            }
            // if it's a jpg file of MUG
            else
            {
                image.SizeWidth = maxHeight;
                image.SizeHeight = maxHeight;
            }
            return image;
        }


        /// <summary>
        /// This works fine only with the svg version 3.0.84
        /// </summary>
        /// <param name="imageFilePath"></param>
        private void TransformImageFile(string imageFilePath)
        {
            SvgDocument document = SvgDocument.Open(imageFilePath);
            int i = 0;
            while (i < document.Children.Count)
            {
                var child = document.Children[i];
                if (child is SvgGroup)
                {
                    if (child.Children.Where(c => c is SvgRectangle).Count() > 0)
                    {
                        document.Children.RemoveAt(i);
                        continue;
                    }
                    child.Transforms?.RemoveAll(t => true);
                    if (child.ContainsAttribute("clip-path") && child.Children.Count > 0)
                    {
                        SvgElement innerGroup = document.Children[i] = child.Children[0];
                        innerGroup.Transforms.RemoveAll(t => true);
                        List<SvgElement> toBeRemoved = new List<SvgElement>();
                        foreach (SvgElement innerElement in innerGroup.Children)
                        {
                            if (innerElement is SvgImage image)
                            {
                                if (image.X > 360)
                                {
                                    image.X = 360;
                                }
                                else if (image.X < -360)
                                {
                                    image.X = -360;
                                }
                                if (image.Y > 360)
                                {
                                    image.Y = 360;
                                }
                                else if (image.Y < -360)
                                {
                                    image.Y = -360;
                                }
                            }
                            else
                            {
                                toBeRemoved.Add(innerElement);
                            }

                        }
                        foreach (var e in toBeRemoved)
                        {
                            innerGroup.Children.Remove(e);
                        }
                    }
                }

                i++;
            }

            File.WriteAllText(imageFilePath, document.GetXML());
        }

        private double GetMaxWidth(string imageFilePath, double maxWidth, double maxTextWidth)
        {
            if (((SvgDocument)SvgDocument.Open(imageFilePath)).GetXML().Contains("<text") && !((SvgDocument)SvgDocument.Open(imageFilePath)).GetXML().Contains("<image"))
            {
                return maxTextWidth;
            }
            return maxWidth;
        }

        private void EmptyTempDirectory(string destinationFolderPath)
        {
            string tempDirectoryPath = destinationFolderPath + @"\temp\";
            DirectoryInfo di = new DirectoryInfo(tempDirectoryPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
    }
}