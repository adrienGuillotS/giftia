using DesignerSuite.Core.Models;
using KeychainQuickDesigner.Module.Models;
using KeychainQuickDesigner.ModuleDocker.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace KeychainQuickDesigner.Module.Services
{
    public class XmlFileReaderService : IFileReaderService
    {
        private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        public async Task<IOrderDataItem> ReadDataFromFileAsync(string xmlFilePath)
        {
            return await Task.Run(() => { return ReadDataFromFile(xmlFilePath); });
        }

        public IOrderDataItem ReadDataFromFile(string xmlFilePath)
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

            KeychainOrderDataItem imageData = new()
            {
                ImageName = imageName ?? string.Empty,
                SnapshotImageName = snapshotImageName ?? string.Empty,
                SvgIconName = string.IsNullOrEmpty(svgIcon) || svgIcon.ToLower().Equals("null") ? string.Empty : svgIcon,
                InputValue = inputValue != null ? WhitespaceRegex.Replace(inputValue, " ").Trim() : string.Empty,
                FontFamily = fontFamily,
                ColorValue = colorValue
            };
            if (inputValue?.StartsWith("𝑔𝒾𝑔𝒾", StringComparison.OrdinalIgnoreCase) == true)
            {
                Debug.WriteLine(inputValue);
            }
            return imageData;
        }
    }
}
