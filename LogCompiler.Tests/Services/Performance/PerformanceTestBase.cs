using LogCompiler.Tests.Data;
using LogCompilerBeta.Interfaces.ContentReader;
using System.Diagnostics;
using Xunit.Abstractions;

namespace LogCompiler.Tests.Services.Performance
{
    public class PerformanceTestBase
    {
        protected readonly ITestOutputHelper _testOutputHelper;
        protected readonly BaseTestFixture _fixture;

        public PerformanceTestBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _fixture = new BaseTestFixture();
        }

        public async Task<PerformanceTestResult> MeasurePerformanceAsync(
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

        public void OutputTestResult(PerformanceTestResult result)
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
            _testOutputHelper.WriteLine(message);
        }

        protected string CreatePerformanceTestFile(long sizeInBytes, string prefix = "")
        {
            var fileName = $"{prefix}_perf_{sizeInBytes}_{DateTime.Now:HHmmssfff}.tmp";
            var tempFile = Path.Combine(Path.GetTempPath(), fileName);
            TestData.CreateLargeTestFilePrecise(tempFile, sizeInBytes);
            // We'll manage cleanup through the fixture
            return _fixture.CreateTempFileWithSize(sizeInBytes);
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }

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
