using System.Collections.Generic;
using System.Linq;
using IcpcResolver.UserControl;
using Newtonsoft.Json;

namespace IcpcResolver.Window
{
    public class TeamRankInfo
    {
        [JsonConstructor]
        public TeamRankInfo()
        {
            
        }

        public TeamRankInfo(dynamic baseInfo)
        {
            GroupIds = (baseInfo.group_ids as List<object>)?.Select(x => x as string).ToList();
            Id = baseInfo.id;
            Name = baseInfo.name;
            OrganizationId = baseInfo.organization_id;
        }
        public List<string> GroupIds;
        public string Id;
        public string Name;
        public string OrganizationId;
        public int AcceptCount;
        public int Penalty;
        public List<SubmissionInfo> SubmissionInfosBefore;
        public List<SubmissionInfo> SubmissionInfosAfter;
        public List<string> AwardName = new();

        public TeamDto ToTeamDto()
        {
            // generate status `from`
            var problemDtoFrom = SubmissionInfosBefore.Select(submissionInfo => new ProblemDto
            {
                Label = submissionInfo.ProblemLabel, Status = ConvertStatus(submissionInfo.SubmissionStatus),
                Time = submissionInfo.SubmissionTime is null ? 0 : submissionInfo.GetIntSubmissionTime(),
                Try = ConvertStatus(submissionInfo.SubmissionStatus) == ProblemStatus.Accept
                    ? submissionInfo.TryTime + 2
                    : submissionInfo.TryTime + 1
            }).ToList();

            // generate status `to`
            var problemDtoTo = SubmissionInfosAfter.Select(submissionInfo => new ProblemDto
            {
                Label = submissionInfo.ProblemLabel, Status = ConvertStatus(submissionInfo.SubmissionStatus),
                Time = submissionInfo.SubmissionTime is null ? 0 : submissionInfo.GetIntSubmissionTime(),
                Try = ConvertStatus(submissionInfo.SubmissionStatus) == ProblemStatus.Accept
                    ? submissionInfo.TryTime + 2
                    : submissionInfo.TryTime + 1
            }).ToList();

            return new TeamDto
            {
                TeamId = Id,
                TeamName = Name,
                SchoolName = OrganizationId,
                Awards = AwardName.Select(a =>
                {
                    return a switch
                    {
                        "Gold Medal" => "Gold Medal|medalist",
                        "Silver Medal" => "Silver Medal|medalist",
                        "Bronze Medal" => "Bronze Medal|medalist",
                        _ => a + "|normal"
                    };
                }).ToList(),
                ProblemsFrom = problemDtoFrom.OrderBy(p => p.Label).ToList(),
                ProblemsTo = problemDtoTo.OrderBy(p => p.Label).ToList()
            };
        }

        private static ProblemStatus ConvertStatus(string inStatus)
        {
            return inStatus switch
            {
                null => ProblemStatus.NotTried,
                "FB" => ProblemStatus.FirstBlood,
                "AC" => ProblemStatus.Accept,
                _ => ProblemStatus.UnAccept
            };
        }

    }
}