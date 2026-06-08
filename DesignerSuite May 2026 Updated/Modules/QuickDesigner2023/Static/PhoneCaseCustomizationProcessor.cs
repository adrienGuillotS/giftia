using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDesigner2023.Module.Static
{
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    // Phone case specific model
    public record PhoneCaseData
    {
        public string? UploadedImage { get; init; }      // Buyer uploaded image (b01e014f-2d33-460b-8cfa-94f58e8d8f25.jpg)
        public string? PreviewImage { get; init; }       // Snapshot/preview image (9ddd4b59-be22-7a1a-c2d5-0430e179fe2b.jpg)
        public string? Text { get; init; }               // Custom text input
        public string? InputValue { get; init; }         // Text input value
        public string? FontFamily { get; init; }         // Font selection
        public string? Fill { get; init; }               // Text color
        public string? SvgImage { get; init; }           // SVG overlay
    }

    // Reusing existing JSON model classes from previous implementation
    public record CustomizationDataObject
    {
        [JsonPropertyName("customizationData")]
        public CustomizationDataContainer? CustomizationData { get; init; }
    }

    public record CustomizationDataContainer
    {
        [JsonPropertyName("children")]
        public List<SurfaceContainer>? Children { get; init; }
    }

    public record SurfaceContainer
    {
        [JsonPropertyName("snapshot")]
        public Snapshot? Snapshot { get; init; }

        [JsonPropertyName("svg")]
        public string? Svg { get; init; }

        [JsonPropertyName("children")]
        public List<CustomizationItem>? Children { get; init; }
    }

    public record Snapshot
    {
        [JsonPropertyName("imageName")]
        public string? ImageName { get; init; }
    }

    public record CustomizationItem
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("children")]
        public List<CustomizationDetail>? Children { get; init; }
    }

    public record CustomizationDetail
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("inputValue")]
        public string? InputValue { get; init; }

        [JsonPropertyName("fontSelection")]
        public FontSelection? FontSelection { get; init; }

        [JsonPropertyName("colorSelection")]
        public ColorSelection? ColorSelection { get; init; }

        [JsonPropertyName("image")]
        public ImageData? Image { get; init; }

        [JsonPropertyName("children")]
        public List<CustomizationDetail>? Children { get; init; }
    }

    public record ImageData
    {
        [JsonPropertyName("imageName")]
        public string? ImageName { get; init; }

        [JsonPropertyName("buyerFilename")]
        public string? BuyerFilename { get; init; }
    }

    public record FontSelection
    {
        [JsonPropertyName("family")]
        public string? Family { get; init; }
    }

    public record ColorSelection
    {
        [JsonPropertyName("value")]
        public string? Value { get; init; }
    }

    // Phone case specific processor
    public static class PhoneCaseProcessor
    {
        public static PhoneCaseData ProcessPhoneCaseData(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            var data = JsonSerializer.Deserialize<CustomizationDataObject>(json)
                ?? throw new InvalidOperationException("Invalid JSON structure");

            var children = data.CustomizationData?.Children
                ?? throw new InvalidOperationException("No customization data found");

            if (!children.Any())
                throw new InvalidOperationException("No surfaces found in customization data");

            // Phone cases typically have only one surface
            var surface = children[0];

            return new PhoneCaseData
            {
                PreviewImage = surface.Snapshot?.ImageName,
                SvgImage = surface.Svg,
                UploadedImage = FindFirstWithProperty(surface.Children, item => item?.Image is not null)?.Image?.ImageName,
                InputValue = FindFirstByType(surface.Children, "TextCustomization")?.InputValue,
                Text = FindFirstByType(surface.Children, "TextCustomization")?.InputValue,
                FontFamily = FindFirstWithProperty(surface.Children, item => item?.FontSelection is not null)?.FontSelection?.Family,
                Fill = FindFirstWithProperty(surface.Children, item => item?.ColorSelection is not null)?.ColorSelection?.Value
            };
        }

        private static CustomizationDetail? FindFirstByType(List<CustomizationItem>? items, string type)
        {
            return items?
                .SelectMany(item => FlattenCustomizationDetails(item.Children))
                .FirstOrDefault(detail => detail?.Type == type && !string.IsNullOrEmpty(detail.InputValue));
        }

        private static CustomizationDetail? FindFirstWithProperty(List<CustomizationItem>? items,
            Func<CustomizationDetail?, bool> predicate)
        {
            return items?
                .SelectMany(item => FlattenCustomizationDetails(item.Children))
                .FirstOrDefault(predicate);
        }

        private static IEnumerable<CustomizationDetail?> FlattenCustomizationDetails(List<CustomizationDetail>? details)
        {
            if (details is null) yield break;

            foreach (var detail in details)
            {
                yield return detail;

                if (detail?.Children is not null)
                {
                    foreach (var child in FlattenCustomizationDetails(detail.Children))
                    {
                        yield return child;
                    }
                }
            }
        }
    }

    // Unified processor that can handle both products
    public static class UniversalCustomizationProcessor
    {
        public static object ProcessCustomizationData(string json, ProductType productType)
        {
            return productType switch
            {
                ProductType.Mug => ProcessMugData(json),
                ProductType.PhoneCase => ProcessPhoneCaseData(json),
                _ => throw new ArgumentException($"Unsupported product type: {productType}")
            };
        }

        public static (LeftSide left, RightSide right, Coaster coaster) ProcessMugData(string json)
        {
            // Reuse the existing mug processing logic
            var data = JsonSerializer.Deserialize<CustomizationDataObject>(json)
                ?? throw new InvalidOperationException("Invalid JSON structure");

            var children = data.CustomizationData?.Children
                ?? throw new InvalidOperationException("No customization data found");

            if (children.Count < 3)
                throw new InvalidOperationException("Expected at least 3 surfaces for mug data");

            return (
                ProcessSurface<LeftSide>(children[0]),
                ProcessSurface<RightSide>(children[1]),
                ProcessSurface<Coaster>(children[2])
            );
        }

        public static PhoneCaseData ProcessPhoneCaseData(string json)
        {
            return PhoneCaseProcessor.ProcessPhoneCaseData(json);
        }

        private static T ProcessSurface<T>(SurfaceContainer surface) where T : SurfaceBase, new()
        {
            ArgumentNullException.ThrowIfNull(surface);

            return new T
            {
                PreviewImage = surface.Snapshot?.ImageName,
                ImageName = surface.Snapshot?.ImageName,
                SvgImage = surface.Svg,
                UploadedImage = FindFirstWithProperty(surface.Children, item => item?.Image is not null)?.Image?.ImageName,
                InputValue = FindFirstByType(surface.Children, "TextCustomization")?.InputValue,
                Text = FindFirstByType(surface.Children, "TextCustomization")?.InputValue,
                FontFamily = FindFirstWithProperty(surface.Children, item => item?.FontSelection is not null)?.FontSelection?.Family,
                Fill = FindFirstWithProperty(surface.Children, item => item?.ColorSelection is not null)?.ColorSelection?.Value
            };
        }

        private static CustomizationDetail? FindFirstByType(List<CustomizationItem>? items, string type)
        {
            return items?
                .SelectMany(item => FlattenCustomizationDetails(item.Children))
                .FirstOrDefault(detail => detail?.Type == type && !string.IsNullOrEmpty(detail.InputValue));
        }

        private static CustomizationDetail? FindFirstWithProperty(List<CustomizationItem>? items,
            Func<CustomizationDetail?, bool> predicate)
        {
            return items?
                .SelectMany(item => FlattenCustomizationDetails(item.Children))
                .FirstOrDefault(predicate);
        }

        private static IEnumerable<CustomizationDetail?> FlattenCustomizationDetails(List<CustomizationDetail>? details)
        {
            if (details is null) yield break;

            foreach (var detail in details)
            {
                yield return detail;

                if (detail?.Children is not null)
                {
                    foreach (var child in FlattenCustomizationDetails(detail.Children))
                    {
                        yield return child;
                    }
                }
            }
        }
    }

    // Product type enum
    public enum ProductType
    {
        Mug,
        PhoneCase
    }

    // Updated file processor to handle both product types
    public static class JsonFileProcessor
    {
        public static async Task ProcessAllJsonFilesInFolderAsync(string folderPath, ProductType productType)
        {
            ArgumentException.ThrowIfNullOrEmpty(folderPath);

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Directory does not exist: {folderPath}");
                return;
            }

            var jsonFiles = Directory.EnumerateFiles(folderPath, "*.json");

            if (!jsonFiles.Any())
            {
                Console.WriteLine($"No JSON files found in: {folderPath}");
                return;
            }

            Console.WriteLine($"Found {jsonFiles.Count()} JSON file(s) to process as {productType}:");

            await Parallel.ForEachAsync(jsonFiles, async (filePath, cancellationToken) =>
            {
                await ProcessJsonFileAsync(filePath, productType);
            });
        }

        private static async Task ProcessJsonFileAsync(string filePath, ProductType productType)
        {
            try
            {
                Console.WriteLine($"\nProcessing file: {Path.GetFileName(filePath)}");
                Console.WriteLine("==============================================");

                var jsonString = await File.ReadAllTextAsync(filePath);

                if (productType == ProductType.PhoneCase)
                {
                    var phoneCase = PhoneCaseProcessor.ProcessPhoneCaseData(jsonString);
                    PrintPhoneCaseData(phoneCase);
                }
                else if (productType == ProductType.Mug)
                {
                    var (left, right, coaster) = UniversalCustomizationProcessor.ProcessMugData(jsonString);
                    PrintMugData(left, right, coaster);
                }

                Console.WriteLine("==============================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private static void PrintPhoneCaseData(PhoneCaseData phoneCase)
        {
            Console.WriteLine($"Uploaded Image: {phoneCase.UploadedImage ?? "None"}");
            Console.WriteLine($"Preview Image: {phoneCase.PreviewImage ?? "Not found"}");
            Console.WriteLine($"Text: {phoneCase.Text ?? "None"}");
            Console.WriteLine($"Input Value: {phoneCase.InputValue ?? "None"}");
            Console.WriteLine($"Font Family: {phoneCase.FontFamily ?? "Default"}");
            Console.WriteLine($"Text Color: {phoneCase.Fill ?? "#000000"}");
            Console.WriteLine($"SVG Image: {phoneCase.SvgImage ?? "None"}");
        }

        private static void PrintMugData(LeftSide left, RightSide right, Coaster coaster)
        {
            Console.WriteLine($"Left Side Text: {left.Text ?? "None"}");
            Console.WriteLine($"Left Side Font: {left.FontFamily ?? "Default"}");
            Console.WriteLine($"Left Side Color: {left.Fill ?? "#000000"}");
            Console.WriteLine($"Left Side Preview Image: {left.PreviewImage ?? "Not found"}");
            Console.WriteLine($"Left Side Uploaded Image: {left.UploadedImage ?? "None"}");
            Console.WriteLine($"Left Side SVG: {left.SvgImage ?? "None"}");

            Console.WriteLine($"\nRight Side Text: {right.Text ?? "None"}");
            Console.WriteLine($"Right Side Font: {right.FontFamily ?? "Default"}");
            Console.WriteLine($"Right Side Color: {right.Fill ?? "#000000"}");
            Console.WriteLine($"Right Side Preview Image: {right.PreviewImage ?? "Not found"}");
            Console.WriteLine($"Right Side Uploaded Image: {right.UploadedImage ?? "None"}");
            Console.WriteLine($"Right Side SVG: {right.SvgImage ?? "None"}");

            Console.WriteLine($"\nCoaster Text: {coaster.Text ?? "None"}");
            Console.WriteLine($"Coaster Font: {coaster.FontFamily ?? "Default"}");
            Console.WriteLine($"Coaster Color: {coaster.Fill ?? "#000000"}");
            Console.WriteLine($"Coaster Preview Image: {coaster.PreviewImage ?? "Not found"}");
            Console.WriteLine($"Coaster Uploaded Image: {coaster.UploadedImage ?? "None"}");
            Console.WriteLine($"Coaster SVG: {coaster.SvgImage ?? "None"}");
        }

        // Auto-detect product type based on JSON content
        public static ProductType DetectProductType(string json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<CustomizationDataObject>(json);
                var surfaceCount = data?.CustomizationData?.Children?.Count ?? 0;

                // Mugs typically have 3 surfaces, phone cases have 1
                return surfaceCount >= 3 ? ProductType.Mug : ProductType.PhoneCase;
            }
            catch
            {
                return ProductType.PhoneCase; // Default to phone case if detection fails
            }
        }

        // Process files with auto-detection
        public static async Task ProcessFilesWithAutoDetectionAsync(string folderPath)
        {
            var jsonFiles = Directory.EnumerateFiles(folderPath, "*.json");

            foreach (var filePath in jsonFiles)
            {
                try
                {
                    var jsonString = await File.ReadAllTextAsync(filePath);
                    var productType = DetectProductType(jsonString);

                    Console.WriteLine($"\nProcessing {productType}: {Path.GetFileName(filePath)}");
                    Console.WriteLine("==============================================");

                    if (productType == ProductType.PhoneCase)
                    {
                        var phoneCase = PhoneCaseProcessor.ProcessPhoneCaseData(jsonString);
                        PrintPhoneCaseData(phoneCase);
                    }
                    else
                    {
                        var (left, right, coaster) = UniversalCustomizationProcessor.ProcessMugData(jsonString);
                        PrintMugData(left, right, coaster);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            }
        }
    }

    // Usage examples
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            string folderPath = @"G:\Testing Workspace\Umar\upgrading programme 2025\Test Files\PhoneCases";

            // Process all files as phone cases
            await JsonFileProcessor.ProcessAllJsonFilesInFolderAsync(folderPath, ProductType.PhoneCase);

            // Or use auto-detection
            await JsonFileProcessor.ProcessFilesWithAutoDetectionAsync(folderPath);

            // Process individual phone case file
            string phoneCaseFile = @"path\to\phone-case.json";
            if (File.Exists(phoneCaseFile))
            {
                var jsonString = await File.ReadAllTextAsync(phoneCaseFile);
                var phoneCase = PhoneCaseProcessor.ProcessPhoneCaseData(jsonString);

                Console.WriteLine($"Uploaded Image: {phoneCase.UploadedImage}");
                Console.WriteLine($"Preview Image: {phoneCase.PreviewImage}");
                Console.WriteLine($"Text: {phoneCase.Text}");
            }
        }
    }
}
