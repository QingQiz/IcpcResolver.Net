using System.Collections.Generic;
using System.IO;
using System.Linq;
using IcpcResolver.Utils.EventFeed;

namespace IcpcResolver.Window
{
    public class Validator
    {
        public List<School> SchoolsList;
        public List<Group> GroupsList;
        public List<TeamInfo> TeamsList;
        public List<Problem> ProblemsList;
        public List<SubmissionWithResult> SubmissionWithResultsList;
        public List<ReturnSummary> ReturnSummaryList = new();
        public ContestInfo ContestInfo;
        public readonly StreamReader JsonFileStream;
        private readonly EventFeedParser _parser;
        
        public Validator(string pathToJson)
        {
            _parser = new EventFeedParser(pathToJson);

            // JsonFileStream = new StreamReader(pathToJson);
            // SchoolsList = new List<School>();
            // GroupsList = new List<Group>();
            // TeamsList = new List<TeamInfo>();
            // ProblemsList = new List<Problem>();
            // ReturnSummaryList = new List<ReturnSummary>();
            // SubmissionWithResultsList = new List<SubmissionWithResult>();
            // ContestInfo = new ContestInfo();
        }

        public Validator(List<School> sl, List<Group> gl, List<TeamInfo> ti, List<Problem> p,
            List<SubmissionWithResult> swr, ContestInfo ci)
        {
            this.SchoolsList = sl;
            this.GroupsList = gl;
            this.TeamsList = ti;
            this.ProblemsList = p;
            this.SubmissionWithResultsList = swr;
            this.ContestInfo = ci;
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

        // public string Export()
        // {
        //     // Serialize json data and save as file
        //     // Export separated by lines, groups, school, team, problem, submission respectively.
        //     var res = _parser.Export().Select(e => JsonConvert.SerializeObject(e) as string);
        //     return string.Join('\n', res);
        // }
    }

    public class ReturnSummary
    {
        public bool HasError => ErrList.Any();
        public string ErrType;
        public List<string> ErrList = new();
    }

    // ReSharper disable InconsistentNaming
    public class School
    {
        public string id { get; set; }
        public string shortname { get; set; }
        public string formal_name { get; set; }
    }

    public class Group
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool hidden { get; set; }
    }

    public class TeamInfo
    {
        public string id { get; set; }
        public string organization_id { get; set; }
        public string name { get; set; }
        public List<string> group_ids { get; set; }
    }

    public class Problem
    {
        public string id { get; set; }
        public string short_name { get; set; }
    }

    public class Submission
    {
        public string id { get; set; }
        public string team_id { get; set; }
        public string problem_id { get; set; }
        public string contest_time { get; set; }
    }

    public class SubmissionWithResult: Submission
    {
        // Construct full submission from deserialize object
        public SubmissionWithResult(Submission submission)
        {
            this.id = submission.id;
            this.problem_id = submission.problem_id;
            this.team_id = submission.team_id;
            this.contest_time = submission.contest_time;
        }
        // Leave this empty constructor for Json deserialize
        public SubmissionWithResult()
        {
        }
        public string judgeResult { get; set; }
    }

    public class ContestInfo
    {
        public string formal_name { get; set; }
        public string penalty_time { get; set; }
        public string duration { get; set; }
        public string scoreboard_freeze_duration { get; set; }
    }
    // ReSharper restore InconsistentNaming
}
