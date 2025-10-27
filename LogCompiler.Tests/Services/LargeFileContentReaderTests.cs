using LogCompiler.Tests.Data;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogCompiler.Tests.Services
{
    // LargeFileContentReaderTests.cs
    public class LargeFileContentReaderTests : BaseTestFixture
    {
        private readonly Mock<ILogger<LargeFileContentReader>> _mockLogger;

        public LargeFileContentReaderTests()
        {
            _mockLogger = new Mock<ILogger<LargeFileContentReader>>();
        }

        [Fact]
        public async Task ReadAsync_WithValidFile_ProcessesParallelBatches()
        {
            // Arrange
            var lines = TestData.GenerateSampleLines(250000); // Large number of lines
            var filePath = CreateTempFile(lines);
            var reader = new LargeFileContentReader(_mockLogger.Object, batchSize: 50000);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.True(result.RejectMessages.Count > 0);
            Assert.True(result.ExecutionReportMessages.Count > 0);
        }

        [Fact]
        public async Task ReadAsync_WithEmptyLines_SkipsEmptyLines()
        {
            // Arrange
            var lines = new List<string>
        {
            "",
            "2023-12-01 10:00:00.000 8=FIX.4.2|35=3|49=TEST",
            "   ",
            "2023-12-01 10:00:01.000 8=FIX.4.2|35=8|49=TEST",
            null
        }.Where(x => x != null).ToList()!;

            var filePath = CreateTempFile(lines);
            var reader = new LargeFileContentReader(_mockLogger.Object, batchSize: 1000);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Single(result.RejectMessages);
            Assert.Single(result.ExecutionReportMessages);
        }

        //[Fact]
        //public void CanHandle_WithLargeFile_ReturnsTrue()
        //{
        //    // Arrange
        //    var reader = new LargeFileContentReader(_mockLogger.Object);
        //    var fileInfo = new FileInfo(CreateTempFileWithSize(1500 * 1024 * 1024)); // 1.5GB

        //    // Act
        //    var result = reader.CanHandle(fileInfo);

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public void CanHandle_WithVeryLargeFile_ReturnsFalse()
        //{
        //    // Arrange
        //    var reader = new LargeFileContentReader(_mockLogger.Object);
        //    var fileInfo = new FileInfo(CreateTempFileWithSize(3L * 1024 * 1024 * 1024)); // 3GB

        //    // Act
        //    var result = reader.CanHandle(fileInfo);

        //    // Assert
        //    Assert.False(result);
        //}


        [Fact]
        public async Task ReadAsync_WithRareRejectsInParallel_CorrectlyAggregatesResults()
        {
            // Arrange
            var totalLines = 500000;
            var expectedRejects = 50; // 1:10000 ratio
            var lines = TestData.GenerateSampleLinesWithSpecificRatio(totalLines, expectedRejects);

            var filePath = CreateTempFile(lines);
            var reader = new LargeFileContentReader(_mockLogger.Object, batchSize: 50000);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Equal(expectedRejects, result.RejectMessages.Count);
            Assert.Equal(totalLines - expectedRejects, result.ExecutionReportMessages.Count);

            // Verify no duplicates and all messages are processed
            var totalProcessed = result.RejectMessages.Count + result.ExecutionReportMessages.Count;
            Assert.Equal(totalLines, totalProcessed);
        }
    }
}
