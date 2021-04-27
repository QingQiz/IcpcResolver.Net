using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using IcpcResolver.Net.UserControl;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Resolver : System.Windows.Window
    {
        public Resolver()
        {
            InitializeComponent();

            var values = Enum.GetValues(typeof(ProblemStatus));
            var random = new Random();

            ProblemDto GetProblem(int n) => new()
            {
                Label = new string(new[] {(char) ('A' + n)}),
                Status = (ProblemStatus) (values.GetValue(random.Next(values.Length)) ?? ProblemStatus.NotTried),
                Time = random.Next(1, 300), Try = random.Next(1, 5)
            };

            const int problemN = 16;

            var teams = Enumerable.Range(0, MaxTeamNumberToDisplay)
                .Select(n => new TeamDto
                {
                    Rank = n,
                    Name = "Team" + n,
                    Problems = Enumerable.Range(0, problemN).Select((Func<int, ProblemDto>) GetProblem)
                });

            var cnt = 0;
            foreach (var t in teams)
            {
                var team = new Team(t);

                Teams.RowDefinitions.Add(new RowDefinition());
                Teams.Children.Add(team);
                
                Grid.SetRow(team, cnt++);
                Grid.SetColumn(team, 0);
            }
        }

        public const int MaxTeamNumberToDisplay = 12;

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