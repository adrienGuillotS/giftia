using PSQuickDesigner.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSQuickDesigner.Models
{
    public class FillColor
    {
        public string Name { get; set; }
        public string StringValue { get; set; }
        public InitialFillEnum Fill { get; set; }
    }
}
