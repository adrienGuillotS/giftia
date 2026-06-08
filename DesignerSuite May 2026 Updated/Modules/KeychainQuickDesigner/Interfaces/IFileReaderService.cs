using DesignerSuite.Core.Models;
using KeychainQuickDesigner.Module.Models;

namespace KeychainQuickDesigner.ModuleDocker.Interfaces
{
    public interface IFileReaderService
    {
        IOrderDataItem ReadDataFromFile(string xmlFilePath);
        Task<IOrderDataItem> ReadDataFromFileAsync(string xmlFilePath);
    }
}
