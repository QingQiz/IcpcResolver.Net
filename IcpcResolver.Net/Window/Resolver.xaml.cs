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
            var problems = new[]
            {
                new ProblemDto
                {
                    Label = "A",
                    Status = ProblemStatus.Accept,
                    Time = 233,
                    Try = 1
                },
                new ProblemDto
                {
                    Label = "B",
                    Status = ProblemStatus.UnAccept,
                    Time = 233,
                    Try = 2
                },
                new ProblemDto
                {
                    Label = "C",
                    Status = ProblemStatus.NotTried,
                    Time = 0,
                    Try = 0
                },
                new ProblemDto
                {
                    Label = "D",
                    Status = ProblemStatus.Pending,
                    Time = 2,
                    Try = 299
                },
                new ProblemDto
                {
                    Label = "E",
                    Status = ProblemStatus.FirstBlood,
                    Time = 1,
                    Try = 12 
                }
            };
                
            Panel.Children.Add(new Team(new TeamDto
            {
                Rank = 1,
                Name = "Team1",
                Problems = problems
            }));
            Panel.Children.Add(new Team(new TeamDto
            {
                Rank = 2,
                Name = "Team2",
                Problems = problems
            }));
            Panel.Children.Add(new Team(new TeamDto
            {
                Rank = 3,
                Name = "Team3",
                Problems = problems
            }));
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