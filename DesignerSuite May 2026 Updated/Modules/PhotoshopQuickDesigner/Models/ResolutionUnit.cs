using Photoshop;
using PSQuickDesigner.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSQuickDesigner.Models
{
    public class ResolutionUnit
    {
        public string Name { get; set; }
        public string StringValue { get; set; }
        public PsResolutions Value { get; set; }
    }
}
