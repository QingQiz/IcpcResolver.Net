using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

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
            MessageBox.Show("NotImplemented");
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