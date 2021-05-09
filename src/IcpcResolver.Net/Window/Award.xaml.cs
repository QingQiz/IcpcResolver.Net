using System.Windows.Input;
using IcpcResolver.Net.UserControl;

namespace IcpcResolver.Net.Window
{
    public partial class Award : System.Windows.Window
    {
        public Award(TeamDto teamInfo)
        {
            InitializeComponent();
            
            Cursor = Cursors.None;

            var awards = teamInfo.Awards;
        }

        /// <summary>
        /// keyboard handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e.Handled) return;

            // space to close window
            if (e.IsDown && e.Key == Key.Space)
            {
                Close();
            }
        }
    }
}