using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IcpcResolver.UserControl;
using IcpcResolver.Utils;
using Microsoft.Win32;

namespace IcpcResolver.Window
{
    /// <summary>
    /// Interaction logic for ResolverConfig.xaml
    /// </summary>
    public partial class ResolverConfigWindow
    {
        private ContestSummary _contestInfo;
        private Validator _validator;
        private bool _processing, _loaded;
        private AwardUtilities _awardInfo;

        public ResolverConfigWindow()
        {
            InitializeComponent();
        }

        #region EventHandler

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
                    SaveButton.IsEnabled = true;
                    _loaded = true;
                    LoadContestInfo();
                }
            }
            finally
            {
                Cursor = Cursors.Arrow;
                _processing = false;
            }
        }

        private void AutoFixEventFeed_OnClick(object sender, System.Windows.RoutedEventArgs e)
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
                SaveButton.IsEnabled = true;
                AutoFixButton.IsEnabled = false;
                _loaded = true;
                LoadContestInfo();
            }
            else
            {
                MessageBox.Show("Problem unable to fix automatically, please fix it manually.", "Event validator",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectPhotoFolder_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void SelectSchoolFolder_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void Run_OnClick(object sender, RoutedEventArgs e)
        {
            // Get Animation Config
            ResolverConfig aniConfig;

            try
            {
                var updateProblemStatusDuration = UpdateProblemStatusDuration.Text.Split(',');
                Assertion.Assert(updateProblemStatusDuration.Length == 2,
                    "the format of `Update Problem Status Duration` should be `number,number`");

                aniConfig = new ResolverConfig
                {
                    TeamGridHeight = int.Parse(TeamGridHeight.Text),
                    MaxDisplayCount = int.Parse(MaxDisplayCount.Text),
                    MaxRenderCount = int.Parse(MaxRenderCount.Text),
                    ScrollDownDuration = int.Parse(ScrollDownDuration.Text),
                    ScrollDownInterval = int.Parse(ScrollDownInterval.Text),
                    CursorUpDuration = int.Parse(CursorUpDuration.Text),
                    UpdateTeamRankDuration = int.Parse(UpdateTeamRankDuration.Text),
                    AnimationFrameRate = int.Parse(AnimationFrameRate.Text),
                    UpdateProblemStatusDuration = new Tuple<int, int>(int.Parse(updateProblemStatusDuration[0]),
                        int.Parse(updateProblemStatusDuration[1])),
                    AutoUpdateTeamStatusUntilRank = int.Parse(AutoUpdateTeamRankUntilRank.Text)
                };

                // checker
                Assertion.Assert(aniConfig.AnimationFrameRate > 0, "aniConfig.AnimationFrameRate > 0");
                Assertion.Assert(aniConfig.MaxRenderCount >= aniConfig.MaxDisplayCount,
                    "aniConfig.MaxRenderCount >= aniConfig.MaxDisplayCount");
                Assertion.Assert(aniConfig.MaxDisplayCount > 0, "aniConfig.MaxDisplayCount > 0");
                Assertion.Assert(aniConfig.ScrollDownDuration > 0, "aniConfig.ScrollDownDuration > 0");
                Assertion.Assert(aniConfig.CursorUpDuration > 0, "aniConfig.CursorUpDuration > 0");
                Assertion.Assert(aniConfig.UpdateTeamRankDuration > 0, "aniConfig.UpdateTeamRankDuration > 0");
                Assertion.Assert(aniConfig.UpdateProblemStatusDuration.Item1 > 0,
                    "aniConfig.UpdateProblemStatusDuration.Item1 > 0");
                Assertion.Assert(aniConfig.UpdateProblemStatusDuration.Item2 > 0,
                    "aniConfig.UpdateProblemStatusDuration.Item2 > 0");
                Assertion.Assert(aniConfig.AutoUpdateTeamStatusUntilRank > 0,
                    "aniConfig.AutoUpdateTeamStatusUntilRank > 0");
                Assertion.Assert(aniConfig.TeamGridHeight > 0, "aniConfig.TeamGridHeight > 0");
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

            // Convert awardInfo tp ResolverDto
            var teamDtoList = new List<TeamDto>();
            foreach (var teamAward in this._awardInfo.TeamRankInfos)
            {
                var problemDtoFrom = teamAward.SubmissionInfosBefore.Select(submissionInfo => new ProblemDto()
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
                    PenaltyTime = int.Parse(this.PenaltyTime.Text),
                    ProblemsFrom = problemDtoFrom,
                    ProblemsTo = problemDtoTo
                };
                teamDto.PostInit();
                teamDtoList.Add(teamDto);
            }

            // Show Resolver
            var resolver = new Resolver(new ResolverDto
            {
                Teams = teamDtoList
                    .OrderByDescending(t => t.Solved)
                    .ThenBy(t => t.TimeAll)
                    .ThenBy(t => t.DisplayName)
                    .Select(t => new Team(t)
                    {
                        Height = aniConfig.TeamGridHeight,
                    }).ToList(),
                ResolverConfig = aniConfig
            });
            resolver.Show();
            Close();
        }

        private void AwardView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.EditAward.IsEnabled = true;
            this.DeleteAward.IsEnabled = true;
        }

        private void DeleteAward_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete selected award?",
                "Award Utilities", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var selectedItem = this.AwardView.SelectedItem as ListViewItem;
                this._awardInfo.TeamRankInfos.First(x => x.Id == selectedItem.Id).AwardName.Clear();
                this.RefreshAwardView();
                this.AwardView.UnselectAll();
                this.DeleteAward.IsEnabled = false;
                this.EditAward.IsEnabled = false;
            }
        }

        private void EditAward_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.AwardView.SelectedItem is ListViewItem selectedItem)
            {
                var awardWindow = new AddEditAward(selectedItem.Name, this._awardInfo.TeamRankInfos.First(x => x.Id == selectedItem.Id).AwardName)
                {
                    Owner = this,
                    ShowInTaskbar = false
                };
                awardWindow.ShowDialog();
                if (awardWindow.AwardInfoChanged)
                {
                    var changedTeamAward = this._awardInfo.TeamRankInfos.First(x => x.Id == selectedItem.Id).AwardName;
                    changedTeamAward.Clear();
                    changedTeamAward.AddRange(awardWindow.ReturnedAward);
                }
            }
            this.AwardView.UnselectAll();
            this.DeleteAward.IsEnabled = false;
            this.EditAward.IsEnabled = false;
            this.RefreshAwardView();
        }

        private void CalculateAwards_OnClick(object sender, RoutedEventArgs e)
        {
            int goldCount, silverCount, bronzeCount, penaltyTime, teamCount;

            // Try parse medal count and penalty time
            if (!_loaded)
            {
                MessageBox.Show("Please load contest info from Contest Data Config tab first.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                goldCount = int.Parse(GoldNumber.Text);
                silverCount = int.Parse(SilverNumber.Text);
                bronzeCount = int.Parse(BronzeNumber.Text);
                penaltyTime = int.Parse(PenaltyTime.Text);
                teamCount = int.Parse(TeamCount.Text);
            }
            catch
            {
                MessageBox.Show("Only number allowed in Gold/Silver/Bronze/Penalty number.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update contest info from user input
            _contestInfo.PenaltyTime = int.Parse(PenaltyTime.Text);
            _contestInfo.ContestLength = ContestLength.Text;
            _contestInfo.ContestName = ContestName.Text;
            _contestInfo.FreezeTime = FreezeTime.Text;

            _awardInfo = new AwardUtilities(_validator, penaltyTime);
            if (goldCount + silverCount + bronzeCount > teamCount)
            {
                MessageBox.Show($"Too many medal: have {teamCount} teams, but total {goldCount + silverCount + bronzeCount} medals given.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _awardInfo.CalculateTeamResolverInfo(_validator);
            _awardInfo.CalculateTeamRank();

            // Make medal award from user input
            foreach (var t in _awardInfo.TeamRankInfos)
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
            _awardInfo.TeamRankInfos[0].AwardName.Add(FirstStandingTitle.Text);
            // Give first blood award
            var isChecked = FirstBlood.IsChecked;
            if (isChecked != null && (bool) isChecked)
            {
                foreach (var firstSolveInfo in _awardInfo.FirstSolveInfos)
                {
                    if (firstSolveInfo.Solved)
                    {
                        var problemName = _validator.ProblemsList.First(x => x.id == firstSolveInfo.ProblemId).short_name;
                        _awardInfo.TeamRankInfos.First(x=>x.Id == firstSolveInfo.TeamId).AwardName.Add($"First solve {problemName}");
                    }
                }
            }
            // Give last solve award
            var lastAcceptIsChecked = this.LastAccept.IsChecked;
            if (lastAcceptIsChecked != null && (bool) lastAcceptIsChecked)
                _awardInfo.TeamRankInfos.First(x => x.Id == _awardInfo.LastSolveTeamId).AwardName.Add("Last Accept submission");

            // Draw award items in item view
            RefreshAwardView();
            // Enable run
            RunButton.IsEnabled = true;
        }

        #endregion


        #region Utils

        private void LoadContestInfo()
        {
            var summary = _validator.GetContestSummary();
            _contestInfo = summary;
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
            CalculateAwards_OnClick(null, null);
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
            this.AwardView.Items.Clear();
            foreach (var team in this._awardInfo.TeamRankInfos)
            {
                this.AwardView.Items.Add(new ListViewItem
                {
                    Id = team.Id,
                    Name = team.Name,
                    Solved = team.AcceptCount,
                    Time = team.Penalty,
                    Awards = String.Join(", ", team.AwardName)
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
