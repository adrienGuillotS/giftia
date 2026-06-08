using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string folderPath = @"G:\Testing Workspace\Umar\Temu\Temu Mug"; // 🔹 change this

        var result = GetZipFilesGroupedBySku(folderPath);

        foreach (var kvp in result)
        {
            Console.WriteLine($"SKU: {kvp.Key}");
            foreach (var zip in kvp.Value)
                Console.WriteLine($"  {zip}");
        }
    }


    public static Dictionary<string, List<string>> GetZipFilesGroupedBySku(string folderPath)
    {
        var skuToZipMapping = new Dictionary<string, List<string>>();
        var zipFiles = Directory.GetFiles(folderPath, "*.zip");

        foreach (var zipFilePath in zipFiles)
        {
            var sku = ExtractSkuFromZip(zipFilePath);
            if (string.IsNullOrEmpty(sku))
                continue;

            if (!skuToZipMapping.ContainsKey(sku))
            {
                skuToZipMapping[sku] = new List<string>();
            }

            skuToZipMapping[sku].Add(zipFilePath);
        }

        return skuToZipMapping;
    }

    private static string? ExtractSkuFromZip(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        var match = Regex.Match(fileName, @"SKU\s*ID\s*-\s*(\d+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

}
