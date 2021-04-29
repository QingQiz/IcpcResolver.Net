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
        #region Init

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
                        TeamRank = 0,
                        TeamName = "Team" + n,
                        ProblemsFrom = Problems(),
                        ProblemsTo = Problems()
                    }.PostInit();
                })
                .OrderByDescending(t => t.Solved)
                .ThenBy(t => t.TimeAll)
                .ToList();
            
        }

        public Resolver()
        {
            InitializeComponent();
            _teamDtos = DataGenerator(12, 30);
            InitResolverBackgroundGrid();
        }

        private void WindowRendered(object sender, EventArgs e)
        {
            InitTeams();
            InitCursor();
        }

        private void InitCursor()
        {
            // init cursor
            _cursor = new Border
            {
                Background = new SolidColorBrush(Colors.FromString(Colors.Blue)),
                Height = AppConst.TeamGridHeight,
                Visibility = Visibility.Hidden,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0,
                    _teams.Count < AppConst.MaxDisplayCount
                        ? (_teams.Count - 1) * AppConst.TeamGridHeight
                        : (AppConst.MaxDisplayCount - 1) * AppConst.TeamGridHeight
                    , 0, 0)
            };
            Layout.Children.Insert(1, _cursor);
        }

        private void InitTeams()
        {
            // disable animations
            _animationDone = false;

            for (var i = 0; i < AppConst.MaxRenderCount; i++)
            {
                _teams.Add(new Team(_teamDtos[i])
                {
                    Height = AppConst.TeamGridHeight,
                });
                Teams.Children.Add(_teams[i]);
            }

            for (var i = AppConst.MaxRenderCount; i < _teamDtos.Count; i++)
            {
                _teams.Add(new Team(_teamDtos[i])
                {
                    Height = AppConst.TeamGridHeight
                });
            }
            ReCalcTeamRank();

            // enable animations
            _animationDone = true;
        }

        /// <summary>
        /// init resolver background
        /// </summary>
        private void InitResolverBackgroundGrid()
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

        }

        #endregion

        private readonly List<Team> _teams = new();
        private readonly List<TeamDto> _teamDtos;
        private Border _cursor;

        private bool _animationDone = true;
        private bool _scrollDown;
        private int _currentTeamIdx;


        #region Animations

        private void ReCalcTeamRank()
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
                FillBehavior = FillBehavior.HoldEnd,
            };
            ani.Completed += (_, _) =>
            {
                _animationDone = true;
            };

            _cursor.BeginAnimation(Border.MarginProperty, ani);
        }

        /// <summary>
        /// scroll down
        /// </summary>
        /// <param name="duration">one team scroll up duration (milliseconds)</param>
        /// <param name="durationAdjust">adjust time span between animations of two row</param>
        private void ScrollDown(int duration, int durationAdjust=0)
        {
            if (_teams.Count <= AppConst.MaxDisplayCount)
            {
                _scrollDown = true;
                return;
            }

            var d = new Duration(TimeSpan.FromMilliseconds(duration));

            _animationDone = false;
            
            var animations = new List<ThicknessAnimation>();

            var stopTeamIdx = _teams.Count - AppConst.MaxDisplayCount;

            // create animations
            for (var i = 0; i < stopTeamIdx; i++)
            {
                var ani = new ThicknessAnimation
                {
                    BeginTime = TimeSpan.FromMilliseconds((duration - durationAdjust) * i),
                    From = Teams.Margin,
                    To = new Thickness(0, Teams.Margin.Top - AppConst.TeamGridHeight, 0, 0),
                    Duration = d,
                    FillBehavior = FillBehavior.HoldEnd
                };
                Timeline.SetDesiredFrameRate(ani, 120);

                animations.Add(ani);
            }

            // add event handler to each animation
            animations.Last().Completed += (_, _) =>
            {
                var teamIdx = animations.Count - 1;

                if (teamIdx + AppConst.MaxRenderCount < _teams.Count)
                {
                    Teams.Children.Add(_teams[teamIdx + AppConst.MaxRenderCount]);
                }
                // NOTE: no need to remove

                _currentTeamIdx = _teams.Count - 1;
                _animationDone = true;
                _scrollDown = true;
            };

            for (var i = 0; i < animations.Count; i++)
            {
                var i1 = i;
                animations[i].Completed += (_, _) =>
                {
                    Teams.Children.RemoveAt(0);
                    if (i1 + AppConst.MaxRenderCount < _teams.Count)
                    {
                        Teams.Children.Add(_teams[i1 + AppConst.MaxRenderCount]);
                    }
                };
            }

            for (var i = 0; i < animations.Count; i++)
            {
                _teams[i].BeginAnimation(MarginProperty, animations[i]);
            }
        }
        
        #endregion


        #region KeyHandler
        
        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Handled)
            {
                // press `shift` THEN press `escape` close window
                case false when (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                                && e.IsDown && e.Key == Key.Escape:
                    Close();
                    break;
                // break when animation is running
                case false when !_animationDone:
                    break;
                // key down and key is `space` and `scrolled down`
                case false when e.IsDown && e.Key == Key.Space && _scrollDown:
                    // show cursor first
                    if (_cursor.Visibility == Visibility.Hidden)
                    {
                        _cursor.Visibility = Visibility.Visible;
                        break;
                    }
                    // TODO move cursor up automatically
                    _animationDone = false;
                    await _teams[_currentTeamIdx].UpdateTeamStatusStep();
                    _animationDone = true;
                    break;
                // key down and key is `space` and not `scrolled down`
                case false when e.IsDown && e.Key == Key.Space && !_scrollDown:
                    ScrollDown(30);
                    break;
            }
        }

        #endregion
    }
}