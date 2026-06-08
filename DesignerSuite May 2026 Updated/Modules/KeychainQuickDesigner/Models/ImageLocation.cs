using CsvHelper.Configuration.Attributes;
using KeychainQuickDesigner.Module.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeychainQuickDesigner.Module.Models
{
    /// <summary>
    /// Contains Image location for cdr file
    /// </summary>
    public class ImageLocation : IShapeLocation
    {
        [Name("Row Image Number")]
        public string Label { get; set; } = string.Empty; // Ensure non-null default value

        [Name("Position X (mm)")]
        public double PositionX { get; set; }

        [Name("Position Y (mm)")]
        public double PositionY { get; set; }
    }
}
