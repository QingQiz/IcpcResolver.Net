using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using IcpcResolver.Net.UserControl;
using IcpcResolver.Net.AppConstants;
using Colors = IcpcResolver.Net.AppConstants.Colors;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for Resolver.xaml
    /// </summary>
    public partial class Resolver : System.Windows.Window
    {
        private static List<TeamDto> DataGenerator(int problemN, int teamN)
        {
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

            return Enumerable
                .Range(0, teamN)
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
            
        }

        public Resolver()
        {
            InitializeComponent();

            var teamDtos = DataGenerator(13, 15);
            
            for (var i = 0; i < teamDtos.Count; i++)
            {
                var team = new Team(teamDtos[i])
                {
                    Height = AppConst.TeamGridHeight
                };

                _teams.Add(team);

                if (i >= AppConst.MaxDisplayCount) continue;

                Teams.Children.Add(team);
            }
            
            InitGrid();
            UpdateTeamRank();
        }

        private List<Team> _teams = new();
        private Border _cursor;

        private int _currentTeamIdx = 0;
        private bool _animationDone = true;
        private bool _scrollDown = false;

        /// <summary>
        /// init resolver background
        /// </summary>
        private void InitGrid()
        {
            for (var i = 0; i <= AppConst.MaxDisplayCount; i++)
            {
                var border = new Border
                {
                    Background = (i & 1) == 0
                        ? new SolidColorBrush(Colors.FromString(Colors.BgGray))
                        : new SolidColorBrush(Colors.FromString(Colors.Black)),
                    Height = AppConst.TeamGridHeight,
                    Opacity = 1
                };
                
                BgGrid.Children.Add(border);
            }

            // init cursor
            _cursor = new Border
            {
                Background = new SolidColorBrush(Colors.FromString(Colors.Blue)),
                Height = AppConst.TeamGridHeight,
                Visibility = Visibility.Hidden,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, _teams.Count < AppConst.MaxDisplayCount
                    ? (AppConst.MaxDisplayCount - _teams.Count + 1) * AppConst.TeamGridHeight
                    : AppConst.TeamGridHeight)
            };
            Layout.Children.Insert(1, _cursor);
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

        /// <summary>
        /// move cursor up
        /// </summary>
        /// <param name="duration">animation duration in milliseconds</param>
        private void CursorUp(int duration)
        {
            _animationDone = false;

            var ani = new ThicknessAnimation
            {
                From = _cursor.Margin,
                To = new Thickness(0, 0 , 0, _cursor.Margin.Bottom + AppConst.TeamGridHeight),
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                FillBehavior = FillBehavior.HoldEnd
            };
            ani.Completed += (_, _) =>
            {
                _animationDone = true;
            };

            _cursor.BeginAnimation(Border.MarginProperty, ani);
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
                case false when _cursor.Visibility == Visibility.Hidden:
                    _cursor.Visibility = Visibility.Visible;
                    break;
                case false when e.IsDown && e.Key == Key.Space:
                    if (!_animationDone) break;

                    if (_scrollDown)
                    {
                        _animationDone = false;
                        // TODO update team rank
                        await _teams[_currentTeamIdx].UpdateTeamStatusStep();
                        _animationDone = true;
                    }
                    else
                    {
                        CursorUp(1000);
                        //_scrollDown = true;
                    }
                    break;
            }
        }
    }
}