using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Corel.Interop.VGCore;

namespace DesignerSuite.Core.Utilities.Corel
{

    public class CorelAppManager
    {

        [DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(
          [MarshalAs(UnmanagedType.LPWStr)] string progID,
          nint reserved,
          [MarshalAs(UnmanagedType.IUnknown)] out object obj);

        private Application corelApp;
        private readonly object lockObj = new object();
        private const int MaxRetries = 3;

        public CorelAppManager()
        {
            ConnectToRunningOrCreateNew();
        }

        public Application App
        {
            get
            {
                if (!IsCorelAppAlive())
                    Reconnect();
                return corelApp;
            }
        }

        /// <summary>
        /// Safely executes a CorelDRAW operation with retry logic.
        /// Automatically reconnects and retries if COM errors occur.
        /// </summary>
        public T ExecuteWithRetry<T>(Func<Application, T> corelAction, string operationName = "")
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    if (!IsCorelAppAlive())
                        Reconnect();

                    return corelAction(App);
                }
                catch (COMException ex)
                {
                    Debug.WriteLine($"⚠️ [CorelAppManager] COM error during '{operationName}' (attempt {attempt}/{MaxRetries}): {ex.Message}");
                    Reconnect();
                    Thread.Sleep(1000); // small pause before retry
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ [CorelAppManager] Exception in '{operationName}': {ex.Message}");
                    throw;
                }
            }

            throw new InvalidOperationException($"Failed '{operationName}' after {MaxRetries} retries.");
        }

        public void ExecuteWithRetry(Action<Application> corelAction, string operationName = "")
            => ExecuteWithRetry(app => { corelAction(app); return true; }, operationName);

        private bool IsCorelAppAlive()
        {
            try
            {
                if (corelApp == null)
                    return false;

                _ = corelApp.Version; // lightweight COM check
                return true;
            }
            catch { return false; }
        }

        private void Reconnect()
        {
            lock (lockObj)
            {
                try
                {
                    if (corelApp != null)
                    {
                        try { Marshal.ReleaseComObject(corelApp); } catch { }
                        corelApp = null;
                    }

                    ConnectToRunningOrCreateNew();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CorelAppManager] Reconnect failed: {ex.Message}");
                    throw;
                }
            }
        }

        private void ConnectToRunningOrCreateNew()
        {
            try
            {
                GetActiveObject("CorelDRAW.Application.19", nint.Zero, out object obj);
                corelApp = ((Application)obj).Application;
                Debug.WriteLine("✅ Connected to running CorelDRAW instance.");
            }
            catch (COMException)
            {
                Debug.WriteLine("⚠️ No running CorelDRAW found. Starting new instance...");
                try
                {
                    corelApp = new Application().Application;
                    corelApp.Visible = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to start CorelDRAW: {ex.Message}");
                }
            }
        }

        public void QuitCorel(bool forceClose = false)
        {
            try
            {
                if (corelApp != null)
                {
                    if (forceClose)
                        corelApp.Quit();

                    Marshal.ReleaseComObject(corelApp);
                    corelApp = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CorelAppManager] Quit failed: {ex.Message}");
            }
        }
    }

}