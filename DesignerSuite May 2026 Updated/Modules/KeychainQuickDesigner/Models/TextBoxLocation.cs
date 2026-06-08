using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeychainQuickDesigner.Module.Models
{
    public class TextBoxLocation : IShapeLocation
    {
        [Name("Position Number")]
        public string Label { get; set; } = string.Empty; // Ensure non-null default value

        [Name("Position X (mm)")]
        public double PositionX { get; set; }

        [Name("Position Y (mm)")]
        public double PositionY { get; set; }
    }
}
