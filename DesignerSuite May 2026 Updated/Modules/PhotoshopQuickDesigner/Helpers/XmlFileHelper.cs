using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.XPath;

namespace PSQuickDesigner
{
    public static class XmlFileHelper
    {
        public static List<OrderDocuement> ReadXmlFile(string xmlFile)
        {
            if (!File.Exists(xmlFile))
            {
                throw new System.IO.FileNotFoundException($"{xmlFile} could not be found.");
            }

            List<OrderDocuement> orderDocuments = new List<OrderDocuement>();
            string orderId = ExtractOrderId(xmlFile);

            var xpathDoc = new XPathDocument(xmlFile);
            var xpathNav = xpathDoc.CreateNavigator();

            // Try different XPath patterns based on XML structure
            // Pattern 1: For new pillow format with multiple children under customizationData
            var previewContainers = xpathNav.Select("/data/customizationData/children[type='PreviewContainerCustomization']");

            // Pattern 2: For old format with nested children
            if (previewContainers.Count == 0)
            {
                previewContainers = xpathNav.Select("/data/customizationData/children/children[type='PreviewContainerCustomization']");
            }

            // Pattern 3: For alternative old format
            if (previewContainers.Count == 0)
            {
                previewContainers = xpathNav.Select("/data/customizationData/children");
            }

            int itemCounter = 1;
            while (previewContainers.MoveNext())
            {
                var current = previewContainers.Current;
                if (current == null) { continue; }

                var order = new OrderDocuement()
                {
                    DocumentName = itemCounter == 1 ? orderId : $"{orderId}-{itemCounter}",
                    Text = FindNodeValue(current, "inputValue")?.Replace("\n", "\r"),
                    MainImage = current.SelectSingleNode(".//children[type='ImageCustomization']//imageName")?.Value,
                    SmallImage = ExtractSnapshotImage(current),
                    FontFamily = current.SelectSingleNode(".//children[type='FontCustomization']//family")?.Value,
                    FontColorHex = current.SelectSingleNode(".//children[type='ColorCustomization']//value")?.Value,
                };

                itemCounter++;
                orderDocuments.Add(order);
            }

            return orderDocuments;
        }

        private static string ExtractOrderId(string xmlFile)
        {
            var xpathDoc = new XPathDocument(xmlFile);
            var xpathNav = xpathDoc.CreateNavigator();
            var orderIdNode = xpathNav.SelectSingleNode("/data/orderId");
            return orderIdNode?.Value ?? string.Empty;
        }

        private static string ExtractSnapshotImage(XPathNavigator? node)
        {
            if (node == null) return null;

            // Try to find snapshot directly under the current node
            var snapshot = node.SelectSingleNode("snapshot/imageName");
            if (snapshot != null)
            {
                return snapshot.Value;
            }

            // Try to find snapshot in nested structure
            snapshot = node.SelectSingleNode(".//snapshot/imageName");
            if (snapshot != null)
            {
                return snapshot.Value;
            }

            return null;
        }
        // Keep the old method for backward compatibility
        private static string FindNodeValue(XPathNavigator? node, string nodeName)
        {
            if (node == null) return null;

            foreach (XPathNavigator child in node.SelectChildren(XPathNodeType.Element))
            {
                if (child.Name == nodeName)
                {
                    return child.Value;
                }
                else
                {
                    string result = FindNodeValue(child, nodeName);
                    if (result != null) return result;
                }
            }
            return null;
        }
    }
}