using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for Loader.xaml
    /// </summary>
    public partial class Loader
    {
        private Validator _validator;
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

        private void ValidateData(object sender, RoutedEventArgs e)
        {
            LoadCheck.IsChecked = SubmissionCheck.IsChecked = TeamInfoCheck.IsChecked = UnjudgedCheck.IsChecked = false;
            _validator = new Validator(EventFeedFilePath.Text);
            _validator.LoadAllEventData();
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
            }
        }

        private void LoadExportFromJsonFile(string openFilePath)
        {
            var jsonFileStream = new StreamReader(openFilePath);
            var contestInfoObj = JsonConvert.DeserializeObject<ContestInfo>(jsonFileStream.ReadLine());
            var groupsList = JsonConvert.DeserializeObject<List<Group>>(jsonFileStream.ReadLine());
            var schoolsList = JsonConvert.DeserializeObject<List<School>>(jsonFileStream.ReadLine());
            var teamsList = JsonConvert.DeserializeObject<List<TeamInfo>>(jsonFileStream.ReadLine());
            var problemsList = JsonConvert.DeserializeObject<List<Problem>>(jsonFileStream.ReadLine());
            var submissionWithResultsList =
                JsonConvert.DeserializeObject<List<SubmissionWithResult>>(jsonFileStream.ReadLine());

            if (schoolsList != null) TeamCount.Text = schoolsList.Count.ToString();
            if (problemsList != null) ProblemCount.Text = problemsList.Count.ToString();
            if (groupsList != null) GroupCount.Text = groupsList.Count.ToString();
            if (submissionWithResultsList != null) SubmissionCount.Text = submissionWithResultsList.Count.ToString();
            if (contestInfoObj != null) ContestLength.Text = contestInfoObj.duration;
            if (contestInfoObj != null) FreezeTime.Text = contestInfoObj.scoreboard_freeze_duration;
            if (contestInfoObj != null) PenaltyTime.Text = contestInfoObj.penalty_time;
            if (contestInfoObj != null) ContestName.Text = contestInfoObj.formal_name;
        }

        private void SelectPhotoFolder_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void SelectSchoolFolder_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
