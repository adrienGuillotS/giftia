using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSQuickDesigner
{
    public class OrderDocuement
    {
        public required string DocumentName { get; set; }
        public string? Text { get; set; }
        public string? MainImage { get; set; }
        public string? SmallImage { get; set; }
        public string? FontFamily { get; set; }

        /// <summary>
        /// RGB font color hex code, '#' included
        /// </summary>
        public string? FontColorHex { get; set; }
    }
}
