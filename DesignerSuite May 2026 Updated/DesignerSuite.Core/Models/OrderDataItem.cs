using System.IO;
using System.Xml.Linq;

namespace DesignerSuite.Core.Models
{
    public class OrderDataItem : IOrderDataItem
    {
        public static OrderDataItem ReadDataFromXmlFile(string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException(xmlFilePath);
            }

            XDocument xmlDoc = XDocument.Load(xmlFilePath);

            // Extracting imageName under <image> (excluding <snapshot><imageName>)
            var imageName = xmlDoc.Descendants("image")
                                  .Where(image => image.Descendants("snapshot").Descendants("imageName").FirstOrDefault() == null)
                                  .SelectMany(image => image.Descendants("imageName"))
                                  .Select(x => x.Value)
                                  .FirstOrDefault();

            // Extracting imageName under <snapshot>
            var snapshotImageName = xmlDoc.Descendants("snapshot")
                                           .Descendants("imageName")
                                           .Select(x => x.Value)
                                           .FirstOrDefault();

            // Extracting inputValue elements
            var inputValue = xmlDoc.Descendants("inputValue")
                                    .Select(x => x.Value)
                                    .FirstOrDefault();

            // Extracting font family under <fontSelection>
            var fontFamily = xmlDoc.Descendants("fontSelection")
                                      .Descendants("family")
                                      .Select(x => x.Value)
                                      .FirstOrDefault();

            // Extracting color value under <colorSelection>
            var colorValue = xmlDoc.Descendants("colorSelection")
                                    .Descendants("value")
                                    .Select(x => x.Value)
                                    .FirstOrDefault();

            // Creating an ImageData object
            OrderDataItem imageData = new OrderDataItem
            {
                ImageName = imageName,
                SnapshotImageName = snapshotImageName,
                InputValue = inputValue,
                FontFamily = fontFamily,
                ColorValue = colorValue
            };

            // Returning the ImageData object
            return imageData;
        }
    }
}