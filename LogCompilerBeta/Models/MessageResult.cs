using LogCompilerBeta.Entities.YourProjectName.Models;

namespace LogCompilerBeta.Models
{
    public class MessageResult
    {
        public List<OriginalMessage> Messages { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
