using System.IO;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace IcpcResolver.Window
{
    public partial class Loader 
    {
        public Loader()
        {
            InitializeComponent();
        }

        private void GenerateConfig_OnClick(object sender, RoutedEventArgs args)
        {
            var configGenerator = new ResolverConfigWindow();
            configGenerator.Show();
            Close();
        }

        private void LoadConfig_OnClick(object sender, RoutedEventArgs args)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = "ResolverConfig.json"
            };

            // ReSharper disable once InvertIf
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var configFn = openFileDialog.FileName;
                var config = JsonConvert.DeserializeObject<ResolverConfig>(File.ReadAllText(configFn));

                var configWindow = new ResolverConfigWindow(config);
                configWindow.Show();
                Close();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e.Handled) return;

            // Esc to close window
            if (e.IsDown && e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}