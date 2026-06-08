using CsvHelper.Configuration.Attributes;
using QuickDesigner2023.Module.Interfaces;
using System.Windows.Media.Media3D;


namespace QuickDesigner2023.Module.Models
{
    public class MugCsvRecord : ICsvRecord
    {
        [Name("ASIN", "SKU")]
        public required string ASIN { get; set; }

        [Name("Image Width")]
        public int ImageWidth { get; set; }

        [Name("Text Width")]
        public int TextWidth { get; set; }

        [Name("Order Number X (mm)")]
        public double OrderNumberX { get; set; }

        [Name("Order Number Y (mm)")]
        public double OrderNumberY { get; set; }

        [Name("Row Number")]
        public int RowNumber { get; set; }

        [Name("Left Preview Image X")]
        public double LeftPreviewImageX { get; set; }

        [Name("Left Preview Image Y")]
        public double LeftPreviewImageY { get; set; }

        [Name("Right Preview Image X")]
        public double RightPreviewImageX { get; set; }

        [Name("Right Preview Image Y")]
        public double RightPreviewImageY { get; set; }

        [Name("Left Snap Image X")]
        public double LeftSnapImageX { get; set; }

        [Name("Left Snap Image Y")]
        public double LeftSnapImageY { get; set; }

        [Name("Right Snap Image X")]
        public double RightSnapImageX { get; set; }

        [Name("Right Snap Image Y")]
        public double RightSnapImageY { get; set; }

        [Name("Left Text X")]
        public double LeftTextX { get; set; }

        [Name("Left Text Y")]
        public double LeftTextY { get; set; }

        [Name("Right Text X")]
        public double RightTextX { get; set; }

        [Name("Right Text Y")]
        public double RightTextY { get; set; }
    }
}
