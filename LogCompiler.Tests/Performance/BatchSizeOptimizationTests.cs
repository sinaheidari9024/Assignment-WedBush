using LogCompiler.Tests.Data;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace LogCompiler.Tests.Performance
{
    public class BatchSizeOptimizationTests : IClassFixture<BaseTestFixture>, IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly BaseTestFixture _fixture;
        private readonly PerformanceTestBase _performanceBase;

        public BatchSizeOptimizationTests(ITestOutputHelper testOutputHelper, BaseTestFixture fixture)
        {
            _testOutputHelper = testOutputHelper;
            _fixture = fixture;
            _performanceBase = new PerformanceTestBase(testOutputHelper);
        }

        [Theory]
        [InlineData(200 * 1024)]      // 200KB
        [InlineData(2 * 1024 * 1024)] // 2MB
        [InlineData(20 * 1024 * 1024)] // 20MB
        [InlineData(200 * 1024 * 1024)] // 200MB
        public async Task FindOptimalBatchSize_ForDifferentFileSizes(long fileSize)
        {
            var filePath = _fixture.CreateTempFileWithSize(fileSize);
            var batchSizes = new[] { 1000, 5000, 10000, 25000, 50000, 75000, 100000, 150000, 200000, 250000, 500000 };
            var results = new List<PerformanceTestResult>();

            _testOutputHelper.WriteLine($"\n=== BATCH SIZE OPTIMIZATION FOR {fileSize / 1024 / 1024}MB FILE ===");

            foreach (var batchSize in batchSizes)
            {
                var reader = new LargeFileContentReader(
                    Mock.Of<ILogger<LargeFileContentReader>>(), batchSize);

                if (reader.CanHandle(new FileInfo(filePath)))
                {
                    var result = await _performanceBase.MeasurePerformanceAsync(
                        reader, filePath, $"Batch Size: {batchSize}");

                    results.Add(result);

                    _testOutputHelper.WriteLine(
                        $"Batch: {batchSize,7} | Time: {result.ElapsedMilliseconds,6} ms | " +
                        $"Memory: {result.MemoryUsedBytes / 1024,6} KB | " +
                        $"Throughput: {result.TotalLinesProcessed / Math.Max(result.ElapsedMilliseconds, 1) * 1000,8:N0} lines/sec");
                }
            }

            if (results.Any())
            {
                var optimalBatchSize = results
                    .OrderBy(r => r.ElapsedMilliseconds)
                    .First();

                _testOutputHelper.WriteLine($"\nOPTIMAL BATCH SIZE: {optimalBatchSize.TestName.Split(':').Last().Trim()}");
                _testOutputHelper.WriteLine($"Performance: {optimalBatchSize.ElapsedMilliseconds} ms");
            }
        }

        public void Dispose()
        {
            _performanceBase?.Dispose();
        }
    }

}
