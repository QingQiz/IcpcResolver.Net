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

        #region Animations

        /// <summary>
        /// move cursor up
        /// </summary>
        /// <param name="duration">animation duration in milliseconds</param>
        private void CursorUpAnimation(int duration)
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
                ani.Completed += (_, _) =>
                {
                    _status.CursorIdx--;
                    _status.CurrentTeamIdx--;
                    _status.AniEnd();
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
                ani.Completed += (_, _) =>
                {
                    Teams.Margin = new Thickness(0, 0, 0, 0);
                    _teams[newTeamIdx].Margin = new Thickness(0, 0, 0, 0);
                    Teams.Children.RemoveAt(_config.MaxDisplayCount);

                    _status.CurrentTeamIdx--;
                    _status.AniEnd();
                };

                Timeline.SetDesiredFrameRate(ani, _config.AnimationFrameRate);
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
                    BeginTime = TimeSpan.FromMilliseconds((duration + durationAdjust) * i),
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
        /// <returns>
        ///     `-1`: an animation is running
        ///     `1` : team can not be updated
        ///     `0` : update success
        /// </returns>
        private async Task<int> UpdateTeamRankAnimation(int duration)
        {
            if (!_status.AnimationDone) return -1;

            _status.AniStart();

            var updated = await _teams[_status.CurrentTeamIdx]
                .UpdateTeamStatusAnimation(_config.UpdateProblemStatusDuration.Item1,
                    _config.UpdateProblemStatusDuration.Item2);

            if (!updated)
            {
                _status.AniEnd();
                return 1;
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
                return await UpdateTeamRankAnimation(duration);
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
                    Teams.Children.Insert(targetMt / _config.TeamGridHeight, _teams[newIdx]);
                }

                _status.AniEnd();
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
        
        #endregion


        #region KeyHandler
        
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

        private async Task RunAnimationStep()
        {
            if (!_status.AnimationDone) return;

            if (!_status.ScrollDown)
            {
                ScrollDownAnimation(_config.ScrollDownDuration, _config.ScrollDownDurationAdjust);
                return;
            }

            if (!_cursor.IsVisible)
            {
                _cursor.Visibility = Visibility.Visible;
                return;
            }
            
            // TODO update team rank automatically if the team is not awarded.
            // TODO show award window when the team is awarded.
            switch (await UpdateTeamRankAnimation(_config.UpdateTeamRankDuration))
            {
                // no up and no down
                case 1:
                    CursorUpAnimation(_config.CursorUpDuration);
                    break;
                // 1 up and 1 down
                case 0:
                // no action
                case -1:
                    break;
            }
        }

        #endregion
    }
}