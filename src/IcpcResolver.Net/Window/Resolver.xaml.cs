using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using IcpcResolver.Net.UserControl;
using Colors = IcpcResolver.Net.AppConstants.Colors;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for Resolver.xaml
    /// </summary>
    public partial class Resolver : System.Windows.Window
    {
        #region Init

        public Resolver(ResolverDto resolverDto)
        {
            InitializeComponent();

            Config = resolverDto.ResolverConfig;
            _teams = resolverDto.Teams;

            InitTeams();
            InitCursor();
            InitResolverBackgroundGrid();
        }

        private void InitCursor()
        {
            // hide mouse
            Cursor = Cursors.None;
            // init cursor
            _cursor = new Border
            {
                Background = new SolidColorBrush(Colors.FromString(Colors.Blue)),
                Height = Config.TeamGridHeight,
                Visibility = Visibility.Hidden,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0,
                    _teams.Count < Config.MaxDisplayCount
                        ? (_teams.Count - 1) * Config.TeamGridHeight
                        : (Config.MaxDisplayCount - 1) * Config.TeamGridHeight
                    , 0, 0)
            };
            _cursorIdx = (int)_cursor.Margin.Top / Config.TeamGridHeight;
            Layout.Children.Insert(1, _cursor);
        }

        private void InitTeams()
        {
            // disable animations
            _animationDone = false;

            for (var i = 0; i < Config.MaxRenderCount; i++)
            {
                Teams.Children.Add(_teams[i]);
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
            for (var i = 0; i <= Config.MaxDisplayCount + 1; i++)
            {
                var border = new Border
                {
                    Background = (i & 1) == 0
                        ? new SolidColorBrush(Colors.FromString(Colors.BgGray))
                        : new SolidColorBrush(Colors.FromString(Colors.Black)),
                    Height = Config.TeamGridHeight,
                    Opacity = 1
                };
                
                BgGrid.Children.Add(border);
            }
        }

        #endregion

        private readonly List<Team> _teams;
        private Border _cursor;
        private int _cursorIdx;

        private bool _animationDone = true;
        private bool _scrollDown;
        private int _currentTeamIdx;

        private ResolverConfig Config;

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

        #region Animations

        /// <summary>
        /// move cursor up
        /// </summary>
        /// <param name="duration">animation duration in milliseconds</param>
        private void CursorUpAnimation(int duration)
        {
            if (!_animationDone) return;
            if (_cursorIdx == 0) return;

            _animationDone = false;

            var dt = new Duration(TimeSpan.FromMilliseconds(duration));
            if (_cursorIdx > Config.MaxDisplayCount / 2 || _cursorIdx == _currentTeamIdx)
            {
                // cursor up
                var ani = new ThicknessAnimation
                {
                    From = _cursor.Margin,
                    To = new Thickness(0, _cursor.Margin.Top - Config.TeamGridHeight , 0, 0),
                    Duration = dt,
                    FillBehavior = FillBehavior.HoldEnd,
                };
                ani.Completed += (_, _) =>
                {
                    _cursorIdx--;
                    _currentTeamIdx--;
                    _animationDone = true;
                };

                _cursor.BeginAnimation(Border.MarginProperty, ani);
            }
            else
            {
                // not move cursor, but move Teams down
                var newTeamIdx = _currentTeamIdx - _cursorIdx - 1;
                // insert the new team to scroll down
                Teams.Children.Insert(0, _teams[newTeamIdx]);
                // hide new team on init
                Teams.Margin = new Thickness(0, -Config.TeamGridHeight, 0, 0);
                
                // animation
                var ani = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 0, 0),
                    To = new Thickness(0, Config.TeamGridHeight, 0, 0),
                    Duration = dt,
                    FillBehavior = FillBehavior.Stop
                };
                ani.Completed += (_, _) =>
                {
                    Teams.Margin = new Thickness(0, 0, 0, 0);
                    _teams[newTeamIdx].Margin = new Thickness(0, 0, 0, 0);
                    Teams.Children.RemoveAt(Config.MaxDisplayCount);

                    _currentTeamIdx--;
                    _animationDone = true;
                };

                Timeline.SetDesiredFrameRate(ani, Config.AnimationFrameRate);
                _teams[newTeamIdx].BeginAnimation(MarginProperty, ani);
            }
        }

        /// <summary>
        /// scroll down
        /// </summary>
        /// <param name="duration">one team scroll up duration (milliseconds)</param>
        /// <param name="durationAdjust">adjust time span between animations of two row</param>
        private void ScrollDownAnimation(int duration, int durationAdjust=0)
        {
            if (_teams.Count <= Config.MaxDisplayCount)
            {
                _scrollDown = true;
                return;
            }

            var d = new Duration(TimeSpan.FromMilliseconds(duration));

            _animationDone = false;
            
            var animations = new List<ThicknessAnimation>();

            var stopTeamIdx = _teams.Count - Config.MaxDisplayCount;

            // create animations
            for (var i = 0; i < stopTeamIdx; i++)
            {
                var ani = new ThicknessAnimation
                {
                    BeginTime = TimeSpan.FromMilliseconds((duration + durationAdjust) * i),
                    From = Teams.Margin,
                    To = new Thickness(0, Teams.Margin.Top - Config.TeamGridHeight, 0, 0),
                    Duration = d,
                    FillBehavior = FillBehavior.HoldEnd
                };
                Timeline.SetDesiredFrameRate(ani, Config.AnimationFrameRate);

                animations.Add(ani);
            }

            // add event handler to each animation
            animations.Last().Completed += (_, _) =>
            {
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
                    if (i1 + Config.MaxRenderCount < _teams.Count)
                    {
                        Teams.Children.Add(_teams[i1 + Config.MaxRenderCount]);
                    }
                };
            }

            for (var i = 0; i < animations.Count; i++)
            {
                _teams[i].BeginAnimation(MarginProperty, animations[i]);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration">one step duration (milliseconds)</param>
        /// <returns>
        ///     `-1`: an animation is running
        ///     `1` : team can not be updated
        ///     `0` : update success
        /// </returns>
        private async Task<int> UpdateTeamRankAnimation(int duration)
        {
            if (!_animationDone) return -1;

            _animationDone = false;

            var updated = await _teams[_currentTeamIdx]
                .UpdateTeamStatusAnimation(Config.UpdateProblemStatusDuration.Item1,
                    Config.UpdateProblemStatusDuration.Item2);

            if (!updated)
            {
                _animationDone = true;
                return 1;
            }
            
            // find the correct position of current team after update
            var newIdx = -1;
            for (var i = 0; i < _teams.Count; i++)
            {
                if (_teams[i].Solved > _teams[_currentTeamIdx].Solved) continue;

                if (_teams[i].Solved < _teams[_currentTeamIdx].Solved)
                {
                    newIdx = i;
                    break;
                }
                if (_teams[i].Time <= _teams[_currentTeamIdx].Time) continue;

                newIdx = i;
                break;
            }

            if (newIdx >= _currentTeamIdx || newIdx == -1)
            {
                _animationDone = true;
                // rank not change, update again
                return await UpdateTeamRankAnimation(duration);
            }

            // insert current team to correct position
            var temp = _teams[_currentTeamIdx];
            for (var i = _currentTeamIdx ; i > newIdx; --i)
            {
                _teams[i] = _teams[i - 1];
            }

            _teams[newIdx] = temp;
            
            // re-calc team rank
            ReCalcTeamRank();

            var targetMt = (_cursorIdx - _currentTeamIdx + newIdx) * Config.TeamGridHeight;
            if (targetMt < 0)
            {
                targetMt = -Config.TeamGridHeight;
            }

            var dt = new Duration(TimeSpan.FromMilliseconds(duration));

            // animation (move current team to correct position)
            // 1. move current team from Teams to Layout (just like the cursor)
            // 1.1 remove current team from Teams
            Teams.Children.RemoveAt(_cursorIdx);
            // 1.2 add current team to Layout
            Layout.Children.Add(_teams[newIdx]);
            // 1.3 adjust margin
            _teams[newIdx].Margin = _cursor.Margin;
            // 1.4 adjust the margin of the team below old position
            if (_currentTeamIdx != _teams.Count - 1)
            {
                _teams[_currentTeamIdx + 1].Margin = new Thickness(0, Config.TeamGridHeight, 0, 0);
            }
            // 2. create animation to move current team to correct position
            // 2.1 create ThicknessAnimation, make current.Marget to target margin
            var aniUp = new ThicknessAnimation
            {
                From = _teams[newIdx].Margin,
                To = new Thickness(0, targetMt, 0, 0),
                Duration = dt,
                FillBehavior = FillBehavior.Stop
            };
            // 2.2 move back current team to Teams
            aniUp.Completed += (_, _) =>
            {
                // 2.2.0 change margin back
                _teams[newIdx].Margin = new Thickness(0, 0, 0, 0);
                if (targetMt < 0)
                {
                    // 2.2 current team move out of window, remove it
                    Layout.Children.Remove(_teams[newIdx]);
                }
                else
                {
                    // 2.2 move current team back to Teams
                    // 2.2.1 remove current team from Layout
                    Layout.Children.Remove(_teams[newIdx]);
                    // 2.2.2 insert current team to Teams
                    Teams.Children.Insert(targetMt / Config.TeamGridHeight, _teams[newIdx]);
                }

                _animationDone = true;
            };
            Timeline.SetDesiredFrameRate(aniUp, Config.AnimationFrameRate);
            
            // animation (move teams below target position down)
            // 1. create animation to move team down
            var aniDown = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(0, Config.TeamGridHeight, 0, 0),
                Duration = dt,
                FillBehavior = FillBehavior.Stop
            };
            Timeline.SetDesiredFrameRate(aniDown, Config.AnimationFrameRate);
            // 2. add new team if needed
            var nextTeamIdx = newIdx + 1;
            if (targetMt < 0)
            {
                nextTeamIdx = _currentTeamIdx - _cursorIdx;
                if (nextTeamIdx == newIdx) nextTeamIdx = newIdx + 1;

                // 2.1 insert new team to the top of Teams
                Teams.Children.Insert(0, _teams[nextTeamIdx]);
                // 2.2 adjust the margin of Teams to hide the new team inserted
                Teams.Margin = new Thickness(0, -Config.TeamGridHeight, 0, 0);
                // 2.3 add completed event handler to change Teams margin back
                aniDown.Completed += (_, _) =>
                {
                    Teams.Margin = new Thickness(0, 0, 0, 0);
                };
            }
            
            // animation (adjust the margin of the team below old position back)
            ThicknessAnimation aniAdjBack = null;
            if (_currentTeamIdx != _teams.Count - 1)
            {
                aniAdjBack = new ThicknessAnimation
                {
                    From = _teams[_currentTeamIdx + 1].Margin,
                    To = new Thickness(0, 0, 0, 0),
                    Duration = dt,
                    // NOTE: do NOT use FillBehavior.HoldEnd, it will prevent the next adjustment of Margin
                    FillBehavior = FillBehavior.Stop
                };
                aniAdjBack.Completed += (_, _) =>
                {
                    _teams[_currentTeamIdx + 1].Margin = new Thickness(0, 0, 0, 0);
                };
                Timeline.SetDesiredFrameRate(aniDown, Config.AnimationFrameRate);
            }
            
            // START ANIMATION
            if (_currentTeamIdx != _teams.Count - 1)
            {
                _teams[_currentTeamIdx + 1].BeginAnimation(MarginProperty, aniAdjBack);
            }

            _teams[nextTeamIdx].BeginAnimation(MarginProperty, aniDown);
            _teams[newIdx].BeginAnimation(MarginProperty, aniUp);

            return 0;
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

                    switch (await UpdateTeamRankAnimation(Config.UpdateTeamRankDuration))
                    {
                        // no up and no down
                        case 1:
                            CursorUpAnimation(Config.CursorUpDuration);
                            break;
                        // 1 up and 1 down
                        case 0:
                        // no action
                        case -1:
                            break;
                    }
                    break;
                // key down and key is `space` and not `scrolled down`
                case false when e.IsDown && e.Key == Key.Space && !_scrollDown:
                    ScrollDownAnimation(Config.ScrollDownDuration, Config.ScrollDownDurationAdjust);
                    break;
            }
        }

        #endregion
    }
}