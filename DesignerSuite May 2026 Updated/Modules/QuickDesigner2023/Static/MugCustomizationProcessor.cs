using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

// Base class for common properties
public abstract record SurfaceBase
{
    public string? ImageName { get; set; }           // Preview image (snapshot)
    public string? PreviewImage { get; set; }        // Actual preview image filename
    public string? SnapImage { get; set; }           // Buyer-uploaded image
    public string? SvgImage { get; set; }            // SVG overlay
    public string? UploadedImage { get; set; }
    public string? Text { get; set; }
    public string? InputValue { get; set; }
    public string? FontFamily { get; set; }
    public string? Fill { get; set; }

    // All non-empty text entries for this surface
    public List<TextEntry>? AllTextEntries { get; set; }
}

// Represents a single text customization entry
public record TextEntry
{
    public string? InputValue { get; set; }
    public string? FontFamily { get; set; }
    public string? Fill { get; set; }
    public string? FontColorName { get; set; }
}

// Specific surface records
public record LeftSide : SurfaceBase;
public record RightSide : SurfaceBase;
public record Coaster : SurfaceBase;

// JSON model classes with proper null handling
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

public class CustomizationDetail
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("inputValue")]
    public string? InputValue { get; set; }

    [JsonPropertyName("fontSelection")]
    public FontSelection? FontSelection { get; set; }

    [JsonPropertyName("colorSelection")]
    public ColorSelection? ColorSelection { get; set; }

    [JsonPropertyName("image")]
    public ImageData? Image { get; set; }

    [JsonPropertyName("children")]
    public List<CustomizationDetail>? Children { get; set; }

    [JsonIgnore]
    public CustomizationDetail? Parent { get; set; }
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

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

// Modern processor with .NET 9 features
public static class MugCustomizationProcessor
{
    public static (LeftSide left, RightSide right, Coaster coaster) ProcessCustomizationData(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var data = JsonSerializer.Deserialize<CustomizationDataObject>(json, options)
            ?? throw new InvalidOperationException("Invalid JSON structure");

        var children = data.CustomizationData?.Children
            ?? throw new InvalidOperationException("No customization data found");

        if (children.Count < 3)
            throw new InvalidOperationException("Expected at least 3 surfaces in customization data");

        return (
            ProcessSurface<LeftSide>(children[0]),
            ProcessSurface<RightSide>(children[1]),
            ProcessSurface<Coaster>(children[2])
        );
    }

    private static T ProcessSurface<T>(SurfaceContainer surface) where T : SurfaceBase, new()
    {
        ArgumentNullException.ThrowIfNull(surface);

        // Extract ALL non-empty text entries with their font and color
        var allTextEntries = ExtractAllNonEmptyTextEntries(surface.Children);

        // Get the first text entry for backward compatibility (single properties)
        var firstTextEntry = allTextEntries.FirstOrDefault();

        return new T
        {
            PreviewImage = surface.Snapshot?.ImageName,
            ImageName = surface.Snapshot?.ImageName,
            SvgImage = surface.Svg,
            SnapImage = FindFirstImageName(surface.Children),
            InputValue = firstTextEntry?.InputValue,
            Text = firstTextEntry?.InputValue,
            FontFamily = firstTextEntry?.FontFamily,
            Fill = firstTextEntry?.Fill,
            AllTextEntries = allTextEntries
        };
    }

    private static string? FindFirstImageName(List<CustomizationItem>? items)
    {
        if (items == null) return null;

        var allDetails = new List<CustomizationDetail>();
        foreach (var item in items)
        {
            FlattenCustomizationDetailsWithParent(item.Children, null, allDetails);
        }

        var imageDetail = allDetails.FirstOrDefault(detail =>
            detail?.Type == "ImageCustomization" && detail?.Image?.ImageName != null);

        return imageDetail?.Image?.ImageName;
    }

    private static List<TextEntry> ExtractAllNonEmptyTextEntries(List<CustomizationItem>? items)
    {
        var textEntries = new List<TextEntry>();

        if (items == null) return textEntries;

        // First, flatten all details with parent references
        var allDetails = new List<CustomizationDetail>();
        foreach (var item in items)
        {
            FlattenCustomizationDetailsWithParent(item.Children, null, allDetails);
        }

        // Find all TextCustomization nodes with non-empty InputValue
        var textDetails = allDetails
            .Where(detail => detail?.Type == "TextCustomization" && !string.IsNullOrEmpty(detail?.InputValue))
            .ToList();

        foreach (var textDetail in textDetails)
        {
            // Find the parent ContainerCustomization that contains the font and color formatting
            var (fontFamily, fillColor, colorName) = FindFontAndColorForText(textDetail);

            var textEntry = new TextEntry
            {
                InputValue = textDetail.InputValue,
                FontFamily = fontFamily,
                Fill = fillColor,
                FontColorName = colorName
            };

            textEntries.Add(textEntry);
        }

        return textEntries;
    }

    private static (string? fontFamily, string? fillColor, string? colorName) FindFontAndColorForText(CustomizationDetail textDetail)
    {
        string? fontFamily = null;
        string? fillColor = null;
        string? colorName = null;

        // Traverse up to find the ContainerCustomization that contains FontCustomization and ColorCustomization
        var current = textDetail.Parent;

        while (current != null)
        {
            // Look for FontCustomization and ColorCustomization in the current node's siblings
            if (current.Parent?.Children != null)
            {
                foreach (var sibling in current.Parent.Children)
                {
                    if (sibling == null) continue;

                    // Found FontCustomization
                    if (sibling.Type == "FontCustomization" && sibling.FontSelection?.Family != null)
                    {
                        fontFamily = sibling.FontSelection.Family;
                    }

                    // Found ColorCustomization
                    if (sibling.Type == "ColorCustomization" && sibling.ColorSelection != null)
                    {
                        fillColor = sibling.ColorSelection.Value;
                        colorName = sibling.ColorSelection.Name;
                    }
                }
            }

            // If we found both font and color, we can stop
            if (fontFamily != null && fillColor != null)
            {
                break;
            }

            // If we're at a ContainerCustomization level and haven't found both, continue up
            if (current.Type == "ContainerCustomization" && (fontFamily != null || fillColor != null))
            {
                break;
            }

            current = current.Parent;
        }

        return (fontFamily, fillColor, colorName);
    }

    private static void FlattenCustomizationDetailsWithParent(
        List<CustomizationDetail>? details,
        CustomizationDetail? parent,
        List<CustomizationDetail> result)
    {
        if (details == null) return;

        foreach (var detail in details)
        {
            if (detail == null) continue;

            detail.Parent = parent;
            result.Add(detail);

            if (detail.Children != null)
            {
                FlattenCustomizationDetailsWithParent(detail.Children, detail, result);
            }
        }
    }
}