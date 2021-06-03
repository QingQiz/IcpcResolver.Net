using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IcpcResolver.UserControl;
using IcpcResolver.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace IcpcResolver.Window
{
    /// <summary>
    /// Interaction logic for ResolverConfig.xaml
    /// </summary>
    public partial class ResolverConfigWindow
    {
        private Validator _validator;
        private bool _processing, _loaded;
        private readonly ResolverConfig _config = new();

        public ResolverConfigWindow()
        {
            InitializeComponent();
        }

        #region EventHandler

        #region EventFeed

        private void OpenCredWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var reqWindow = new CredRequest
            {
                Owner = this, ShowInTaskbar = false
            };

            reqWindow.ShowDialog();

            var loadEventFeedPath = reqWindow.ReturnedPath;
            if (loadEventFeedPath.Length == 0) return;

            EventFeedFilePath.Text = loadEventFeedPath;
            ValidateFile.IsEnabled = true;
        }

        private void OpenEventFeed_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Event Feed JSON file (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() != true) return;
            EventFeedFilePath.Text = openFileDialog.FileName;
            // init validator
            ValidateFile.IsEnabled = true;
            _validator = new Validator(EventFeedFilePath.Text);
        }

        private async void ValidateEventFeed_OnClick(object sender, RoutedEventArgs e)
        {
            if (_processing) return;

            _processing = true;
            Cursor = Cursors.Wait;

            try
            {
                LoadCheck.IsChecked =
                    SubmissionCheck.IsChecked = TeamInfoCheck.IsChecked = UnjudgedCheck.IsChecked = false;

                _validator.ReturnSummaryList = new List<ReturnSummary>();

                // parse event-feed async
                await Task.Run(() => _validator.LoadAllEventData());
                LoadCheck.IsChecked = true;

                if (_validator.CheckTeamInfo())
                    TeamInfoCheck.IsChecked = true;

                if (_validator.CheckSubmissionInfo())
                    SubmissionCheck.IsChecked = true;

                if (_validator.CheckUnjudgedRuns())
                    UnjudgedCheck.IsChecked = true;

                // create and show validator summary info
                var summaryInfo = "";

                foreach (var summary in _validator.ReturnSummaryList.Where(summary => summary.HasError))
                {
                    summaryInfo += summary.ErrType;
                    summaryInfo += string.Join(",", summary.ErrList) + "\n\n";
                }

                if (!string.IsNullOrEmpty(summaryInfo))
                {
                    MessageBox.Show(summaryInfo + "You can automatically drop these item of fix it manually.",
                        "Event validator", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AutoFixButton.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("All info validate successfully.", "Event validator", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    _loaded = true;
                    LoadContestInfo();
                    CalculateAwards_OnClick(null, null);
                    SaveButton.IsEnabled = true;
                }
            }
            finally
            {
                Cursor = Cursors.Arrow;
                _processing = false;
            }
        }

        private void AutoFixEventFeed_OnClick(object sender, RoutedEventArgs e)
        {
            // Fix invalid teams
            if (TeamInfoCheck.IsChecked == false)
            {
                _validator.RemoveInvalidTeams();
            }

            // Fix invalid submissions (Wrong info and unjudged)
            if (SubmissionCheck.IsChecked == false || UnjudgedCheck.IsChecked == false)
            {
                _validator.RemoveInvalidSubmissions();
            }

            // re-check
            if (_validator.CheckTeamInfo())
                TeamInfoCheck.IsChecked = true;

            if (_validator.CheckSubmissionInfo())
                SubmissionCheck.IsChecked = true;

            _validator.ReturnSummaryList = new List<ReturnSummary>();
            if (_validator.CheckUnjudgedRuns())
                UnjudgedCheck.IsChecked = true;

            if (TeamInfoCheck.IsChecked is true && SubmissionCheck.IsChecked is true && UnjudgedCheck.IsChecked is true)
            {
                MessageBox.Show("All info fixed and validated successfully.", "Event validator", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                AutoFixButton.IsEnabled = false;
                _loaded = true;
                LoadContestInfo();
                CalculateAwards_OnClick(null, null);
                SaveButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Problem unable to fix automatically, please fix it manually.", "Event validator",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        private void SelectPhotoFolder_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Implemented");
        }

        private void SelectSchoolFolder_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Implemented");
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var res = JsonConvert.SerializeObject(_config);
            var saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = "ResolverConfig.json"
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, res);
            }
        }

        private async void Run_OnClick(object sender, RoutedEventArgs e)
        {
            if (_processing) return;

            _processing = true;
            Cursor = Cursors.Wait;

                // ensure async
                await Task.Run(() => { });

                RefreshConfig();

                var teams = new List<TeamDto>();
                // Refresh Teams
                foreach (var teamAward in _config.Awards.TeamRankInfos)
                {
                    var problemDtoFrom = teamAward.SubmissionInfosBefore.Select(submissionInfo => new ProblemDto
                    {
                        Label = submissionInfo.ProblemLabel, Status = ConvertStatus(submissionInfo.SubmissionStatus),
                        Time = submissionInfo.SubmissionTime is null ? 0 : submissionInfo.GetIntSubmissionTime(),
                        Try = ConvertStatus(submissionInfo.SubmissionStatus) == ProblemStatus.Accept
                            ? submissionInfo.TryTime + 2
                            : submissionInfo.TryTime + 1
                    }).ToList();

                    var problemDtoTo = teamAward.SubmissionInfosAfter.Select(submissionInfo => new ProblemDto
                    {
                        Label = submissionInfo.ProblemLabel, Status = ConvertStatus(submissionInfo.SubmissionStatus),
                        Time = submissionInfo.SubmissionTime is null ? 0 : submissionInfo.GetIntSubmissionTime(),
                        Try = ConvertStatus(submissionInfo.SubmissionStatus) == ProblemStatus.Accept
                            ? submissionInfo.TryTime + 2
                            : submissionInfo.TryTime + 1
                    }).ToList();

                    var teamDto = new TeamDto
                    {
                        TeamId = int.Parse(teamAward.Id),
                        TeamName = teamAward.Name,
                        SchoolName = _validator.SchoolsList.First(x => x.id == teamAward.OrganizationId).formal_name,
                        Awards = teamAward.AwardName.Select(a =>
                        {
                            return a switch
                            {
                                "Gold Medal" => "Gold Medal|medalist",
                                "Silver Medal" => "Silver Medal|medalist",
                                "Bronze Medal" => "Bronze Medal|medalist",
                                _ => a + "|normal"
                            };
                        }).ToList(),
                        DisplayName =
                            $"{teamAward.Name} -- {_validator.SchoolsList.First(x => x.id == teamAward.OrganizationId).formal_name}",
                        PenaltyTime = int.Parse(PenaltyTime.Text),
                        ProblemsFrom = problemDtoFrom.OrderBy(p => p.Label).ToList(),
                        ProblemsTo = problemDtoTo.OrderBy(p => p.Label).ToList()
                    };
                    teamDto.PostInit();
                    teams.Add(teamDto);
                }

                // Show Resolver
                var resolver = new Resolver(new ResolverDto
                {
                    Teams = teams
                        .OrderByDescending(t => t.Solved)
                        .ThenBy(t => t.TimeAll)
                        .ThenBy(t => t.DisplayName)
                        .Select(t => new Team(t)
                        {
                            Height = _config.AnimationConfig.TeamGridHeight,
                        }).ToList(),
                    ResolverAnimationConfig = _config.AnimationConfig
                });

            Cursor = Cursors.Arrow;
            _processing = false;
            resolver.Show();
            Close();
        }

        #region Award

        private void AwardView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            EditAward.IsEnabled = true;
            DeleteAward.IsEnabled = true;
        }

        private void DeleteAward_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete selected award?",
                "Award Utilities", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var selectedItem = this.AwardView.SelectedItem as ListViewItem;
                _config.Awards.TeamRankInfos.First(x => selectedItem != null && x.Id == selectedItem.Id).AwardName.Clear();
                RefreshAwardView();
                AwardView.UnselectAll();
                DeleteAward.IsEnabled = false;
                EditAward.IsEnabled = false;
            }
        }

        private void EditAward_OnClick(object sender, RoutedEventArgs e)
        {
            if (AwardView.SelectedItem is ListViewItem selectedItem)
            {
                var awardWindow = new AddEditAward(selectedItem.Name, _config.Awards.TeamRankInfos.First(x => x.Id == selectedItem.Id).AwardName)
                {
                    Owner = this,
                    ShowInTaskbar = false
                };
                awardWindow.ShowDialog();
                if (awardWindow.AwardInfoChanged)
                {
                    var changedTeamAward = _config.Awards.TeamRankInfos.First(x => x.Id == selectedItem.Id).AwardName;
                    changedTeamAward.Clear();
                    changedTeamAward.AddRange(awardWindow.ReturnedAward);
                }
            }
            AwardView.UnselectAll();
            DeleteAward.IsEnabled = false;
            EditAward.IsEnabled = false;
            RefreshAwardView();
        }

        private void CalculateAwards_OnClick(object sender, RoutedEventArgs e)
        {
            // Try parse medal count and penalty time
            if (!_loaded)
            {
                MessageBox.Show("Please load contest info from Contest Data Config tab first.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                CalculateAwards();
            }
            catch (FormatException)
            {
                MessageBox.Show("Only number allowed in Gold/Silver/Bronze/Penalty number.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            catch (ArgumentException err)
            {
                MessageBox.Show(err.Message, "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Draw award items in item view
            RefreshAwardView();
            // Enable run
            RunButton.IsEnabled = true;
        }
        

        #endregion

        #endregion


        #region Utils

        private void CalculateAwards()
        {
            var goldCount = int.Parse(GoldNumber.Text);
            var silverCount = int.Parse(SilverNumber.Text);
            var bronzeCount = int.Parse(BronzeNumber.Text);
            var penaltyTime = int.Parse(PenaltyTime.Text);
            var teamCount = int.Parse(TeamCount.Text);

            // Update contest info from user input
            _config.Contest.PenaltyTime = int.Parse(PenaltyTime.Text);
            _config.Contest.ContestLength = ContestLength.Text;
            _config.Contest.ContestName = ContestName.Text;
            _config.Contest.FreezeTime = FreezeTime.Text;

            if (goldCount + silverCount + bronzeCount > teamCount)
            {
                throw new ArgumentException($"Too many medal: have {teamCount} teams, but total {goldCount + silverCount + bronzeCount} medals given.");
            }

            if (_config.Awards == null)
            {
                _config.Awards = new AwardUtilities(_validator, penaltyTime);
                _config.Awards.CalculateTeamResolverInfo(_validator);
                _config.Awards.CalculateTeamRank();
            }
            else
            {
                // clear award info
                _config.Awards.TeamRankInfos.ForEach(i =>
                {
                    i.AwardName.Clear();
                });
            }

            // Make medal award from user input
            foreach (var t in _config.Awards.TeamRankInfos)
            {
                if (goldCount != 0)
                {
                    t.AwardName.Add("Gold Medal");
                    goldCount--;
                }
                else if (silverCount != 0)
                {
                    t.AwardName.Add("Silver Medal");
                    silverCount--;
                }
                else if (bronzeCount != 0)
                {
                    t.AwardName.Add("Bronze Medal");
                    bronzeCount--;
                }
                else
                {
                    break;
                }
            }

            // Give first standing title
            _config.Awards.TeamRankInfos[0].AwardName.Add(FirstStandingTitle.Text);
            // Give first blood award
            var isChecked = FirstBlood.IsChecked;
            if (isChecked != null && (bool) isChecked)
            {
                foreach (var firstSolveInfo in _config.Awards.FirstSolveInfos)
                {
                    if (!firstSolveInfo.Solved) continue;

                    var problemName = _validator.ProblemsList.First(x => x.id == firstSolveInfo.ProblemId).short_name;
                    _config.Awards.TeamRankInfos.First(x => x.Id == firstSolveInfo.TeamId).AwardName
                        .Add($"First solve {problemName}");
                }
            }

            // Give last solve award
            var lastAcceptIsChecked = LastAccept.IsChecked;
            if (lastAcceptIsChecked != null && (bool) lastAcceptIsChecked)
                _config.Awards.TeamRankInfos.First(x => x.Id == _config.Awards.LastSolveTeamId).AwardName
                    .Add("Last Accept submission");
        }

        private void RefreshConfig()
        {
            // refresh AnimationConfig
            try
            {
                var updateProblemStatusDuration = UpdateProblemStatusDuration.Text.Split(',');
                Assertion.Assert(updateProblemStatusDuration.Length == 2,
                    "the format of `Update Problem Status Duration` should be `number,number`");

                _config.AnimationConfig.TeamGridHeight = int.Parse(TeamGridHeight.Text);
                _config.AnimationConfig.MaxDisplayCount = int.Parse(MaxDisplayCount.Text);
                _config.AnimationConfig.MaxRenderCount = int.Parse(MaxRenderCount.Text);
                _config.AnimationConfig.ScrollDownDuration = int.Parse(ScrollDownDuration.Text);
                _config.AnimationConfig.ScrollDownInterval = int.Parse(ScrollDownInterval.Text);
                _config.AnimationConfig.CursorUpDuration = int.Parse(CursorUpDuration.Text);
                _config.AnimationConfig.UpdateTeamRankDuration = int.Parse(UpdateTeamRankDuration.Text);
                _config.AnimationConfig.AnimationFrameRate = int.Parse(AnimationFrameRate.Text);
                _config.AnimationConfig.UpdateProblemStatusDuration = new Tuple<int, int>(
                    int.Parse(updateProblemStatusDuration[0]),
                    int.Parse(updateProblemStatusDuration[1]));
                _config.AnimationConfig.AutoUpdateTeamStatusUntilRank = int.Parse(AutoUpdateTeamRankUntilRank.Text);

                // checker
                Assertion.Assert(_config.AnimationConfig.AnimationFrameRate > 0,
                    "_config.AnimationConfig.AnimationFrameRate > 0");
                Assertion.Assert(_config.AnimationConfig.MaxRenderCount >= _config.AnimationConfig.MaxDisplayCount,
                    "_config.AnimationConfig.MaxRenderCount >= _config.AnimationConfig.MaxDisplayCount");
                Assertion.Assert(_config.AnimationConfig.MaxDisplayCount > 0,
                    "_config.AnimationConfig.MaxDisplayCount > 0");
                Assertion.Assert(_config.AnimationConfig.ScrollDownDuration > 0,
                    "_config.AnimationConfig.ScrollDownDuration > 0");
                Assertion.Assert(_config.AnimationConfig.CursorUpDuration > 0,
                    "_config.AnimationConfig.CursorUpDuration > 0");
                Assertion.Assert(_config.AnimationConfig.UpdateTeamRankDuration > 0,
                    "_config.AnimationConfig.UpdateTeamRankDuration > 0");
                Assertion.Assert(_config.AnimationConfig.UpdateProblemStatusDuration.Item1 > 0,
                    "_config.AnimationConfig.UpdateProblemStatusDuration.Item1 > 0");
                Assertion.Assert(_config.AnimationConfig.UpdateProblemStatusDuration.Item2 > 0,
                    "_config.AnimationConfig.UpdateProblemStatusDuration.Item2 > 0");
                Assertion.Assert(_config.AnimationConfig.AutoUpdateTeamStatusUntilRank > 0,
                    "_config.AnimationConfig.AutoUpdateTeamStatusUntilRank > 0");
                Assertion.Assert(_config.AnimationConfig.TeamGridHeight > 0,
                    "_config.AnimationConfig.TeamGridHeight > 0");
            }
            catch (FormatException)
            {
                MessageBox.Show("Can not parse value to int in Animation Config", "ResolverConfig", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            catch (AssertionErrorException)
            {
                return;
            }

            // refresh contest info and awards
            CalculateAwards();

            // enable save-btn
            SaveButton.IsEnabled = true;
        }

        private void RefreshContestInfo()
        {
            var summary = _config.Contest;
            TeamCount.Text = summary.TeamCount.ToString();
            ProblemCount.Text = summary.ProblemCount.ToString();
            GroupCount.Text = summary.GroupCount.ToString();
            SubmissionCount.Text = summary.SubmissionCount.ToString();
            ContestLength.Text = summary.ContestLength;
            FreezeTime.Text = summary.FreezeTime;
            PenaltyTime.Text = summary.PenaltyTime.ToString();
            ContestName.Text = summary.ContestName;
            // award name
            FirstStandingTitle.Text = $"Champion of {summary.ContestName}";
        }

        /// <summary>
        /// Load ContestInfo from EventFeed
        /// </summary>
        private void LoadContestInfo()
        {
            var summary = _validator.GetContestSummary();
            _config.Contest = summary;
            RefreshContestInfo();
        }

        private static ProblemStatus ConvertStatus(string inStatus)
        {
            return inStatus switch
            {
                null => ProblemStatus.NotTried,
                "FB" => ProblemStatus.FirstBlood,
                "AC" => ProblemStatus.Accept,
                _ => ProblemStatus.UnAccept
            };
        }

        
        private void RefreshAwardView()
        {
            AwardView.Items.Clear();
            foreach (var team in _config.Awards.TeamRankInfos)
            {
                AwardView.Items.Add(new ListViewItem
                {
                    Id = team.Id,
                    Name = team.Name,
                    Solved = team.AcceptCount,
                    Time = team.Penalty,
                    Awards = string.Join(", ", team.AwardName)
                });
            }
        }
        
        #endregion
    }

    class ListViewItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Solved { get; set; }
        public int Time { get; set; }
        public string Awards { get; set; }
    }
}
