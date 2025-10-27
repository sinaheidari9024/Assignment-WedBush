namespace LogCompiler.Tests.Services
{
    using LogCompiler.Tests.Performance;
    using LogCompilerBeta.Interfaces.ContentReader;
    using System.Diagnostics;
    // PerformanceTestHelper.cs
    using Xunit.Abstractions;

    public static class PerformanceTestHelper
    {
        public static async Task<PerformanceTestResult> MeasurePerformanceAsync(
            IContentReader reader,
            string filePath,
            string testName)
        {
            var stopwatch = Stopwatch.StartNew();
            var memoryBefore = GC.GetTotalMemory(true);

            var result = await reader.ReadAsync(filePath);

            stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);
            GC.Collect();

            return new PerformanceTestResult
            {
                TestName = testName,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                MemoryUsedBytes = Math.Max(0, memoryAfter - memoryBefore),
                TotalLinesProcessed = result.RejectMessages.Count + result.ExecutionReportMessages.Count,
                RejectCount = result.RejectMessages.Count,
                ExecutionReportCount = result.ExecutionReportMessages.Count,
                ReaderType = reader.GetType().Name
            };
        }

        public static void OutputTestResult(ITestOutputHelper output, PerformanceTestResult result)
        {
            var message = $@"
                {result.TestName}
                  Reader: {result.ReaderType}
                  Time: {result.ElapsedMilliseconds} ms
                  Memory: {result.MemoryUsedBytes / 1024.0:N2} KB
                  Total Lines: {result.TotalLinesProcessed}
                  Rejects: {result.RejectCount}
                  Execution Reports: {result.ExecutionReportCount}
                  Throughput: {result.TotalLinesProcessed / Math.Max(result.ElapsedMilliseconds, 1) * 1000:N0} lines/sec
                ";
            output.WriteLine(message);
        }
    }
}
