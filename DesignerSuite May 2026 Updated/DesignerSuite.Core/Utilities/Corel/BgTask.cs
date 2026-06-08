using System;
using Corel.Interop.VGCore;

namespace DesignerSuite.Core.Utilities.Corel
{
    public class BgTask : ICUIRunningTask, ICUIBackgroundTask
    {
        public event Action RunningChange;
        private Application corelApp;
        private bool running = false;
        public bool Running
        {
            get { return running; }
            private set { running = value; if (RunningChange != null) RunningChange(); }
        }

        public string Name
        {
            get
            {
                return "Save & Close Document Task";
            }
        }

        private readonly Document _document;
        private readonly string destinationFolderPath;

        public BgTask(Application app, Document document, string destinationFolderPath)
        {
            this.corelApp = app;
            _document = document;
            this.destinationFolderPath = destinationFolderPath;
        }

        public void FinalizeTask()
        {
            Running = false;
        }

        public void FreeTask()
        {
            Running = false;
        }

        public void QuitTask()
        {
            Running = false;
        }

        public void TryAbort()
        {
            Running = false;
        }
        public void RunTask()
        {
            Running = true;
           // CreateGuideLines(_document);
            SaveCloseDocument(_document, destinationFolderPath);
            Running = false;
        }
       

        private void SaveCloseDocument(Document document, string destinationFolderPath)
        {
            if (document == null) { throw new NullReferenceException("Document cannot be null."); }
            string fileName = destinationFolderPath + "\\" + document?.Name + ".cdr";
            document?.SaveAs(fileName);
            document?.Close();
        }
    }
}
