using System.Windows;
using System.Windows.Threading;

namespace IcpcResolver.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Global exception handling  
            Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowUnhandledException(e);
        }

        void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var errorMessage =
                $"An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError: {e.Exception.Message + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null)}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)";

            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Error) != MessageBoxResult.No) return;

            if (MessageBox.Show(
                    "WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?",
                    "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}