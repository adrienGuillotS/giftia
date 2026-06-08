using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DesignerSuite.App.Navigation
{

    public class NavigationServiceEx
    {
        public event NavigatedEventHandler Navigated;

        public event NavigationFailedEventHandler NavigationFailed;

        private Frame _frame;

        public Frame Frame
        {
            get
            {
                if (this._frame == null)
                {
                    this._frame = new Frame() { NavigationUIVisibility = NavigationUIVisibility.Hidden };
                    this.RegisterFrameEvents();
                }

                return this._frame;
            }
            set
            {
                this.UnregisterFrameEvents();
                this._frame = value;
                this.RegisterFrameEvents();
            }
        }

        public bool CanGoBack => this.Frame.CanGoBack;

        public bool CanGoForward => this.Frame.CanGoForward;

        public void GoBack() => this.Frame.GoBack();

        public void GoForward() => this.Frame.GoForward();

        public bool Navigate(Uri sourcePageUri, object extraData = null)
        {
            if (this.Frame.CurrentSource != sourcePageUri)
            {
                return this.Frame.Navigate(sourcePageUri, extraData);
            }

            return false;
        }

        public bool Navigate(Type sourceType)
        {
            try
            {
                if (this.Frame.NavigationService?.Content?.GetType() != sourceType)
                {
                    var pageInstance = Activator.CreateInstance(sourceType);
                    return _frame.Navigate(pageInstance);
                }

               
            }
            catch (Exception ex)
            {
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NavigationErrors.log");

                using (var sw = new StreamWriter(logFilePath, true))
                {
                    sw.WriteLine("====================================================");
                    sw.WriteLine($"Date/Time: {DateTime.Now}");
                    sw.WriteLine($"SourceType: {sourceType.FullName}");

                    // Walk through all inner exceptions
                    Exception current = ex;
                    int level = 0;
                    while (current != null)
                    {
                        sw.WriteLine($"--- Inner Level {level} ---");
                        sw.WriteLine($"Type: {current.GetType()}");
                        sw.WriteLine($"Message: {current.Message}");
                        sw.WriteLine($"StackTrace: {current.StackTrace}");
                        current = current.InnerException;
                        level++;
                    }
                    sw.WriteLine("====================================================");
                }

                throw; // rethrow so DispatcherUnhandledException can show message
            }

            return false;
        }

        private void RegisterFrameEvents()
        {
            if (this._frame != null)
            {
                this._frame.Navigated += this.Frame_Navigated;
                this._frame.NavigationFailed += this.Frame_NavigationFailed;
            }
        }

        private void UnregisterFrameEvents()
        {
            if (this._frame != null)
            {
                this._frame.Navigated -= this.Frame_Navigated;
                this._frame.NavigationFailed -= this.Frame_NavigationFailed;
            }
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) => this.NavigationFailed?.Invoke(sender, e);

        private void Frame_Navigated(object sender, NavigationEventArgs e) => this.Navigated?.Invoke(sender, e);
    }

}
