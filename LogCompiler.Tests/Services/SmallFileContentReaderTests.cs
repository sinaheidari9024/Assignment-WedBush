using LogCompiler.Tests.Data;
using LogCompilerBeta.Services.ContentReaders;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogCompiler.Tests.Services
{
    public class SmallFileContentReaderTests : BaseTestFixture
    {
        private readonly Mock<ILogger<SmallFileContentReader>> _mockLogger;

        public SmallFileContentReaderTests()
        {
            _mockLogger = new Mock<ILogger<SmallFileContentReader>>();
        }

        [Fact]
        public async Task ReadAsync_WithValidFile_ProcessesAllLines()
        {
            // Arrange
            var lines = new List<string>
        {
            "2023-12-01 10:00:00.000 8=FIX.4.2|9=100|35=3|49=SENDER|56=TARGET|34=1|10=001",
            "2023-12-01 10:00:01.000 8=FIX.4.2|9=100|35=8|49=SENDER|56=TARGET|34=2|10=002",
            "2023-12-01 10:00:02.000 8=FIX.4.2|9=100|35=3|49=SENDER|56=TARGET|34=3|10=003",
            "2023-12-01 10:00:03.000 Invalid line without FIX",
            "2023-12-01 10:00:04.000 8=FIX.4.2|9=100|35=8|49=SENDER|56=TARGET|34=4|10=004"
        };

            var filePath = CreateTempFile(lines);
            var reader = new SmallFileContentReader(_mockLogger.Object);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.True(result.RejectMessages.Count == 2); // Two messages with 35=3
            Assert.True(result.ExecutionReportMessages.Count == 2); // Two messages with 35=8
        }

        [Fact]
        public async Task ReadAsync_WithEmptyFile_ReturnsEmptyResults()
        {
            // Arrange
            var filePath = CreateTempFile(Array.Empty<string>());
            var reader = new SmallFileContentReader(_mockLogger.Object);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Empty(result.RejectMessages);
            Assert.Empty(result.ExecutionReportMessages);
        }

        [Fact]
        public void CanHandle_WithSmallFile_ReturnsTrue()
        {
            // Arrange
            var reader = new SmallFileContentReader(_mockLogger.Object);
            var fileInfo = new FileInfo(CreateTempFileWithSize(100 * 1024 * 1024)); // 100MB

            // Act
            var result = reader.CanHandle(fileInfo);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanHandle_WithLargeFile_ReturnsFalse()
        {
            // Arrange
            var reader = new SmallFileContentReader(_mockLogger.Object);
            var fileInfo = new FileInfo(CreateTempFileWithSize(300 * 1024 * 1024)); // 300MB

            // Act
            var result = reader.CanHandle(fileInfo);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public async Task ReadAsync_With1To10000Ratio_CorrectlyIdentifiesRareRejects()
        {
            // Arrange
            var totalLines = 50000;
            var expectedRejects = 5; // 1:10000 ratio for 50,000 lines
            var lines = TestData.GenerateSampleLinesWithSpecificRatio(totalLines, expectedRejects);

            var filePath = CreateTempFile(lines);
            var reader = new SmallFileContentReader(_mockLogger.Object);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Equal(expectedRejects, result.RejectMessages.Count);
            Assert.Equal(totalLines - expectedRejects, result.ExecutionReportMessages.Count);

            // Verify all reject messages contain 35=3
            Assert.All(result.RejectMessages, msg => Assert.Contains("35=3", msg));

            // Verify all execution report messages contain 35=8
            Assert.All(result.ExecutionReportMessages, msg => Assert.Contains("35=8", msg));
        }

        [Fact]
        public async Task ReadAsync_WithNoRejects_HandlesZeroRejectCase()
        {
            // Arrange
            var lines = TestData.GenerateSampleLinesWithSpecificRatio(10000, 0); // 10,000 lines, 0 rejects

            var filePath = CreateTempFile(lines);
            var reader = new SmallFileContentReader(_mockLogger.Object);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Empty(result.RejectMessages);
            Assert.Equal(10000, result.ExecutionReportMessages.Count);
        }

        [Fact]
        public async Task ReadAsync_WithSingleReject_FindsTheNeedleInHaystack()
        {
            // Arrange
            var lines = TestData.GenerateSampleLinesWithSpecificRatio(10000, 1); // 1 reject in 10,000 lines

            var filePath = CreateTempFile(lines);
            var reader = new SmallFileContentReader(_mockLogger.Object);

            // Act
            var result = await reader.ReadAsync(filePath);

            // Assert
            Assert.Single(result.RejectMessages);
            Assert.Equal(9999, result.ExecutionReportMessages.Count);
            Assert.Contains("35=3", result.RejectMessages.First());
        }
    }
}
