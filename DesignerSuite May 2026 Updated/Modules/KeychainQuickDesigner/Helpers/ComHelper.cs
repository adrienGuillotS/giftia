using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeychainQuickDesigner.Module.Helpers
{
    public static class ComHelper
    {
        /// <summary>
        /// Safely releases a COM object.
        /// </summary>
        public static void SafeRelease(object? comObject)
        {
            try
            {
                if (comObject != null && Marshal.IsComObject(comObject))
                {
                    Marshal.FinalReleaseComObject(comObject);
                }
            }
            catch
            {
                // Ignored — releasing shouldn't throw
            }
            finally
            {
                comObject = null;
            }
        }

        /// <summary>
        /// Forces garbage collection to release COM resources immediately.
        /// </summary>
        public static void ForceGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
