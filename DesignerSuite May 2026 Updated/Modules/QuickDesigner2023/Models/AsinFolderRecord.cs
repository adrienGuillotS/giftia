using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDesigner2023.Module.Models
{
    public class AsinFolderRecord
    {
        [Name("App Title")]
        public required string AppTitle { get; set; }

        [Name("Folder Name")]
        public required string FolderName { get; set; }

        [Name("Asins (semicolon separated)", "Skus (semicolon separated)")]
        public string? AsinsRaw { get; set; }

        [Ignore]
        public List<string> Asins => AsinsRaw?
            .Split(';')
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList() ?? new List<string>();
    }
}
