using Corel.Interop.VGCore;
using System;
using System.Runtime.InteropServices;

namespace DesignerSuite.Core.Utilities.Corel
{
    public class CorelConnector
    {
        //[DllImport("oleaut32.dll", PreserveSig = false)]
        //private static extern void GetActiveObject(
        //   [MarshalAs(UnmanagedType.LPWStr)] string progID,
        //   nint reserved,
        //   [MarshalAs(UnmanagedType.IUnknown)] out object obj);

        public Application? CorelApp { get; private set; }

        private const string ProgId2019 = "CorelDRAW.Application.21";
        private const string ProgId2026 = "CorelDRAW.Application.27";
        private const string DefaultProgId = "CorelDRAW.Application";

        public bool ConnectToCorel()
        {
            try
            {
                // 1. Try attach to running instances (preferred)
                //var app = TryGetRunningInstance(ProgId2019)
                //       ?? TryGetRunningInstance(ProgId2026);
                dynamic? app = null;
                try
                {
                    app = new Application().Application;
                }
                catch (Exception)
                {
                    app = CreateInstance(DefaultProgId);
                }
                // 3. Final assignment (safe cast)
                CorelApp = app as Application;

                if (CorelApp != null)
                {
                    CorelApp.Visible = true;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Corel connection failed: {ex.Message}");
                return false;
            }
        }



        private dynamic? CreateInstance(string progId)
        {
            try
            {
                Type? t = Type.GetTypeFromProgID(progId);
                if (t == null) return null;

                var app = Activator.CreateInstance(t);
                return app;
            }
            catch
            {
                return null;
            }
        }
    }
}