using QuickDesigner2023.Module.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDesigner2023.Module.Models
{
    public class DefaultNonCustomizedRecord : ICsvRecord
    {
        public int RowNumber { get; set; }
        public double PositionY { get; set; }
    }
}
