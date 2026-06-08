using System.Configuration;
using System.Data;
using System.Windows;

namespace DesignerSuite.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                string logDir = AppDomain.CurrentDomain.BaseDirectory; // Working directory
                string logFile = System.IO.Path.Combine(logDir, "error_log.txt");

                string logMessage = $@"====================================================
Date/Time: {DateTime.Now}
 Exception Type: {e.Exception.GetType().FullName}
 Message: {e.Exception.Message}
 Source: {e.Exception.Source}
 TargetSite: {e.Exception.TargetSite}
 StackTrace:
{e.Exception.StackTrace}
";

                // Write inner exceptions if any
                if (e.Exception.InnerException != null)
                {
                    logMessage += $@"
 --- Inner Exception ---
 Type: {e.Exception.InnerException.GetType().FullName}
 Message: {e.Exception.InnerException.Message}
 StackTrace:
{e.Exception.InnerException.StackTrace}
";
                }

                System.IO.File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // fallback – avoid crash if logging fails
            }

            // Show user-friendly message
            MessageBox.Show("An unexpected error occurred:\n\n" + e.Exception.Message +
                            "\n\nDetails have been saved to error_log.txt.",
                            "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Prevents application from crashing
            e.Handled = true;
        }

    }

}
