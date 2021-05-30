using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;

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

        public EventFeedParser(string eventFeedFilePath)
        {
            _filePath = eventFeedFilePath;
        }

        public void Parse()
        {
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
                        break;
                    case "submissions" when op == "delete":
                        _submissions.Remove(data.id);
                        break;
                    case "judgements":
                        _submissions[data.submission_id].judgement_result = data.judgement_type_id;
                        break;
                    // ignored types
                    case "runs":
                    case "state":
                    case "awards":
                    case "contests":
                    case "languages":
                    case "clarifications":
                    case "judgement-types":
                        break;
                    default:
                        throw new Exception($"Unknown op type `{op}` for data type `{type}`");
                }
            }
        }
    }
}