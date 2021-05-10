using Microsoft.Win32;
using System.Diagnostics;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for Loader.xaml
    /// </summary>
    public partial class Loader : System.Windows.Window
    {

        public Loader()
        {
            InitializeComponent();
        }

        private void OpenCredWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            CredRequest ReqWindow = new CredRequest();
            ReqWindow.Owner = this;
            ReqWindow.ShowInTaskbar = false;
            ReqWindow.ShowDialog();
            string LoadEventFeedPath = ReqWindow.ReturnedPath;
            if (LoadEventFeedPath.Length != 0)
            {
                this.EventFeedFilePath.Text = LoadEventFeedPath;
                this.ValidateFile.IsEnabled = true;
            }
        }

        private void OpenLoadEventFileWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Event Feed JSON file (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                this.EventFeedFilePath.Text = openFileDialog.FileName;
                this.ValidateFile.IsEnabled = true;
            }
        }
    }
}
