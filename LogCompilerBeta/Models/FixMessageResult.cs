namespace LogCompilerBeta.Models
{
    public class FixMessageResult
    {
        public List<string> RejectMessages { get; set; } = new List<string>();
        public List<string> ExecutionReportMessages { get; set; } = new List<string>();
        public int TotalCount => RejectMessages.Count + ExecutionReportMessages.Count;
    }
}
