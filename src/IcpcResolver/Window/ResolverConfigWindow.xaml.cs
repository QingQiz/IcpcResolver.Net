﻿using System;
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
        private Validator _validator, _demo;
        private bool _processing = false, _loaded = false;
        private AwardUtilities _awardInfo;
        public ResolverConfigWindow()
        {
            InitializeComponent();
        }

        private void OpenCredWindow(object sender, RoutedEventArgs e)
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
                    SaveAsButton.IsEnabled = true;
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
            if (SubmissionCheck.IsChecked == false || this.UnjudgedCheck.IsChecked == false)
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
                SaveAsButton.IsEnabled = true;
                AutoFixButton.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Problem unable to fix automatically, please fix it manually.", "Event validator",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveAs_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Deprecated");
            // var exportJsonString = _validator.Export();
            //
            // // Pop up save as dialog
            // var saveFileDialog = new System.Windows.Forms.SaveFileDialog
            // {
            //     Filter = "JSON file (*.json)|*.json",
            //     FileName = "export.json"
            // };
            //
            // if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            // {
            //     File.WriteAllText(saveFileDialog.FileName, exportJsonString);
            //     MessageBox.Show("Export data saved, you can load data from other utilities.", "Event exporter",
            //         MessageBoxButton.OK, MessageBoxImage.Information);
            //     LoadExportFile.Text = saveFileDialog.FileName;
            //     LoadExportFromJsonFile(LoadExportFile.Text);
            // }
        }
        
        private void LoadExportFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = "export.json"
            };

            // ReSharper disable once InvertIf
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadExportFile.Text = openFileDialog.FileName;
                LoadExportFromJsonFile(openFileDialog.FileName);
            }
        }

        private void LoadExportFromJsonFile(string openFilePath)
        {
            var jsonFileStream = new StreamReader(openFilePath);
            var contestInfoObj = JsonConvert.DeserializeObject<ContestInfo>(jsonFileStream.ReadLine()!);
            var groupsList = JsonConvert.DeserializeObject<List<Group>>(jsonFileStream.ReadLine()!);
            var schoolsList = JsonConvert.DeserializeObject<List<School>>(jsonFileStream.ReadLine()!);
            var teamsList = JsonConvert.DeserializeObject<List<TeamInfo>>(jsonFileStream.ReadLine()!);
            var problemsList = JsonConvert.DeserializeObject<List<Problem>>(jsonFileStream.ReadLine()!);
            var submissionWithResultsList =
                JsonConvert.DeserializeObject<List<SubmissionWithResult>>(jsonFileStream.ReadLine()!);

            if (schoolsList != null) TeamCount.Text = schoolsList.Count.ToString();
            if (problemsList != null) ProblemCount.Text = problemsList.Count.ToString();
            if (groupsList != null) GroupCount.Text = groupsList.Count.ToString();
            if (submissionWithResultsList != null) SubmissionCount.Text = submissionWithResultsList.Count.ToString();
            if (contestInfoObj != null) ContestLength.Text = contestInfoObj.duration;
            if (contestInfoObj != null) FreezeTime.Text = contestInfoObj.scoreboard_freeze_duration;
            if (contestInfoObj != null) PenaltyTime.Text = contestInfoObj.penalty_time;
            if (contestInfoObj != null) ContestName.Text = contestInfoObj.formal_name;
            _demo = new Validator(schoolsList, groupsList, teamsList, problemsList, submissionWithResultsList,
                contestInfoObj);
            _loaded = true;
            this.FirstStandingTitle.Text = $"Champion of {contestInfoObj.formal_name}";
            CalculateAwards(null, null);
        }

        private void SelectPhotoFolder_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void SelectSchoolFolder_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private ProblemStatus ConvertStatus(string inStatus)
        {
            switch (inStatus)
            {
                case null:
                    return ProblemStatus.NotTried;
                case "FB":
                    return ProblemStatus.FirstBlood;
                case "AC":
                    return ProblemStatus.Accept;
                default:
                    return ProblemStatus.UnAccept;
            }
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
                    TeamId = int.Parse(teamAward.id),
                    TeamName = teamAward.name,
                    SchoolName = _demo.SchoolsList.First(x => x.id == teamAward.organization_id).formal_name,
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
                        $"{teamAward.name} -- {_demo.SchoolsList.First(x => x.id == teamAward.organization_id).formal_name}",
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

        private void deleteAward_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete selected award?",
                "Award Utilities", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var selectedItem = this.AwardView.SelectedItem as ListViewItem;
                this._awardInfo.TeamRankInfos.First(x => x.id == selectedItem.Id).AwardName.Clear();
                this.RefreshAwardView();
                this.AwardView.UnselectAll();
                this.DeleteAward.IsEnabled = false;
                this.EditAward.IsEnabled = false;
            }
        }

        private void editAward_Click(object sender, RoutedEventArgs e)
        {
            if (this.AwardView.SelectedItem is ListViewItem selectedItem)
            {
                var awardWindow = new AddEditAward(selectedItem.Name, this._awardInfo.TeamRankInfos.First(x => x.id == selectedItem.Id).AwardName)
                {
                    Owner = this,
                    ShowInTaskbar = false
                };
                awardWindow.ShowDialog();
                if (awardWindow.AwardInfoChanged)
                {
                    var changedTeamAward = this._awardInfo.TeamRankInfos.First(x => x.id == selectedItem.Id).AwardName;
                    changedTeamAward.Clear();
                    foreach (var award in awardWindow.ReturnedAward)
                        changedTeamAward.Add(award);
                }
            }
            this.AwardView.UnselectAll();
            this.DeleteAward.IsEnabled = false;
            this.EditAward.IsEnabled = false;
            this.RefreshAwardView();
        }
        
        private void RefreshAwardView()
        {
            this.AwardView.Items.Clear();
            foreach (var team in this._awardInfo.TeamRankInfos)
            {
                this.AwardView.Items.Add(new ListViewItem
                {
                    Id = team.id,
                    Name = team.name,
                    Solved = team.AcceptCount,
                    Time = team.Penalty,
                    Awards = String.Join(", ", team.AwardName)
                });
            }
        }

        private void CalculateAwards(object sender, RoutedEventArgs e)
        {
            int goldCount, silverCount, bronzeCount, penaltyTime, teamCount;
            // Try parse medal count and penalty time
            if (!this._loaded)
            {
                MessageBox.Show("Please load contest info from Contest Data Config tab first.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                goldCount = int.Parse(this.GoldNumber.Text);
                silverCount = int.Parse(this.SilverNumber.Text);
                bronzeCount = int.Parse(this.BronzeNumber.Text);
                penaltyTime = int.Parse(this.PenaltyTime.Text);
                teamCount = int.Parse(this.TeamCount.Text);
            }
            catch
            {
                MessageBox.Show("Only number allowed in Gold/Silver/Bronze/Penalty number.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Update contest info from user input
            this._demo.ContestInfo.penalty_time = this.PenaltyTime.Text;
            this._demo.ContestInfo.duration = this.ContestLength.Text;
            this._demo.ContestInfo.formal_name = this.ContestName.Text;
            this._demo.ContestInfo.scoreboard_freeze_duration = this.FreezeTime.Text;
            this._awardInfo = new AwardUtilities(this._demo, penaltyTime);
            if (goldCount + silverCount + bronzeCount > teamCount)
            {
                MessageBox.Show($"Too many medal: have {teamCount} teams, but total {goldCount + silverCount + bronzeCount} medals given.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _awardInfo.CalculateTeamResolverInfo(this._demo);
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
            _awardInfo.TeamRankInfos[0].AwardName.Add(this.FirstStandingTitle.Text);
            // Give first blood award
            var isChecked = this.FirstBlood.IsChecked;
            if (isChecked != null && (bool) isChecked)
            {
                foreach (var firstSolveInfo in _awardInfo.FirstSolveInfos)
                {
                    if (firstSolveInfo.Solved)
                    {
                        string problemName = this._demo.ProblemsList.Find(x => x.id == firstSolveInfo.ProblemId)?.short_name;
                        _awardInfo.TeamRankInfos.First(x=>x.id == firstSolveInfo.TeamId).AwardName.Add($"First solve {problemName}");
                    }
                }
            }
            // Give last solve award
            var lastAcceptIsChecked = this.LastAccept.IsChecked;
            if (lastAcceptIsChecked != null && (bool) lastAcceptIsChecked)
                _awardInfo.TeamRankInfos.First(x => x.id == _awardInfo.LastSolveTeamId).AwardName.Add("Last Accept submission");

            // Draw award items in item view
            this.RefreshAwardView();
            // Enable run
            this.RunButton.IsEnabled = true;
        }
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
