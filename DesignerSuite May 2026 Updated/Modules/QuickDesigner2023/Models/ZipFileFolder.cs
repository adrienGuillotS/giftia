using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDesigner2023.Module.Models
{
    public class ZipFileFolder
    {
        public string ZipFilePath { get; set; }

        /// <summary>
        /// Name of the folder to which the zip file will be moved.
        /// </summary>
        public string FolderName { get; set; }
    }
}
