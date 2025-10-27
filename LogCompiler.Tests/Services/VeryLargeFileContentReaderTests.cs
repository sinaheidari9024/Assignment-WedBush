using LogCompiler.Tests.Data;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogCompiler.Tests.Services
{
    public class VeryLargeFileContentReaderTests : BaseTestFixture
    {
        private readonly Mock<ILogger<VeryLargeFileContentReader>> _mockLogger;

        public VeryLargeFileContentReaderTests()
        {
            _mockLogger = new Mock<ILogger<VeryLargeFileContentReader>>();
        }

        //[Fact]
        //public async Task ReadAsync_WithValidFile_UsesChannelProcessing()
        //{
        //    // Arrange
        //    var lines = TestData.GenerateSampleLines(100000);
        //    var filePath = CreateTempFile(lines);
        //    var reader = new VeryLargeFileContentReader(_mockLogger.Object, batchSize: 10000, maxDegreeOfParallelism: 2);

        //    // Act
        //    var result = await reader.ReadAsync(filePath);

        //    // Assert
        //    Assert.True(result.RejectMessages.Count > 0);
        //    Assert.True(result.ExecutionReportMessages.Count > 0);
        //}

        [Fact]
        public async Task ReadAsync_WithDifferentParallelism_CompletesSuccessfully()
        {
            // Arrange
            var lines = TestData.GenerateSampleLines(50000);
            var filePath = CreateTempFile(lines);
            var reader = new VeryLargeFileContentReader(_mockLogger.Object, maxDegreeOfParallelism: 8);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.NotNull(result);
        }

        //[Fact]
        //public void CanHandle_WithVeryLargeFile_ReturnsTrue()
        //{
        //    // Arrange
        //    var reader = new VeryLargeFileContentReader(_mockLogger.Object);
        //    var fileInfo = new FileInfo(CreateTempFileWithSize(3L * 1024 * 1024 * 1024)); // 3GB

        //    // Act
        //    var result = reader.CanHandle(fileInfo);

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public void CanHandle_WithLargeFile_ReturnsFalse()
        //{
        //    // Arrange
        //    var reader = new VeryLargeFileContentReader(_mockLogger.Object);
        //    var fileInfo = new FileInfo(CreateTempFileWithSize(1500 * 1024 * 1024)); // 1.5GB

        //    // Act
        //    var result = reader.CanHandle(fileInfo);

        //    // Assert
        //    Assert.False(result);
        //}

        [Fact]
        public async Task ReadAsync_WithChannelsAndRareRejects_CorrectlyDistributesWork()
        {
            // Arrange
            var totalLines = 1000000; // 1 million lines
            var expectedRejects = 100; // 1:10000 ratio
            var lines = TestData.GenerateSampleLinesWithSpecificRatio(totalLines, expectedRejects);

            var filePath = CreateTempFile(lines);
            var reader = new VeryLargeFileContentReader(_mockLogger.Object, batchSize: 100000, maxDegreeOfParallelism: 4);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Equal(expectedRejects, result.RejectMessages.Count);
            Assert.Equal(totalLines - expectedRejects, result.ExecutionReportMessages.Count);
        }

    }
}
