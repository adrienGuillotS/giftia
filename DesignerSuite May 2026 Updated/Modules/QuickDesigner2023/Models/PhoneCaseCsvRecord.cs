using CsvHelper.Configuration.Attributes;
using QuickDesigner2023.Module.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDesigner2023.Module.Models
{
    public class PhoneCaseCsvRecord : ICsvRecord
    {
        [Name("ASIN", "SKU")]
        public required string ASIN { get; set; }

        [Name("Row Number")]
        public int RowNumber { get; set; }

        [Name("Order Number X (mm)")]
        public double OrderNumberX { get; set; }

        [Name("Order Number Y (mm)")]
        public double OrderNumberY { get; set; }

        [Name("Image X")]
        public double ImageX { get; set; }

        [Name("Image Y")]
        public double ImageY { get; set; }

        [Name("Preview Image X")]
        public double PreviewImageX { get; set; }

        [Name("Preview Image Y")]
        public double PreviewImageY { get; set; }

        [Name("Main Image Height")]
        public double MainImageHeight { get; set; }

        [Name("Preview Image Width")]
        public double PreviewImageWidth { get; set; }

        [Name("Text Width")]
        public double TextWidth { get; set; }

        [Name("Model")]
        public string? ModelName { get; init; }

        [Name("Model Text X")]
        public double ModelTextX { get; set; }

        [Name("Model Text Y")]
        public double ModelTextY { get; set; }

    }
}
