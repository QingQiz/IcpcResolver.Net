using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IcpcResolver.Window
{
    /// <summary>
    /// Load event feed content from domjudge RestApi with given URL and ID
    /// </summary>
    public partial class CredRequest
    {
        private HttpWebResponse _response;
        private bool _processing = false;
        public string ReturnedPath { get; private set; } = "";
        public CredRequest()
        {
            InitializeComponent();
            GetButton.IsEnabled = false;
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

            if (apiAddress.Length * username.Length * password.Length == 0)
            {
                MessageBox.Show("Please input all given items.", "Event Loader", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                apiAddress += "?stream=false";
                var request = (HttpWebRequest) WebRequest.Create(apiAddress);
                var encodedId = System.Convert
                    .ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                        .GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encodedId);
                request.Accept = "application/x-ndjson";

                try
                {
                    _response = (HttpWebResponse) await request.GetResponseAsync();
                    MessageBox.Show("Authenticate successfully", "Event Loader", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    GetButton.IsEnabled = true;
                }
                catch (WebException exception)
                {
                    var exceptionResponse = (HttpWebResponse) exception.Response;
                    if (exceptionResponse != null)
                    {
                        switch (exceptionResponse.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                MessageBox.Show("The requested url not found", "Event Loader", MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                                break;
                            case HttpStatusCode.Unauthorized:
                                MessageBox.Show("Invalid authentication", "Event Loader", MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                                break;
                            default:
                                throw;
                        }
                    }
                }
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
            await using var eventStream = _response.GetResponseStream();

            var reader = new StreamReader(eventStream);
            var responseFromServer = await reader.ReadToEndAsync();

            var saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = "Event-Feed.json"
            };

            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            await File.WriteAllTextAsync(saveFileDialog.FileName, responseFromServer);

            MessageBox.Show("Event feed saved.", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Information);
            ReturnedPath = saveFileDialog.FileName;
            Close();
        }
    }
}
