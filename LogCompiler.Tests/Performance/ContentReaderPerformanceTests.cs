using LogCompiler.Tests.Data;
using LogCompilerBeta.Interfaces.ContentReader;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace LogCompiler.Tests.Performance
{
    // ContentReaderPerformanceTests.cs
    public class ContentReaderPerformanceTests : IClassFixture<BaseTestFixture>, IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly BaseTestFixture _fixture;
        private readonly PerformanceTestBase _performanceBase;

        public ContentReaderPerformanceTests(ITestOutputHelper testOutputHelper, BaseTestFixture fixture)
        {
            _testOutputHelper = testOutputHelper;
            _fixture = fixture;
            _performanceBase = new PerformanceTestBase(testOutputHelper);
        }

        // Test Case 1: 200KB file with different batch sizes
        [Theory]
        [InlineData(10000)]
        [InlineData(50000)]
        [InlineData(100000)]
        [InlineData(250000)]
        public async Task SmallFile_200KB_WithBatchSize(int batchSize)
        {
            await RunPerformanceTest(200 * 1024, batchSize, "200KB");
        }

        // Test Case 2: 2MB file with different batch sizes
        [Theory]
        [InlineData(10000)]
        [InlineData(50000)]
        [InlineData(100000)]
        [InlineData(250000)]
        public async Task MediumFile_2MB_WithBatchSize(int batchSize)
        {
            await RunPerformanceTest(2 * 1024 * 1024, batchSize, "2MB");
        }

        // Test Case 3: 20MB file with different batch sizes
        [Theory]
        [InlineData(10000)]
        [InlineData(50000)]
        [InlineData(100000)]
        [InlineData(250000)]
        public async Task LargeFile_20MB_WithBatchSize(int batchSize)
        {
            await RunPerformanceTest(20 * 1024 * 1024, batchSize, "20MB");
        }

        // Test Case 4: 200MB file with different batch sizes
        [Theory]
        [InlineData(10000)]
        [InlineData(50000)]
        [InlineData(100000)]
        [InlineData(250000)]
        public async Task VeryLargeFile_200MB_WithBatchSize(int batchSize)
        {
            await RunPerformanceTest(200 * 1024 * 1024, batchSize, "200MB");
        }

        private async Task RunPerformanceTest(long fileSize, int batchSize, string sizeLabel)
        {
            // Arrange
            var filePath = _fixture.CreateTempFileWithSize(fileSize);
            var fileInfo = new FileInfo(filePath);

            var readers = CreateReadersForFile(fileInfo, batchSize);
            var results = new List<PerformanceTestResult>();

            // Act - Test each applicable reader
            foreach (var reader in readers)
            {
                var result = await _performanceBase.MeasurePerformanceAsync(
                    reader,
                    filePath,
                    $"{sizeLabel} - Batch: {batchSize} - {reader.GetType().Name}");

                results.Add(result);
                _performanceBase.OutputTestResult(result);
            }

            // Assert - Basic validation that all readers processed the file correctly
            foreach (var result in results)
            {
                Assert.True(result.ElapsedMilliseconds > 0, "Processing time should be positive");
                Assert.True(result.TotalLinesProcessed > 0, "Should process at least some lines");
                Assert.True(result.RejectCount >= 0, "Reject count should be non-negative");
            }
        }

        private List<IContentReader> CreateReadersForFile(FileInfo fileInfo, int batchSize)
        {
            var readers = new List<IContentReader>();

            // SmallFileContentReader - for files up to 200MB
            var smallReader = new SmallFileContentReader(Mock.Of<ILogger<SmallFileContentReader>>());
            if (smallReader.CanHandle(fileInfo))
                readers.Add(smallReader);

            // MediumFileContentReader
            var mediumReader = new MediumFileContentReader(
                Mock.Of<ILogger<MediumFileContentReader>>(), batchSize);
            if (mediumReader.CanHandle(fileInfo))
                readers.Add(mediumReader);

            // LargeFileContentReader
            var largeReader = new LargeFileContentReader(
                Mock.Of<ILogger<LargeFileContentReader>>(), batchSize);
            if (largeReader.CanHandle(fileInfo))
                readers.Add(largeReader);

            // VeryLargeFileContentReader
            var veryLargeReader = new VeryLargeFileContentReader(
                Mock.Of<ILogger<VeryLargeFileContentReader>>(), batchSize, 4);
            if (veryLargeReader.CanHandle(fileInfo))
                readers.Add(veryLargeReader);

            return readers;
        }

        public void Dispose()
        {
            _performanceBase?.Dispose();
        }
    }
}
