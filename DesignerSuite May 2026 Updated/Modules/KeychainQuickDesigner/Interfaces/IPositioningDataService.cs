using KeychainQuickDesigner.Module.Models;

namespace KeychainQuickDesigner.Module.Interfaces
{
    public interface IPositioningDataService
    {
        /// <summary>
        /// Loads the order data location from the csv file for the cdr file
        /// </summary>
        /// <returns></returns>
        List<OrderDataCSV> LoadOrderDataLocationForCdrFile();

        /// <summary>
        /// Loads image location data from the csv file
        /// </summary>
        /// <returns></returns>
        List<ImageLocation> LoadImageLocationForCdrFile();

        List<TextBoxLocation> LoadTextBoxLocationForSvgFile();
    }
}
