using Corel.Interop.VGCore;
using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Interfaces;
using DesignerSuite.Core.Utilities;
using System.IO;

namespace DesignerSuite.Core.Services
{
    public class SvgImportService(Application corel) : ISvgImportService
    {
        private readonly Application _corel = corel ?? throw new ArgumentNullException(nameof(corel));
        private const string SvgIconsLayerName = "SVG Layer";
        private const string SvgFileName = "symbol.svg";
        private const string TemuSvgFileName = "temu_symbol.svg";

        public void ImportSvgIcons(Document doc, WorkingModeEnum workingMode)
        {
            var layer = doc.ActivePage.CreateLayer(SvgIconsLayerName);
            string assemblyLocation = CoreHelper.GetAppAssemblyPath();

            if (string.IsNullOrEmpty(assemblyLocation))
            {
                throw new InvalidOperationException("The assembly location could not be determined.");
            }

            var path = Path.Combine(assemblyLocation!, workingMode == WorkingModeEnum.Amazon
                ? SvgFileName : TemuSvgFileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Couldn't locate the symbol.svg file in '{Path.GetDirectoryName(path)}'");
            }

            var options = _corel.CreateStructImportOptions();
            options.MaintainLayers = false;

            var filter = layer.ImportEx(path, cdrFilter.cdrSVG, options);
            filter.Finish();

            doc.ClearSelection();
            doc.ActiveLayer.SelectableShapes.All().AddToSelection();
            doc.Selection().MoveToLayer(layer);
            layer.Shapes.All().SetPositionEx(cdrReferencePoint.cdrCenter, 100, 310);
            layer.Shapes.All().Ungroup();
            layer.Shapes.All().Ungroup();
        }

        public Shape? DuplicateSvgIcon(string iconName, double x, double y)
        {
            var icon = _corel.ActiveDocument.ActivePage.Layers
                .Cast<Layer>()
                .FirstOrDefault(l => l.Name == SvgIconsLayerName)
                ?.Shapes.Cast<Shape>()
                .FirstOrDefault(s => s.Name.Equals(iconName, StringComparison.OrdinalIgnoreCase));

            var dup = icon?.Duplicate();
            dup?.SetPositionEx(cdrReferencePoint.cdrTopMiddle, x, y);
            return dup ?? null;
        }

        public void DeleteSvgLayer(Document doc)
        {
            var layer = doc.ActivePage.Layers.Find(SvgIconsLayerName);

            layer?.Delete();

        }
    }

}
