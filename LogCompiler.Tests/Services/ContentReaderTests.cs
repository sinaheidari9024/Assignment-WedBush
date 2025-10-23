using Xunit;
using FluentAssertions;
using Moq;
using LogCompilerBeta.Services;
using Microsoft.Extensions.Logging;
namespace LogCompiler.Tests.Services
{
    public class ContentReaderTests
    {
        private readonly ContentReader _sut; 
        private readonly Mock<ILogger<ContentReader>> _loggerMock;

        public ContentReaderTests()
        {
            _loggerMock = new Mock<ILogger<ContentReader>>();
            _sut = new ContentReader(_loggerMock.Object);
        }

        [Fact]
        public async Task ReadAllAtOnceOptimizedAsync_ValidFileWithMixedMessages_ReturnsCorrectlyCategorizedMessages()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var fileContent = new[]
                {
            "2024-01-01 10:00:00 some text 8=FIX.4.4|9=150|35=8|49=Broker|56=Market|34=2|52=20240101-10:00:00|",
            "2024-01-01 10:00:01 other text 8=FIX.4.4|9=120|35=3|49=Broker|56=Market|34=3|52=20240101-10:00:01|",
            "2024-01-01 10:00:02 another line 8=FIX.4.4|9=130|35=8|49=Broker|56=Market|34=4|52=20240101-10:00:02|",
            "2024-01-01 10:00:03 line without fix message",
            "",
            "2024-01-01 10:00:04 8=FIX.4.4|9=140|35=3|49=Broker|56=Market|34=5|52=20240101-10:00:04|"
        };

                await File.WriteAllLinesAsync(tempFile, fileContent);

                // Act
                var result = await _sut.ReadAllAtOnceOptimizedAsync(tempFile);

                // Assert
                result.Should().NotBeNull();
                result.ExecutionReportMessages.Should().HaveCount(2);
                result.RejectMessages.Should().HaveCount(2);

                result.ExecutionReportMessages[0].Should().Contain("35=8");
                result.ExecutionReportMessages[1].Should().Contain("35=8");
                result.RejectMessages[0].Should().Contain("35=3");
                result.RejectMessages[1].Should().Contain("35=3");

                // Verify logging
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Read 2 reject messages and 2 execution report messages")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                    Times.Once);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ReadAllAtOnceOptimizedAsync_FileWithOnlyExecutionReports_ReturnsOnlyExecutionReports()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var fileContent = new[]
                {
            "2024-01-01 10:00:00 8=FIX.4.4|35=8|",
            "2024-01-01 10:00:01 8=FIX.4.4|35=8|",
            "2024-01-01 10:00:02 8=FIX.4.4|35=8|"
        };

                await File.WriteAllLinesAsync(tempFile, fileContent);

                // Act
                var result = await _sut.ReadAllAtOnceOptimizedAsync(tempFile);

                // Assert
                result.Should().NotBeNull();
                result.ExecutionReportMessages.Should().HaveCount(3);
                result.RejectMessages.Should().BeEmpty();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ReadAllAtOnceOptimizedAsync_FileWithOnlyRejectMessages_ReturnsOnlyRejectMessages()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var fileContent = new[]
                {
            "2024-01-01 10:00:00 8=FIX.4.4|35=3|",
            "2024-01-01 10:00:01 8=FIX.4.4|35=3|"
        };

                await File.WriteAllLinesAsync(tempFile, fileContent);

                // Act
                var result = await _sut.ReadAllAtOnceOptimizedAsync(tempFile);

                // Assert
                result.Should().NotBeNull();
                result.RejectMessages.Should().HaveCount(2);
                result.ExecutionReportMessages.Should().BeEmpty();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ReadAllAtOnceOptimizedAsync_EmptyFile_ReturnsEmptyResults()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                // Act
                var result = await _sut.ReadAllAtOnceOptimizedAsync(tempFile);

                // Assert
                result.Should().NotBeNull();
                result.ExecutionReportMessages.Should().BeEmpty();
                result.RejectMessages.Should().BeEmpty();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ReadAllAtOnceOptimizedAsync_FileWithNoFixMessages_ReturnsEmptyResults()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var fileContent = new[]
                {
            "2024-01-01 10:00:00 This is a log line without FIX message",
            "2024-01-01 10:00:01 Another line without FIX",
            ""
        };

                await File.WriteAllLinesAsync(tempFile, fileContent);

                // Act
                var result = await _sut.ReadAllAtOnceOptimizedAsync(tempFile);

                // Assert
                result.Should().NotBeNull();
                result.ExecutionReportMessages.Should().BeEmpty();
                result.RejectMessages.Should().BeEmpty();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}