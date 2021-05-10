using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Load event feed content from domjudge RestApi with given URL and ID
    /// </summary>
    public partial class CredRequest : System.Windows.Window
    {
        HttpWebResponse Response;
        public string ReturnedPath { get; set; } = "";
        public CredRequest()
        {
            InitializeComponent();
            this.GetButton.IsEnabled = false;
            this.AddressBox.Text = "http://192.168.0.102/domjudge/api/v4/contests/8/event-feed";
        }


        private void VerifyInfo(object sender, RoutedEventArgs e)
        {
            string ApiAddress = AddressBox.Text;
            string Username = UsernameBox.Text;
            string Password = PasswordBox.Password.ToString();
            if (ApiAddress.Length * Username.Length * Password.Length == 0) {
                string messageBoxText = "Please input all given items.";
                string caption = "Event Loader";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
            else {
                ApiAddress = ApiAddress + "?stream=false";
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(ApiAddress);
                string EncodedID = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                               .GetBytes(Username + ":" + Password));
                Request.Headers.Add("Authorization", "Basic " + EncodedID);
                Request.Accept = "application/x-ndjson";
                try {
                    this.Response = (HttpWebResponse)Request.GetResponse();
                    MessageBox.Show("Authencate successfully", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.GetButton.IsEnabled = true;
                } 
                catch (WebException exception)
                {
                    HttpWebResponse ExceptionResponse = (HttpWebResponse)exception.Response;
                    switch (ExceptionResponse.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            MessageBox.Show("The requested url not found", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        case HttpStatusCode.Unauthorized:
                            MessageBox.Show("Invalid authentication", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        default:
                            MessageBox.Show("Internal Error", "Event Loader", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            using (Stream EventStream = this.Response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(EventStream);
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
