using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;

namespace IcpcResolver.Utils.EventFeed
{
    public class EventFeedParser
    {
        private readonly Dictionary<string, dynamic> _schools = new();
        private readonly Dictionary<string, dynamic> _groups = new();
        private readonly Dictionary<string, dynamic> _teams = new();
        private readonly Dictionary<string, dynamic> _problems = new();
        private readonly Dictionary<string, dynamic> _submissions = new();
        private readonly string _filePath;
        private bool _parsed;

        public dynamic Contest;
        public IEnumerable<dynamic> Schools => _schools.Values.Select(x => x);
        public IEnumerable<dynamic> Groups => _groups.Values.Select(x => x);
        public IEnumerable<dynamic> Teams => _teams.Values.Select(x => x);
        public IEnumerable<dynamic> Problems => _problems.Values.Select(x => x);
        public IEnumerable<dynamic> Submissions => _submissions.Values.Select(x => x);

        public EventFeedParser(string eventFeedFilePath)
        {
            _filePath = eventFeedFilePath;
        }

        /// <summary>
        /// parse event feed file. it may take a long time to parse event feed.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Parse()
        {
            if (_parsed) return;

            var text = System.IO.File.ReadAllLines(_filePath);
            var events = text
                .Select(x => JsonConvert.DeserializeObject<ExpandoObject>(x) as dynamic)
                .ToList();

            foreach (var @event in events)
            {
                var type = @event.type as string;
                var op = @event.op as string;
                var data = @event.data;

                switch (type)
                {
                    // for organizations
                    case "organizations" when op == "create":
                    case "organizations" when op == "update":
                        _schools[data.id] = data;
                        break;
                    // for groups
                    case "groups" when op == "create":
                    case "groups" when op == "update":
                        _groups[data.id] = data;
                        break;
                    // for teams
                    case "teams" when op == "create":
                    case "teams" when op == "update":
                        _teams[data.id] = data;
                        break;
                    case "teams" when op == "delete":
                        _teams.Remove(data.id);
                        break;
                    // for problems
                    case "problems" when op == "create":
                    case "problems" when op == "update":
                        _problems[data.id] = data;
                        break;
                    case "problems" when op == "delete":
                        _problems.Remove(data.id);
                        break;
                    // for submissions
                    case "submissions" when op == "create":
                        _submissions[data.id] = data;
                        _submissions[data.id].judgement_result = null;
                        break;
                    case "submissions" when op == "delete":
                        _submissions.Remove(data.id);
                        break;
                    case "judgements":
                        _submissions[data.submission_id].judgement_result = data.judgement_type_id;
                        break;
                    case "contests":
                        Contest = data;
                        break;
                    // ignored types
                    case "runs":
                    case "state":
                    case "awards":
                    case "languages":
                    case "clarifications":
                    case "judgement-types":
                        break;
                    default:
                        throw new Exception($"Unknown op type `{op}` for data type `{type}`");
                }
            }
            _parsed = true;
        }

        /// <summary>
        /// check teams' groups
        /// </summary>
        /// <returns>a list of team id which has wrong group ids</returns>
        public IEnumerable<string> CheckTeamGroups()
        {
            foreach (var (tId, team) in _teams)
            {
                foreach (var groupId in team.group_ids)
                {
                    if (!_groups.ContainsKey(groupId))
                    {
                        yield return tId;
                    }
                }
            }
        }

        /// <summary>
        /// Check teams' organization
        /// </summary>
        /// <returns>a list of team id which has a wrong organization id</returns>
        public IEnumerable<string> CheckTeamOrganizations()
        {
            foreach (var (tId, team) in _teams)
            {
                if (string.IsNullOrEmpty(team.organization_id) || !_schools.ContainsKey(team.organization_id))
                {
                    yield return tId;
                }
            }
        }

        /// <summary>
        /// check all submissions
        /// </summary>
        /// <returns>a list of submission id which has a wrong team id</returns>
        public IEnumerable<string> CheckSubmissions()
        {
            foreach (var (sId, submission) in _submissions)
            {
                if (!_teams.ContainsKey(submission.team_id))
                {
                    yield return sId;
                }
            }
        }

        /// <summary>
        /// check the problem of submission
        /// </summary>
        /// <returns>a list of submission id which has a wrong problem id</returns>
        public IEnumerable<string> CheckProblems()
        {
            foreach (var (sId, submission) in _submissions)
            {
                if (!_problems.ContainsKey(submission.problem_id))
                {
                    yield return sId;
                }
            }
        }

        /// <summary>
        /// check un-judged runs
        /// </summary>
        /// <returns>a list of submission id</returns>
        public IEnumerable<string> CheckUnJudgedRuns()
        {
            foreach (var (sId, submission) in _submissions)
            {
                if (string.IsNullOrEmpty(submission.judgement_result))
                {
                    yield return sId;
                }
            }
        }

        /// <summary>
        /// remove teams from team list
        /// </summary>
        /// <param name="tIds">the teams to be removed</param>
        public void RemoveTeams(List<string> tIds)
        {
            foreach (var tId in tIds)
            {
                _teams.Remove(tId);
            }
        }

        /// <summary>
        /// remove submission from team list
        /// </summary>
        /// <param name="sIds">the submissions to be removed</param>
        public void RemoveSubmissions(List<string> sIds)
        {
            foreach (var sId in sIds)
            {
                _submissions.Remove(sId);
            }
        }
    }
}