using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDesigner2025
{
    using CsvHelper.Configuration.Attributes;

    public class ImageDataCSV
    {
        [Name("Size")]
        public string SizeCaption { get; set; }

        [Name("ASIN", "SKU")]
        public string ASIN { get; set; }

        [Name("Page Width (cm)")]
        public double PageWidth { get; set; }

        [Name("Page Height (cm)")]
        public double PageHeight { get; set; }

        [Name("Order Number X (mm)")]
        public double OrderNumberX { get; set; }

        [Name("Order Number Y (mm)")]
        public double OrderNumberY { get; set; }

        [Name("Actual Image Width (mm)")]
        public double ActualImageWidth { get; set; }

        [Name("Actual Image Height (mm)")]
        public double ActualImageHeight { get; set; }

        [Name("Text Width (mm)")]
        public double TextWidth { get; set; }

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
