using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using IcpcResolver.UserControl;
using Colors = IcpcResolver.AppConstants.Colors;

namespace IcpcResolver.Window
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

            _config = resolverDto.ResolverConfig;
            _teams = resolverDto.Teams;
            _status = new ResolverStatus();

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
                Height = _config.TeamGridHeight,
                Visibility = Visibility.Hidden,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0,
                    _teams.Count < _config.MaxDisplayCount
                        ? (_teams.Count - 1) * _config.TeamGridHeight
                        : (_config.MaxDisplayCount - 1) * _config.TeamGridHeight
                    , 0, 0)
            };
            _status.CursorIdx = (int)_cursor.Margin.Top / _config.TeamGridHeight;
            Layout.Children.Insert(1, _cursor);
        }

        private void InitTeams()
        {
            // disable animations
            _status.AniStart();

            for (var i = 0; i < _config.MaxRenderCount; i++)
            {
                Teams.Children.Add(_teams[i]);
            }
            ReCalcTeamRank();

            // enable animations
            _status.AniEnd();
        }

        /// <summary>
        /// init resolver background
        /// </summary>
        private void InitResolverBackgroundGrid()
        {
            for (var i = 0; i <= _config.MaxDisplayCount + 1; i++)
            {
                var border = new Border
                {
                    Background = (i & 1) == 0
                        ? new SolidColorBrush(Colors.FromString(Colors.BgGray))
                        : new SolidColorBrush(Colors.FromString(Colors.Black)),
                    Height = _config.TeamGridHeight,
                    Opacity = 1
                };
                
                BgGrid.Children.Add(border);
            }
        }

        #endregion

        #region Variables & Help Functions

        private readonly List<Team> _teams;
        private Border _cursor;

        private readonly ResolverConfig _config;
        private readonly ResolverStatus _status;

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

        #endregion

        #region Animations

        /// <summary>
        /// move cursor up
        /// </summary>
        /// <param name="duration">animation duration in milliseconds</param>
        /// <param name="callback">animation complete callback</param>
        private void CursorUpAnimation(int duration, Func<object, EventArgs, object> callback=null)
        {
            if (!_status.AnimationDone) return;
            if (_status.CursorIdx == 0) return;

            _status.AniStart();

            var dt = new Duration(TimeSpan.FromMilliseconds(duration));
            if (_status.CursorIdx > _config.MaxDisplayCount / 2 || _status.CursorIdx == _status.CurrentTeamIdx)
            {
                // cursor up
                var ani = new ThicknessAnimation
                {
                    From = _cursor.Margin,
                    To = new Thickness(0, _cursor.Margin.Top - _config.TeamGridHeight , 0, 0),
                    Duration = dt,
                    FillBehavior = FillBehavior.HoldEnd,
                };
                ani.Completed += (a, b) =>
                {
                    _status.CursorIdx--;
                    _status.CurrentTeamIdx--;
                    _status.AniEnd();

                    callback?.Invoke(a, b);
                };

                _cursor.BeginAnimation(Border.MarginProperty, ani);
            }
            else
            {
                // not move cursor, but move Teams down
                var newTeamIdx = _status.CurrentTeamIdx - _status.CursorIdx - 1;
                // insert the new team to scroll down
                Teams.Children.Insert(0, _teams[newTeamIdx]);
                // hide new team on init
                Teams.Margin = new Thickness(0, -_config.TeamGridHeight, 0, 0);
                
                // animation
                var ani = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 0, 0),
                    To = new Thickness(0, _config.TeamGridHeight, 0, 0),
                    Duration = dt,
                    FillBehavior = FillBehavior.Stop
                };
                ani.Completed += (a, b) =>
                {
                    Teams.Margin = new Thickness(0, 0, 0, 0);
                    _teams[newTeamIdx].Margin = new Thickness(0, 0, 0, 0);
                    Teams.Children.RemoveAt(_config.MaxDisplayCount);

                    _status.CurrentTeamIdx--;
                    _status.AniEnd();

                    callback?.Invoke(a, b);
                };

                Timeline.SetDesiredFrameRate(ani, _config.AnimationFrameRate);
                _teams[newTeamIdx].BeginAnimation(MarginProperty, ani);
            }
        }

        /// <summary>
        /// scroll down
        /// </summary>
        /// <param name="duration">one team scroll up duration (milliseconds)</param>
        /// <param name="interval">time interval between animations of two row</param>
        private void ScrollDownAnimation(int duration, int interval=0)
        {
            if (_teams.Count <= _config.MaxDisplayCount)
            {
                _status.ScrollDown = true;
                return;
            }

            var d = new Duration(TimeSpan.FromMilliseconds(duration));

            _status.AniStart();
            
            var animations = new List<ThicknessAnimation>();

            var stopTeamIdx = _teams.Count - _config.MaxDisplayCount;

            // create animations
            for (var i = 0; i < stopTeamIdx; i++)
            {
                var ani = new ThicknessAnimation
                {
                    BeginTime = TimeSpan.FromMilliseconds((duration + interval) * i),
                    From = Teams.Margin,
                    To = new Thickness(0, Teams.Margin.Top - _config.TeamGridHeight, 0, 0),
                    Duration = d,
                    FillBehavior = FillBehavior.HoldEnd
                };
                Timeline.SetDesiredFrameRate(ani, _config.AnimationFrameRate);

                animations.Add(ani);
            }

            // add event handler to each animation
            animations.Last().Completed += (_, _) =>
            {
                _status.CurrentTeamIdx = _teams.Count - 1;
                _status.AniEnd();
                _status.ScrollDown = true;
            };

            for (var i = 0; i < animations.Count; i++)
            {
                var i1 = i;
                animations[i].Completed += (_, _) =>
                {
                    Teams.Children.RemoveAt(0);
                    if (i1 + _config.MaxRenderCount < _teams.Count)
                    {
                        Teams.Children.Add(_teams[i1 + _config.MaxRenderCount]);
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
        /// <param name="callback">callback when animation completed</param>
        /// <returns>
        ///     `-1`: an animation is running
        ///     `2` : no update (problem status or team rank)
        ///     `1` : problem status is updated, but team rank is not updated
        ///     `0` : team rank is updated
        /// </returns>
        private async Task<int> UpdateTeamRankAnimation(int duration, Func<object, EventArgs, object> callback=null)
        {
            if (!_status.AnimationDone) return -1;

            _status.AniStart();

            var updated = await _teams[_status.CurrentTeamIdx]
                .UpdateTeamStatusAnimation(_config.UpdateProblemStatusDuration.Item1,
                    _config.UpdateProblemStatusDuration.Item2);

            if (!updated)
            {
                _status.AniEnd();
                return 2;
            }
            
            // find the correct position of current team after update
            var newIdx = -1;
            for (var i = 0; i < _teams.Count; i++)
            {
                if (_teams[i].Solved > _teams[_status.CurrentTeamIdx].Solved) continue;

                if (_teams[i].Solved < _teams[_status.CurrentTeamIdx].Solved)
                {
                    newIdx = i;
                    break;
                }
                if (_teams[i].Time <= _teams[_status.CurrentTeamIdx].Time) continue;

                newIdx = i;
                break;
            }

            if (newIdx >= _status.CurrentTeamIdx || newIdx == -1)
            {
                _status.AniEnd();
                // rank not change, update again
                await UpdateTeamRankAnimation(duration);
                return 1;
            }

            // insert current team to correct position
            var temp = _teams[_status.CurrentTeamIdx];
            for (var i = _status.CurrentTeamIdx ; i > newIdx; --i)
            {
                _teams[i] = _teams[i - 1];
            }

            _teams[newIdx] = temp;
            
            // re-calc team rank
            ReCalcTeamRank();

            var targetMt = (_status.CursorIdx - _status.CurrentTeamIdx + newIdx) * _config.TeamGridHeight;
            if (targetMt < 0)
            {
                targetMt = -_config.TeamGridHeight;
            }

            var dt = new Duration(TimeSpan.FromMilliseconds(duration));

            // animation (move current team to correct position)
            // 1. move current team from Teams to Layout (just like the cursor)
            // 1.1 remove current team from Teams
            Teams.Children.RemoveAt(_status.CursorIdx);
            // 1.2 add current team to Layout
            Layout.Children.Add(_teams[newIdx]);
            // 1.3 adjust margin
            _teams[newIdx].Margin = _cursor.Margin;
            // 1.4 adjust the margin of the team below old position
            if (_status.CurrentTeamIdx != _teams.Count - 1)
            {
                _teams[_status.CurrentTeamIdx + 1].Margin = new Thickness(0, _config.TeamGridHeight, 0, 0);
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
            aniUp.Completed += (a, b) =>
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
                    Teams.Children.Insert(targetMt / _config.TeamGridHeight, _teams[newIdx]);
                }

                _status.AniEnd();

                callback?.Invoke(a, b);
            };
            Timeline.SetDesiredFrameRate(aniUp, _config.AnimationFrameRate);
            
            // animation (move teams below target position down)
            // 1. create animation to move team down
            var aniDown = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(0, _config.TeamGridHeight, 0, 0),
                Duration = dt,
                FillBehavior = FillBehavior.Stop
            };
            Timeline.SetDesiredFrameRate(aniDown, _config.AnimationFrameRate);
            // 2. add new team if needed
            var nextTeamIdx = newIdx + 1;
            if (targetMt < 0)
            {
                nextTeamIdx = _status.CurrentTeamIdx - _status.CursorIdx;
                if (nextTeamIdx == newIdx) nextTeamIdx = newIdx + 1;

                // 2.1 insert new team to the top of Teams
                Teams.Children.Insert(0, _teams[nextTeamIdx]);
                // 2.2 adjust the margin of Teams to hide the new team inserted
                Teams.Margin = new Thickness(0, -_config.TeamGridHeight, 0, 0);
                // 2.3 add completed event handler to change Teams margin back
                aniDown.Completed += (_, _) =>
                {
                    Teams.Margin = new Thickness(0, 0, 0, 0);
                };
            }
            
            // animation (adjust the margin of the team below old position back)
            ThicknessAnimation aniAdjBack = null;
            if (_status.CurrentTeamIdx != _teams.Count - 1)
            {
                aniAdjBack = new ThicknessAnimation
                {
                    From = _teams[_status.CurrentTeamIdx + 1].Margin,
                    To = new Thickness(0, 0, 0, 0),
                    Duration = dt,
                    // NOTE: do NOT use FillBehavior.HoldEnd, it will prevent the next adjustment of Margin
                    FillBehavior = FillBehavior.Stop
                };
                aniAdjBack.Completed += (_, _) =>
                {
                    _teams[_status.CurrentTeamIdx + 1].Margin = new Thickness(0, 0, 0, 0);
                };
                Timeline.SetDesiredFrameRate(aniDown, _config.AnimationFrameRate);
            }
            
            // START ANIMATION
            if (_status.CurrentTeamIdx != _teams.Count - 1)
            {
                _teams[_status.CurrentTeamIdx + 1].BeginAnimation(MarginProperty, aniAdjBack);
            }

            _teams[nextTeamIdx].BeginAnimation(MarginProperty, aniDown);
            _teams[newIdx].BeginAnimation(MarginProperty, aniUp);

            return 0;
        }

        /// <summary>
        /// run animation a step
        /// </summary>
        private async Task RunAnimationStep()
        {
            if (!_status.AnimationDone) return;

            // 1. scroll down
            if (!_status.ScrollDown)
            {
                ScrollDownAnimation(_config.ScrollDownDuration, _config.ScrollDownInterval);
                return;
            }

            // 2. show cursor
            if (!_cursor.IsVisible)
            {
                _cursor.Visibility = Visibility.Visible;
                return;
            }

            // 3 begin scroll up
            // 3.1 show award if needed
            if (_status.ShouldShowAward)
            {
                _status.ShouldShowAward = false;
                var awardWindow = new Award(_teams[_status.CurrentTeamIdx].TeamInfo);
                awardWindow.ShowDialog();
                return;
            }

            // help functions
            var aniCallback = new Func<object>(() =>
            {
                if (_teams[_status.CurrentTeamIdx].TeamRank > _config.AutoUpdateTeamStatusUntilRank)
                {
                    // auto run animation until ^
                    // NOTE: should NOT call by await or invoke .Wait()
#pragma warning disable 4014
                    RunAnimationStep();
#pragma warning restore 4014
                }

                return null;
            });

            // 3.2 cursor up if needed
            if (_status.ShouldCursorUp)
            {
                _status.ShouldCursorUp = false;
                CursorUpAnimation(_config.CursorUpDuration, (_, _) => aniCallback());
                return;
            }

            // 3.3 update team rank
            var res = await UpdateTeamRankAnimation(_config.UpdateTeamRankDuration, (_, _) => aniCallback());

            if (res != 1 && res != 2) return;

            if (_teams[_status.CurrentTeamIdx].Awards.Any())
            {
                if (res == 2)
                {
                    _status.ShouldCursorUp = true;

                    // no update and not run animation automatically, show award window directly
                    // if we don't show window directly, the SPACE will need to be pressed twice to show award window
                    // if we don't check the second condition, the award window will be shown automatically
                    if (_teams[_status.CurrentTeamIdx].TeamRank <= _config.AutoUpdateTeamStatusUntilRank)
                    {
                        var awardWindow = new Award(_teams[_status.CurrentTeamIdx].TeamInfo);
                        awardWindow.ShowDialog();
                    }
                    else
                    {
                        _status.ShouldShowAward = true;
                    }
                }
                else
                {
                    _status.ShouldCursorUp = true;
                    _status.ShouldShowAward = true;
                }
                return;
            }

            // cursor up if the team rank is not updated
            CursorUpAnimation(_config.CursorUpDuration, (_, _) => aniCallback());
        }

        #endregion

        #region KeyHandler
        
        /// <summary>
        /// keyboard handler
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e.Handled) return;

            // left-shift + Esc to close window
            if (Keyboard.IsKeyDown(Key.LeftShift) && e.IsDown && e.Key == Key.Escape)
            {
                Close();
                return;
            }

            // space to run animation a step
            if (e.IsDown && e.Key == Key.Space)
            {
                await RunAnimationStep();
            }
        }

        #endregion
    }
}