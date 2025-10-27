namespace LogCompiler.Tests.Model
{
    public class PerformanceTestResult
    {
        public string TestName { get; set; } = string.Empty;
        public string ReaderType { get; set; } = string.Empty;
        public long ElapsedMilliseconds { get; set; }
        public long MemoryUsedBytes { get; set; }
        public int TotalLinesProcessed { get; set; }
        public int RejectCount { get; set; }
        public int ExecutionReportCount { get; set; }
    }
}
