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
    public partial class Loader : System.Windows.Window
    {
        private Validator _validator;
        public Loader()
        {
            InitializeComponent();
        }

        private void OpenCredWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            CredRequest reqWindow = new CredRequest();
            reqWindow.Owner = this;
            reqWindow.ShowInTaskbar = false;
            reqWindow.ShowDialog();
            string loadEventFeedPath = reqWindow.ReturnedPath;
            if (loadEventFeedPath.Length == 0) return;
            this.EventFeedFilePath.Text = loadEventFeedPath;
            this.ValidateFile.IsEnabled = true;
        }

        private void OpenLoadEventFileWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Event Feed JSON file (*.json)|*.json";
            if (openFileDialog.ShowDialog() != true) return;
            this.EventFeedFilePath.Text = openFileDialog.FileName;
            this.ValidateFile.IsEnabled = true;
        }

        private void ValidateData(object sender, System.Windows.RoutedEventArgs e)
        {
            loadCheck.IsChecked = submissionCheck.IsChecked = teamInfoCheck.IsChecked = unjudgedCheck.IsChecked = false;
            _validator = new Validator(EventFeedFilePath.Text);
            _validator.LoadAllEventData();
            this.loadCheck.IsChecked = true;
            if (_validator.CheckTeamInfo())
                this.teamInfoCheck.IsChecked = true;
            if (_validator.CheckSubmissionInfo())
                this.submissionCheck.IsChecked = true;
            if (_validator.CheckUnjudgedRuns())
                this.unjudgedCheck.IsChecked = true;
            string summaryInfo = "";
            foreach (var summary in _validator.returnSummaryList)
            {
                if (summary.retStatus)
                    continue;
                summaryInfo += summary.errType;
                summaryInfo += String.Join(",", summary.errList) + "\n";
            }
            if (summaryInfo.Length != 0)
            {
                MessageBox.Show(summaryInfo + "You can automatically drop these item of fix it manually.", "Event validator", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.autoFixButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("All info validate successfully.", "Event validator", MessageBoxButton.OK, MessageBoxImage.Information);
                this.saveAsButton.IsEnabled = true;
            }
        }

        private void AutoFix(object sender, System.Windows.RoutedEventArgs e)
        {
            // Fix invalid teams
            _validator.returnSummaryList = new List<returnSummary>();
            if (this.teamInfoCheck.IsChecked == false)
            {
                _validator.CheckTeamInfo();
                foreach (var summary in _validator.returnSummaryList)
                    if (summary.retStatus == false)
                        foreach (var teamid in summary.errList)
                        {
                            _validator.TeamsList.Remove(_validator.TeamsList.First(x => x.id == teamid));
                            Trace.WriteLine("Drop team id: " + teamid);
                        }
            }

            // Fix invalid submissions (Wrong info and unjudged)
            _validator.returnSummaryList = new List<returnSummary>();
            if (this.submissionCheck.IsChecked == false || this.unjudgedCheck.IsChecked == false)
            {
                _validator.CheckSubmissionInfo();
                _validator.CheckUnjudgedRuns();
                foreach (var summary in _validator.returnSummaryList)
                    if (summary.retStatus == false)
                        foreach (var submissionid in summary.errList)
                        {
                            _validator.SubmissionWithResultsList.RemoveAll(x => x.id == submissionid);
                            Trace.WriteLine("Drop submission id: " + submissionid);
                        }
            }
            if (_validator.CheckTeamInfo())
                teamInfoCheck.IsChecked = true;
            if (_validator.CheckSubmissionInfo())
                submissionCheck.IsChecked = true;
            _validator.returnSummaryList = new List<returnSummary>();
            if (_validator.CheckUnjudgedRuns())
                unjudgedCheck.IsChecked = true;
            if (teamInfoCheck.IsChecked is true && submissionCheck.IsChecked is true && unjudgedCheck.IsChecked is true)
            {
                MessageBox.Show("All info fixed and validated successfully.", "Event validator", MessageBoxButton.OK, MessageBoxImage.Information);
                this.saveAsButton.IsEnabled = true;
                this.autoFixButton.IsEnabled = false;
            }
            else
                MessageBox.Show("Problem unable to fix automatically, please fix it manually.", "Event validator", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void SaveAs(object sender, System.Windows.RoutedEventArgs e)
        {
            // Serialize json data and save as file
            // Export separated by lines, groups, school, team, problem, submission respectively.
            string exportJsonString = "";
            exportJsonString += JsonConvert.SerializeObject(_validator.contestInfo) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.GroupsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.SchoolsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.TeamsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.ProblemsList) + "\n";
            exportJsonString += JsonConvert.SerializeObject(_validator.SubmissionWithResultsList) + "\n";

            // Pop up save as dialog
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "JSON file (*.json)|*.json";
            saveFileDialog.FileName = "export.json";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, exportJsonString);
                MessageBox.Show("Export data saved, you can load data from other utilities.", "Event exporter", MessageBoxButton.OK, MessageBoxImage.Information);
                this.loadExportFile.Text = saveFileDialog.FileName;
            }
        }
        
        private void LoadExportFile(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "JSON file (*.json)|*.json";
            openFileDialog.FileName = "export.json";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.loadExportFile.Text = openFileDialog.FileName;
                LoadExportFromJsonFile(openFileDialog.FileName);
            }
        }

        private void LoadExportFromJsonFile(string openFilePath)
        {
            StreamReader jsonFileStream = new StreamReader(openFilePath);
            string a, b, c, d, e;
            ContestInfo contestInfoObj = JsonConvert.DeserializeObject<ContestInfo>(jsonFileStream.ReadLine()); 
            List<Group> groupsList = JsonConvert.DeserializeObject<List<Group>>(jsonFileStream.ReadLine());
            List<School> schoolsList = JsonConvert.DeserializeObject<List<School>>(jsonFileStream.ReadLine()); 
            List <TeamInfo> teamsList = JsonConvert.DeserializeObject<List<TeamInfo>>(jsonFileStream.ReadLine());
            List<Problem> problemsList = JsonConvert.DeserializeObject<List<Problem>>(jsonFileStream.ReadLine());
            List<SubmissionWithResult> SubmissionWithResultsList = JsonConvert.DeserializeObject<List<SubmissionWithResult>>(jsonFileStream.ReadLine());
            if (schoolsList != null) this.teamCount.Text = schoolsList.Count.ToString();
            if (problemsList != null) this.problemCount.Text = problemsList.Count.ToString();
            if (groupsList != null) this.groupCount.Text = groupsList.Count.ToString();
            if (SubmissionWithResultsList != null) this.submissionCount.Text = SubmissionWithResultsList.Count.ToString();
            if (contestInfoObj != null) this.contestLength.Text = contestInfoObj.duration;
            if (contestInfoObj != null) this.freezeTime.Text = contestInfoObj.scoreboard_freeze_duration;
            if (contestInfoObj != null) this.penaltyTime.Text = contestInfoObj.penalty_time;
            if (contestInfoObj != null) this.contestName.Text = contestInfoObj.formal_name;
        }

        private void SelectPhotoFolder(object sender, RoutedEventArgs e)
        {

        }

        private void SelectSchoolFolder(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
