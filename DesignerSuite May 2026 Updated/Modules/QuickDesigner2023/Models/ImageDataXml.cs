using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QuickDesigner2023.Module.Models
{
    public class ImageDataXml
    {
        public List<string> MainImages { get; set; }
        public List<string> SnapshotImageNames { get; set; }
        public List<string> InputValues { get; set; }
        public List<string> FontFamilies { get; set; }
        public List<string> ColorValues { get; set; }

        public static ImageDataXml ReadDataFromXmlFile(string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException(xmlFilePath);
            }

            XDocument xmlDoc = XDocument.Load(xmlFilePath);

            // Extracting imageName under <image> (excluding <snapshot><imageName>)
            var imageNames = xmlDoc.Descendants("image")
                                  .Where(image => image.Descendants("snapshot").Descendants("imageName").FirstOrDefault() == null)
                                  .SelectMany(image => image.Descendants("imageName"))
                                  .Select(x => x.Value)
                                  .ToList();

            // Extracting imageName under <snapshot>
            var snapshotImageNames = xmlDoc.Descendants("snapshot")
                                           .Descendants("imageName")
                                           .Select(x => x.Value)
                                           .ToList();

            // Extracting inputValue elements
            var inputValues = xmlDoc.Descendants("inputValue")
                                    .Select(x => x.Value)
                                    .ToList();

            // Extracting font family under <fontSelection>
            var fontFamilies = xmlDoc.Descendants("fontSelection")
                                      .Descendants("family")
                                      .Select(x => x.Value)
                                      .ToList();

            // Extracting color value under <colorSelection>
            var colorValues = xmlDoc.Descendants("colorSelection")
                                    .Descendants("value")
                                    .Select(x => x.Value)
                                    .ToList();

            // Creating an ImageData object
            ImageDataXml imageData = new ImageDataXml
            {
                MainImages = imageNames,
                SnapshotImageNames = snapshotImageNames,
                InputValues = inputValues,
                FontFamilies = fontFamilies,
                ColorValues = colorValues
            };

            // Returning the ImageData object
            return imageData;
        }
    }
}
