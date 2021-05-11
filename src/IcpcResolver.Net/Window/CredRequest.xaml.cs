using System.Net;
using System.Text;
using System.IO;
using System.Windows;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Load event feed content from domjudge RestApi with given URL and ID
    /// </summary>
    public partial class CredRequest : System.Windows.Window
    {
        HttpWebResponse _response;
        public string ReturnedPath { get; set; } = "";
        public CredRequest()
        {
            InitializeComponent();
            this.GetButton.IsEnabled = false;
            this.AddressBox.Text = "http://192.168.0.102/domjudge/api/v4/contests/8/event-feed";
        }


        private void VerifyInfo(object sender, RoutedEventArgs e)
        {
            string apiAddress = AddressBox.Text;
            string username = UsernameBox.Text;
            string password = PasswordBox.Password.ToString();
            if (apiAddress.Length * username.Length * password.Length == 0) {
                MessageBox.Show("Please input all given items.", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else {
                apiAddress = apiAddress + "?stream=false";
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(apiAddress);
                string encodedId = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                               .GetBytes(username + ":" + password));
                Request.Headers.Add("Authorization", "Basic " + encodedId);
                Request.Accept = "application/x-ndjson";
                try {
                    this._response = (HttpWebResponse)Request.GetResponse();
                    MessageBox.Show("Authenticate successfully", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.GetButton.IsEnabled = true;
                } 
                catch (WebException exception)
                {
                    HttpWebResponse exceptionResponse = (HttpWebResponse)exception.Response;
                    if (exceptionResponse != null)
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
                                MessageBox.Show("Internal Error", "Event Loader", MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                                break;
                        }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GetButton_Click(object sender, RoutedEventArgs e)
        {
            using (Stream eventStream = this._response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(eventStream);
                string responseFromServer = reader.ReadToEnd();
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "JSON file (*.json)|*.json";
                saveFileDialog.FileName = "Event-Feed.json";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, responseFromServer);
                    MessageBox.Show("Event feed saved.", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.ReturnedPath = saveFileDialog.FileName;
                    this.Close();
                }
            }
        }
    }
}
