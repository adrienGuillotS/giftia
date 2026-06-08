using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeychainQuickDesigner.Module.Models
{
    public interface IShapeLocation
    {
        string Label { get; set; }

        double PositionX { get; set; }

        double PositionY { get; set; }
    }
}
