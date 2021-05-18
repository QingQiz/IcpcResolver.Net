using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IcpcResolver.Window
{
    public class Validator
    {
        public List<School> SchoolsList;
        public List<Group> GroupsList;
        public List<TeamInfo> TeamsList;
        public List<Problem> ProblemsList;
        public List<SubmissionWithResult> SubmissionWithResultsList;
        public List<ReturnSummary> ReturnSummaryList;
        public ContestInfo ContestInfo;
        public readonly StreamReader JsonFileStream;
        
        public Validator(string pathToJson)
        {
            JsonFileStream = new StreamReader(pathToJson);
            SchoolsList = new List<School>();
            GroupsList = new List<Group>();
            TeamsList = new List<TeamInfo>();
            ProblemsList = new List<Problem>();
            ReturnSummaryList = new List<ReturnSummary>();
            SubmissionWithResultsList = new List<SubmissionWithResult>();
            ContestInfo = new ContestInfo();
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
            string jsonLine;
            while ((jsonLine = JsonFileStream.ReadLine()) != null)
            {
                var jsonLoader = JObject.Parse(jsonLine);
                JToken dataType = jsonLoader["type"], opType = jsonLoader["op"], opData = jsonLoader["data"];
                if (dataType != null)
                    switch (dataType.ToString())
                    {
                        case "organizations":
                            var school = opData.ToObject<School>();
                            if (opType.ToString() == "create")
                            {
                                SchoolsList.Add(school);
                            }
                            else if (opType.ToString() == "update")
                            {
                                var idx = SchoolsList.FindIndex(x => x.id == school.id);
                                if (idx != -1)
                                    SchoolsList[idx] = school;
                                else
                                    SchoolsList.Add(school);
                            }
                            break;
                        case "groups":
                            var group = opData.ToObject<Group>();
                            if (opType.ToString() == "create")
                            {
                                GroupsList.Add(group);
                            }
                            else if (opType.ToString() == "update")
                            {
                                var idx = GroupsList.FindIndex(x => x.id == group.id);
                                if (idx != -1)
                                    GroupsList[idx] = group;
                                else
                                    GroupsList.Add(group);
                            }
                            break;
                        case "teams":
                            var team = opData.ToObject<TeamInfo>();
                            if (opType.ToString() == "create")
                            {
                                TeamsList.Add(team);
                            }
                            else if (opType.ToString() == "update")
                            {
                                var idx = TeamsList.FindIndex(x => x.id == team.id);
                                if (idx != -1)
                                    TeamsList[idx] = team;
                                else
                                    TeamsList.Add(team);
                            }
                            break;
                        case "problems":
                            var problem = opData.ToObject<Problem>();
                            if (opType.ToString() == "create")
                            {
                                ProblemsList.Add(problem);
                            }
                            else if (opType.ToString() == "update")
                            {
                                var idx = ProblemsList.FindIndex(x => x.id == problem.id);
                                if (idx != -1)
                                    ProblemsList[idx] = problem;
                                else
                                    ProblemsList.Add(problem);
                            } else if (opType.ToString() == "delete")
                            {
                                ProblemsList.RemoveAt(ProblemsList.FindIndex(x => x.id == opType["id"]?.ToString()));
                            }
                            break;
                        case "submissions":
                            var submission = opData.ToObject<Submission>();
                            if (opType.ToString() == "delete")
                            {
                                SubmissionWithResultsList.RemoveAt(
                                    SubmissionWithResultsList.FindIndex(x => x.id == opData["id"]?.ToString()));
                            }
                            else
                            {
                                var submissionWithResult = new SubmissionWithResult(submission);
                                SubmissionWithResultsList.Add(submissionWithResult);
                            }

                            break;
                        case "judgements":
                            string submissionId = opData["submission_id"].ToString(),
                                judgeResult = opData["judgement_type_id"].ToString();
                            SubmissionWithResultsList.First(x => x.id == submissionId).judgeResult = judgeResult;
                            break;
                        case "contests":
                            ContestInfo = opData.ToObject<ContestInfo>();
                            break;
                        default:
                            break;
                    }
            }
        }
        public bool CheckTeamInfo()
        {
            // Check if all group id in GroupList
            var r = new ReturnSummary("No group ID found in group definition.\nTeam ID with error: ");
            foreach (var team in TeamsList)
            {
                foreach (var gid in team.group_ids)
                {
                    if (GroupsList.Exists(x => x.id == gid) == false)
                    {
                        r.RetStatus = false;
                        r.ErrList.Add(team.id);
                    }
                }
            }
            ReturnSummaryList.Add(r);
            // Check if all school id in SchoolList
            var r1 = new ReturnSummary("No organization ID found in organization definition.\nTeam ID with error: ");
            foreach (var team in TeamsList)
            {
                if (SchoolsList.Exists(x => x.id == team.organization_id) == false)
                {
                    r1.RetStatus = false;
                    r1.ErrList.Add(team.id);
                }
            }
            ReturnSummaryList.Add(r1);
            // Return team info check result
            return r.RetStatus && r1.RetStatus;
        }

        public bool CheckSubmissionInfo()
        {
            // Check if all teams in TeamList;
            var r = new ReturnSummary("No team found in team definition.\nSubmission ID with error: ");
            foreach (var submission in SubmissionWithResultsList)
            {
                if (TeamsList.Exists(x => x.id == submission.team_id) == false)
                {
                    r.RetStatus = false;
                    r.ErrList.Add(submission.id);
                }
            }
            ReturnSummaryList.Add(r);
            // Check if all problems in ProblemList;
            var r1 = new ReturnSummary("No problem found in problem definition.\nSubmission ID with error: ");
            foreach (var submission in SubmissionWithResultsList)
            {
                if (ProblemsList.Exists(x => x.id == submission.problem_id) == false)
                {
                    r1.RetStatus = false;
                    r1.ErrList.Add(submission.id);
                }
            }
            ReturnSummaryList.Add(r1);
            // Return submission info check result
            return r.RetStatus && r1.RetStatus;
        }

        public bool CheckUnjudgedRuns()
        {
            // Check if all submissions have judge result;
            var r = new ReturnSummary("No judge result found in these submissions.\nSubmission ID without judge result: ");
            foreach (var submission in SubmissionWithResultsList)
            {
                if (submission.judgeResult is null or "")
                {
                    r.RetStatus = false;
                    r.ErrList.Add(submission.id);
                }
            }
            ReturnSummaryList.Add(r);
            // Return submission info check result
            return r.RetStatus;
        }
    }

    public class ReturnSummary
    {
        public ReturnSummary(string errInfo)
        {
            RetStatus = true;
            ErrType = errInfo;
            ErrList = new List<string>();
        }
        public bool RetStatus;
        public string ErrType;
        public List<string> ErrList;
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
