using LogCompiler.Tests.Data;
using LogCompiler.Tests.Performance;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace LogCompiler.Tests.Services.Performance
{
    public class ComparativePerformanceTests : BaseTestFixture
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ComparativePerformanceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(200 * 1024, "200KB")]
        [InlineData(2 * 1024 * 1024, "2MB")]
        [InlineData(20 * 1024 * 1024, "20MB")]
        [InlineData(200 * 1024 * 1024, "200MB")]
        public async Task CompareAllReaders_SameFileSize(long fileSize, string sizeLabel)
        {
            _testOutputHelper.WriteLine($"\n=== COMPARING ALL READERS FOR {sizeLabel} ===");

            var filePath = CreateTempFileWithSize(fileSize);
            var results = new List<PerformanceTestResult>();

            // Test SmallFileReader
            var smallReader = new SmallFileContentReader(Mock.Of<ILogger<SmallFileContentReader>>());
            var smallResult = await PerformanceTestHelper.MeasurePerformanceAsync(
                smallReader, filePath, $"SmallFileReader - {sizeLabel}");
            results.Add(smallResult);

            // Test MediumFileReader with optimal batch size
            var mediumReader = new MediumFileContentReader(Mock.Of<ILogger<MediumFileContentReader>>(), 10000);
            var mediumResult = await PerformanceTestHelper.MeasurePerformanceAsync(
                mediumReader, filePath, $"MediumFileReader - {sizeLabel} - Batch: 10000");
            results.Add(mediumResult);

            // Test LargeFileReader with optimal batch size
            var largeReader = new LargeFileContentReader(Mock.Of<ILogger<LargeFileContentReader>>(), 50000);
            var largeResult = await PerformanceTestHelper.MeasurePerformanceAsync(
                largeReader, filePath, $"LargeFileReader - {sizeLabel} - Batch: 50000");
            results.Add(largeResult);

            // Test VeryLargeFileReader with optimal config
            var veryLargeReader = new VeryLargeFileContentReader(Mock.Of<ILogger<VeryLargeFileContentReader>>(), 100000, 4);
            var veryLargeResult = await PerformanceTestHelper.MeasurePerformanceAsync(
                veryLargeReader, filePath, $"VeryLargeFileReader - {sizeLabel} - Batch: 100000 - Parallelism: 4");
            results.Add(veryLargeResult);

            // Output comparison
            _testOutputHelper.WriteLine($"\n--- {sizeLabel} PERFORMANCE SUMMARY ---");
            foreach (var result in results.OrderBy(r => r.ElapsedMilliseconds))
            {
                _testOutputHelper.WriteLine(
                    $"{result.ReaderType,-25} | Time: {result.ElapsedMilliseconds,6} ms | " +
                    $"Memory: {result.MemoryUsedBytes / 1024,6} KB | " +
                    $"Throughput: {result.TotalLinesProcessed / Math.Max(result.ElapsedMilliseconds, 1) * 1000,8:N0} lines/sec");
            }

            var fastest = results.OrderBy(r => r.ElapsedMilliseconds).First();
            _testOutputHelper.WriteLine($"\n🏆 FASTEST: {fastest.ReaderType} - {fastest.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task BatchSizeComparison_20MB_File()
        {
            var fileSize = 20 * 1024 * 1024;
            var filePath = CreateTempFileWithSize(fileSize);
            var batchSizes = new[] { 1000, 5000, 10000, 25000, 50000, 100000, 250000 };

            _testOutputHelper.WriteLine($"\n=== BATCH SIZE COMPARISON FOR 20MB FILE ===");

            foreach (var batchSize in batchSizes)
            {
                var reader = new LargeFileContentReader(Mock.Of<ILogger<LargeFileContentReader>>(), batchSize);
                var result = await PerformanceTestHelper.MeasurePerformanceAsync(
                    reader, filePath, $"Batch: {batchSize}");

                _testOutputHelper.WriteLine(
                    $"Batch: {batchSize,7} | Time: {result.ElapsedMilliseconds,6} ms | " +
                    $"Memory: {result.MemoryUsedBytes / 1024,6} KB | " +
                    $"Throughput: {result.TotalLinesProcessed / Math.Max(result.ElapsedMilliseconds, 1) * 1000,8:N0} lines/sec");
            }
        }

        [Fact]
        public async Task ParallelismComparison_200MB_File()
        {
            var fileSize = 200 * 1024 * 1024;
            var filePath = CreateTempFileWithSize(fileSize);
            var parallelismLevels = new[] { 1, 2, 4, 8, 16 };

            _testOutputHelper.WriteLine($"\n=== PARALLELISM COMPARISON FOR 200MB FILE ===");

            foreach (var parallelism in parallelismLevels)
            {
                var reader = new VeryLargeFileContentReader(
                    Mock.Of<ILogger<VeryLargeFileContentReader>>(), 100000, parallelism);

                var result = await PerformanceTestHelper.MeasurePerformanceAsync(
                    reader, filePath, $"Parallelism: {parallelism}");

                _testOutputHelper.WriteLine(
                    $"Parallelism: {parallelism,2} | Time: {result.ElapsedMilliseconds,6} ms | " +
                    $"Memory: {result.MemoryUsedBytes / 1024,6} KB | " +
                    $"Throughput: {result.TotalLinesProcessed / Math.Max(result.ElapsedMilliseconds, 1) * 1000,8:N0} lines/sec");
            }
        }
    }
}
