using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using IcpcResolver.Net.UserControl;
using Colors = IcpcResolver.Net.AppConstants.Colors;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for Resolver.xaml
    /// </summary>
    public partial class Resolver : System.Windows.Window
    {
        public Resolver()
        {
            InitializeComponent();
            InitGrid();

            var values = Enum.GetValues(typeof(ProblemStatus));
            var random = new Random();

            ProblemDto GetProblem(int n)
            {
                var status = (ProblemStatus) (values.GetValue(random.Next(values.Length)) ?? ProblemStatus.NotTried);

                return new ProblemDto
                {
                    Label = new string(new[] {(char) ('A' + n)}),
                    Status = status,
                    Time = status == ProblemStatus.NotTried ? 0 : random.Next(1, 300),
                    Try = status == ProblemStatus.NotTried ? 0 : random.Next(1, 5)
                };
            }

            const int problemN = 13;

            var teamDtos = Enumerable
                .Range(0, 20)
                .Select(n =>
                {
                    // NOTE there must be `ToList`
                    List<ProblemDto> Problems() =>
                        Enumerable.Range(0, problemN)
                            .Select((Func<int, ProblemDto>) GetProblem)
                            .ToList();

                    return new TeamDto
                    {
                        Rank = 0,
                        Name = "Team" + n,
                        ProblemsFrom = Problems(),
                        ProblemsTo = Problems()
                    }.PostInit();
                })
                .OrderByDescending(t => t.AcceptedCount)
                .ThenBy(t => t.TimeAll)
                .ToList();

            for (var i = 0; i < teamDtos.Count; i++)
            {
                var team = new Team(teamDtos[i]);
            
                _teams.Add(team);

                if (i >= MaxTeamNumberToDisplay) continue;

                Teams.Children.Add(team);
                Grid.SetRow(team, i);
                Grid.SetColumn(team, 0);
            }
            
            UpdateTeamRank();
        }

        private const int MaxTeamNumberToDisplay = 32;

        private List<Team> _teams = new();
        private int _currentTeamIdx = 0;
        private bool _teamUpdated = true;

        /// <summary>
        /// init grid background color to (gray, black, gray, black...)
        /// </summary>
        private void InitGrid()
        {
            for (var i = 0; i <= MaxTeamNumberToDisplay; i++)
            {
                var border = new Border
                {
                    Background = (i & 1) == 0
                        ? new SolidColorBrush(Colors.FromString(Colors.BgGray))
                        : new SolidColorBrush(Colors.FromString(Colors.Black)),
                    Height = 85
                };
                
                Teams.RowDefinitions.Add(new RowDefinition());
                Teams.Children.Add(border);
                
                Grid.SetRow(border, i);
                Grid.SetColumn(border, 0);
            }
        }

        private void UpdateTeamRank()
        {
            _teams[0].TeamRank = 1;
            for (int i = 1, j = 1; i < _teams.Count; i++)
            {
                if (_teams[i].Solved != _teams[i - 1].Solved || _teams[i].Time != _teams[i - 1].Time)
                {
                    _teams[i].TeamRank = ++j;
                }
                else
                {
                    _teams[i].TeamRank = j;
                }
            }
        }

        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Handled)
            {
                // press `shift` THEN press `escape`
                case false when (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                                && e.IsDown && e.Key == Key.Escape:
                    Close();
                    break;
                case false when e.IsDown && e.Key == Key.Space:
                    if (!_teamUpdated) break;
                    _teamUpdated = false;
                    await _teams[_currentTeamIdx].UpdateTeamStatusStep();
                    _teamUpdated = true;
                    MessageBox.Show(_teamUpdated.ToString());
                    break;
            }
        }
    }
}