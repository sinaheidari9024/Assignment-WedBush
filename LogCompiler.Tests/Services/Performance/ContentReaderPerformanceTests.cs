using LogCompiler.Tests.Data;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace LogCompiler.Tests.Services.Performance
{
    public class ContentReaderPerformanceTests : BaseTestFixture
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ContentReaderPerformanceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        // Test SmallFileContentReader with all file sizes
        [Theory]
        [InlineData(200 * 1024, "200KB")]
        [InlineData(2 * 1024 * 1024, "2MB")]
        [InlineData(20 * 1024 * 1024, "20MB")]
        [InlineData(200 * 1024 * 1024, "200MB")]
        public async Task SmallFileReader_AllSizes(long fileSize, string sizeLabel)
        {
            // Arrange
            var filePath = CreateTempFileWithSize(fileSize);
            var reader = new SmallFileContentReader(Mock.Of<ILogger<SmallFileContentReader>>());

            // Act
            var result = await PerformanceTestHelper.MeasurePerformanceAsync(
                reader, filePath, $"SmallFileReader - {sizeLabel}");

            // Assert & Output
            PerformanceTestHelper.OutputTestResult(_testOutputHelper, result);
            Assert.True(result.ElapsedMilliseconds > 0);
            Assert.True(result.TotalLinesProcessed > 0);
        }

        // Test MediumFileContentReader with different batch sizes
        [Theory]
        [InlineData(200 * 1024, 10000, "200KB")]
        [InlineData(200 * 1024, 50000, "200KB")]
        [InlineData(200 * 1024, 100000, "200KB")]
        [InlineData(200 * 1024, 250000, "200KB")]
        [InlineData(2 * 1024 * 1024, 10000, "2MB")]
        [InlineData(2 * 1024 * 1024, 50000, "2MB")]
        [InlineData(2 * 1024 * 1024, 100000, "2MB")]
        [InlineData(2 * 1024 * 1024, 250000, "2MB")]
        [InlineData(20 * 1024 * 1024, 10000, "20MB")]
        [InlineData(20 * 1024 * 1024, 50000, "20MB")]
        [InlineData(20 * 1024 * 1024, 100000, "20MB")]
        [InlineData(20 * 1024 * 1024, 250000, "20MB")]
        [InlineData(200 * 1024 * 1024, 10000, "200MB")]
        [InlineData(200 * 1024 * 1024, 50000, "200MB")]
        [InlineData(200 * 1024 * 1024, 100000, "200MB")]
        [InlineData(200 * 1024 * 1024, 250000, "200MB")]
        public async Task MediumFileReader_AllSizesAndBatchSizes(long fileSize, int batchSize, string sizeLabel)
        {
            // Arrange
            var filePath = CreateTempFileWithSize(fileSize);
            var reader = new MediumFileContentReader(
                Mock.Of<ILogger<MediumFileContentReader>>(), batchSize);

            // Act
            var result = await PerformanceTestHelper.MeasurePerformanceAsync(
                reader, filePath, $"MediumFileReader - {sizeLabel} - Batch: {batchSize}");

            // Assert & Output
            PerformanceTestHelper.OutputTestResult(_testOutputHelper, result);
            Assert.True(result.ElapsedMilliseconds > 0);
            Assert.True(result.TotalLinesProcessed > 0);
        }

        // Test LargeFileContentReader with different batch sizes
        [Theory]
        [InlineData(200 * 1024, 10000, "200KB")]
        [InlineData(200 * 1024, 50000, "200KB")]
        [InlineData(200 * 1024, 100000, "200KB")]
        [InlineData(200 * 1024, 250000, "200KB")]
        [InlineData(2 * 1024 * 1024, 10000, "2MB")]
        [InlineData(2 * 1024 * 1024, 50000, "2MB")]
        [InlineData(2 * 1024 * 1024, 100000, "2MB")]
        [InlineData(2 * 1024 * 1024, 250000, "2MB")]
        [InlineData(20 * 1024 * 1024, 10000, "20MB")]
        [InlineData(20 * 1024 * 1024, 50000, "20MB")]
        [InlineData(20 * 1024 * 1024, 100000, "20MB")]
        [InlineData(20 * 1024 * 1024, 250000, "20MB")]
        [InlineData(200 * 1024 * 1024, 10000, "200MB")]
        [InlineData(200 * 1024 * 1024, 50000, "200MB")]
        [InlineData(200 * 1024 * 1024, 100000, "200MB")]
        [InlineData(200 * 1024 * 1024, 250000, "200MB")]
        public async Task LargeFileReader_AllSizesAndBatchSizes(long fileSize, int batchSize, string sizeLabel)
        {
            // Arrange
            var filePath = CreateTempFileWithSize(fileSize);
            var reader = new LargeFileContentReader(
                Mock.Of<ILogger<LargeFileContentReader>>(), batchSize);

            // Act
            var result = await PerformanceTestHelper.MeasurePerformanceAsync(
                reader, filePath, $"LargeFileReader - {sizeLabel} - Batch: {batchSize}");

            // Assert & Output
            PerformanceTestHelper.OutputTestResult(_testOutputHelper, result);
            Assert.True(result.ElapsedMilliseconds > 0);
            Assert.True(result.TotalLinesProcessed > 0);
        }

        [Theory]
        [InlineData(200 * 1024, 10000, 2, "200KB")]
        [InlineData(200 * 1024, 50000, 2, "200KB")]
        [InlineData(200 * 1024, 100000, 2, "200KB")]
        [InlineData(200 * 1024, 250000, 2, "200KB")]
        [InlineData(200 * 1024, 10000, 4, "200KB")]
        [InlineData(200 * 1024, 50000, 4, "200KB")]
        [InlineData(200 * 1024, 100000, 4, "200KB")]
        [InlineData(200 * 1024, 250000, 4, "200KB")]
        [InlineData(2 * 1024 * 1024, 10000, 2, "2MB")]
        [InlineData(2 * 1024 * 1024, 50000, 2, "2MB")]
        [InlineData(2 * 1024 * 1024, 100000, 2, "2MB")]
        [InlineData(2 * 1024 * 1024, 250000, 2, "2MB")]
        [InlineData(2 * 1024 * 1024, 10000, 4, "2MB")]
        [InlineData(2 * 1024 * 1024, 50000, 4, "2MB")]
        [InlineData(2 * 1024 * 1024, 100000, 4, "2MB")]
        [InlineData(2 * 1024 * 1024, 250000, 4, "2MB")]
        [InlineData(20 * 1024 * 1024, 10000, 2, "20MB")]
        [InlineData(20 * 1024 * 1024, 50000, 2, "20MB")]
        [InlineData(20 * 1024 * 1024, 100000, 2, "20MB")]
        [InlineData(20 * 1024 * 1024, 250000, 2, "20MB")]
        [InlineData(20 * 1024 * 1024, 10000, 4, "20MB")]
        [InlineData(20 * 1024 * 1024, 50000, 4, "20MB")]
        [InlineData(20 * 1024 * 1024, 100000, 4, "20MB")]
        [InlineData(20 * 1024 * 1024, 250000, 4, "20MB")]
        [InlineData(200 * 1024 * 1024, 10000, 2, "200MB")]
        [InlineData(200 * 1024 * 1024, 50000, 2, "200MB")]
        [InlineData(200 * 1024 * 1024, 100000, 2, "200MB")]
        [InlineData(200 * 1024 * 1024, 250000, 2, "200MB")]
        [InlineData(200 * 1024 * 1024, 10000, 4, "200MB")]
        [InlineData(200 * 1024 * 1024, 50000, 4, "200MB")]
        [InlineData(200 * 1024 * 1024, 100000, 4, "200MB")]
        [InlineData(200 * 1024 * 1024, 250000, 4, "200MB")]
        public async Task VeryLargeFileReader_AllSizesAndConfigs(long fileSize, int batchSize, int maxParallelism, string sizeLabel)
        {
            // Arrange
            var filePath = CreateTempFileWithSize(fileSize);
            var reader = new VeryLargeFileContentReader(
                Mock.Of<ILogger<VeryLargeFileContentReader>>(), batchSize, maxParallelism);

            // Act
            var result = await PerformanceTestHelper.MeasurePerformanceAsync(
                reader, filePath, $"VeryLargeFileReader - {sizeLabel} - Batch: {batchSize} - Parallelism: {maxParallelism}");

            // Assert & Output
            PerformanceTestHelper.OutputTestResult(_testOutputHelper, result);
            Assert.True(result.ElapsedMilliseconds > 0);
            Assert.True(result.TotalLinesProcessed > 0);
        }
    }
}
