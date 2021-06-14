using Newtonsoft.Json;

namespace IcpcResolver.Window
{
    public class SubmissionInfo
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

        [JsonConstructor]
        public SubmissionInfo()
        {
        }

        public SubmissionInfo(string id, string label, int tries)
        {
            ProblemId = id;
            ProblemLabel = label;
            TryTime = tries;
            SubmissionStatus = null;
            SubmissionTime = null;
        }
    }
}