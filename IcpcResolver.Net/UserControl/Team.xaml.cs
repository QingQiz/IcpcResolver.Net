using System.Windows;
using System.Windows.Controls;

namespace IcpcResolver.Net.UserControl
{
    public partial class Team : System.Windows.Controls.UserControl
    {
        private Team()
        {
            InitializeComponent();
        }

        public Team(TeamDto team) : this()
        {
            TeamRank = team.Rank;
            TeamName = team.Name;

            var cnt = 0;
            foreach (var problemViewModel in team.Problems)
            {
                Problems.ColumnDefinitions.Add(new ColumnDefinition());
                
                var problem = new Problem(problemViewModel);
                Problems.Children.Add(problem);

                Grid.SetRow(problem, 0);
                Grid.SetColumn(problem, cnt++);
            }
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