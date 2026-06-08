using DesignerSuite.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Utilities
{
    public static class CoreHelper
    {
        public static string GetAppAssemblyPath()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(location))
            {
                location = AppContext.BaseDirectory;
            }
            string path = Path.GetDirectoryName(location);
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Unable to determine the application assembly path.");
            }
            return path;
        }

        public static string GetWorkingModeString(WorkingModeEnum workingMode)
        {
            return workingMode == WorkingModeEnum.Amazon ? "Amazon" : "Temu";
        }

        public static void EmptyTempDirectory(string tempDirectoryPath)
        {
            if (!Directory.Exists(tempDirectoryPath)) return;
            DirectoryInfo di = new(tempDirectoryPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
    }
}
