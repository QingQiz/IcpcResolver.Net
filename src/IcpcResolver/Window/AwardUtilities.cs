﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IcpcResolver.Window
{
    class AwardUtilities
    {
        public readonly List<TeamRankInfo> TeamRankInfos;
        public readonly List<FirstSolveInfo> FirstSolveInfos;
        public string LastSolveTeamId;
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly int PenaltyTime;
        // A list of no penalty status collection, may changed later
        private static readonly List<string> Accept = new() {"AC", "FB"};
        private static readonly List<string> Reject = new() {"WA", "TLE", "MLE", "NO", "RE", "OLE", "RTE"};

        public AwardUtilities(Validator info, int penaltyTime)
        // Construct AwardUtilities with info from Validator
        {
            PenaltyTime = penaltyTime;
            TeamRankInfos = new List<TeamRankInfo>();
            FirstSolveInfos = new List<FirstSolveInfo>();
            // Initialize teams and problems, make it empty
            foreach (var team in info.TeamsList)
            {
                var teamInfoItem = new TeamRankInfo(team)
                {
                    SubmissionInfosBefore = new List<SubmissionInfo>(),
                    SubmissionInfosAfter = new List<SubmissionInfo>()
                };
                foreach (var problem in info.ProblemsList)
                {
                    SubmissionInfo submissionInfo = new SubmissionInfo(problem.id, problem.short_name, 0);
                    SubmissionInfo submissionInfo1 = new SubmissionInfo(problem.id, problem.short_name, 0);
                    teamInfoItem.SubmissionInfosBefore.Add(submissionInfo);
                    teamInfoItem.SubmissionInfosAfter.Add(submissionInfo1);
                }
                teamInfoItem.AcceptCount = 0;
                teamInfoItem.Penalty = 0;
                TeamRankInfos.Add(teamInfoItem);
            }
            // Initialize first blood info
            foreach (var problem in info.ProblemsList)
            {
                FirstSolveInfo firstSolve = new FirstSolveInfo();
                firstSolve.ProblemId = problem.id;
                firstSolve.Solved = false;
                firstSolve.TeamId = "";
                this.FirstSolveInfos.Add(firstSolve);
            }
        }

        // Enumerate submissions to calculate resolver data
        public void CalculateTeamResolverInfo(Validator info)
        {
            int contestLength = TimeInMinute(info.ContestInfo.duration),
                freezeLength = TimeInMinute(info.ContestInfo.scoreboard_freeze_duration);
            int contestBeforeFreezeLength = contestLength - freezeLength;
            foreach (var submission in info.Submissions)
            {
                // Get current submission information: time, teamId, problemId, result, etc.
                int currentTime = TimeInMinute(submission.contest_time);
                string currentTeamId = submission.team_id;
                string currentProblemId = submission.problem_id;
                string currentJudgeResult = submission.judgement_result;
                int currentTeamRankInfoId = TeamRankInfos.FindIndex(x => x.Id == currentTeamId);
                SubmissionInfo currentSubmissionInfoBefore = TeamRankInfos[currentTeamRankInfoId].SubmissionInfosBefore
                    .Find(x => x.ProblemId == currentProblemId);
                SubmissionInfo currentSubmissionInfoAfter = TeamRankInfos[currentTeamRankInfoId].SubmissionInfosAfter
                    .Find(x => x.ProblemId == currentProblemId);
                if (currentSubmissionInfoBefore == null || currentSubmissionInfoAfter == null)
                {
                    throw new InvalidDataException(currentProblemId);
                }

                // Process first blood items
                bool problemSolvedStatus = this.FirstSolveInfos.First(x => x.ProblemId == currentProblemId).Solved;
                if (Accept.Contains(currentJudgeResult) && problemSolvedStatus == false)
                {
                    this.FirstSolveInfos.First(x => x.ProblemId == currentProblemId).Solved = true;
                    this.FirstSolveInfos.First(x => x.ProblemId == currentProblemId).TeamId = currentTeamId;
                    if (currentTime < contestBeforeFreezeLength)
                    {
                        currentSubmissionInfoBefore.SubmissionStatus = "FB";
                        currentSubmissionInfoBefore.SubmissionTime = submission.contest_time;
                        currentSubmissionInfoAfter.SubmissionStatus = currentSubmissionInfoBefore.SubmissionStatus;
                        currentSubmissionInfoAfter.SubmissionTime = currentSubmissionInfoBefore.SubmissionTime;
                    }
                    else
                    {
                        currentSubmissionInfoAfter.SubmissionStatus = "FB";
                        currentSubmissionInfoAfter.SubmissionTime = submission.contest_time;
                    }
                }

                // Process last accept: current result is accept and last result if reject
                if (Accept.Contains(currentJudgeResult) && Reject.Contains(currentSubmissionInfoAfter.SubmissionStatus))
                    LastSolveTeamId = currentTeamId;

                // Go through submissions before freeze
                if (currentTime < contestBeforeFreezeLength)
                {
                    if (Accept.Contains(currentSubmissionInfoBefore.SubmissionStatus))
                        continue;
                    currentSubmissionInfoBefore.SubmissionTime = submission.contest_time;
                    currentSubmissionInfoBefore.SubmissionStatus = currentJudgeResult;
                    if (Reject.Contains(currentJudgeResult))
                        currentSubmissionInfoBefore.TryTime++;
                    // Make an copy to after status, so currentSubmissionInfoAfter always stands for latest AC? status
                    currentSubmissionInfoAfter.SubmissionTime = currentSubmissionInfoBefore.SubmissionTime;
                    currentSubmissionInfoAfter.TryTime = currentSubmissionInfoBefore.TryTime;
                    currentSubmissionInfoAfter.SubmissionStatus = currentSubmissionInfoBefore.SubmissionStatus;
                }
                // Go through submissions after freeze
                else
                {
                    if (Accept.Contains(currentSubmissionInfoAfter.SubmissionStatus))
                        continue;
                    currentSubmissionInfoAfter.SubmissionTime = submission.contest_time;
                    currentSubmissionInfoAfter.SubmissionStatus = currentJudgeResult;
                    if (Reject.Contains(currentJudgeResult))
                        currentSubmissionInfoAfter.TryTime++;
                }
            }
        }

        // Calculate team rank with accept count and penalty
        public void CalculateTeamRank()
        {
            // CalculateResolverInfo() required
            foreach (var team in TeamRankInfos)
            {
                foreach (var problem in team.SubmissionInfosAfter)
                {
                    if (Accept.Contains(problem.SubmissionStatus))
                    {
                        team.AcceptCount++;
                        team.Penalty += this.CalculatePenalty(problem.TryTime, problem.SubmissionTime);
                    }
                }
            }

            TeamRankInfos.Sort(new Comparison<TeamRankInfo>((x, y) =>
                {
                    int ret = y.AcceptCount.CompareTo(x.AcceptCount);
                    return (ret != 0) ? ret : x.Penalty.CompareTo(y.Penalty);
                }
            ));
        }
        public int CalculatePenalty(int tryTimes, string acTime)
        {
            return tryTimes * PenaltyTime + TimeInMinute(acTime);
        }

        public int TimeInMinute(string timeString)
        {
            // Input format: hh:mm:ss:fff, only extract hh and mm
            string hour = timeString.Split(":")[0],
                minute = timeString.Split(":")[1];
            return int.Parse(hour) * 60 + int.Parse(minute);
        }
    }

    class TeamRankInfo
    {
        public TeamRankInfo(dynamic baseInfo)
        {
            GroupIds = (baseInfo.group_ids as List<object>)?.Select(x => x as string).ToList();
            Id = baseInfo.id;
            Name = baseInfo.name;
            OrganizationId = baseInfo.organization_id;
        }
        public List<string> GroupIds;
        public readonly string Id;
        public readonly string Name;
        public readonly string OrganizationId;
        public int AcceptCount;
        public int Penalty;
        public List<SubmissionInfo> SubmissionInfosBefore;
        public List<SubmissionInfo> SubmissionInfosAfter;
        public List<string> AwardName = new();
    }


    class SubmissionInfo
    {
        public string ProblemId { get; set; }
        public string ProblemLabel { get; set; }
        public int TryTime { get; set; }
        public string SubmissionTime { get; set; }
        public string SubmissionStatus { get; set; }

        public int GetIntSubmissionTime()
        {
            string hour = SubmissionTime.Split(":")[0],
                minute = SubmissionTime.Split(":")[1];
            return int.Parse(hour) * 60 + int.Parse(minute);
        }

        public SubmissionInfo(string id, string label, int tries)
        {
            this.ProblemId = id;
            this.ProblemLabel = label;
            this.TryTime = tries;
            this.SubmissionStatus = null;
            this.SubmissionTime = null;
        }
    }

    class FirstSolveInfo
    {
        public string ProblemId { get; set; }
        public string TeamId { get; set; }
        public bool Solved { get; set; }
    }
}
