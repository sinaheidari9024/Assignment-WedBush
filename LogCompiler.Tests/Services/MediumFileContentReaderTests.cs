using LogCompiler.Tests.Data;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogCompiler.Tests.Services
{
    public class MediumFileContentReaderTests : BaseTestFixture
    {
        private readonly Mock<ILogger<MediumFileContentReader>> _mockLogger;

        public MediumFileContentReaderTests()
        {
            _mockLogger = new Mock<ILogger<MediumFileContentReader>>();
        }

        [Fact]
        public async Task ReadAsync_WithValidFile_ProcessesInBatches()
        {
            // Arrange
            var lines = TestData.GenerateSampleLines(15000); 
            var filePath = CreateTempFile(lines);
            var reader = new MediumFileContentReader(_mockLogger.Object, batchSize: 5000);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.True(result.RejectMessages.Count > 0);
            Assert.True(result.ExecutionReportMessages.Count > 0);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Processed file")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task ReadAsync_WithInvalidBatchSize_ThrowsException()
        {
            // Arrange
            var reader = new MediumFileContentReader(_mockLogger.Object, batchSize: 0);
            var filePath = CreateTempFile(new[] { "test" });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => reader.ReadAsync(filePath));
        }

        [Fact]
        public void CanHandle_WithMediumFile_ReturnsTrue()
        {
            // Arrange
            var reader = new MediumFileContentReader(_mockLogger.Object);
            var fileInfo = new FileInfo(CreateTempFileWithSize(500 * 1024 * 1024)); // 500MB

            // Act
            var result = reader.CanHandle(fileInfo);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanHandle_WithSmallFile_ReturnsFalse()
        {
            // Arrange
            var reader = new MediumFileContentReader(_mockLogger.Object);
            var fileInfo = new FileInfo(CreateTempFileWithSize(100 * 1024 * 1024)); // 100MB

            // Act
            var result = reader.CanHandle(fileInfo);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ReadAsync_WithLargeDatasetAndRareRejects_ProcessesCorrectly()
        {
            // Arrange
            var totalLines = 100000;
            var expectedRejects = 10; // 1:10000 ratio
            var lines = TestData.GenerateSampleLinesWithSpecificRatio(totalLines, expectedRejects);

            var filePath = CreateTempFile(lines);
            var reader = new MediumFileContentReader(_mockLogger.Object, batchSize: 10000);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Equal(expectedRejects, result.RejectMessages.Count);
            Assert.Equal(totalLines - expectedRejects, result.ExecutionReportMessages.Count);
        }
    }
}
