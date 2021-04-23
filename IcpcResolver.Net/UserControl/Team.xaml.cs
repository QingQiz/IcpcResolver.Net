using System;
using System.Windows;
using System.Windows.Controls;

namespace IcpcResolver.Net.UserControl
{
    public partial class Team : System.Windows.Controls.UserControl
    {
        public Team()
        {
            InitializeComponent();
        }

        public int TeamRank
        {
            get => (int) GetValue(TeamRankProperty);
            set => SetValue(TeamRankProperty, value);
        }

        public static readonly DependencyProperty TeamRankProperty =
            DependencyProperty.Register("TeamRank", typeof(int), typeof(Team));

        public string TeamName
        {
            get => (string) GetValue(TeamNameProperty);
            set => SetValue(TeamNameProperty, value);
        }

        public static readonly DependencyProperty TeamNameProperty =
            DependencyProperty.Register("TeamName", typeof(string), typeof(Team));

    }
}