using System.Collections.Generic;
using System.Linq;
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

            Solved = team.Problems.Count(p => p.IsAccepted());
            Time = team.ScoreAll;

            var cnt = 0;
            foreach (var problemViewModel in team.Problems)
            {
                Problems.ColumnDefinitions.Add(new ColumnDefinition());
                
                var problem = new Problem(problemViewModel);
                Problems.Children.Add(problem);
                _problems.Add(problem);

                Grid.SetRow(problem, 0);
                Grid.SetColumn(problem, cnt++);
            }
        }

        private List<Problem> _problems = new();

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

        public int Time
        {
            get => (int) GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        private static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(int), typeof(Team));

        public int Solved
        {
            get => (int) GetValue(SolvedProperty);
            set => SetValue(SolvedProperty, value);
        }

        private static readonly DependencyProperty SolvedProperty =
            DependencyProperty.Register("Solved", typeof(int), typeof(Team));

    }
}