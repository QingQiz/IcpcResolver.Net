using System.Collections.Generic;
using System.Threading.Tasks;
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
            _teamInfo = team;
            TeamName = _teamInfo.TeamName;

            Solved = team.Solved;
            Time = team.TimeAll;

            var cnt = 0;
            foreach (var problemViewModel in _teamInfo.ProblemsFrom)
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
        private TeamDto _teamInfo;


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


        public async Task<bool> UpdateTeamStatusAnimation(int durationBeforeHighlight, int durationBeforeUpdate)
        {
            var isUpdated = false;
            for (var i = 0; i < _teamInfo.ProblemsFrom.Count; i++)
            {
                // only update pending problems
                if (_teamInfo.ProblemsFrom[i].Status != ProblemStatus.Pending) continue;

                await _problems[i].UpdateStatusAnimation(_teamInfo.ProblemsTo[i], durationBeforeHighlight,
                    durationBeforeUpdate);
                isUpdated = true;

                _teamInfo.ProblemsFrom[i].Status = _teamInfo.ProblemsTo[i].Status;
                // for rollback
                _teamInfo.ProblemsTo[i].Status = ProblemStatus.Pending;

                // if problem is note accepted, update next
                if (!_teamInfo.ProblemsFrom[i].IsAccepted) continue;
                break;
            }
            // if problem is accepted, update solved-count and time and break
            Solved = _teamInfo.Solved;
            Time = _teamInfo.TimeAll;
            return isUpdated;
        }
    }
}