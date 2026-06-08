using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class CustomizationExtractor
    {
        public Dictionary<string, (List<string> Texts, List<string> Images)> ExtractSurfaces(CustomizationData customizationData)
        {
            var result = new Dictionary<string, (List<string>, List<string>)>();

            foreach (var surface in customizationData.Children)
            {
                var texts = new List<string>();
                var images = new List<string>();

                foreach (var child in surface.Children)
                    ExtractRecursive(child, texts, images);

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
