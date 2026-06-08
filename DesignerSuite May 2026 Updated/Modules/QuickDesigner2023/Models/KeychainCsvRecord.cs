using CsvHelper.Configuration.Attributes;
using QuickDesigner2023.Module.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDesigner2023.Module.Models
{
    public class KeychainCsvRecord : ICsvRecord
    {
        [Name("ASIN", "SKU")]
        public required string ASIN { get; set; }

        [Name("Row Number")]
        public int RowNumber { get; set; }

        [Name("Order Number X (mm)")]
        public double OrderNumberX { get; set; }

        [Name("Order Number Y (mm)")]
        public double OrderNumberY { get; set; }

        [Name("Text Width (mm)")]
        public double TextShapeMaxWidth { get; set; }

        [Name("Text Shape Max Height (mm)")]
        public double TextIconGroupMaxHeight { get; set; }

        [Name("Text X (mm)")]
        public double TextX { get; set; }

        [Name("Text Y (mm)")]
        public double TextY { get; set; }

        [Name("Actual Image Height (mm)")]
        public double ActualImageHeight { get; set; }

        [Name("Actual Image X (mm)")]
        public double ActualImageX { get; set; }

        [Name("Actual Image Y (mm)")]
        public double ActualImageY { get; set; }

        [Name("Preview Image Width (mm)")]
        public double PreviewImageWidth { get; set; }

        [Name("Preview Image X (mm)")]
        public double PreviewImageX { get; set; }

        [Name("Preview Image Y (mm)")]
        public double PreviewImageY { get; set; }
    }
}
