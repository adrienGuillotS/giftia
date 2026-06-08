using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BirthdayCertificateDesigner2023.Helpers
{
    public static class XmlFileHepler
    {
        public static string UpdateBirthdayInJsonFile(string json, string newBirthday)
        {
            var root = JObject.Parse(json);

            // Find all elements where type == 2
            var type2Elements = root
                .SelectTokens("$.surfaces[*].regions[*].elements[*]")
                .Where(e => e["type"]?.Value<int>() == 2)
                .ToList();

            // Birthday is the second "type 2" element (index 1)
            if (type2Elements.Count > 1)
            {
                type2Elements[1]["text"] = newBirthday;
            }

            return root.ToString((Newtonsoft.Json.Formatting)Formatting.Indented);
        }

        public static void UpdateInputValue(XmlDocument xmlDoc, string newValue, string label = "Date of Birth")
        {
            XmlNodeList customizationNodes = xmlDoc.SelectNodes("//children[type='FlatContainerCustomization']");

            foreach (XmlNode customizationNode in customizationNodes)
            {
                XmlNodeList childrenNodes = customizationNode.SelectNodes("children");

                foreach (XmlNode childNode in childrenNodes)
                {
                    XmlNode nameLabel = childNode.SelectSingleNode("label");
                    XmlNode inputValueNode = childNode.SelectSingleNode("inputValue");

                    if (nameLabel != null && nameLabel.InnerText == label && inputValueNode != null)
                    {
                        inputValueNode.InnerText = newValue;
                        break;
                    }
                }
            }
        }

    }
}
