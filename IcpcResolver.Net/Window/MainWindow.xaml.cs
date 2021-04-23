using System.Windows.Input;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // press `shift` THEN press `escape`
            if (!e.Handled
                && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                && e.IsDown
                && e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}