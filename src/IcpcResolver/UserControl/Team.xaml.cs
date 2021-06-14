﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace IcpcResolver.UserControl
{
    public partial class Team
    {
        private Team()
        {
            InitializeComponent();
        }

        public Team(TeamDto team) : this()
        {
            TeamInfo = team;
            DisplayName = TeamInfo.DisplayName;

            Solved = team.Solved;
            Time = team.TimeAll;

            var cnt = 0;
            foreach (var problemViewModel in TeamInfo.ProblemsFrom)
            {
                Problems.ColumnDefinitions.Add(new ColumnDefinition());
                
                var problem = new Problem(problemViewModel);
                Problems.Children.Add(problem);
                _problems.Add(problem);

                Grid.SetRow(problem, 0);
                Grid.SetColumn(problem, cnt++);
            }

            if (string.IsNullOrWhiteSpace(team.IconPath))
            {
                // remove icon
                LayoutGrid.Children.Remove(SchoolIcon);
            }
            else
            {
                // add icon
                // crop icon to square
                var icon = new BitmapImage(new Uri(team.IconPath));
                var len = (int) Math.Min(icon.Height, icon.Width);

                SchoolIcon.Source = new CroppedBitmap(icon, new Int32Rect(0, 0, len, len));
                SchoolIcon.Width = Height - 15;
                SchoolIcon.Width = Height - 15;
            }
        }

        private readonly List<Problem> _problems = new();

        public List<string> Awards => TeamInfo.Awards;
        public TeamDto TeamInfo { get; }


        public int TeamRank
        {
            get => (int) GetValue(TeamRankProperty);
            set => SetValue(TeamRankProperty, value);
        }

        public static readonly DependencyProperty TeamRankProperty =
            DependencyProperty.Register("TeamRank", typeof(int), typeof(Team));

        public string DisplayName
        {
            get => (string) GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(Team));

        public int Time
        {
            get => (int) GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        private static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(int), typeof(Team));

        public int Solved
        {
            get => (int) GetValue(SolvedProperty);
            set => SetValue(SolvedProperty, value);
        }

        private static readonly DependencyProperty SolvedProperty =
            DependencyProperty.Register("Solved", typeof(int), typeof(Team));


        public async Task<bool> UpdateTeamStatusAnimation(int durationBeforeHighlight, int durationBeforeUpdate)
        {
            var isUpdated = false;
            for (var i = 0; i < TeamInfo.ProblemsFrom.Count; i++)
            {
                // only update pending problems
                if (TeamInfo.ProblemsFrom[i].Status != ProblemStatus.Pending) continue;

                await _problems[i].UpdateStatusAnimation(TeamInfo.ProblemsTo[i], durationBeforeHighlight,
                    durationBeforeUpdate);
                isUpdated = true;

                TeamInfo.ProblemsFrom[i].Status = TeamInfo.ProblemsTo[i].Status;
                // for rollback
                TeamInfo.ProblemsTo[i].Status = ProblemStatus.Pending;

                // if problem is note accepted, update next
                if (!TeamInfo.ProblemsFrom[i].IsAccepted) continue;
                break;
            }
            // if problem is accepted, update solved-count and time and break
            Solved = TeamInfo.Solved;
            Time = TeamInfo.TimeAll;
            return isUpdated;
        }
    }
}