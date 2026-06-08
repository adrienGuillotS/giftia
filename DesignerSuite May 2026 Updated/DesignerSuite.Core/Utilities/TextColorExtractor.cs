using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesignerSuite.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DesignerSuite.Core.Utilities
{

    public static class TextColorExtractor
    {
        public static List<TextColorInfo> Extract(string json)
        {
            var results = new List<TextColorInfo>();

            if (string.IsNullOrWhiteSpace(json))
                return results;

            JObject root = JObject.Parse(json);

            var surfaces = root["surfaces"];
            if (surfaces == null) return results;

            foreach (var surface in surfaces)
            {
                var regions = surface["regions"];
                if (regions == null) continue;

                foreach (var region in regions)
                {
                    var elements = region["elements"];
                    if (elements == null) continue;

                    foreach (var element in elements)
                    {
                        var textColorInfo = new TextColorInfo();
                        // Type 2 = text
                        if ((int?)element["type"] == 2)
                        {
                            string text = (string)element["text"];
                            var color = element["color"];

                            if (!string.IsNullOrEmpty(text))
                            {

                                // Normalize text (remove line breaks)
                                text = text.Replace("\n", " ").Replace("\r", " ").Trim();
                                textColorInfo.Text = text;
                            }
                            if (color != null)
                            {
                                int r = (int)color["red"];
                                int g = (int)color["green"];
                                int b = (int)color["blue"];
                                string hex = $"#{r:X2}{g:X2}{b:X2}";
                                textColorInfo.ColorHex = hex;
                            }
                            results.Add(textColorInfo);
                        }
                    }
                }
            }

            return results;
        }
    }

}
