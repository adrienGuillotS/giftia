using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    public class AmazonOrderParser
    {
        /// <summary>
        /// Parses the Amazon order JSON file into a strongly-typed OrderRoot object.
        /// </summary>
        public OrderRoot Parse(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("JSON file not found.", path);

            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<OrderRoot>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        /// <summary>
        /// Extracts grouped customization data (texts + images) by surface (e.g., Mug Side 1, Mug Side 2, Coaster).
        /// </summary>
        public Dictionary<string, (List<string> Texts, List<string> Images)> ExtractCustomizationData(OrderRoot order)
        {
            var result = new Dictionary<string, (List<string>, List<string>)>();

            if (order?.CustomizationData?.Children == null)
                return result;

            foreach (var surface in order.CustomizationData.Children)
            {
                var texts = new List<string>();
                var images = new List<string>();

                if (surface.Children != null)
                {
                    foreach (var child in surface.Children)
                        ExtractRecursive(child, texts, images);
                }

                result[surface.Name ?? "Unknown Surface"] = (texts, images);
            }

            return result;
        }

        private void ExtractRecursive(CustomizationChild node, List<string> texts, List<string> images)
        {
            if (!string.IsNullOrEmpty(node.InputValue))
                texts.Add($"{node.InputValue} (Font: {node.FontFamily}, Color: {node.ColorValue})");

            if (!string.IsNullOrEmpty(node.ImageName))
                images.Add(node.ImageName);

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                    ExtractRecursive(child, texts, images);
            }
        }
    }


}
