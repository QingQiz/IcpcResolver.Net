using System.Windows.Input;
using IcpcResolver.Net.UserControl;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var problems = new[]
            {
                new ProblemViewModel
                {
                    Label = "A",
                    Status = ProblemStatus.Accept,
                    Time = 233,
                    Try = 1
                },
                new ProblemViewModel
                {
                    Label = "B",
                    Status = ProblemStatus.UnAccept,
                    Time = 233,
                    Try = 2
                },
                new ProblemViewModel
                {
                    Label = "C",
                    Status = ProblemStatus.NotTried,
                    Time = 0,
                    Try = 0
                },
                new ProblemViewModel
                {
                    Label = "D",
                    Status = ProblemStatus.Pending,
                    Time = 2,
                    Try = 299
                },
                new ProblemViewModel
                {
                    Label = "E",
                    Status = ProblemStatus.FirstBlood,
                    Time = 1,
                    Try = 12 
                }
            };
                
            Panel.Children.Add(new Team(new TeamViewModel
            {
                Rank = 1,
                Name = "Team1",
                Problems = problems
            }));
            Panel.Children.Add(new Team(new TeamViewModel
            {
                Rank = 2,
                Name = "Team2",
                Problems = problems
            }));
            Panel.Children.Add(new Team(new TeamViewModel
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