using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Models
{
    public abstract class IOrderDataItem
    {
        public string? ImageName { get; set; }

        public string? InputValue { get; set; }
        public string? FontFamily { get; set; }
        public string? ColorValue { get; set; }
        public string? SvgImageName { get; set; }
        public string? SnapshotImageName { get; set; }
    }
}
