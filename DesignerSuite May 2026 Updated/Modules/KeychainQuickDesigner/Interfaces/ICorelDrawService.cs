using Corel.Interop.VGCore;
using KeychainQuickDesigner.Module.Enums;

namespace KeychainQuickDesigner.Module.Interfaces
{
    public interface ICorelDrawService
    {
        Application CorelApp { get; }
        Document CreateNewDocument(string name, double pageWidth = 210, double pageHeight = 297);
        Layer CreateMasterLayer(Document doc);
        Shape ImportImage(Layer layer, string path);
        Shape ResizeShape(Shape shape, double size, bool isWidth = false);
        void AlignShapes(Shape a, Shape b);
        void SaveDocument(Document doc, string destinationFolder, DocumentExportTypeEnum format = DocumentExportTypeEnum.Cdr);
        List<string> ExportDocumentsToTemporaryPdfs(string cdrFilesDirectoryPath, string tempFolderPath, IProgress<int> progress);
        void Refresh();
        public void FitPageToContent(Document doc);
    }
}