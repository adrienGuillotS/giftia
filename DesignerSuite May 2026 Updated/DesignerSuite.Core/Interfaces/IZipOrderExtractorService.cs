using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Interfaces
{
    public interface IZipOrderExtractorService
    {
        /// <summary>
        /// Returns a mapping of key (ASIN or SKU) to a list of zip file paths.
        /// </summary>
        Dictionary<string, List<string>> GetZipFilesGrouped(string folderPath);

        /// <summary>
        /// Extracts the key (ASIN or SKU) from a given zip file.
        /// </summary>
        string ExtractKeyFromZip(string zipFilePath);
    }
}
