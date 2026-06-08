using DesignerSuite.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Interfaces
{
    public interface IImageDataExtractorService
    {
        /// <summary>
        /// Extracts image data from a given source zip file and returns parsed data.
        /// </summary>
        /// <param name="zipFilePath">Path to the zip file.</param>
        /// <param name="destinationFolderPath">Path to extract images and metadata files.</param>
        /// <returns>Parsed image data object.</returns>
        IOrderDataItem ExtractImages(string zipFilePath, string destinationFolderPath);
    }

}
