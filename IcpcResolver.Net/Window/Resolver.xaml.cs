using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using IcpcResolver.Net.UserControl;

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

            const int problemN = 16;

            var teamDtos = Enumerable
                .Range(0, MaxTeamNumberToDisplay)
                .Select(n =>
                {
                    // NOTE there must be `ToList`
                    var problems = Enumerable
                        .Range(0, problemN).Select((Func<int, ProblemDto>) GetProblem)
                        .ToList();
                    return new TeamDto
                    {
                        Rank = 0,
                        Name = "Team" + n,
                        Problems = problems
                    };
                })
                .OrderByDescending(t => t.AcceptedCount)
                .ThenBy(t => t.ScoreAll)
                .ToList();

            for (var i = 0; i < Math.Min(MaxTeamNumberToDisplay, teamDtos.Count); i++)
            {
                var team = new Team(teamDtos[i]);
            
                Teams.Children.Add(team);
                _teams.Add(team);
            
                Grid.SetRow(team, i);
                Grid.SetColumn(team, 0);
            }
        }

        private const int MaxTeamNumberToDisplay = 13;

        private List<Team> _teams = new();

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
                        ? new SolidColorBrush(Color.FromRgb(0x3c, 0x3c, 0x3c))
                        : new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Height = 84
                };
                
                Teams.RowDefinitions.Add(new RowDefinition());
                Teams.Children.Add(border);
                
                Grid.SetRow(border, i);
                Grid.SetColumn(border, 0);
            }
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