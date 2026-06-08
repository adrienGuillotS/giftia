using Photoshop;
using PSQuickDesigner.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSQuickDesigner.Converters
{
    public class UnitConverter
    {
        public static double ConvertCentimeters(float centimeters, PsUnits unit,
             PsResolutions psResolutions, float resolution = 96f)
        {
            // Conversion factors
            const float inchesPerCentimeter = 1f / 2.54f;
            const float millimetersPerCentimeter = 10f;
            const float pointsPerCentimeter = 28.3465f; // Standard printing point size
            const float picasPerCentimeter = 2.3622f; // Standard printing pica size


            // Convert to centimeters based on the input unit
            switch (unit)
            {
                case PsUnits.psPixels:
                    //return centimeters / pixelsPerInch * inchesToCentimeters;
                    return psResolutions == PsResolutions.psPixelsPerCentimeter ?
                        ConvertCmToPixelsPPC(centimeters, resolution) : ConvertCmToPixelsPPI(centimeters, resolution);
                case PsUnits.psInches:
                    return centimeters * inchesPerCentimeter;
                case PsUnits.psCM:
                    return centimeters;
                case PsUnits.psMM:
                    return centimeters * millimetersPerCentimeter;
                case PsUnits.psPoints:
                    return centimeters * pointsPerCentimeter;
                case PsUnits.psPicas:
                    return centimeters * picasPerCentimeter; // 1 pica = 12 points
                default:
                    throw new ArgumentException("Invalid unit specified.");
            }
        }
        static int ConvertCmToPixelsPPI(float centimeters, float resolutionPPI)
        {
            const float centimetersToInches = 1f / 2.54f;

            // Convert centimeters to inches
            float inches = centimeters * centimetersToInches;

            // Calculate pixels based on PPI
            int pixels = (int)(inches * resolutionPPI);
            return pixels;
        }
        static int ConvertCmToPixelsPPC(float centimeters, float resolutionPPC)
        {
            // Calculate pixels based on PPC
            int pixels = (int)(centimeters * resolutionPPC);
            return pixels;
        }


    }
}
