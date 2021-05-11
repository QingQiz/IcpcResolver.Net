using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IcpcResolver.Net.Window
{
    class Validator
    {
        public List<School> SchoolsList;
        public List<Group> GroupsList;
        public List<TeamInfo> TeamsList;
        public List<Problem> ProblemsList;
        public List<SubmissionWithResult> SubmissionWithResultsList;
        public List<returnSummary> returnSummaryList;
        public ContestInfo contestInfo;
        public StreamReader jsonFileStream;
        
        public Validator(string pathToJson)
        {
            this.jsonFileStream = new StreamReader(pathToJson);
            SchoolsList = new List<School>();
            GroupsList = new List<Group>();
            TeamsList = new List<TeamInfo>();
            ProblemsList = new List<Problem>();
            returnSummaryList = new List<returnSummary>();
            SubmissionWithResultsList = new List<SubmissionWithResult>();
            contestInfo = new ContestInfo();
        }

        public void LoadAllEventData()
        {
            string jsonLine;
            while ((jsonLine = this.jsonFileStream.ReadLine()) != null)
            {
                JObject jsonLoader = JObject.Parse(jsonLine);
                JToken dataType = jsonLoader["type"], opType = jsonLoader["op"], opData = jsonLoader["data"];
                if (dataType != null)
                    switch (dataType.ToString())
                    {
                        case "organizations":
                            School school = opData.ToObject<School>();
                            if (opType.ToString() == "create")
                            {
                                SchoolsList.Add(school);
                            }
                            else if (opType.ToString() == "update")
                            {
                                int idx = SchoolsList.FindIndex(x => x.id == school.id);
                                if (idx != -1)
                                    SchoolsList[idx] = school;
                                else
                                    SchoolsList.Add(school);
                            }
                            break;
                        case "groups":
                            Group group = opData.ToObject<Group>();
                            if (opType.ToString() == "create")
                            {
                                GroupsList.Add(group);
                            }
                            else if (opType.ToString() == "update")
                            {
                                int idx = GroupsList.FindIndex(x => x.id == group.id);
                                if (idx != -1)
                                    GroupsList[idx] = group;
                                else
                                    GroupsList.Add(group);
                            }
                            break;
                        case "teams":
                            TeamInfo team = opData.ToObject<TeamInfo>();
                            if (opType.ToString() == "create")
                            {
                                TeamsList.Add(team);
                            }
                            else if (opType.ToString() == "update")
                            {
                                int idx = TeamsList.FindIndex(x => x.id == team.id);
                                if (idx != -1)
                                    TeamsList[idx] = team;
                                else
                                    TeamsList.Add(team);
                            }
                            break;
                        case "problems":
                            Problem problem = opData.ToObject<Problem>();
                            if (opType.ToString() == "create")
                            {
                                ProblemsList.Add(problem);
                            }
                            else if (opType.ToString() == "update")
                            {
                                int idx = ProblemsList.FindIndex(x => x.id == problem.id);
                                if (idx != -1)
                                    ProblemsList[idx] = problem;
                                else
                                    ProblemsList.Add(problem);
                            }
                            break;
                        case "submissions":
                            Submission submission = opData.ToObject<Submission>();
                            if (opType.ToString() == "delete")
                            {
                                SubmissionWithResultsList.RemoveAt(
                                    SubmissionWithResultsList.FindIndex(x => x.id == opData["id"].ToString()));
                            }
                            else
                            {
                                SubmissionWithResult submissionWithResult = new SubmissionWithResult(submission);
                                SubmissionWithResultsList.Add(submissionWithResult);
                            }

                            break;
                        case "judgements":
                            string submissionId = opData["submission_id"].ToString(), judgeResult = opData["judgement_type_id"].ToString();
                            SubmissionWithResultsList.First(x => x.id == submissionId).judgeResult = judgeResult;
                            break;
                        case "contests":
                            contestInfo = opData.ToObject<ContestInfo>();
                            break;
                        default:
                            break;
                    }
            }
        }
        public bool CheckTeamInfo()
        {
            // Check if all group id in GroupList
            returnSummary r = new returnSummary("No group ID found in group definition.\nTeam ID with error: ");
            foreach (var team in TeamsList)
            {
                foreach (var gid in team.group_ids)
                {
                    if (GroupsList.Exists(x => x.id == gid) == false)
                    {
                        r.retStatus = false;
                        r.errList.Add(team.id);
                    }
                }
            }
            returnSummaryList.Add(r);
            // Check if all school id in SchoolList
            returnSummary r1 = new returnSummary("No organization ID found in organization definition.\nTeam ID with error: ");
            foreach (var team in TeamsList)
            {
                if (SchoolsList.Exists(x => x.id == team.organization_id) == false)
                {
                    r1.retStatus = false;
                    r1.errList.Add(team.id);
                }
            }
            returnSummaryList.Add(r1);
            // Return team info check result
            return r.retStatus && r1.retStatus;
        }

        public bool CheckSubmissionInfo()
        {
            // Check if all teams in TeamList;
            returnSummary r = new returnSummary("No team found in team definition.\nSubmission ID with error: ");
            foreach (var submission in SubmissionWithResultsList)
            {
                if (TeamsList.Exists(x => x.id == submission.team_id) == false)
                {
                    r.retStatus = false;
                    r.errList.Add(submission.id);
                }
            }
            returnSummaryList.Add(r);
            // Check if all problems in ProblemList;
            returnSummary r1 = new returnSummary("No problem found in problem definition.\nSubmission ID with error: ");
            foreach (var submission in SubmissionWithResultsList)
            {
                if (ProblemsList.Exists(x => x.id == submission.problem_id) == false)
                {
                    r1.retStatus = false;
                    r1.errList.Add(submission.id);
                }
            }
            returnSummaryList.Add(r1);
            // Return submission info check result
            return r.retStatus && r1.retStatus;
        }

        public bool CheckUnjudgedRuns()
        {
            // Check if all submissions have judge result;
            returnSummary r = new returnSummary("No judge result found in these submissions.\nSubmission ID without judge result: ");
            foreach (var submission in SubmissionWithResultsList)
            {
                if (submission.judgeResult is null or "")
                {
                    r.retStatus = false;
                    r.errList.Add(submission.id);
                }
            }
            returnSummaryList.Add(r);
            // Return submission info check result
            return r.retStatus;
        }
    }

    public class returnSummary
    {
        public returnSummary(string errInfo)
        {
            this.retStatus = true;
            this.errType = errInfo;
            this.errList = new List<string>();
        }
        public bool retStatus;
        public string errType;
        public List<string> errList;
    }

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
}
