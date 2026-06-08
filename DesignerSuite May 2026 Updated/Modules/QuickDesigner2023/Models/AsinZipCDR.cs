
using QuickDesigner2023.Module.Enums;
using QuickDesigner2023.Module.Interfaces;

namespace QuickDesigner2023.Module.Models
{
    public class AsinZipCDR
    {
        public required string ASIN { get; set; }
        public AsinType AsinType { get; set; }
        public IEnumerable<ICsvRecord>? CsvRecords { get; set; }
        public required List<string> ZipFiles { get; set; }
    }
}