
using Corel.Interop.VGCore;
using DesignerSuite.Core.Enums;

namespace DesignerSuite.Core.Interfaces
{
    public interface ISvgImportService
    {
        void ImportSvgIcons(Document doc, WorkingModeEnum workingMode);
        Shape? DuplicateSvgIcon(string iconName, double x, double y);
        void DeleteSvgLayer(Document doc);
    }

}