using LogCompiler.Tests.Data;
using LogCompilerBeta.Interfaces.ContentReader;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace LogCompiler.Tests.Performance
{
    public class ComparativePerformanceTests : IClassFixture<BaseTestFixture>, IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly BaseTestFixture _fixture;
        private readonly PerformanceTestBase _performanceBase;

        public ComparativePerformanceTests(ITestOutputHelper testOutputHelper, BaseTestFixture fixture)
        {
            _testOutputHelper = testOutputHelper;
            _fixture = fixture;
            _performanceBase = new PerformanceTestBase(testOutputHelper);
        }

        [Fact]
        public async Task CompareAllReaders_200KB_File()
        {
            await CompareAllReadersForSize(200 * 1024, "200KB");
        }

        [Fact]
        public async Task CompareAllReaders_2MB_File()
        {
            await CompareAllReadersForSize(2 * 1024 * 1024, "2MB");
        }

        [Fact]
        public async Task CompareAllReaders_20MB_File()
        {
            await CompareAllReadersForSize(20 * 1024 * 1024, "20MB");
        }

        [Fact]
        public async Task CompareAllReaders_200MB_File()
        {
            await CompareAllReadersForSize(200 * 1024 * 1024, "200MB");
        }

        private async Task CompareAllReadersForSize(long fileSize, string sizeLabel)
        {
            // Arrange
            var filePath = _fixture.CreateTempFileWithSize(fileSize);
            var batchSizes = new[] { 10000, 50000, 100000, 250000 };
            var allResults = new List<PerformanceTestResult>();

            _testOutputHelper.WriteLine($"\n=== {sizeLabel} PERFORMANCE COMPARISON ===");

            // Act - Test all readers with all batch sizes
            foreach (var batchSize in batchSizes)
            {
                var readers = CreateAllReaders(batchSize);

                foreach (var reader in readers)
                {
                    if (reader.CanHandle(new FileInfo(filePath)))
                    {
                        var result = await _performanceBase.MeasurePerformanceAsync(
                            reader,
                            filePath,
                            $"{sizeLabel} - {reader.GetType().Name} - Batch: {batchSize}");

                        allResults.Add(result);
                        _testOutputHelper.WriteLine(FormatComparativeResult(result));
                    }
                }
            }

            // Output summary
            if (allResults.Any())
            {
                var bestPerformer = allResults.OrderBy(r => r.ElapsedMilliseconds).First();
                _testOutputHelper.WriteLine($"\nFastest: {bestPerformer.ReaderType} - {bestPerformer.ElapsedMilliseconds} ms");

                var mostEfficient = allResults.OrderBy(r => r.MemoryUsedBytes).First();
                _testOutputHelper.WriteLine($"Most Memory Efficient: {mostEfficient.ReaderType} - {mostEfficient.MemoryUsedBytes / 1024:N2} KB");
            }
        }

        private List<IContentReader> CreateAllReaders(int batchSize)
        {
            return new List<IContentReader>
        {
            new SmallFileContentReader(Mock.Of<ILogger<SmallFileContentReader>>()),
            new MediumFileContentReader(Mock.Of<ILogger<MediumFileContentReader>>(), batchSize),
            new LargeFileContentReader(Mock.Of<ILogger<LargeFileContentReader>>(), batchSize),
            new VeryLargeFileContentReader(Mock.Of<ILogger<VeryLargeFileContentReader>>(), batchSize, 4)
        };
        }

        private string FormatComparativeResult(PerformanceTestResult result)
        {
            return $"{result.ReaderType,-25} | Batch: {result.TestName.Split("Batch: ").Last(),-8} | Time: {result.ElapsedMilliseconds,6} ms | Memory: {result.MemoryUsedBytes / 1024,6} KB | Throughput: {result.TotalLinesProcessed / Math.Max(result.ElapsedMilliseconds, 1) * 1000,8:N0} lines/sec";
        }

        public void Dispose()
        {
            _performanceBase?.Dispose();
        }
    }

}
