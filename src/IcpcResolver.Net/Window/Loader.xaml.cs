using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IcpcResolver.Net.UserControl;
using IcpcResolver.Net.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for Loader.xaml
    /// </summary>
    public partial class Loader
    {
        private Validator _validator, _demo;
        private bool _processing = false, _loaded = false;
        public Loader()
        {
            InitializeComponent();
        }

        private void OpenCredWindow(object sender, System.Windows.RoutedEventArgs e)
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

        private void OpenLoadEventFileWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Event Feed JSON file (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() != true) return;
            EventFeedFilePath.Text = openFileDialog.FileName;
            ValidateFile.IsEnabled = true;
        }

        private async void ValidateData(object sender, RoutedEventArgs e)
        {
            if (_processing) return;

            _processing = true;
            Cursor = Cursors.Wait;

            try
            {
                LoadCheck.IsChecked =
                    SubmissionCheck.IsChecked = TeamInfoCheck.IsChecked = UnjudgedCheck.IsChecked = false;
                _validator = new Validator(EventFeedFilePath.Text);
                // validate event-feed async
                await Task.Run(() => _validator.LoadAllEventData());
                LoadCheck.IsChecked = true;

                if (_validator.CheckTeamInfo())
                    TeamInfoCheck.IsChecked = true;
                if (_validator.CheckSubmissionInfo())
                    SubmissionCheck.IsChecked = true;
                if (_validator.CheckUnjudgedRuns())
                    UnjudgedCheck.IsChecked = true;

                var summaryInfo = "";

                foreach (var summary in _validator.ReturnSummaryList.Where(summary => !summary.RetStatus))
                {
                    summaryInfo += summary.ErrType;
                    summaryInfo += string.Join(",", summary.ErrList) + "\n";
                }

                if (summaryInfo.Length != 0)
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

        private void AutoFix(object sender, System.Windows.RoutedEventArgs e)
        {
            // Fix invalid teams
            _validator.ReturnSummaryList = new List<ReturnSummary>();
            if (TeamInfoCheck.IsChecked == false)
            {
                _validator.CheckTeamInfo();
                foreach (var summary in _validator.ReturnSummaryList.Where(s => s.RetStatus == false))
                {
                    foreach (var teamId in summary.ErrList)
                    {
                        _validator.TeamsList.Remove(_validator.TeamsList.First(x => x.id == teamId));
                        Trace.WriteLine("Drop team id: " + teamId);
                    }
                }
            }

            // Fix invalid submissions (Wrong info and unjudged)
            _validator.ReturnSummaryList = new List<ReturnSummary>();
            if (SubmissionCheck.IsChecked == false || this.UnjudgedCheck.IsChecked == false)
            {
                _validator.CheckSubmissionInfo();
                _validator.CheckUnjudgedRuns();
                foreach (var summary in _validator.ReturnSummaryList.Where(s => s.RetStatus == false))
                {
                    foreach (var submissionId in summary.ErrList)
                    {
                        _validator.SubmissionWithResultsList.RemoveAll(x => x.id == submissionId);
                        Trace.WriteLine("Drop submission id: " + submissionId);
                    }
                }
            }

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

        private void SaveAs(object sender, RoutedEventArgs e)
        {
            // Serialize json data and save as file
            // Export separated by lines, groups, school, team, problem, submission respectively.
            var exportJsonString = "";
            exportJsonString += JsonConvert.SerializeObject(_validator.ContestInfo) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.GroupsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.SchoolsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.TeamsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.ProblemsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.SubmissionWithResultsList) + "\n";

            // Pop up save as dialog
            var saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = "export.json"
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, exportJsonString);
                MessageBox.Show("Export data saved, you can load data from other utilities.", "Event exporter",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadExportFile.Text = saveFileDialog.FileName;
                LoadExportFromJsonFile(LoadExportFile.Text);
            }
        }
        
        private void LoadExportFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = "export.json"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadExportFile.Text = openFileDialog.FileName;
                LoadExportFromJsonFile(openFileDialog.FileName);
                this._loaded = true;
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
            this._demo = new Validator(schoolsList, groupsList, teamsList, problemsList, submissionWithResultsList,
                contestInfoObj);
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
                MessageBox.Show("Can not parse value to int in Animation Config", "Loader", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            catch (AssertionErrorException)
            {
                return;
            }

            // Show Resolver
            var resolver = new Resolver(new ResolverDto
            {
                ResolverConfig = aniConfig,
                // Fake team info
                Teams = ResolverDto.DataGenerator(12, 30).Select(t => new Team(t)
                {
                    Height = aniConfig.TeamGridHeight,
                }).ToList(),
            });
            resolver.Show();
            Close();
        }

        private void CalculateAwards(object sender, RoutedEventArgs e)
        {
            int goldCount, silverCount, bronzeCount, penaltyTime, teamCount;
            string firstStanding;
            // Try parse medal count and penaltytime
            if (!this._loaded)
            {
                MessageBox.Show("Please load contest info from Contest Data Config tab first.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                goldCount = int.Parse(this.goldNumber.Text);
                silverCount = int.Parse(this.silverNumber.Text);
                bronzeCount = int.Parse(this.bronzeNumber.Text);
                penaltyTime = int.Parse(this.PenaltyTime.Text);
                teamCount = int.Parse(this.TeamCount.Text);
                firstStanding = this.firstStandingTitle.Text;
            }
            catch
            {
                MessageBox.Show("Only number allowed in Gold/Silver/Bronze/Penalty number.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Update contest info from user input
            this.AwardView.Items.Clear();
            this._demo.ContestInfo.penalty_time = this.PenaltyTime.Text;
            this._demo.ContestInfo.duration = this.ContestLength.Text;
            this._demo.ContestInfo.formal_name = this.ContestName.Text;
            this._demo.ContestInfo.scoreboard_freeze_duration = this.FreezeTime.Text;
            AwardUtilities awardInfo = new AwardUtilities(this._demo, penaltyTime);

            if (goldCount + silverCount + bronzeCount > teamCount)
            {
                MessageBox.Show($"Too many medal: have {teamCount} teams, but total {goldCount + silverCount + bronzeCount} medals given.",
                    "Award Utilities", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            awardInfo.CalculateTeamResolverInfo(this._demo);
            awardInfo.CalculateTeamRank();
            // Make medal award from user input
            for (int i = 0; i < awardInfo.TeamRankInfos.Count(); i++)
            {
                if (goldCount != 0)
                {
                    awardInfo.TeamRankInfos[i].AwardName.Add("Gold Medal");
                    goldCount--;
                } else if (silverCount != 0)
                {
                    awardInfo.TeamRankInfos[i].AwardName.Add("Silver Medal");
                    silverCount--;
                } else if (bronzeCount != 0)
                {
                    awardInfo.TeamRankInfos[i].AwardName.Add("Bronze Medal");
                    bronzeCount--;
                }
                else
                    break;
            }
            // Give first standing title
            awardInfo.TeamRankInfos[0].AwardName.Add(this.firstStandingTitle.Text);
            // Give first blood award
            var isChecked = this.firstBlood.IsChecked;
            if (isChecked != null && (bool) isChecked)
            {
                foreach (var firstSolveInfo in awardInfo.FirstSolveInfos)
                {
                    if (firstSolveInfo.Solved)
                    {
                        string problemName = this._demo.ProblemsList.Find(x => x.id == firstSolveInfo.ProblemId)?.short_name;
                        awardInfo.TeamRankInfos.First(x=>x.id == firstSolveInfo.TeamId).AwardName.Add($"First solve {problemName}");
                    }
                }
            }
            // Give last solve award
            var lastAcceptIsChecked = this.lastAccept.IsChecked;
            if (lastAcceptIsChecked != null && (bool) lastAcceptIsChecked)
                awardInfo.TeamRankInfos.First(x => x.id == awardInfo.LastSolveTeamId).AwardName.Add("Last Accept submission");

            foreach (var team in awardInfo.TeamRankInfos)
            {
                this.AwardView.Items.Add(new ListViewItem
                {
                    Id = team.id, Name = team.name, 
                    Solved = team.AcceptCount, Time = team.Penalty,
                    Awards = String.Join(", ", team.AwardName)
                });
                // Trace.WriteLine($"TeamName: {team.name}, Accept Number: {team.AcceptCount}, Penalty: {team.Penalty}");   
            }

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
