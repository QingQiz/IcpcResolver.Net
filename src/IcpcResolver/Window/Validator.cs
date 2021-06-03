using System.Collections.Generic;
using System.Linq;
using IcpcResolver.Utils.EventFeed;

namespace IcpcResolver.Window
{
    public class Validator
    {
        public IEnumerable<dynamic> SchoolsList => _parser.Schools;
        public IEnumerable<dynamic> GroupsList => _parser.Groups;
        public IEnumerable<dynamic> TeamsList => _parser.Teams;
        public IEnumerable<dynamic> ProblemsList => _parser.Problems;
        public IEnumerable<dynamic> Submissions => _parser.Submissions; 

        public List<ReturnSummary> ReturnSummaryList = new();
        public dynamic ContestInfo => _parser.Contest;
        private readonly EventFeedParser _parser;
        
        public Validator(string pathToJson)
        {
            _parser = new EventFeedParser(pathToJson);
        }

        public void LoadAllEventData()
        {
            _parser.Parse();
        }

        public bool CheckTeamInfo()
        {
            // Check if all group id in GroupList
            var r = new ReturnSummary
            {
                ErrType = "No group ID found in group definition.\nTeam ID with error: ",
                ErrList = _parser.CheckTeamGroups().ToList()
            };
            ReturnSummaryList.Add(r);

            // Check if all school id in SchoolList
            var r1 = new ReturnSummary
            {
                ErrType = "No organization ID found in organization definition.\nTeam ID with error: ",
                ErrList = _parser.CheckTeamOrganizations().ToList()
            };
            ReturnSummaryList.Add(r1);

            // Return team info check result
            return !r.HasError && !r1.HasError;
        }

        public bool CheckSubmissionInfo()
        {
            // Check if all teams in TeamList;
            var r = new ReturnSummary
            {
                ErrType = "No team found in team definition.\nSubmission ID with error: ",
                ErrList = _parser.CheckSubmissions().ToList(),
            };
            ReturnSummaryList.Add(r);

            // Check if all problems in ProblemList;
            var r1 = new ReturnSummary
            {
                ErrType = "No problem found in problem definition.\nSubmission ID with error: ",
                ErrList = _parser.CheckProblems().ToList(),
            };
            ReturnSummaryList.Add(r1);

            // Return submission info check result
            return !r.HasError && !r1.HasError;
        }

        public bool CheckUnjudgedRuns()
        {
            // Check if all submissions have judge result;
            var r = new ReturnSummary
            {
                ErrType = "No judge result found in these submissions.\nSubmission ID without judge result: ",
                ErrList = _parser.CheckUnJudgedRuns().ToList()
            };
            ReturnSummaryList.Add(r);

            // Return submission info check result
            return !r.HasError;
        }

        public void RemoveInvalidTeams()
        {
            ReturnSummaryList = new List<ReturnSummary>();
            CheckTeamInfo();

            ReturnSummaryList
                .Where(s => s.HasError)
                .Select(s => s.ErrList)
                .ToList().ForEach(el => _parser.RemoveTeams(el));
        }

        public void RemoveInvalidSubmissions()
        {
            ReturnSummaryList = new List<ReturnSummary>();
            CheckSubmissionInfo();
            CheckUnjudgedRuns();

            ReturnSummaryList
                .Where(s => s.HasError)
                .Select(s => s.ErrList)
                .ToList().ForEach(el => _parser.RemoveSubmissions(el));
        }

        public ContestSummary GetContestSummary()
        {
            return new()
            {
                ContestLength = _parser.Contest.duration,
                FreezeTime = _parser.Contest.scoreboard_freeze_duration,
                PenaltyTime = _parser.Contest.penalty_time,
                ContestName = _parser.Contest.formal_name,
                TeamCount = _parser.Teams.Count(),
                ProblemCount = _parser.Problems.Count(),
                SubmissionCount = _parser.Submissions.Count(),
                GroupCount = _parser.Groups.Count()
            };
        }
    }

    public class ReturnSummary
    {
        public bool HasError => ErrList.Any();
        public string ErrType;
        public List<string> ErrList = new();
    }

    public class ContestSummary
    {
        public string ContestLength;
        public string FreezeTime;
        public long PenaltyTime;
        public string ContestName;
        public int TeamCount;
        public int ProblemCount;
        public int SubmissionCount;
        public int GroupCount;
    }
}
