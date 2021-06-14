using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IcpcResolver.AppConstants;
using IcpcResolver.UserControl;
using Colors = IcpcResolver.AppConstants.Colors;

namespace IcpcResolver.Window
{
    public partial class Award
    {
        public Award(TeamDto teamInfo)
        {
            InitializeComponent();
            
            Cursor = Cursors.None;

            _teamInfo = teamInfo;
            TeamName = _teamInfo.TeamName;
            SchoolName = _teamInfo.SchoolName;

            foreach (var award in teamInfo.Awards)
            {
                var awardType = award.Split("|")[^1];
                var awardName = award[..^(awardType.Length + 1)];

                AwardsPanel.Children.Add(new Label
                {
                    Content = awardName,
                    FontSize = awardType == "normal" ? 35 : 45,
                    FontWeight = awardType == "normal" ? FontWeights.Normal: FontWeights.ExtraBold
                });
            }
            
            // school icon
            if (string.IsNullOrWhiteSpace(_teamInfo.IconPath))
            {
                TeamInfoGrid.Children.Remove(SchoolIcon);
            }
            else
            {
                // crop
                var icon = new BitmapImage(new Uri(_teamInfo.IconPath));
                var len = (int) Math.Min(icon.Height, icon.Width);

                SchoolIcon.Source = new CroppedBitmap(icon, new Int32Rect(0, 0, len, len));
            }
            
            // team photo
            if (!string.IsNullOrWhiteSpace(_teamInfo.PhotoPath))
            {
                var brush = new ImageBrush(new BitmapImage(new Uri(_teamInfo.PhotoPath)))
                {
                    Stretch = Stretch.UniformToFill
                };

                LayoutGrid.Background = brush;
            }
            else
            {
                LayoutGrid.Background = new SolidColorBrush(Colors.FromString(Colors.Black));
            }
        }

        private readonly TeamDto _teamInfo;
        
        #region Property

        public int TeamPanelHeight => (int) GetValue(TeamPanelHeightProperty);

        private static readonly DependencyProperty TeamPanelHeightProperty =
            DependencyProperty.Register("TeamPanelHeight", typeof(int), typeof(Award),
                new PropertyMetadata(AppConst.TeamGridHeight));

        private string TeamName
        {
            get => (string) GetValue(TeamNameProperty);
            init => SetValue(TeamNameProperty, value);
        }

        private static readonly DependencyProperty TeamNameProperty =
            DependencyProperty.Register("TeamName", typeof(string), typeof(Award));

        private string SchoolName
        {
            get => (string) GetValue(SchoolNameProperty);
            init => SetValue(SchoolNameProperty, value);
        }

        private static readonly DependencyProperty SchoolNameProperty =
            DependencyProperty.Register("SchoolName", typeof(string), typeof(Award));

        #endregion

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