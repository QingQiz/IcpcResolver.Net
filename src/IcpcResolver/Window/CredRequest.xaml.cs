using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IcpcResolver.Utils.EventFeed;

namespace IcpcResolver.Window
{
    /// <summary>
    /// Load event feed content from domjudge RestApi with given URL and ID
    /// </summary>
    public partial class CredRequest
    {
        private EventFeedRequest _eventFeedRequest;
        private bool _processing;
        public string ReturnedPath { get; private set; } = "";
        public CredRequest()
        {
            InitializeComponent();
            GetButton.IsEnabled = false;
            // default uri
            AddressBox.Text = "http://192.168.0.102/domjudge/api/v4/contests/8/event-feed";
        }

        private async Task OnProcessingWrapper(object sender, RoutedEventArgs e, Func<object, RoutedEventArgs, Task> func)
        {
            if (_processing) return;

            _processing = true;
            Cursor = Cursors.Wait;
            
            try
            {
                await func(sender, e);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                _processing = false;
            }
        }

        private async void VerifyInfoWrapper(object sender, RoutedEventArgs e)
        {
            await OnProcessingWrapper(sender, e, VerifyInfo);
        }

        private async Task VerifyInfo(object sender, RoutedEventArgs e)
        {
            var apiAddress = AddressBox.Text;
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            _eventFeedRequest = new EventFeedRequest(apiAddress, username, password);
            var res = await _eventFeedRequest.Validate();

            switch (res)
            {
                case HttpStatusCode.NotFound:
                    MessageBox.Show("The requested url not found", "Event Loader", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;
                case HttpStatusCode.Unauthorized:
                    MessageBox.Show("Invalid authentication", "Event Loader", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;
                case HttpStatusCode.OK:
                    MessageBox.Show("Authenticate successfully", "Event Loader", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    GetButton.IsEnabled = true;
                    break;
                default:
                    throw new Exception("invalid response code: " + res);
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void DownloadEventFeedWrapper(object sender, RoutedEventArgs e)
        {
            await OnProcessingWrapper(sender, e, DownloadEventFeed);
        }

        private async Task DownloadEventFeed(object sender, RoutedEventArgs e)
        {
            var response = await _eventFeedRequest.Download();

            var saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = "Event-Feed.json"
            };

            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            await File.WriteAllTextAsync(saveFileDialog.FileName, response);

            MessageBox.Show("Event feed saved.", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Information);
            ReturnedPath = saveFileDialog.FileName;
            Close();
        }
    }
}
