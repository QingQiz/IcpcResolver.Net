using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IcpcResolver.Net.AppConstants;
using IcpcResolver.Net.UserControl;

namespace IcpcResolver.Net.Window
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