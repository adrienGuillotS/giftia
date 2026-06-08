using Corel.Interop.VGCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Utilities.Corel
{
    public static class CorelExtension
    {
        /// <summary>
        /// Active optimization features, and start a command group
        /// </summary>
        /// <param name="app"></param>
        /// <param name="commandGroup"></param>
        /// <param name="optimization"></param>
        /// <param name="enableEvents"></param>
        /// <param name="preservSeletion"></param>
        public static void BeginDraw(this Application app, bool commandGroup = true, bool optimization = true, bool enableEvents = false, bool preservSeletion = true)
        {
            if (app.ActiveDocument != null)
            {
                if (commandGroup)
                    app.ActiveDocument.BeginCommandGroup();
                if (preservSeletion)
                    app.ActiveDocument.PreserveSelection = preservSeletion;
            }

            app.Optimization = optimization;
            app.EventsEnabled = enableEvents;
        }
        /// <summary>
        /// Desables optimization features, close the command group and reflesh UI
        /// </summary>
        /// <param name="app"></param>
        public static void EndDraw(this Application app)
        {
            if (app.ActiveDocument != null)
            {
                app.ActiveDocument.EndCommandGroup();
                app.ActiveDocument.PreserveSelection = false;
            }
            app.Optimization = false;
            app.EventsEnabled = true;
            app.Refresh();
        }
    }
}
