using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSQuickDesigner
{
    internal class PixelConverter
    {
        // Convert pixels per centimeter to pixels per inch
        public static double PixelsPerCmToPixelsPerInch(double pixelsPerCm)
        {
            const double cmPerInch = 2.54;
            return pixelsPerCm / cmPerInch;
        }

        // Convert pixels per inch to pixels per centimeter
        public static double PixelsPerInchToPixelsPerCm(double pixelsPerInch)
        {
            const double cmPerInch = 2.54;
            return pixelsPerInch * cmPerInch;
        }
    }
}
