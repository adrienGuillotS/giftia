using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    public static class FileNameHelper
    {
        /// <summary>
        /// remove newlines, tabs, extra spaces, and invalid characters from the file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string NormalizeFileName(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            // Remove line breaks, tabs, and trim spaces
            fileName = fileName.Replace("\r", "")
                               .Replace("\n", " ")
                               .Replace("\t", "")
                               .Trim();

            // Replace invalid file name characters with underscore
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            // Optionally collapse multiple spaces or underscores
            fileName = Regex.Replace(fileName, @"[\s_]+", "_");

            return fileName;
        }
        /// <summary>
        /// extract the order number (the 210-07803715469432015 part) from a filename like
        /// 210-07803715469432015 SKU ID - 67403069292561 (1).zip
        /// </summary>
        public static string ExtractOrderNumber(this string fileName)
        {
            var match = Regex.Match(fileName, @"\b\d{3}-\d{17}\b");
            return match.Success ? match.Value : fileName;
        }

        public static string RemoveNoText(this string text)
        {
            string cleaned = text.Replace(@"\s+", " ").Trim();
            if (cleaned.ToLower().Contains("not text", StringComparison.OrdinalIgnoreCase)
                || cleaned.ToLower().Contains("no text", StringComparison.OrdinalIgnoreCase)
                || cleaned.ToLower().Contains("no txt", StringComparison.OrdinalIgnoreCase))
            {
                text = string.Empty;
            }
            return text;
        }
    }

}
